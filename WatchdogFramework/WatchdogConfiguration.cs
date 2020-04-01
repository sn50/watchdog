using System.Collections.Generic;
using DataStore.Entity;

namespace WatchdogFramework
{
    public class WatchdogConfiguration
    {
        public List<Server> Servers { get; set; }
        public int OpenTimeDeltaInSeconds { get; set; }
        public double VolumeToBalanceRatio { get; set; }
    }
}