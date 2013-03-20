using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HA1_Assembly
{
	public class GameTypesXmlReader
	{
		public List<GameType> GameTypes { get; private set; }

		public GameTypesXmlReader()
		{
			GameTypes = new List<GameType>();
		}

		public void Parse(string a_Filename, Dictionary<string, List<PropertyField>> a_GameBehaviorProperties)
		{
			XmlReader xmlReader = XmlReader.Create(a_Filename);

			GameType gameType = null;
			while (xmlReader.Read())
			{
				xmlReader.MoveToElement();

				if (xmlReader.NodeType == XmlNodeType.Element)
				{
					if (xmlReader.Name == "Type")
					{
						gameType = new GameType();
						gameType.Name = xmlReader.GetAttribute("name");
						GameTypes.Add(gameType);
					}
					else if (xmlReader.Name == "Behavior")
					{
						string behaviorName = xmlReader.GetAttribute("name");

						foreach (string item in a_GameBehaviorProperties.Keys)
						{
							if (behaviorName == item)
							{
								gameType.Behaviors.Add(behaviorName, true);
							}
						}
					}
				}
			}
		}

		public void ParseProperties(Dictionary<string, List<PropertyField>> a_GameBehaviorProperties)
		{
			foreach (GameType gameType in GameTypes)
			{
				gameType.Properties.Add("Name", new PropertyField() { PropertyType = typeof(String), PropertyName = "Name" });

				foreach (KeyValuePair<string, List<PropertyField>> item in a_GameBehaviorProperties)
				{
					//Check whether or not the current game type has a behavior
					bool hasBehavior = false;
					gameType.Behaviors.TryGetValue(item.Key, out hasBehavior);

					if (hasBehavior)
					{
						foreach (PropertyField property in item.Value)
						{
							//Prevent duplicate properties
							if (!gameType.Properties.ContainsKey(property.PropertyName))
							{
								gameType.Properties.Add(property.PropertyName, property);
							}
						}
					}
				}
			}
		}
	}
}
