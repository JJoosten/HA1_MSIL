#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace HA1_Assembly
{
    public static class Program
    {
        private static HA1Game game;

        [STAThread]
        static void Main()
        {
            game = new HA1Game();
            game.Run();
        }
    }
}
