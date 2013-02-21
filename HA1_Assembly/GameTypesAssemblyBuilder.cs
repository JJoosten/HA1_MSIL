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

		public void GenerateAssembly( List<GameType> gameTypes, Dictionary<string, List<PropertyField>> gameBehaviorProperties )
		{
			assemblyBuilder = new CustomAssemblyBuilder("GameTypes");

			foreach (GameType gameType in gameTypes)
			{
				Dictionary<string, PropertyField> properties = new Dictionary<string, PropertyField>();
				properties.Add("Name", new PropertyField() { PropertyType = typeof(String), PropertyName = "Name" });

				foreach (KeyValuePair<string, List<PropertyField>> item in gameBehaviorProperties)
				{
					//Check whether or not the current game type has a behavior
					bool hasBehavior = false;
					gameType.Behaviors.TryGetValue(item.Key, out hasBehavior);

					if (hasBehavior)
					{
						foreach (PropertyField property in item.Value)
						{
							//Prevent duplicate properties
							if (!properties.ContainsKey(property.PropertyName))
							{
								properties.Add(property.PropertyName, property);
							}
						}
					}
				}
				assemblyBuilder.CreateType(gameType.Name, properties);
			}
			assemblyBuilder.Save();
		}
	}
}
