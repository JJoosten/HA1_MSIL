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

		private Bullet[] m_Bullets;
		private int m_MaxBullets;
		private int m_NumActiveBulets;
		private Rectangle m_BulletRect;

		private KeyboardState m_PrevKeyboardState;

		public Player()
		{
			// default initialize position
			Position = new Vector3(640, 360, 0);
			AABB = new Rectangle(0, 0, 0, 0);
			SpriteRectangle = new Rectangle(0, 0, 0, 0);
			Sprite = null;
			Velocity = new Vector3(0.0f, 0.0f, 0);
			Acceleration = new Vector3(0.0f, 0.0f, 0);
			m_PrevKeyboardState = Keyboard.GetState();
		}

		public void InitBullets()
		{
			m_BulletRect = new Rectangle(37, 169, 32, 32);

			m_MaxBullets = 10;
			m_Bullets = new Bullet[m_MaxBullets];
			for (uint i = 0; i < m_MaxBullets; i++)
			{
				m_Bullets[i] = new Bullet();
			}
			m_NumActiveBulets = 0;
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
			if (m_PrevKeyboardState.IsKeyUp(Keys.Space) && Keyboard.GetState().IsKeyDown(Keys.Space))
			{
				ShootBullet();
			}

			Velocity *= 0.9f;

			Velocity += Acceleration * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;
			Position += Velocity * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;

			//if (Position.Y > 440 - AABB.Height)
			//	Position = new Vector2(Position.X, 440 - AABB.Height);

			m_PrevKeyboardState = Keyboard.GetState();
		}

		public void Draw(GameTime a_GameTime, SpriteBatch a_SpriteBatch)
		{
			a_SpriteBatch.Draw(Sprite, new Vector2( 320, 240 ), SpriteRectangle, Color.White, 0.0f, new Vector2(0,0), new Vector2(1,1), SpriteEffects.None, 0);
		}

		public void UpdateBullets(GameTime a_GameTime)
		{
			for (uint i = 0; i < m_NumActiveBulets; i++)
			{
				m_Bullets[i].Position += m_Bullets[i].Velocity * a_GameTime.ElapsedGameTime.Milliseconds;

				Vector3 pos = m_Bullets[i].Position - Position;
				if (pos.X < 0 || pos.X > 640 || pos.Y < 0 || pos.Y > 480)
				{
					Bullet tmpBullet = m_Bullets[i];
					m_Bullets[i] = m_Bullets[m_NumActiveBulets - 1];
					m_Bullets[m_NumActiveBulets - 1] = tmpBullet;
					m_NumActiveBulets--;
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
				m_Bullets[m_NumActiveBulets].Velocity = new Vector3(0, -0.2f, 0); // Vector3.Normalize(Velocity);
				m_Bullets[m_NumActiveBulets].Position = Position + new Vector3(320, 240, 0);
				m_NumActiveBulets++;
			}
		}
	}
}
