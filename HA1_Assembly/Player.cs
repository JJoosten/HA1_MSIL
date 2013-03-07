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
		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; set; }
		public Vector2 Acceleration { get; set; }

		public Player()
		{
			// default initialize position
			Position = new Vector2(0, 440);
			AABB = new Rectangle(0, 0, 0, 0);
			SpriteRectangle = new Rectangle(0, 0, 0, 0);
			Sprite = null;
			Velocity = new Vector2(0, 0.0f);
			Acceleration = new Vector2(0.0f, 200.0f);
		}
  
		public void Update(GameTime a_GameTime)
		{
			// simple movement
			/*int movY = Keyboard.GetState().IsKeyDown(Keys.W) == true ? -1 : 0;
			movY += Keyboard.GetState().IsKeyDown(Keys.S) == true ? 1 : 0;
			int movX = Keyboard.GetState().IsKeyDown(Keys.D) == true ? 1 : 0;
			movX -= Keyboard.GetState().IsKeyDown(Keys.A) == true ? 1 : 0;*/

			Velocity = new Vector2(0.0f, Velocity.Y);
			if (Keyboard.GetState().IsKeyDown(Keys.Left))
			{
				Velocity = new Vector2(-100.0f, Velocity.Y);
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Right))
			{
				Velocity = new Vector2(100.0f, Velocity.Y);
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Up))
			{
				Velocity = new Vector2(Velocity.X, -200.0f);
			}

			Velocity += Acceleration * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;
			Position += Velocity * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;

			if (Position.Y > 440 - AABB.Height)
				Position = new Vector2(Position.X, 440 - AABB.Height);
		}

		public void Draw(GameTime a_GameTime, SpriteBatch a_SpriteBatch)
		{
			a_SpriteBatch.Draw(Sprite, Position, SpriteRectangle, Color.White, 0.0f, new Vector2(0,0), new Vector2(1,1), SpriteEffects.None, 0);
		}
	}
}
