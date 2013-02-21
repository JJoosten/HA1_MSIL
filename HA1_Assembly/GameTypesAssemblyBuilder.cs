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
		private List<PropertyField>[] gameBehaviorProperties;

		public GameTypesAssemblyBuilder()
		{
			gameBehaviorProperties = new List<PropertyField>[(int)GameBehaviors.Count];

			//Specify all required properties for a specific behavior (in this case GameBehaviors.Collidable)
			List<PropertyField> properties1 = new List<PropertyField>();
			properties1.Add(new PropertyField() { PropertyType = typeof(Vector2), PropertyName = "Position" });
			properties1.Add(new PropertyField() { PropertyType = typeof(Vector2), PropertyName = "Scale" });
			properties1.Add(new PropertyField() { PropertyType = typeof(Vector2), PropertyName = "Rotation" });
			properties1.Add(new PropertyField() { PropertyType = typeof(AABB), PropertyName = "AABB" });
			gameBehaviorProperties[(int)GameBehaviors.Collidable] = properties1;

			//Specify all required properties for GameBehaviors.Movable
			List<PropertyField> properties2 = new List<PropertyField>();
			properties2.Add(new PropertyField() { PropertyType = typeof(Vector2), PropertyName = "Position" });
			properties2.Add(new PropertyField() { PropertyType = typeof(Vector2), PropertyName = "Velocity" });
			properties2.Add(new PropertyField() { PropertyType = typeof(Vector2), PropertyName = "Acceleration" });
			gameBehaviorProperties[(int)GameBehaviors.Movable] = properties2;
		}

		public void GenerateAssembly( List<GameType> gameTypes )
		{
			assemblyBuilder = new CustomAssemblyBuilder("GameTypes");

			foreach (GameType gameType in gameTypes)
			{
				Dictionary<string, PropertyField> properties = new Dictionary<string, PropertyField>();
				properties.Add("Name", new PropertyField() { PropertyType = typeof(String), PropertyName = "Name" });

				for (int i = 0; i < (int)GameBehaviors.Count; i++)
				{
					//Check whether or not the current game type has a behavior
					if (gameType.Behaviors[i])
					{
						foreach (PropertyField property in gameBehaviorProperties[i])
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
