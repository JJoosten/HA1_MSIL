#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace HA1_Assembly
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using( HA1Game game = new HA1Game() )
                game.Run();
        }
    }
}
