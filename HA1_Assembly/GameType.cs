using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA1_Assembly
{
	public enum GameBehaviors
	{
		Collidable,
		Movable,
		Count
	}

	public class GameType
	{
		public string Name { get; set; }

		public bool[] Behaviors { get; set; }

		public GameType()
		{
			Behaviors = new bool[(int)GameBehaviors.Count];
		}
	}
}
