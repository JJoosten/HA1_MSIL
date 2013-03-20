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

        private List<Object> m_Movables;
        private Func<Rectangle, bool> m_GenStaticCollisionCheck;
        private Action<List<Object>, List<Object>> m_GenInitialize;
        private Action<SpriteBatch, Texture2D[]> m_GenGameDraw;

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

			m_SceneManager = new SceneManager();
			m_SceneManager.ParseObjects(m_SceneXmlReader.Objects, gameTypesXml.GameTypes, behaviorTypesXml.GameBehaviorProperties);

            List<Object> staticCollidableList = m_SceneManager.GetStaticObjectList("Collidable");
            List<Object> staticDrawableList = m_SceneManager.GetStaticObjectList("Drawable");

            List<Rectangle> staticRectangles = new List<Rectangle>();
            foreach (Object obj in staticCollidableList)
                staticRectangles.Add(GetRectangleFromObject(obj));

            AssemblyQuadTree quadTree = new AssemblyQuadTree(new Rectangle(0, 0, 800, 480), staticRectangles);

			// this class will generate the game assembly
			GameAssemblyBuilder gameAssemblyBuilder = new GameAssemblyBuilder();
			gameAssemblyBuilder.GenerateGameObjects(m_SceneXmlReader.Objects, gameTypesXml.GameTypes);
            gameAssemblyBuilder.GenerateDrawFunction(staticDrawableList);
            gameAssemblyBuilder.GenerateStaticCollisionFunction(quadTree);
			gameAssemblyBuilder.Save();

            // loads game dll (genGame.dll)
			LoadGameDLL();

			base.Initialize();
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
            m_GenStaticCollisionCheck = (Func<Rectangle, bool>)Delegate.CreateDelegate(typeof(Func<Rectangle, bool>), m_GenGame, staticCollisionCheck);
            
            // create initialize delegate and call initialize
            var initialize = genGameType.GetMethod("Initialize");
            m_GenInitialize = (Action<List<Object>, List<Object>>)Delegate.CreateDelegate(typeof(Action<List<Object>, List<Object>>), m_GenGame, initialize);
            m_GenInitialize(m_SceneXmlReader.Objects, m_Movables);

            // create draw delegate
            var draw = genGameType.GetMethod("Draw");
            m_GenGameDraw = (Action<SpriteBatch, Texture2D[]>)Delegate.CreateDelegate(typeof(Action<SpriteBatch, Texture2D[]>), m_GenGame, draw);

		}

		protected override void Update(GameTime a_GameTime)
		{
			if ( GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) )
				Exit();

			m_Player.Update(a_GameTime);
			m_Player.UpdateBullets(a_GameTime);

            //Collision detection
            Rectangle playerRect = new Rectangle((int)m_Player.Position.X, (int)m_Player.Position.Y, (int)m_Player.SpriteRectangle.Width, (int)m_Player.SpriteRectangle.Height);


            bool check = (bool)m_GenStaticCollisionCheck(playerRect);
            if (check)
            {
                Console.WriteLine("Check");
            }

			base.Update(a_GameTime);
		}

		protected override void Draw(GameTime a_GameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			m_SpriteBatch.Begin();
			m_Player.Draw(a_GameTime, m_SpriteBatch);
			m_SpriteBatch.End();

			Matrix mat = Matrix.CreateTranslation(-m_Player.Position);
			m_SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, mat);

			m_Player.DrawBullets(a_GameTime, m_SpriteBatch);

            // insert call to DLL render method
            Texture2D[] textureArray = m_SceneXmlReader.Sprites.ToArray();
            m_GenGameDraw(m_SpriteBatch, textureArray);

			m_SpriteBatch.End();
		
			base.Draw(a_GameTime);
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
        

	}
}
