using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStore.Context;
using DataStore.Entity;
using MetaQuotes.MT5CommonAPI;
using MT5Wrapper;
using MT5Wrapper.Interface;
using Pipeline.DisruptorAsync;
using Pipeline.DisruptorAsync.Impl;
using WatchdogFramework.Interface;

namespace WatchdogFramework
{
    public class Watchdog : IDisposable
    {
        private readonly List<Server> _servers;
        private readonly int _openTimeDeltaInSeconds;
        private readonly ILogger _logger;
        private readonly List<MT5Api> _eventsApiInst = new List<MT5Api>();
        private readonly List<MT5Api> _getBalanceApiInst = new List<MT5Api>();
        private IPipeline<Deal, bool> _pipeline;

        public Watchdog(
            ILogger logger,
            List<Server> servers,
            int openTimeDeltaInSeconds
            )
        {
            if (servers == null) throw new ArgumentNullException(nameof(servers));
            _openTimeDeltaInSeconds = openTimeDeltaInSeconds;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _servers = SaveServersToDataStore(servers);


            InitPipeline();

            for (int i = 0; i < _servers.Count; i++)
            {
                var inst1 = new MT5Api();
                var inst2 = new MT5Api();
                
                HookConnectionEventHandlers(inst1, $"Deal-Events API instance for server [{_servers[i]}]");
                HookConnectionEventHandlers(inst2, $"Get-Balance API instance for server [{_servers[i]}]");

                _eventsApiInst.Add(inst1);
                _getBalanceApiInst.Add(inst2);
            }
        }

        /// <summary>
        /// Save / Update servers from/in data store,
        /// and return up-to-date server list, in sync with the DB,
        /// with their IDs.
        /// </summary>
        private List<Server> SaveServersToDataStore(List<Server> servers)
        {
            var outputList = new List<Server>();
            // save servers to DB if not already saved
            // plus get all server data (ID) from DB 
            using (var ctx = new WatchdogDbContext())
            {
                foreach (var serverFromConfig in servers)
                {
                    var server = ctx.Servers.FirstOrDefault(s =>
                        s.IpAddress == serverFromConfig.IpAddress &&
                        s.Login == serverFromConfig.Login &&
                        s.Password == serverFromConfig.Password);

                    if (server == null)
                    {
                        server = serverFromConfig;
                        ctx.Servers.Add(server);
                    }
                    else
                    {
                        // update server name if found, could be changed
                        server.Name = serverFromConfig.Name;
                    }
                    outputList.Add(server);
                }
                ctx.SaveChanges();
            }

            return outputList;
        }

        private void InitPipeline()
        {
            var builder = new DisruptorPipelineBuilder();
            _pipeline = builder.Build<Deal, Deal>(deal =>
            {
                // count volume to balance ratio
                using (var ctx = new WatchdogDbContext())
                {
                    var dealEntity = ctx.Deals.First(d => d.Id == deal.Id);
                    dealEntity.VolumeToBalanceRatio = deal.Volume / deal.Balance;
                    ctx.SaveChanges();
                }
                return deal;

            }, 1).AddStep(deal =>
            {
                // find connections based on rules
                using (var ctx = new WatchdogDbContext())
                {
                    // RULES DEFINITION - START
                    var matches = ctx.Deals
                        .Where(d => d.Id != deal.Id)
                        .Where(d => DbFunctions.DiffSeconds(d.Date, deal.Date) < _openTimeDeltaInSeconds)
                        .Where(d => d.SymbolId == deal.SymbolId)
                        //.Where(d => d.VolumeToBalanceRatio) // TODO: what is the correct condition of the VtBR?
                        .ToList();

                    // RULES DEFINITION - END

                    if (matches.Any())
                    {
                        var dealWithGroup = matches.FirstOrDefault(d => d.GroupId != null);
                        var dealGroup = dealWithGroup != null
                            ? dealWithGroup.Group
                            : ctx.DealGroups.Add(new DealGroup());

                        var dealEntity = ctx.Deals.First(d => d.Id == deal.Id);
                        dealEntity.Group = dealGroup;

                        foreach (var match in matches)
                        {
                            match.Group = dealGroup;
                        }

                        // TODO: Cleanup empty groups, if any / could be scheduled as a periodic DB cleanup process

                        ctx.SaveChanges();

                        // LOG it
                        var sb = new StringBuilder($"Matches with [{deal}]:").AppendLine();
                        matches.ForEach(d => sb.AppendLine($" ==> [{d}]"));
                        _logger.Log(sb.ToString());
                    }
                }

                return true;
            }, 1).Create();
        }

