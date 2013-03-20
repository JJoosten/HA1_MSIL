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
		private CustomAssemblyBuilder m_AssemblyBuilder;

		public GameTypesAssemblyBuilder()
		{
		}

		public void GenerateAssembly( List<GameType> a_GameTypes )
		{
			m_AssemblyBuilder = new CustomAssemblyBuilder("GameTypes");

			foreach (GameType gameType in a_GameTypes)
			{
				m_AssemblyBuilder.CreateType(gameType.Name, gameType.Properties);
			}
			m_AssemblyBuilder.Save();
		}
	}
}
