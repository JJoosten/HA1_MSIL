using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace HA1_Assembly
{
	public class SceneXmlReader
	{
		private ContentManager m_ContentManager;

		public List<Object> Objects { get; set; }

		public SceneXmlReader(ContentManager a_ContentManager)
		{
			m_ContentManager = a_ContentManager;

			Objects = new List<object>();
		}

		public void Parse(string a_Filename, List<GameType> a_GameTypes)
		{
			Assembly assembly = Assembly.LoadFrom(Directory.GetCurrentDirectory() + "/GameTypes.dll");

			XmlReader xmlReader = XmlReader.Create(a_Filename);

			while (xmlReader.Read())
			{
				xmlReader.MoveToElement();

				if (xmlReader.NodeType == XmlNodeType.Element)
				{
					foreach (GameType gameType in a_GameTypes)
					{
						if (xmlReader.Name == gameType.Name)
						{
							Object o = assembly.CreateInstance(gameType.Name);
							Type type = assembly.GetType(gameType.Name);

							foreach (PropertyField property in gameType.Properties.Values)
							{
								PropertyInfo info = type.GetProperty(property.PropertyName);

								string s = info.PropertyType.ToString();
								if (info.PropertyType == typeof(Vector2))
								{
									Vector2 v = ParseVector2(xmlReader, property.PropertyName);
									info.SetValue(o, v, null);
								}
								else if (info.PropertyType == typeof(Texture2D))
								{
									//Texture2D sprite = ParseSprite(xmlReader, property.PropertyName);
									//info.SetValue(o, sprite, null);
								}
							}
							Objects.Add(o);
						}
					}
				}
			}
		}

		private Vector2 ParseVector2(XmlReader xmlReader, string propertyPrefix)
		{
			//xmlReader.MoveToAttribute(string.Format("{0}X", propertyPrefix));
			string strPropertyX = xmlReader.GetAttribute(string.Format("{0}X", propertyPrefix));
			string strPropertyY = xmlReader.GetAttribute(string.Format("{0}Y", propertyPrefix));
			//string strPropertyZ = xmlReader.GetAttribute(string.Format("{0}Z"), propertyPrefix);

			Vector2 v = new Vector2(0, 0);
			if (!float.TryParse(strPropertyX, out v.X))
			{
				Console.WriteLine(string.Format("Failed to parse {0}X", propertyPrefix));
			}
			if (!float.TryParse(strPropertyY, out v.Y))
			{
				Console.WriteLine(string.Format("Failed to parse {0}Y", propertyPrefix));
			}
			return v;
		}

		private Texture2D ParseSprite(XmlReader xmlReader, string propertyName)
		{
			string strSprite = xmlReader.GetAttribute(propertyName);

			Texture2D sprite = m_ContentManager.Load<Texture2D>(strSprite);
			return sprite;
		}
	}
}
