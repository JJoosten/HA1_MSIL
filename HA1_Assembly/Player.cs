using System;
using System.Collections.Generic;
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

		private Bullet[] m_Bullets;
		private uint m_MaxBullets;
		private uint m_NumActiveBulets;
		private Rectangle m_BulletRect;

		private KeyboardState m_PrevKeyboardState;
		private float m_ShootDT;

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
			m_PrevKeyboardState = Keyboard.GetState();
			m_ShootDT = 200.0f;
		}

		public void InitBullets()
		{
			m_BulletRect = new Rectangle(40, 172, 28,28);

			m_MaxBullets = 10;
			
            m_Bullets = new Bullet[m_MaxBullets];
			for (uint i = 0; i < m_MaxBullets; i++)
				m_Bullets[i] = new Bullet();

			m_NumActiveBulets = 0;
		}

		public uint GetNumActiveBullets()
		{
			return m_NumActiveBulets;
		}

		public Bullet[] GetBullets()
		{
			return m_Bullets;
		}
  
		public void Update(GameTime a_GameTime)
		{
            float dtSpeed = ((float)a_GameTime.ElapsedGameTime.Milliseconds / 1000.0f) * 1000.0f;

            // move player
            Vector3 addativeDirection = new Vector3(0);
            if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
                addativeDirection = new Vector3(-1.0f, 0, 0);
			else if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
                addativeDirection = new Vector3(1.0f, 0, 0);

			if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
                addativeDirection += new Vector3(0, -1.0f, 0);
			else if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
                addativeDirection += new Vector3(0, 1.0f, 0);

			if (m_PrevKeyboardState.IsKeyUp(Keys.Space) && Keyboard.GetState().IsKeyDown(Keys.Space))
			{
				ShootBullet();
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Space))
			{
				m_ShootDT -= a_GameTime.ElapsedGameTime.Milliseconds;
				if (m_ShootDT <= 0)
				{
					ShootBullet();
					m_ShootDT = 200.0f;
				}
			}
				
            if( addativeDirection.LengthSquared() != 0)
                addativeDirection.Normalize();
            Velocity += addativeDirection * dtSpeed; 

			Velocity += Acceleration * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;
            Position += Velocity * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;

            // rotate player
            if (Velocity.LengthSquared() != 0)
            {
                float diff = ((float)Math.Atan2(Velocity.X, Velocity.Y) - Rotation);

                if (diff >= MathHelper.ToRadians(180) ||
                    diff <= -MathHelper.ToRadians(180))
                    Rotation *= -1;

                Rotation += diff * 0.1f;
            }
            
            Velocity *= 0.95f;

			m_PrevKeyboardState = Keyboard.GetState();
		}

		public void Draw(GameTime a_GameTime, SpriteBatch a_SpriteBatch)
		{
            Vector2 centerOfSprite = new Vector2(SpriteRectangle.Width, SpriteRectangle.Height) * 0.5f;
            Vector2 position = new Vector2(640, 360);
            a_SpriteBatch.Draw(Sprite, position, SpriteRectangle, Color.White, -Rotation, centerOfSprite, new Vector2(1, 1), SpriteEffects.None, 0);
		}

		public void UpdateBullets(GameTime a_GameTime)
		{
			for (uint i = 0; i < m_NumActiveBulets; i++)
			{
				m_Bullets[i].Position += m_Bullets[i].Velocity * a_GameTime.ElapsedGameTime.Milliseconds;

				Vector3 pos = m_Bullets[i].Position - Position;
				if (pos.X < 0 || pos.X > 1280 || pos.Y < 0 || pos.Y > 720)
				{
					DestroyBullet(i);
				}
			}
		}

		public void DrawBullets(GameTime a_GameTime, SpriteBatch a_SpriteBatch)
		{
			for (uint i = 0; i < m_NumActiveBulets; i++)
			{
				Vector2 pos = new Vector2(m_Bullets[i].Position.X, m_Bullets[i].Position.Y);
				a_SpriteBatch.Draw(Sprite, pos, m_BulletRect, Color.White);
			}
		}

		public void ShootBullet()
		{
			if (m_NumActiveBulets < m_MaxBullets - 1)
			{
				m_Bullets[m_NumActiveBulets].Velocity = new Vector3(0, -1.0f, 0); // Vector3.Normalize(Velocity);
				m_Bullets[m_NumActiveBulets].Position = Position + new Vector3(640, 360, 0);
				m_NumActiveBulets++;
			}
		}

		public void DestroyBullet(uint a_index)
		{
			Bullet tmpBullet = m_Bullets[a_index];
			m_Bullets[a_index] = m_Bullets[m_NumActiveBulets - 1];
			m_Bullets[m_NumActiveBulets - 1] = tmpBullet;
			m_NumActiveBulets--;
		}
	}
}
