using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HA1_Assembly
{
	public class GameAssemblyBuilder
	{
		private CustomAssemblyBuilder m_AssemblyBuilder;
		private TypeBuilder m_TypeBuilder;

		public GameAssemblyBuilder()
		{
			m_AssemblyBuilder = new CustomAssemblyBuilder("GenGame");

			// creates GenGame class
			m_TypeBuilder = m_AssemblyBuilder.CreateType("GenGame");
		}

		public void GenerateGameObjects( List<Object> a_Objects, List<GameType> a_GameTypes )
		{
			MethodBuilder mb = m_TypeBuilder.DefineMethod("Initialize", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), new Type[] { typeof(List<Object>), typeof(List<Object>) });

			ILGenerator gen = mb.GetILGenerator();

			Type listType = a_Objects.GetType();
			MethodInfo miGetItem = listType.GetMethod("get_Item");
			MethodInfo miAdd = listType.GetMethod("Add");

			gen.Emit(OpCodes.Nop);
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_2);
			FieldBuilder fbList = m_TypeBuilder.DefineField(string.Format("m_{0}", "Dynamics"), typeof(List<Object>), FieldAttributes.Public);
			gen.Emit(OpCodes.Stfld, fbList);

			// Example: initializing object list
			/*ConstructorInfo ci = listType.GetConstructor(System.Type.EmptyTypes);
			gen.Emit(OpCodes.Newobj, ci);
			FieldBuilder fbList = m_TypeBuilder.DefineField(string.Format("m_{0}", "Dynamics"), typeof(List<Object>), FieldAttributes.Public);
			gen.Emit(OpCodes.Stfld, fbList);*/

			// Example: adding an object to a list
			/*gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, fbList);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldc_I4, (int)i);
			gen.Emit(OpCodes.Callvirt, miGetItem);
			gen.Emit(OpCodes.Castclass, type);
			gen.Emit(OpCodes.Callvirt, miAdd);*/

			int i = 0;
			foreach (Object item in a_Objects)
			{
				Type type = item.GetType();
				LocalBuilder paramValues = gen.DeclareLocal(type);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Ldc_I4, (int)i);

				gen.Emit(OpCodes.Callvirt, miGetItem);
				gen.Emit(OpCodes.Castclass, type);
				FieldBuilder fieldBuilder = m_TypeBuilder.DefineField(string.Format("m_{0}_{1}", type.Name, i), type, FieldAttributes.Public);
				gen.Emit(OpCodes.Stfld, fieldBuilder);
				i++;
			}
			gen.Emit(OpCodes.Ret);
		}

		public void GenerateUpdateFunction(List<Object> a_DynamicObjects)
		{
			// TODO: generate the update function with all the different update objects

			MethodBuilder mb = m_TypeBuilder.DefineMethod("Update", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), new Type[] { typeof(List<Object>), typeof(float) });
			ILGenerator gen = mb.GetILGenerator();

			LocalBuilder local1 = gen.DeclareLocal(typeof(int));
			LocalBuilder local2 = gen.DeclareLocal(typeof(bool));

			Type listType = a_DynamicObjects.GetType();
			MethodInfo miGetItem = listType.GetMethod("get_Item");
			MethodInfo miGetCount = listType.GetMethod("get_Count");

			MethodInfo miMulVectorFloat = typeof(Vector2).GetMethod("op_Multiply", new Type[] { typeof(Vector2), typeof(float) });
			MethodInfo miAddVectorVector = typeof(Vector2).GetMethod("op_Addition", new Type[] { typeof(Vector2), typeof(Vector2) });

			Type objectType = a_DynamicObjects[0].GetType();
			//PropertyInfo infoVelocity = objectType.GetProperty("Velocity");
			MethodInfo miGetPosition = objectType.GetMethod("get_Position");
			MethodInfo miSetPosition = objectType.GetMethod("set_Position");
			MethodInfo miGetVelocity = objectType.GetMethod("get_Velocity");
			MethodInfo miSetVelocity = objectType.GetMethod("set_Velocity", new Type[] { typeof(Vector2) });
			MethodInfo miGetAcceleration = objectType.GetMethod("get_Acceleration");

			Label label1 = gen.DefineLabel();
			Label label2 = gen.DefineLabel();

			gen.Emit(OpCodes.Nop);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Stloc, local1);
			gen.Emit(OpCodes.Br_S, label1);
			gen.MarkLabel(label2);
			gen.Emit(OpCodes.Nop);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldloc_0);

			gen.Emit(OpCodes.Callvirt, miGetItem);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Callvirt, miGetVelocity);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Callvirt, miGetItem);
			gen.Emit(OpCodes.Callvirt, miGetAcceleration);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Call, miMulVectorFloat);
			gen.Emit(OpCodes.Call, miAddVectorVector);
			gen.Emit(OpCodes.Callvirt, miSetVelocity);

			gen.Emit(OpCodes.Nop);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Callvirt, miGetItem);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Callvirt, miGetPosition);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Callvirt, miGetItem);
			gen.Emit(OpCodes.Callvirt, miGetVelocity);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Call, miMulVectorFloat);
			gen.Emit(OpCodes.Call, miAddVectorVector);
			gen.Emit(OpCodes.Callvirt, miSetPosition);

			gen.Emit(OpCodes.Nop);
			gen.Emit(OpCodes.Nop);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Add);
			gen.Emit(OpCodes.Stloc_0, local2);
			gen.MarkLabel(label1);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Callvirt, miGetCount);
			gen.Emit(OpCodes.Clt);
			gen.Emit(OpCodes.Stloc_1, local1);
			gen.Emit(OpCodes.Ldloc_1);
			gen.Emit(OpCodes.Brtrue_S, label2);
			gen.Emit(OpCodes.Ret);

			/*gen.Emit(OpCodes.Nop);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Callvirt, miGetItem);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Callvirt, miGetVelocity);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Callvirt, miGetItem);
			gen.Emit(OpCodes.Callvirt, miGetAcceleration);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Call, miMulVectorFloat);
			gen.Emit(OpCodes.Call, miAddVectorVector);
			gen.Emit(OpCodes.Callvirt, miSetVelocity);
			gen.Emit(OpCodes.Nop);
			gen.Emit(OpCodes.Ret);*/
		}

        public void GenerateDrawFunction(List<Object> a_StaticDrawableObjects, List<Object> a_DynamicDrawableObjects)
        {
            MethodBuilder mb = m_TypeBuilder.DefineMethod("Draw", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), new Type[] { typeof(SpriteBatch), typeof(Texture2D[]) });
            ILGenerator ilGenerator = mb.GetILGenerator();

            // TODO; generate draw function with all the different game objects that get drawn
            if (a_StaticDrawableObjects != null)
            {
                foreach (Object drawable in a_StaticDrawableObjects)
                {
                    Type type = drawable.GetType();
                    
                    // get sprite
                    PropertyInfo propertyInfo = type.GetProperty("SpriteID");
                    int textureID = (int)propertyInfo.GetValue(drawable, null);

                    // get position
                    propertyInfo = type.GetProperty("Position");
                    Vector2 position = (Vector2)propertyInfo.GetValue(drawable, null);

                    propertyInfo = type.GetProperty("SpriteRectangle");
                    Rectangle rectangle = (Rectangle)propertyInfo.GetValue(drawable, null);

                    propertyInfo = type.GetProperty("Rotation");
                    float rotation = (float)propertyInfo.GetValue(drawable, null);

                    propertyInfo = type.GetProperty("Scale");
                    Vector2 scale = (Vector2)propertyInfo.GetValue(drawable, null);

                    ilGenerator.Emit(OpCodes.Ldarg_1); // loads the local argument spritebatch on the evaluation stack
                    ilGenerator.Emit(OpCodes.Ldarg_2); // loads the local argument texture array on the evaluation stack

                    Behaviors.DrawStaticSprite(ilGenerator, textureID, position, rectangle, rotation, scale);
                }
            }

            // end the creation of this function
            ilGenerator.Emit(OpCodes.Ret);
        }

		public void Save()
		{
			m_TypeBuilder.CreateType();
			m_AssemblyBuilder.Save();
		}
	}
}
