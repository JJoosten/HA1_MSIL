#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.Reflection;
#endregion

namespace HA1_Assembly
{
	public class HA1Game : Game
	{
        private static uint m_ScreenWidth = 1280;
        private static uint m_ScreenHeight = 720;

		private GraphicsDeviceManager m_Graphics;
		private SpriteBatch m_SpriteBatch;
		private CodeParser m_CodeGenerator;
		private SceneXmlReader m_SceneXmlReader;
		private SceneManager m_SceneManager;
		private Player m_Player;
		private Object m_GenGame;
        private AssemblyQuadTree quadTree;

        private List<Object> m_Movables;
        private Func<Rectangle, int> m_GenStaticCollisionCheck;
        //private Action<List<Object>, List<Object>> m_GenInitialize;

        private Action<SpriteBatch, Texture2D[]> m_GenGameDraw;
        private List<Object> m_Collidables;

		private List<Object> m_DynamicObjects;

        private Boolean m_DrawDebugRectangles = false;
        private List<VertexPositionColor[]> m_PrimitiveLines;
        private KeyboardState m_PrevKeyboardState;


		public HA1Game()
			: base()
		{
			m_Graphics = new GraphicsDeviceManager( this );
            m_Graphics.PreferredBackBufferWidth = 1280;
            m_Graphics.PreferredBackBufferHeight = 720;
            m_Graphics.ApplyChanges();

			m_SpriteBatch = new SpriteBatch(m_Graphics.GraphicsDevice);

			m_CodeGenerator = new CodeParser();

			m_Player = new Player();

			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
            // setup player
			m_Player.Sprite = Content.Load<Texture2D>(@"Sprites\TileSet.png");
			m_Player.SpriteRectangle = new Rectangle(467, 302, 38, 192);
            m_Player.AABB = new Rectangle(0, 0, 38, 192);
			m_Player.InitBullets();

            // setup dynamic game and create genGame.dll
			BehaviorTypesXmlReader behaviorTypesXml = new BehaviorTypesXmlReader();
			behaviorTypesXml.Parse(Directory.GetCurrentDirectory() + @"\Content\BehaviorTypes.xml");

			GameTypesXmlReader gameTypesXml = new GameTypesXmlReader();
			gameTypesXml.Parse(Directory.GetCurrentDirectory() + @"\Content\GameTypes.xml", behaviorTypesXml.GameBehaviorProperties);            
			gameTypesXml.ParseProperties(behaviorTypesXml.GameBehaviorProperties);

			GameTypesAssemblyBuilder ass = new GameTypesAssemblyBuilder();
			ass.GenerateAssembly(gameTypesXml.GameTypes);

			m_SceneXmlReader = new SceneXmlReader(Content);
			m_SceneXmlReader.Parse(Directory.GetCurrentDirectory() + @"\Content\Scene.xml", gameTypesXml.GameTypes);

			InitDynamicObjects(gameTypesXml);
			CloneDrawableObjects(gameTypesXml);

			m_SceneManager = new SceneManager();
			m_SceneManager.ParseObjects(m_SceneXmlReader.Objects, gameTypesXml.GameTypes, behaviorTypesXml.GameBehaviorProperties);

            List<Object> staticCollidableList = m_SceneManager.GetStaticObjectList("Collidable");
            List<Object> staticDrawableList = m_SceneManager.GetStaticObjectList("Drawable");
			m_DynamicObjects = m_SceneManager.GetObjectList("Movable");

            List<Tuple<Rectangle, int>> staticRectangles = new List<Tuple<Rectangle, int>>();
            foreach (Object obj in staticCollidableList)
                staticRectangles.Add(new Tuple<Rectangle, int>(GetRectangleFromObject(obj), GetHashFromObject(obj)));

            quadTree = new AssemblyQuadTree(new Rectangle(-3000, -3000, 6000, 6000), staticRectangles);

			// this class will generate the game assembly
			GameAssemblyBuilder gameAssemblyBuilder = new GameAssemblyBuilder();
			//gameAssemblyBuilder.GenerateGameObjects(m_SceneXmlReader.Objects, gameTypesXml.GameTypes);
            gameAssemblyBuilder.GenerateDrawFunction(staticDrawableList);
            gameAssemblyBuilder.GenerateStaticCollisionFunction(quadTree);
			gameAssemblyBuilder.Save();

            m_Collidables = staticCollidableList;

            // loads game dll (genGame.dll)
			LoadGameDLL();

            // clear all game objects, wont delete objects that are still referenced!
            m_SceneXmlReader.Clear();
            m_SceneManager.Clear();

            CreateDebugQuad();

			base.Initialize();
		}

