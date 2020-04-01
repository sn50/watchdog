using System;
using System.Collections.Generic;
using System.Configuration;
using DataStore.Context;
using DataStore.Entity;
using WatchdogFramework.Configuration;
using WatchdogFramework.Interface;

namespace WatchdogFramework
{
    public class Program
    {
        private static ILogger _logger;

        static void Main(string[] args)
        {
            _logger = new ToConsoleLogger();
            InitDatabase();
            new Program(_logger).Run();
        }

        public Program(
            ILogger logger
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Load and return config values from app config.
        /// </summary>
        /// <remarks>
        /// source: https://ivankahl.com/creating-custom-configuration-sections-in-app-config/
        /// </remarks>
        private static WatchdogConfiguration GetAppConfiguration()
        {
            var watchdogConfiguration = new WatchdogConfiguration { Servers = new List<Server>() };
            
            var customConfig = (ServerInstancesConfigurationSection)ConfigurationManager.GetSection("custom");
            foreach (ServerInstanceConfigurationElement serverConfig in customConfig.ServerInstances)
            {
                watchdogConfiguration.Servers.Add(new Server
                {
                    IpAddress = serverConfig.Address,
                    Name = serverConfig.Name,
                    Login = serverConfig.Login,
                    Password = serverConfig.Password,
                });
            }
            watchdogConfiguration.OpenTimeDeltaInSeconds = int.Parse(ConfigurationManager.AppSettings["openTimeDeltaInSeconds"]);
            watchdogConfiguration.VolumeToBalanceRatio = double.Parse(ConfigurationManager.AppSettings["volumeToBalanceRatio"]);

            return watchdogConfiguration;
        }

        private static void InitDatabase()
        {
            using (var ctx = new WatchdogDbContext())
            {
                ctx.Database.CreateIfNotExists();
            }
        }

        private void Run()
        {
            var config = GetAppConfiguration();

            using (var watchdog = new Watchdog(_logger, config.Servers, config.OpenTimeDeltaInSeconds))
            {
                watchdog.Start();
                UiTools.WaitTillUserQuits();
            }
        }
    }
}
