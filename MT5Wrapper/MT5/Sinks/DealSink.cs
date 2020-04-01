using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using MT5Wrapper.Interface.EventSource;
using System;

namespace MT5Wrapper.MT5.Sinks
{
	internal class DealSink : CIMTDealSink, IDealEventSource
	{
		private bool Disposed { get; set; } = false;

		#region IDealEventSource

		public event DealEventHandler DealAddEventHandler;

		#endregion IDealEventSource

		private CIMTManagerAPI Manager { get; }

		public DealSink(CIMTManagerAPI manager)
		{
			Manager = manager ?? throw new ArgumentNullException(nameof(manager));
			try {
				Initialize();
			}
			catch (Exception ex) {
				throw new TypeInitializationException(typeof(DealSink).AssemblyQualifiedName, ex);
			}
		}

		private void Initialize()
		{
			MTRetCode res = RegisterSink();
			if (MTRetCode.MT_RET_OK != res) {
				throw new MT5Exception("Register deal sink failed", res);
			}

			res = Manager.DealSubscribe(this);
			if (MTRetCode.MT_RET_OK != res) {
				throw new MT5Exception("Subscribe deal sink failed", res);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (Disposed) {
				return;
			}

			if (disposing) {
				Manager.DealUnsubscribe(this);
			}

			base.Dispose(disposing);

			Disposed = true;
		}

		public override void OnDealAdd(CIMTDeal deal)
		{
			DealAddEventHandler?.Invoke(this, deal);
		}
	}
}