        private void HookConnectionEventHandlers(IMT5Api mt5Api, string name)
        {
            mt5Api.ConnectionEvents.ConnectedEventHandler += (sender, args) =>
            {
                _logger.Log($"Connected {name}.");
            };

            mt5Api.ConnectionEvents.DisconnectedEventHandler += (sender, args) =>
            {
                _logger.Log($"Disconnected {name}.");
            };
        }

        /// <summary>
        /// Start receiving new deal events and logging
        /// based on the set-up rules.
        /// </summary>
        public void Start()
        {
            for (int i = 0; i < _servers.Count; i++)
            {
                var server = _servers[i];
                var eventsApiInst = _eventsApiInst[i];
                var getBalanceApiInst = _getBalanceApiInst[i];

                async void DealEvents_OnDealAddEventHandler(object control, CIMTDeal deal)
                {
                    using (var ctx = new WatchdogDbContext())
                    {
                        // get user from db
                        var userLogin = (long)deal.Login();
                        var user = ctx.Users.FirstOrDefault(u => u.Login == userLogin) ?? ctx.Users.Add(new User
                        {
                            Login = userLogin,
                        });

                        // get symbol from db
                        var symbolName = deal.Symbol();
                        var symbol = ctx.Symbols.FirstOrDefault(s => s.Name == symbolName) ?? ctx.Symbols.Add(new Symbol
                        {
                            Name = symbolName
                        });

                        var externalId = deal.Deal();
                        var dealType = ToDealType(deal.Action());
                        var volume = deal.Volume();
                        var fromMilliseconds = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(deal.TimeMsc());

                        var dealEntity = ctx.Deals.Add(new Deal
                        {
                            Symbol = symbol,
                            User = user,
                            ServerId = server.Id,

                            ExternalId = (long)externalId,
                            Type = dealType,
                            Volume = (long)volume,
                            Date = fromMilliseconds,
                        });

                        dealEntity.Balance = getBalanceApiInst.GetUserBalance(deal.Login()); // TODO how to do this correctly?
                        ctx.SaveChanges();

                        //_logger.Log($"Incoming deal: {dealEntity} (Time: {deal.Time()} || TimeMSC: {deal.TimeMsc()})");
                        _logger.Log($"Incoming deal: {dealEntity}");

                        await _pipeline.Execute(dealEntity);
                    }
                }

                eventsApiInst.DealEvents.DealAddEventHandler += DealEvents_OnDealAddEventHandler;

                _logger.Log($"Starting connecting to [{server.Name}, {server.IpAddress}] ...");

                var connectionParams = new ConnectionParams
                {
                    IP = server.IpAddress,
                    Login = (ulong)server.Login,
                    Password = server.Password,
                    Name = server.Name,
                };
                getBalanceApiInst.Connect(connectionParams);
                eventsApiInst.Connect(connectionParams);
            }
        }

        private static DealType ToDealType(uint action)
        {
            switch (action)
            {
                case 0:
                    return DealType.Buy;
                case 1:
                    return DealType.Sell;
                default:
                    throw new ArgumentException("Action must be either 0 (buy) or 1 (sell)!");
            }
        }

        public void Dispose()
        {
            _pipeline?.Dispose();

            DisconnectApis();
            DisposeApis();
        }

        private void DisconnectApis()
        {
            foreach (var api in _eventsApiInst)
            {
                api.Disconnect();
            }

            foreach (var api in _getBalanceApiInst)
            {
                api.Disconnect();
            }
        }

        private void DisposeApis()
        {
            foreach (var api in _eventsApiInst)
            {
                api.Dispose();
            }

            foreach (var api in _getBalanceApiInst)
            {
                api.Dispose();
            }
        }
    }
}
