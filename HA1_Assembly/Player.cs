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
        public float Rotation { get; set; }

		public Player()
		{
			// default initialize position
			Position = new Vector3(640, 360, 0);
			AABB = new Rectangle(0, 0, 0, 0);
			SpriteRectangle = new Rectangle(0, 0, 0, 0);
			Sprite = null;
			Velocity = new Vector3(0.0f, 0.0f, 0);
			Acceleration = new Vector3(0.0f, 0.0f, 0);
            Rotation = 0;
		}
  
		public void Update(GameTime a_GameTime)
		{
            float dtSpeed = ((float)a_GameTime.ElapsedGameTime.Milliseconds / 1000.0f) * 100.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
			{
                Velocity += new Vector3(-8.0f, 0, 0) * dtSpeed;
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
			{
                Velocity += new Vector3(8.0f, 0, 0) * dtSpeed;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
			{
                Velocity += new Vector3(0, -20.0f, 0) * dtSpeed;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
			{
                Velocity += new Vector3(0, 10.0f, 0) * dtSpeed;
			}

			Velocity += Acceleration * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;
            Position += Velocity * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;

            if (Velocity.LengthSquared() != 0)
            {
                float diff = ((float)Math.Atan2(Velocity.X, Velocity.Y) - Rotation);

                if (diff >= MathHelper.ToRadians(180) ||
                    diff <= -MathHelper.ToRadians(180))
                {
                    Console.WriteLine("Deg: " + diff);
                    Rotation *= -1;
                }


                Rotation += diff * 0.1f;
            }
            
            Velocity *= 0.95f;

		}

		public void Draw(GameTime a_GameTime, SpriteBatch a_SpriteBatch)
		{
            Vector2 centerOfSprite = new Vector2(SpriteRectangle.Width, SpriteRectangle.Height) * 0.5f;
            Vector2 position = new Vector2(640, 360);
            a_SpriteBatch.Draw(Sprite, position, SpriteRectangle, Color.White, -Rotation, centerOfSprite, new Vector2(1, 1), SpriteEffects.None, 0);
		}
	}
}
