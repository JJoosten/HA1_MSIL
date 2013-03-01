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
		private Player m_Player;
        private Object m_GenGame;

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

            SceneManager sceneManager = new SceneManager();
			sceneManager.ParseObjects(m_SceneXmlReader.Objects, gameTypesXml.GameTypes, behaviorTypesXml.GameBehaviorProperties);
            //Retrieve lists with different type of behaviours
            List<Object> collidableList = sceneManager.GetObjectList("Collidable");
            List<Object> drawableList = sceneManager.GetObjectList("Drawable");
            List<Object> movableList = sceneManager.GetObjectList("Movable");
			
            // this class will generate the game assembly
            GameAssemblyBuilder gameAssemblyBuilder = new GameAssemblyBuilder();
			gameAssemblyBuilder.GenerateGameObjects(m_SceneXmlReader.Objects, gameTypesXml.GameTypes);
            gameAssemblyBuilder.GenerateDrawFunction();
            gameAssemblyBuilder.Save();

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
            Assembly assembly = Assembly.LoadFrom(Directory.GetCurrentDirectory() + "/GenGame.dll");

			m_GenGame = assembly.CreateInstance("GenGame");
			Type genGameType = m_GenGame.GetType();

			MethodInfo mi = genGameType.GetMethod("Initialize");
			mi.Invoke(m_GenGame, new object[] { m_SceneXmlReader.Objects });

			FieldInfo fi = genGameType.GetField("m_Tree_3");
			Object o = fi.GetValue(m_GenGame);
        }

        protected override void Update(GameTime a_GameTime)
		{
			if ( GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) )
				Exit();

            m_Player.Update(a_GameTime);

            // insert call to DLL update method

            base.Update(a_GameTime);
        }

        protected override void Draw(GameTime a_GameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            m_SpriteBatch.Begin();
            
            m_Player.Draw(a_GameTime, m_SpriteBatch);
            
            // insert call to DLL render method
			
            m_SpriteBatch.End();
		
			base.Draw(a_GameTime);
		}
	}
}