        private void CreateDebugQuad()
        {
            m_PrimitiveLines = new List<VertexPositionColor[]>();

            // generate primitive
            foreach (Object collidable in m_Collidables)
            {
                Type type = collidable.GetType();

                PropertyInfo propertyInfo = type.GetProperty("Position");
                Vector2 position = (Vector2)propertyInfo.GetValue(collidable, null);

                propertyInfo = type.GetProperty("SpriteRectangle");
                Rectangle rectangle = (Rectangle)propertyInfo.GetValue(collidable, null);

                Vector3 pos = new Vector3(position, 0);
                VertexPositionColor[] primitivePositionColor = new VertexPositionColor[8];
                primitivePositionColor[0].Position = pos; 
                primitivePositionColor[1].Position = pos + new Vector3((float)rectangle.Width, 0, 0); 
                primitivePositionColor[2].Position = pos; 
                primitivePositionColor[3].Position = pos + new Vector3(0, (float)rectangle.Height, 0); 
                primitivePositionColor[4].Position = pos + new Vector3((float)rectangle.Width, 0, 0); 
                primitivePositionColor[5].Position = pos + new Vector3((float)rectangle.Width, (float)rectangle.Height, 0); 
                primitivePositionColor[6].Position = pos + new Vector3(0, (float)rectangle.Height, 0); 
                primitivePositionColor[7].Position = pos + new Vector3((float)rectangle.Width, (float)rectangle.Height, 0); 

                for (int i = 0; i < 8; ++i)
                    primitivePositionColor[i].Color = Color.Black;

                m_PrimitiveLines.Add(primitivePositionColor);
            }
        }
		
		private void InitDynamicObjects(GameTypesXmlReader a_GameTypesXml)
		{
			foreach (Object item in m_SceneXmlReader.Objects)
			{
				Type type = item.GetType();
				GameType gameType = a_GameTypesXml.GameTypes.Find(t => t.Name == type.Name);

				bool hasBehavior = false;
				bool hasValue = gameType.Behaviors.TryGetValue("Movable", out hasBehavior);

				if (gameType != null && hasBehavior)
				{
					PropertyInfo propertyInfo = type.GetProperty("Position");
					Vector2 position = (Vector2)propertyInfo.GetValue(item, null);

					propertyInfo = type.GetProperty("InitialPosition");
					propertyInfo.SetValue(item, position, null);
				}
			}
		}

