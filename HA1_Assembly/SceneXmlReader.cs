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
        public List<Texture2D> Sprites { get; set; }
        public Dictionary<string, int> SpritesDictionary = new Dictionary<string, int>();

		public SceneXmlReader(ContentManager a_ContentManager)
		{
			m_ContentManager = a_ContentManager;

			Objects = new List<object>();
            Sprites = new List<Texture2D>();
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
								if (info.PropertyType == typeof(String))
								{
									string s = ParseString(xmlReader, property.PropertyName);
									info.SetValue(o, s, null);
								}
								else if (info.PropertyType == typeof(int))
								{
									int i = ParseInt(xmlReader, property.PropertyName);
									info.SetValue(o, i, null);
								}
								else if (info.PropertyType == typeof(float))
								{
									float f = ParseFloat(xmlReader, property.PropertyName);
									info.SetValue(o, f, null);
								}
								else if (info.PropertyType == typeof(Vector2))
								{
									Vector2 v = ParseVector2(xmlReader, property.PropertyName);
									info.SetValue(o, v, null);
								}
								else if (info.PropertyType == typeof(Texture2D))
								{
                                    int textureid = ParseSprite(xmlReader, property.PropertyName);
                                    PropertyInfo spriteid = type.GetProperty("SpriteID");
                                    spriteid.SetValue(o, textureid, null);
								}
								else if (info.PropertyType == typeof(Rectangle))
								{
									Rectangle rectangle = ParseRectangle(xmlReader, property.PropertyName);
									info.SetValue(o, rectangle, null);
								}
							}

                            PropertyInfo infoHash = type.GetProperty("Hash");
                            infoHash.SetValue(o, gameType.Hash, null);

							Objects.Add(o);
						}
					}
				}
			}
		}

		private string ParseString(XmlReader xmlReader, string propertyName)
		{
			string s = null;
			if (xmlReader.MoveToAttribute(propertyName))
			{
				s = xmlReader.GetAttribute(propertyName);
			}
			return s;
		}

		private int ParseInt(XmlReader xmlReader, string propertyName)
		{
			int i = 0;
			if (xmlReader.MoveToAttribute(propertyName))
			{
				string strInt = xmlReader.GetAttribute(propertyName);

				if (!int.TryParse(strInt.Trim(), out i))
				{
					Console.WriteLine(string.Format("Failed to parse {0}", propertyName));
				}
			}
			return i;
		}

		private float ParseFloat(XmlReader xmlReader, string propertyName)
		{
			float f = 0.0f;
			if (xmlReader.MoveToAttribute(propertyName))
			{
				string strInt = xmlReader.GetAttribute(propertyName);

				if (!float.TryParse(strInt.Trim().Replace('.', ','), out f))
				{
					Console.WriteLine(string.Format("Failed to parse {0}", propertyName));
				}
			}
			return f;
		}

		private Vector2 ParseVector2(XmlReader xmlReader, string propertyName)
		{
			Vector2 v = new Vector2(0, 0);
			if(xmlReader.MoveToAttribute(propertyName))
			{
				string strProperty = xmlReader.GetAttribute(propertyName);

				string[] split = strProperty.Split(',');
				if (split.Length == 2)
				{
					if (!float.TryParse(split[0].Trim().Replace('.', ','), out v.X))
					{
						Console.WriteLine(string.Format("Failed to parse {0}.X", propertyName));
					}
					if (!float.TryParse(split[1].Trim().Replace('.', ','), out v.Y))
					{
						Console.WriteLine(string.Format("Failed to parse {0}.Y", propertyName));
					}
				}
			}
			return v;
		}

		private int ParseSprite(XmlReader xmlReader, string propertyName)
		{
            int textureID = 0;
			Texture2D sprite = null;
			if (xmlReader.MoveToAttribute(propertyName))
			{
				string strSprite = xmlReader.GetAttribute(propertyName);

                if (SpritesDictionary.ContainsKey(strSprite))
                {
                    textureID = SpritesDictionary[strSprite];
                }
                else
                {
                    sprite = m_ContentManager.Load<Texture2D>(strSprite);
                    textureID = Sprites.Count;
                    SpritesDictionary.Add(strSprite, textureID);
                    Sprites.Add(sprite);
                }
			}

            return textureID;
		}

		private Rectangle ParseRectangle(XmlReader xmlReader, string propertyName)
		{
			Rectangle rectangle = new Rectangle();

			string strPropertyMin = xmlReader.GetAttribute(string.Format("{0}Min", propertyName));
			string strPropertyMax = xmlReader.GetAttribute(string.Format("{0}Max", propertyName));

			if (strPropertyMin != null && strPropertyMax != null)
			{
				string[] splitMin = strPropertyMin.Split(',');
				string[] splitMax = strPropertyMax.Split(',');

				if (splitMin.Length == 2 && splitMax.Length == 2)
				{
					if (!int.TryParse(splitMin[0].Trim().Replace('.', ','), out rectangle.X))
					{
						Console.WriteLine(string.Format("Failed to parse {0}Min.X", propertyName));
					}
					if (!int.TryParse(splitMin[1].Trim().Replace('.', ','), out rectangle.Y))
					{
						Console.WriteLine(string.Format("Failed to parse {0}Min.Y", propertyName));
					}
					if (!int.TryParse(splitMax[0].Trim().Replace('.', ','), out rectangle.Width))
					{
						Console.WriteLine(string.Format("Failed to parse {0}Max.X", propertyName));
					}
					if (!int.TryParse(splitMax[1].Trim().Replace('.', ','), out rectangle.Height))
					{
						Console.WriteLine(string.Format("Failed to parse {0}Max.Y", propertyName));
					}
				}
			}
			return rectangle;
		}
	}
}
