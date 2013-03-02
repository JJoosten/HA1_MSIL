using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HA1_Assembly
{
    public static class Behaviors
    {
        public static void DrawStaticSprite(ILGenerator ilGenerator, Texture2D texture, Vector2 position, Rectangle textureRectangle, Color color)
        {
            // the first param on evaluation stack is the local var spritebatch

            // TODO: push the texture from a member var on the evualuationStack
            ilGenerator.Emit(OpCodes.Ldc_R4, position.X);
            ilGenerator.Emit(OpCodes.Ldc_R4, position.Y);
            ilGenerator.Emit(OpCodes.Ldc_I4, textureRectangle.X);
            ilGenerator.Emit(OpCodes.Ldc_I4, textureRectangle.Y);
            ilGenerator.Emit(OpCodes.Ldc_I4, textureRectangle.Width);
            ilGenerator.Emit(OpCodes.Ldc_I4, textureRectangle.Height);
            ilGenerator.Emit(OpCodes.Ldc_I4_4, color.PackedValue);

            MethodInfo drawCall = typeof(SpriteBatch).GetMethod("Draw", new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle), typeof(Color)});
            ilGenerator.EmitCall(OpCodes.Call, drawCall, null);
        }
    }
}
