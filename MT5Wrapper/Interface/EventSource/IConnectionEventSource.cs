using System;

namespace MT5Wrapper.Interface.EventSource
{
	public interface IConnectionEventSource
	{
		event EventHandler ConnectedEventHandler;
		event EventHandler DisconnectedEventHandler;
	}
}