		private void CloneDrawableObjects(GameTypesXmlReader a_GameTypesXml)
		{
			Random random = new Random();
			Vector2 offset = new Vector2();
			List<Object> clonedObjects = new List<object>();
			foreach (Object item in m_SceneXmlReader.Objects)
			{
				Type type = item.GetType();
				GameType gameType = a_GameTypesXml.GameTypes.Find(t => t.Name == type.Name);

				bool hasBehavior = false;
				bool hasValue = gameType.Behaviors.TryGetValue("Drawable", out hasBehavior);

				if (gameType != null && hasBehavior)
				{
					// get position
					PropertyInfo propertyInfo = type.GetProperty("Position");
					Vector2 position = (Vector2)propertyInfo.GetValue(item, null);

					propertyInfo = type.GetProperty("SpriteRectangle");
					Rectangle rectangle = (Rectangle)propertyInfo.GetValue(item, null);

					propertyInfo = type.GetProperty("SpriteRepeat");
					Vector2 spriteRepeat = (Vector2)propertyInfo.GetValue(item, null);
                    
                    propertyInfo = type.GetProperty("RepeatRandomRangeX");
                    Vector2 repeatRandomRangeX = (Vector2)propertyInfo.GetValue(item, null);
                    float repeatRandomWidthX = repeatRandomRangeX.Y - repeatRandomRangeX.X;

                    propertyInfo = type.GetProperty("RepeatRandomRangeY");
                    Vector2 repeatRandomRangeY = (Vector2)propertyInfo.GetValue(item, null);
                    float repeatRandomWidthY = repeatRandomRangeY.Y - repeatRandomRangeY.X;

					for (int y = 0; y < spriteRepeat.Y; y++)
					{
						for (int x = 0; x < spriteRepeat.X; x++)
						{
							if (x == 0 && y == 0)
								continue; //Do not clone the first one because it already exists

                            // use the random property to determine the offset of the original position
                            const int divisionValue = 1 << 10;
                            float randomNumberX = (float)random.Next(0, divisionValue) / (float)(divisionValue);
                            float randomNumberY = (float)random.Next(0, divisionValue) / (float)(divisionValue);

                            offset.X = rectangle.Width * x + repeatRandomRangeX.X + randomNumberX * repeatRandomWidthX;
                            offset.Y = rectangle.Height * y + repeatRandomRangeY.X + randomNumberY * repeatRandomWidthY;

							ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
							Object newObject = ci.Invoke(null);

							foreach (PropertyField property in gameType.Properties.Values)
							{
								propertyInfo = type.GetProperty(property.PropertyName);
								propertyInfo.SetValue(newObject, propertyInfo.GetValue(item, null), null);
							}

							propertyInfo = type.GetProperty("Position");
							propertyInfo.SetValue(newObject, position + offset, null);

							clonedObjects.Add(newObject);
						}
					}
				}
			}

			m_SceneXmlReader.Objects.AddRange(clonedObjects);
		}

		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			m_SpriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
            
		}

		private void LoadGameDLL()
		{
            // generate game
			Assembly assembly = Assembly.LoadFrom(Directory.GetCurrentDirectory() + "/GenGame.dll");
			m_GenGame = assembly.CreateInstance("GenGame");
			Type genGameType = m_GenGame.GetType();

            // get list of movables
            m_Movables = m_SceneManager.GetObjectList("Movable");

            // create collision delegate
            var staticCollisionCheck = genGameType.GetMethod("StaticCollisionCheck");
            m_GenStaticCollisionCheck = (Func<Rectangle, int>)Delegate.CreateDelegate(typeof(Func<Rectangle, int>), m_GenGame, staticCollisionCheck);
            
            // create initialize delegate and call initialize
            var initialize = genGameType.GetMethod("Initialize");

            // initialization of genGame.dll is not needed anymore because all its objects are static 
            //m_GenInitialize = (Action<List<Object>, List<Object>>)Delegate.CreateDelegate(typeof(Action<List<Object>, List<Object>>), m_GenGame, initialize);
            //m_GenInitialize(m_SceneXmlReader.Objects, m_Movables);

            // create draw delegate
            var draw = genGameType.GetMethod("Draw");
            m_GenGameDraw = (Action<SpriteBatch, Texture2D[]>)Delegate.CreateDelegate(typeof(Action<SpriteBatch, Texture2D[]>), m_GenGame, draw);

		}

