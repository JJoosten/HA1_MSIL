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
#endregion

namespace HA1_Assembly
{
    public class HA1Game : Game
    {
        private GraphicsDeviceManager m_Graphics;
        private SpriteBatch m_SpriteBatch;
        private CodeParser m_CodeGenerator;
        private Player m_Player;


        public HA1Game()
            : base()
        {
            m_Graphics = new GraphicsDeviceManager( this );

            m_SpriteBatch = new SpriteBatch(m_Graphics.GraphicsDevice);

            m_CodeGenerator = new CodeParser();

            m_Player = new Player() { Position = new Vector2(10,10) };

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

			GameTypesAssemblyBuilder ass = new GameTypesAssemblyBuilder();
			ass.GenerateAssembly(gameTypesXml.GameTypes, behaviorTypesXml.GameBehaviorProperties);

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

        protected override void Update(GameTime a_GameTime)
        {
            if ( GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) )
                Exit();

            // TODO: Add your update logic here
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
