#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace HA1_Assembly
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private static HA1Game game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            game = new HA1Game();
            game.Run();
        }
    }
}
