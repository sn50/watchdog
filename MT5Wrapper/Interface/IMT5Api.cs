using MT5Wrapper.Interface.EventSource;

namespace MT5Wrapper.Interface
{
	public interface IMT5Api
	{
		string Name { get; }
		IDealEventSource DealEvents { get; }
		IConnectionEventSource ConnectionEvents { get; }

		void Connect(ConnectionParams connectionParams);
		void Disconnect();

		decimal GetUserBalance(ulong login);
	}
}
