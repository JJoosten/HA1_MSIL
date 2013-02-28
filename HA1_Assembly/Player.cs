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

        public Player()
        {
            // default initialize position
            Position = new Vector2(0,0);
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
            
            Position += movement * 100.0f * (1.0f / a_GameTime.ElapsedGameTime.Milliseconds);
        }

        public void Draw(GameTime a_GameTime, SpriteBatch a_SpriteBatch)
        {
            a_SpriteBatch.Draw(Sprite, Position, SpriteRectangle, Color.White);
        }
    }
}
