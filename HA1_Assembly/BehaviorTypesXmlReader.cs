using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HA1_Assembly
{
	public class BehaviorTypesXmlReader
	{
		public Dictionary<string, List<PropertyField>> GameBehaviorProperties { get; private set; }

		public BehaviorTypesXmlReader()
		{
			GameBehaviorProperties = new Dictionary<string, List<PropertyField>>();
		}

		public void Parse( string filename )
		{
			XmlReader xmlReader = XmlReader.Create(filename);

			List<PropertyField> properties = null;
			while (xmlReader.Read())
			{
				xmlReader.MoveToElement();

				if (xmlReader.NodeType == XmlNodeType.Element)
				{
					if (xmlReader.Name == "Behavior")
					{
						properties = new List<PropertyField>();
						xmlReader.MoveToAttribute("name");
						string behaviorName = xmlReader.GetAttribute("name");
						GameBehaviorProperties.Add(behaviorName, properties);
					}
					else if (xmlReader.Name == "Property")
					{
						string propertyName = xmlReader.GetAttribute("name");
						string propertyType = xmlReader.GetAttribute("type");

						Type type = null;
						switch (propertyType)
						{
							case "Vector2":
								type = typeof(Vector2);
								break;
							case "Rectangle":
								type = typeof(Rectangle);
								break;
							case "Texture2D":
								type = typeof(Texture2D);
								break;
							default:
								break;
						}

						PropertyField property = new PropertyField() { PropertyName = propertyName, PropertyType = type };
						properties.Add(property);
					}
				}
			}
		}
	}
}
