﻿#region Using Statements
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
		private GraphicsDeviceManager m_Graphics;
		private SpriteBatch m_SpriteBatch;
		private CodeParser m_CodeGenerator;
		private SceneXmlReader m_SceneXmlReader;
		private SceneManager m_SceneManager;
		private Player m_Player;
		private Object m_GenGame;

        private MethodInfo m_GenInitialize;
        private MethodInfo m_GenGameUpdate;
        private MethodInfo m_GenGameDraw;
        private MethodInfo m_GenStaticCollisionCheck;

		public HA1Game()
			: base()
		{
			m_Graphics = new GraphicsDeviceManager( this );

			m_SpriteBatch = new SpriteBatch(m_Graphics.GraphicsDevice);

			m_CodeGenerator = new CodeParser();

			m_Player = new Player();

			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			m_Player.Sprite = Content.Load<Texture2D>(@"Sprites\TileSet.png");
			m_Player.SpriteRectangle = new Rectangle(5, 5, 32, 32);
		  
			//m_CodeGenerator.ParseDataLayout( Directory.GetCurrentDirectory() + @"\Content\DataStructures.xml");
			
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
			//Retrieve lists with different type of behaviors
			List<Object> collidableList = m_SceneManager.GetObjectList("Collidable");
			List<Object> drawableList = m_SceneManager.GetObjectList("Drawable");
			List<Object> movableList = m_SceneManager.GetObjectList("Movable");

            List<Object> staticList = m_SceneManager.GetStaticObjectList();
            //TEST JURRE
            List<Rectangle> staticRectangles = new List<Rectangle>();
            foreach (Object obj in staticList)
            {
                staticRectangles.Add(GetRectangleFromObject(obj));
            }

            AssemblyQuadTree quadTree = new AssemblyQuadTree(new Rectangle(0, 0, 1280, 720), staticRectangles);

        
			// this class will generate the game assembly
			GameAssemblyBuilder gameAssemblyBuilder = new GameAssemblyBuilder();
			gameAssemblyBuilder.GenerateGameObjects(m_SceneXmlReader.Objects, gameTypesXml.GameTypes);
            gameAssemblyBuilder.GenerateDrawFunction(drawableList, drawableList);
            gameAssemblyBuilder.GenerateStaticCollisionFunction(quadTree);
			gameAssemblyBuilder.Save();

			LoadGameDLL();

            //Exit();

			base.Initialize();

            Rectangle rect = new Rectangle();
            Rectangle rect2 = new Rectangle();
            rect.Intersects(rect2);

		}

		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			m_SpriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
            
		}

		private void LoadGameDLL()
		{
			Assembly assembly = Assembly.LoadFrom(Directory.GetCurrentDirectory() + "/GenGame.dll");

			m_GenGame = assembly.CreateInstance("GenGame");
			Type genGameType = m_GenGame.GetType();

			MethodInfo mi = genGameType.GetMethod("Initialize");
			List<Object> movableList = m_SceneManager.GetObjectList("Movable");            
			mi.Invoke(m_GenGame, new object[] { m_SceneXmlReader.Objects, movableList });
            m_GenInitialize = genGameType.GetMethod("Initialize");
            m_GenInitialize.Invoke(m_GenGame, new object[] { m_SceneXmlReader.Objects, movableList });

            m_GenGameUpdate = genGameType.GetMethod("Update");

            m_GenGameDraw = genGameType.GetMethod("Draw");

            m_GenStaticCollisionCheck = genGameType.GetMethod("StaticCollisionCheck");
		}

		protected override void Update(GameTime a_GameTime)
		{
			if ( GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) )
				Exit();

			m_Player.Update(a_GameTime);

			// insert call to DLL update method
            

            //Collision detection
            Rectangle playerRect = new Rectangle((int)m_Player.Position.X, (int)m_Player.Position.Y, (int)m_Player.SpriteRectangle.Width, (int)m_Player.SpriteRectangle.Height);

            bool check = (bool)m_GenStaticCollisionCheck.Invoke(m_GenGame, new object[] { playerRect });
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

            // insert call to DLL render method
            Texture2D[] textureArray = m_SceneXmlReader.Sprites.ToArray();
            m_GenGameDraw.Invoke(m_GenGame, new object[] { m_SpriteBatch, textureArray });

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
