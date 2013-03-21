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
        public int Hash { get; set; }

		public Dictionary<string, bool> Behaviors { get; set; }

		public Dictionary<string, PropertyField> Properties { get; set; }

		public GameType()
		{
			Behaviors = new Dictionary<string, bool>();
			Properties = new Dictionary<string, PropertyField>();
		}
	}
}
