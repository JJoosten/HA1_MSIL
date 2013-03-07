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
        public float Speed { get; set; }

        public Player()
        {
            // default initialize position
            Position = new Vector2(0,0);
            AABB = new Rectangle(0, 0, 0, 0);
            SpriteRectangle = new Rectangle(0, 0, 0, 0);
            Sprite = null;
            Speed = 100.0f;
        }
  
        public void Update(GameTime a_GameTime)
        {
            // simple movement
            int movY = Keyboard.GetState().IsKeyDown(Keys.W) == true ? -1 : 0;
            movY += Keyboard.GetState().IsKeyDown(Keys.S) == true ? 1 : 0;
            int movX = Keyboard.GetState().IsKeyDown(Keys.D) == true ? 1 : 0;
            movX -= Keyboard.GetState().IsKeyDown(Keys.A) == true ? 1 : 0;

            Vector2 movement = new Vector2(movX, movY);

            if( movement.LengthSquared() > 0)
                movement.Normalize();

            Position += movement * Speed * a_GameTime.ElapsedGameTime.Milliseconds * 0.001f;
        }

        public void Draw(GameTime a_GameTime, SpriteBatch a_SpriteBatch)
        {
            a_SpriteBatch.Draw(Sprite, Position, SpriteRectangle, Color.White, 10.0f, new Vector2(0,0), new Vector2(2,2), SpriteEffects.None, 0);
        }
    }
}
