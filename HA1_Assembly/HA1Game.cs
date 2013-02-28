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
#endregion

namespace HA1_Assembly
{
	public class HA1Game : Game
	{
		private GraphicsDeviceManager m_Graphics;
		private SpriteBatch m_SpriteBatch;
		private CodeParser m_CodeGenerator;


		public HA1Game()
			: base()
		{
			m_Graphics = new GraphicsDeviceManager( this );
			
			m_CodeGenerator = new CodeParser();

			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			//m_CodeGenerator.ParseDataLayout( Directory.GetCurrentDirectory() + @"\Content\DataStructures.xml");

			BehaviorTypesXmlReader behaviorTypesXml = new BehaviorTypesXmlReader();
			behaviorTypesXml.Parse(Directory.GetCurrentDirectory() + @"\Content\BehaviorTypes.xml");

			GameTypesXmlReader gameTypesXml = new GameTypesXmlReader();
			gameTypesXml.Parse(Directory.GetCurrentDirectory() + @"\Content\GameTypes.xml", behaviorTypesXml.GameBehaviorProperties);            
			gameTypesXml.ParseProperties(behaviorTypesXml.GameBehaviorProperties);

			GameTypesAssemblyBuilder ass = new GameTypesAssemblyBuilder();
			ass.GenerateAssembly(gameTypesXml.GameTypes);

			SceneXmlReader sceneXmlReader = new SceneXmlReader(Content);
			sceneXmlReader.Parse(Directory.GetCurrentDirectory() + @"\Content\Scene.xml", gameTypesXml.GameTypes);

            SceneManager sceneManager = new SceneManager();
            sceneManager.ParseObjects(sceneXmlReader.Objects, gameTypesXml.GameTypes);

            List<Object> drawableObjects = sceneManager.DrawableObjects;


            base.Initialize();
		}

		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			m_SpriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
		}

		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime a_GameTime)
		{
			if ( GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) )
				Exit();

			// TODO: Add your update logic here

			base.Update(a_GameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime a_GameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add render code here!

			base.Draw(a_GameTime);
		}
	}
}
