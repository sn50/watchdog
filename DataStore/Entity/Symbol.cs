using System;

namespace DataStore.Entity
{
    /// <summary>
    /// Trading symbol, such as GBPUSD etc.
    /// </summary>
    public class Symbol
    {
        public Symbol()
        {
            
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}