		protected override void Update(GameTime a_GameTime)
		{
			if ( GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) )
				Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !m_PrevKeyboardState.IsKeyDown(Keys.Enter))
            {
                m_DrawDebugRectangles = !m_DrawDebugRectangles;
            }

            Vector3 position = m_Player.Position;

			m_Player.Update(a_GameTime);
			m_Player.UpdateBullets(a_GameTime);

			UpdateDynamicObjects(a_GameTime);

            //Collision detection
            Matrix rotMat = Matrix.CreateRotationZ(m_Player.Rotation);
            Vector2 playerRectTL = new Vector2((float)m_Player.AABB.X, (float)m_Player.AABB.Y);
            Vector2 playerRectBR = playerRectTL + new Vector2((float)m_Player.AABB.Width, (float)m_Player.AABB.Height);
            playerRectTL = Vector2.Transform(playerRectTL, rotMat);
            playerRectBR = Vector2.Transform(playerRectBR, rotMat);

            // calculate new bounds
            float minX = playerRectTL.X < playerRectBR.X ? playerRectTL.X : playerRectBR.X;
            float minY = playerRectTL.Y < playerRectBR.Y ? playerRectTL.Y : playerRectBR.Y;
            float maxX = playerRectTL.X > playerRectBR.X ? playerRectTL.X : playerRectBR.X;
            float maxY = playerRectTL.Y > playerRectBR.Y ? playerRectTL.Y : playerRectBR.Y;
            float width = maxX - minX;
            float height = maxY - minY;

            // new aabb
            Rectangle playerRect = new Rectangle();
            playerRect.X = (int)minX;
            playerRect.Y = (int)minY;
            playerRect.Width = (int)width;
            playerRect.Height = (int)height;

            // set aabb to player position
            playerRect.X = (int)m_Player.Position.X + 640 - ((int)playerRect.Width / 2);
            playerRect.Y = (int)m_Player.Position.Y + 360 - ((int)playerRect.Height / 2);
            playerRect.Width = (int)width;
            playerRect.Height = (int)height;


            int collisionHash = quadTree.CheckForCollision(playerRect); //(int)m_GenStaticCollisionCheck(playerRect);
            if (collisionHash != 0)
            {
                //Collided so game over
                //Console.WriteLine( String.Format("Hitted a object with hash {0}", collisionHash ) );
            }

            int RockHash = ("Rock").GetHashCode();
            int SandHash = ("Sand").GetHashCode();

            if (collisionHash == RockHash)
            {
                m_Player.Position = new Vector3(640, 360, 0);
            }
            else if (collisionHash == SandHash)
            {
                m_Player.Position = position;
            }

            m_PrevKeyboardState = Keyboard.GetState();
			base.Update(a_GameTime);
		}

		private void UpdateDynamicObjects(GameTime a_GameTime)
		{
			Random random = new Random();
			Vector2 offset = new Vector2();
			foreach (Object item in m_DynamicObjects)
			{
				Type type = item.GetType();
				PropertyInfo propertyInfoInitialPos = type.GetProperty("InitialPosition");
				Vector2 initialPosition = (Vector2)propertyInfoInitialPos.GetValue(item, null);

				PropertyInfo propertyInfoPos = type.GetProperty("Position");
				Vector2 position = (Vector2)propertyInfoPos.GetValue(item, null);

				PropertyInfo propertyInfoVel = type.GetProperty("Velocity");
				Vector2 velocity = (Vector2)propertyInfoVel.GetValue(item, null);

				PropertyInfo propertyInfoAcc = type.GetProperty("Acceleration");
				Vector2 acceleration = (Vector2)propertyInfoAcc.GetValue(item, null);

				PropertyInfo propertyInfoRect = type.GetProperty("SpriteRectangle");
                Rectangle rectangle = (Rectangle)propertyInfoRect.GetValue(item, null);

				PropertyInfo propertyInfo = type.GetProperty("SpriteRepeat");
				Vector2 spriteRepeat = (Vector2)propertyInfo.GetValue(item, null);
                    
                propertyInfo = type.GetProperty("RepeatRandomRangeX");
                Vector2 repeatRandomRangeX = (Vector2)propertyInfo.GetValue(item, null);
                float repeatRandomWidthX = repeatRandomRangeX.Y - repeatRandomRangeX.X;

                propertyInfo = type.GetProperty("RepeatRandomRangeY");
                Vector2 repeatRandomRangeY = (Vector2)propertyInfo.GetValue(item, null);
                float repeatRandomWidthY = repeatRandomRangeY.Y - repeatRandomRangeY.X;

				velocity += acceleration * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;
				position += velocity * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;

				propertyInfoVel.SetValue(item, velocity, null);
				propertyInfoPos.SetValue(item, position, null);

				rectangle.X = (int)position.X;
				rectangle.Y = (int)position.Y;

				if (type.Name == "Airplane")
				{
					Bullet[] bullets = m_Player.GetBullets();
					for (uint i = 0; i < m_Player.GetNumActiveBullets(); i++)
					{
						Bullet bullet = bullets[i];
						Rectangle bulletRect = new Rectangle((int)bullet.Position.X, (int)bullet.Position.Y, 32, 32);
						if ( bulletRect.Intersects(rectangle) )
						{
							const int divisionValue = 1 << 10;
                            float randomNumberX = (float)random.Next(0, divisionValue) / (float)(divisionValue);
                            float randomNumberY = (float)random.Next(0, divisionValue) / (float)(divisionValue);

                            offset.X = rectangle.Width + repeatRandomRangeX.X + randomNumberX * repeatRandomWidthX;
                            offset.Y = rectangle.Height + repeatRandomRangeY.X + randomNumberY * repeatRandomWidthY;

							propertyInfoPos.SetValue(item, initialPosition + offset, null);
							m_Player.DestroyBullet(i);
						}
					}
				}
			}
		}

		protected override void Draw(GameTime a_GameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix mat = Matrix.CreateTranslation(-m_Player.Position);
            m_SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, mat);
            {
                // insert call to DLL render method
                Texture2D[] textureArray = m_SceneXmlReader.Sprites.ToArray();
                m_GenGameDraw(m_SpriteBatch, textureArray);
            }
            m_SpriteBatch.End();

            m_SpriteBatch.Begin();
            {
                m_Player.Draw(a_GameTime, m_SpriteBatch);
            }
            m_SpriteBatch.End();

            // draw debug
            if (m_DrawDebugRectangles)
            {
                m_SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, mat);
                {
                    foreach (VertexPositionColor[] primitive in m_PrimitiveLines)
                    {
                        Vector3[] oldPositions = new Vector3[8];
                        for (int i = 0; i < 8; ++i)
                        {
                            oldPositions[i] = primitive[i].Position;
                            primitive[i].Position = Vector3.Transform(primitive[i].Position, mat);
                        }

                        GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, primitive, 0, 4);

                        for (int i = 0; i < 8; ++i)
                            primitive[i].Position = oldPositions[i];
                    }
                }
                m_SpriteBatch.End();
            }

			m_SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, mat);
            {
				DrawDynamicObjects(a_GameTime);
				m_Player.DrawBullets(a_GameTime, m_SpriteBatch);
            }
            m_SpriteBatch.End();



            base.Draw(a_GameTime);
        }

		private void DrawDynamicObjects(GameTime a_GameTime)
		{
			Texture2D[] textureArray = m_SceneXmlReader.Sprites.ToArray();
			foreach (Object drawable in m_DynamicObjects)
			{
				Type type = drawable.GetType();
				PropertyInfo propertyInfo = type.GetProperty("SpriteID");
				int textureID = (int)propertyInfo.GetValue(drawable, null);

				Texture2D texture = textureArray[textureID];

				// get position
				propertyInfo = type.GetProperty("Position");
				Vector2 position = (Vector2)propertyInfo.GetValue(drawable, null);

				propertyInfo = type.GetProperty("SpriteRectangle");
				Rectangle rectangle = (Rectangle)propertyInfo.GetValue(drawable, null);

				propertyInfo = type.GetProperty("SpriteRepeat");
				Vector2 spriteRepeat = (Vector2)propertyInfo.GetValue(drawable, null);
				if (spriteRepeat.X == 0) spriteRepeat.X = 1;
				if (spriteRepeat.Y == 0) spriteRepeat.Y = 1;

				propertyInfo = type.GetProperty("Rotation");
				float rotation = (float)propertyInfo.GetValue(drawable, null);

				propertyInfo = type.GetProperty("Scale");
				Vector2 scale = (Vector2)propertyInfo.GetValue(drawable, null);

				propertyInfo = type.GetProperty("Layer");
				float layer = (float)propertyInfo.GetValue(drawable, null);

				m_SpriteBatch.Draw(texture, position, rectangle, Color.White, rotation, Vector2.Zero, scale, 0, 0.0f);
			}
		}

        //JURRE
        public Rectangle GetRectangleFromObject(Object a_Object)
        {
            Type t = a_Object.GetType();
            MethodInfo methodInfo = t.GetProperty("AABB").GetGetMethod();
            Boolean check = true;
            Rectangle rect = (Rectangle)methodInfo.Invoke(a_Object, null);
            methodInfo = t.GetProperty("Position").GetGetMethod();
            Vector2 position = (Vector2)methodInfo.Invoke(a_Object, null);
            rect.X += (int)position.X;
            rect.Y += (int)position.Y;
            return rect;
        }

        public int GetHashFromObject(Object a_Object)
        {
            Type t = a_Object.GetType();
            MethodInfo methodInfo = t.GetProperty("Hash").GetGetMethod();
            int Hash = (int)methodInfo.Invoke(a_Object, null);
            return Hash;
        }
	}
}
