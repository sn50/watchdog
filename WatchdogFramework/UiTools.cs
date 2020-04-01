using System;

namespace WatchdogFramework
{
    public static class UiTools
    {
        /// <summary>
        /// Wait for user input 'Q' to continue the program execution,
        /// otherwise wait for another user input. Report via messages
        /// to console, so user knows what is happening.
        /// </summary>
        public static void WaitTillUserQuits()
        {
            Console.WriteLine("Running. Type 'Q' on your keyboard to quit the program. ");
            var input = Console.ReadKey(true);
            while (true)
            {
                if (input.Key == ConsoleKey.Q)
                {
                    Console.WriteLine("Quitting program...");
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid character. Type 'Q' on your keyboard to quit the program.");
                }

                input = Console.ReadKey(true);
            }
        }
    }
}