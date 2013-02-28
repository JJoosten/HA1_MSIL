using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace HA1_Assembly
{
	public class GameTypesAssemblyBuilder
	{
		private CustomAssemblyBuilder assemblyBuilder;

		public GameTypesAssemblyBuilder()
		{
		}

		public void GenerateAssembly( List<GameType> gameTypes )
		{
			assemblyBuilder = new CustomAssemblyBuilder("GameTypes");

			foreach (GameType gameType in gameTypes)
			{
				assemblyBuilder.CreateType(gameType.Name, gameType.Properties);
			}
			assemblyBuilder.Save();
		}
	}
}
