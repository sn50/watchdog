using System;

namespace DataStore.Entity
{
    /// <summary>
    /// Buy or sell type of deal.
    /// </summary>
    [Flags]
    public enum DealType
    {
        Buy = 0,
        Sell = 1,
    }
}