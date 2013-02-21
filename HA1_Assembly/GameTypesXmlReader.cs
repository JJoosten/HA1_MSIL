﻿using System;
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

		public void Parse( string filename )
		{
			XmlReader xmlReader = XmlReader.Create(filename);

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

						for (int i = 0; i < (int)GameBehaviors.Count; i++)
						{
							GameBehaviors behavior = (GameBehaviors)i;
							if (behaviorName == behavior.ToString())
							{
								gameType.Behaviors[(int)behavior] = true;
							}
						}
					}
				}
			}
		}
	}
}
