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
		public static void DrawStaticSprite(ILGenerator ilGenerator, int texture_ID, Vector2 position, Rectangle textureRectangle, float rotation, Vector2 scale, float layer)
		{
			// ldarg_1 is already loaded on the stack by caller function(spritebatch)
			// ldarg_2 is already loaded on the stack by caller function(texture)

			// push texture from texture array on evaluation stack
			ilGenerator.Emit(OpCodes.Ldc_I4, texture_ID);
			ilGenerator.Emit(OpCodes.Ldelem, typeof(Texture2D));

			// generate vector2 and push on evaluation stack
			ilGenerator.Emit(OpCodes.Ldc_R4, position.X);
			ilGenerator.Emit(OpCodes.Ldc_R4, position.Y);
			ilGenerator.Emit(OpCodes.Newobj, typeof(Vector2).GetConstructor(new Type[] { typeof(float), typeof(float) })) ;
			
			// generate nullable rectangle and push on evaluation stack
			ilGenerator.Emit(OpCodes.Ldc_I4, textureRectangle.X);
			ilGenerator.Emit(OpCodes.Ldc_I4, textureRectangle.Y);
			ilGenerator.Emit(OpCodes.Ldc_I4, textureRectangle.Width);
			ilGenerator.Emit(OpCodes.Ldc_I4, textureRectangle.Height);
			ilGenerator.Emit(OpCodes.Newobj, typeof(Rectangle).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }));
			ilGenerator.Emit(OpCodes.Newobj, typeof(Nullable<Rectangle>).GetConstructor(new Type[]{ typeof(Rectangle) }));

			// generate color and push on evaluation stack
			ilGenerator.Emit(OpCodes.Ldc_R4, 1.0f);
			ilGenerator.Emit(OpCodes.Ldc_R4, 1.0f);
			ilGenerator.Emit(OpCodes.Ldc_R4, 1.0f);
			ilGenerator.Emit(OpCodes.Ldc_R4, 1.0f);
			ilGenerator.Emit(OpCodes.Newobj, typeof(Color).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) }));

			// generate rotation and push on evaluation stack
			ilGenerator.Emit(OpCodes.Ldc_R4, rotation);

			// generate origin
			ilGenerator.Emit(OpCodes.Ldc_R4, 0.0f);
			ilGenerator.Emit(OpCodes.Ldc_R4, 0.0f);
			ilGenerator.Emit(OpCodes.Newobj, typeof(Vector2).GetConstructor(new Type[] { typeof(float), typeof(float) })) ;
			
			// generate vector2 and push on evaluation stack (scale)
			ilGenerator.Emit(OpCodes.Ldc_R4, scale.X);
			ilGenerator.Emit(OpCodes.Ldc_R4, scale.Y);
			ilGenerator.Emit(OpCodes.Newobj, typeof(Vector2).GetConstructor(new Type[] { typeof(float), typeof(float) }));
			 
			// generate SpriteEffect and push on evaluation stack 
			ilGenerator.Emit(OpCodes.Ldc_I4_0);
			
			// generate layer and push on evaluation stack
			ilGenerator.Emit(OpCodes.Ldc_R4, layer);



			// call draw
			MethodInfo drawCall = typeof(SpriteBatch).GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Texture2D), 
																																		typeof(Vector2), 
																																		typeof(Nullable<Rectangle>), 
																																		typeof(Color), 
																																		typeof(float), 
																																		typeof(Vector2), 
																																		typeof(Vector2), 
																																		typeof(SpriteEffects), 
																																		typeof(float) }, null);
			ilGenerator.Emit(OpCodes.Call, drawCall);

		}
	}
}
