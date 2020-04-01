using MetaQuotes.MT5CommonAPI;

namespace MT5Wrapper.Interface.EventSource
{
	public delegate void DealEventHandler(object control, CIMTDeal deal);

	public interface IDealEventSource
	{
		event DealEventHandler DealAddEventHandler;
	}
}
