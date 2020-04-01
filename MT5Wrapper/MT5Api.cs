using MetaQuotes.MT5CommonAPI;
using MT5Wrapper.Interface;
using MT5Wrapper.Interface.EventSource;
using MT5Wrapper.MT5.Sinks;
using System;

namespace MT5Wrapper
{
	public class MT5Api : IMT5Api, IDisposable
	{
		private bool Disposed { get; set; } = false;

		public string Name { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (Disposed) {
				return;
			}

			if (disposing) {
				_dealSink?.Dispose();
				ManagerSink?.Dispose();
			}
			Disposed = true;
		}

		private ManagerSink ManagerSink { get; } = new ManagerSink();

		private DealSink _dealSink;
		public IDealEventSource DealEvents {
			get {
				return _dealSink ?? (_dealSink = new DealSink(ManagerSink.ManagerAPI));
			}
		}

		public IConnectionEventSource ConnectionEvents { get => ManagerSink; }

		public void Connect(ConnectionParams connectionParams)
		{
			Name = connectionParams.Name;
			ManagerSink.Connect(connectionParams);
		}

		public void Disconnect()
		{
			Name = default;
			ManagerSink.Disconnect();
		}

		public decimal GetUserBalance(ulong login)
		{
			using (var account = ManagerSink.ManagerAPI.UserCreateAccount()) {
				if (null == account) {
					throw new MT5Exception($"Create user object for user {login}@{Name} failed");
				}
				var retCode = ManagerSink.ManagerAPI.UserAccountRequest(login, account);
				if (MTRetCode.MT_RET_OK != retCode) {
					throw new MT5Exception($"UserAccountRequest for user {login}@{Name} failed", retCode);
				}
				return Convert.ToDecimal(account.Balance());
			}
		}
	}
}
