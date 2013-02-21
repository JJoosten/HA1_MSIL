using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA1_Assembly
{
	public class GameType
	{
		public string Name { get; set; }

		public Dictionary<string, bool> Behaviors { get; set; }

		public GameType()
		{
			Behaviors = new Dictionary<string, bool>();
		}
	}
}
