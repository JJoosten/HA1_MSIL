using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HA1_Assembly
{
	public class Player
	{
		public Texture2D Sprite { get; set; }
		public Rectangle SpriteRectangle { get; set; }
		public Rectangle AABB { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 Velocity { get; set; }
		public Vector3 Acceleration { get; set; }

		public Player()
		{
			// default initialize position
			Position = new Vector3(640, 360, 0);
			AABB = new Rectangle(0, 0, 0, 0);
			SpriteRectangle = new Rectangle(0, 0, 0, 0);
			Sprite = null;
			Velocity = new Vector3(0.0f, 0.0f, 0);
			Acceleration = new Vector3(0.0f, 0.0f, 0);
		}
  
		public void Update(GameTime a_GameTime)
		{
			// simple movement
			/*int movY = Keyboard.GetState().IsKeyDown(Keys.W) == true ? -1 : 0;
			movY += Keyboard.GetState().IsKeyDown(Keys.S) == true ? 1 : 0;
			int movX = Keyboard.GetState().IsKeyDown(Keys.D) == true ? 1 : 0;
			movX -= Keyboard.GetState().IsKeyDown(Keys.A) == true ? 1 : 0;*/

			if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
			{
				Velocity += new Vector3(-20.0f, 0, 0);
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
			{
				Velocity += new Vector3(20.0f, 0, 0);
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
			{
				Velocity += new Vector3(0, -20.0f, 0);
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
			{
				Velocity += new Vector3(0, 20.0f, 0);
			}

			Velocity *= 0.9f;

			Velocity += Acceleration * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;
			Position += Velocity * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;

			//if (Position.Y > 440 - AABB.Height)
			//	Position = new Vector2(Position.X, 440 - AABB.Height);
		}

		public void Draw(GameTime a_GameTime, SpriteBatch a_SpriteBatch)
		{
			a_SpriteBatch.Draw(Sprite, new Vector2( 320, 240 ), SpriteRectangle, Color.White, 0.0f, new Vector2(0,0), new Vector2(1,1), SpriteEffects.None, 0);
		}
	}
}
