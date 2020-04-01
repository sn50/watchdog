using System;
using WatchdogFramework.Interface;

namespace WatchdogFramework
{
    /// <summary>
    /// Logger implementation that logs to console.
    /// </summary>
    public class ToConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
