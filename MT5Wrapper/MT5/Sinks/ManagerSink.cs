using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using MT5Wrapper.Interface.EventSource;
using System;
using System.IO;
using System.Reflection;

namespace MT5Wrapper.MT5.Sinks
{
	internal class ManagerSink : CIMTManagerSink, IConnectionEventSource
	{
		private bool Disposed { get; set; } = false;

		public CIMTManagerAPI ManagerAPI { get; private set; }

		#region IConnectionEventSource

		public event EventHandler ConnectedEventHandler;
		public event EventHandler DisconnectedEventHandler;

		#endregion IConnectionEventSource

		public ManagerSink()
		{
			try {
				Initialize();
			}
			catch (Exception ex) {
				throw new TypeInitializationException(typeof(ManagerSink).AssemblyQualifiedName, ex);
			}
		}

		private string GetMTDllPath()
		{
			var exePath = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
			var dirPath = Path.GetDirectoryName(exePath.LocalPath);
			var dllPath = $@"{dirPath}\";
			return dllPath;
		}

		private void Initialize()
		{
			try {
				// Initialize the factory
				MTRetCode res = SMTManagerAPIFactory.Initialize(GetMTDllPath());
				if (MTRetCode.MT_RET_OK != res) {
					throw new MT5Exception("SMTManagerAPIFactory.Initialize failed", res);
				}
				// Receive the API version 
				res = SMTManagerAPIFactory.GetVersion(out uint version);
				if (MTRetCode.MT_RET_OK != res) {
					throw new MT5Exception("SMTManagerAPIFactory.GetVersion failed", res);
				}
				// Check API version
				if (version != SMTManagerAPIFactory.ManagerAPIVersion) {
					throw new MT5Exception($"Manager API version mismatch - {version}!={SMTManagerAPIFactory.ManagerAPIVersion}");
				}
				// Create new manager
				ManagerAPI = SMTManagerAPIFactory.CreateManager(version, out res);
				if (MTRetCode.MT_RET_OK != res) {
					throw new MT5Exception("SMTManagerAPIFactory.CreateManager failed", res);
				}
				if (null == ManagerAPI) {
					throw new MT5Exception("SMTManagerAPIFactory.CreateManager returned null");
				}
				//
				res = RegisterSink();
				if (MTRetCode.MT_RET_OK != res) {
					throw new MT5Exception("CIMTManagerSink.RegisterSink failed", res);
				}
				// Subscribe for events
				res = ManagerAPI.Subscribe(this);
				if (MTRetCode.MT_RET_OK != res) {
					throw new MT5Exception("CIMTManagerAPI.Subscribe failed", res);
				}
			}
			catch {
				ManagerAPI?.Release();
				SMTManagerAPIFactory.Shutdown();
				throw;
			}
		}

		#region CIMTManagerSink

		public override void OnDisconnect()
		{
			DisconnectedEventHandler?.Invoke(this, EventArgs.Empty);
			base.OnDisconnect();
		}
		public override void OnConnect()
		{
			ConnectedEventHandler?.Invoke(this, EventArgs.Empty);
			base.OnConnect();
		}

		protected override void Dispose(bool disposing)
		{
			if (Disposed) {
				return;
			}

			if (disposing) {
				ManagerAPI?.Unsubscribe(this);
				Disconnect();
				ManagerAPI?.Release();
				SMTManagerAPIFactory.Shutdown();
			}
			base.Dispose(disposing);

			Disposed = true;
		}

		#endregion CIMTManagerSink

		public void Connect(ConnectionParams config)
		{
			if (Disposed) {
				throw new ObjectDisposedException(GetType().FullName);
			}

			if (null == config) {
				throw new ArgumentNullException(nameof(config));
			}

			MTRetCode res = ManagerAPI.Connect(config.IP, config.Login, config.Password, null, CIMTManagerAPI.EnPumpModes.PUMP_MODE_FULL, config.ConnectionTimeout);

			if (MTRetCode.MT_RET_OK != res) {
				throw new MT5Exception($"Connect to {config.IP} as {config.Login} failed", res);
			}
		}

		public void Disconnect()
		{
			if (Disposed) {
				throw new ObjectDisposedException(GetType().FullName);
			}

			ManagerAPI.Disconnect();
		}
	}
}
