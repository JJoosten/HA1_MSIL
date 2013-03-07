﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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

		public void GenerateUpdateFunction()
		{
			// TODO: generate the update function with all the different update objects
		}

        public void GenerateDrawFunction( )//List<Object> a_StaticDrawableObjects, List<Object> a_DynamicDrawableObjects)
        {
            ILGenerator ilGenerator = m_AssemblyBuilder.CreateFunction(m_TypeBuilder, "Draw", MethodAttributes.Public, typeof(void), new Type[] { typeof(SpriteBatch) });

            /*
            // TODO; generate draw function with all the different game objects that get drawn
            if (a_StaticDrawableObjects != null)
            {
                foreach (Object drawable in a_StaticDrawableObjects)
                {
                    Type type = drawable.GetType();

                    //ilGenerator.Emit(OpCodes.Ldarg_1); // loads the local argument spritebatch on the evaluation stack
                    

                    //Behaviors.DrawStaticSprite(ilGenerator);
                }
            }*/

            // end the creation of this function
            ilGenerator.Emit(OpCodes.Ret);
        }

        public void GenerateStaticCollisionFunction(AssemblyQuadTree a_Tree)
        {
            ILGenerator ilGenerator = m_AssemblyBuilder.CreateFunction(m_TypeBuilder, "StaticCollisionCheck", MethodAttributes.Public, typeof(Boolean), new Type[] { typeof(Rectangle) });
            //Generate the massive if statements

            Label trueLabel = ilGenerator.DefineLabel();
            Label falseLabel = ilGenerator.DefineLabel();
            
            LocalBuilder localRectangle = ilGenerator.DeclareLocal(typeof(Rectangle));
            LocalBuilder localIntX = ilGenerator.DeclareLocal(typeof(int));
            LocalBuilder localIntY = ilGenerator.DeclareLocal(typeof(int));
            LocalBuilder localIntWidth = ilGenerator.DeclareLocal(typeof(int));
            LocalBuilder localIntHeight = ilGenerator.DeclareLocal(typeof(int));

            ilGenerator.Emit(OpCodes.Ldarg_1);                                      //Load rectangle onto stack
            ilGenerator.Emit(OpCodes.Dup);                                          //Duplicate the rectangle
            ilGenerator.Emit(OpCodes.Ldfld, typeof(Rectangle).GetField("X") );      //Get the X field from the rectangle at the top of the stack ( gets popped and pushed )
            ilGenerator.Emit(OpCodes.Stloc_1);                                      //Store X in our local_1 variable
            ilGenerator.Emit(OpCodes.Dup);                                          //Duplicate the rectangle
            ilGenerator.Emit(OpCodes.Ldfld, typeof(Rectangle).GetField("Y"));       //Get the Y field from the rectangle at the top of the stack ( gets popped and pushed )
            ilGenerator.Emit(OpCodes.Stloc_2);                                      //Store Y in our local_2 variable
            ilGenerator.Emit(OpCodes.Dup);                                          //Duplicate the rectangle
            ilGenerator.Emit(OpCodes.Ldfld, typeof(Rectangle).GetField("Width"));   //Get the Width field from the rectangle at the top of the stack ( gets popped and pushed )
            ilGenerator.Emit(OpCodes.Stloc_3);                                      //Store Width in our local_3 variable
            ilGenerator.Emit(OpCodes.Ldfld, typeof(Rectangle).GetField("Height"));  //Get the Height field from the rectangle at the top of the stack ( gets popped and pushed )
            ilGenerator.Emit(OpCodes.Stloc, 4);                                     //Store Height in our local_4 variable

            a_Tree.GenerateAssembly(ilGenerator, falseLabel, trueLabel);

            ilGenerator.MarkLabel(falseLabel);
            ilGenerator.Emit(OpCodes.Ldc_I4, 0);
            ilGenerator.Emit(OpCodes.Ret);

            ilGenerator.MarkLabel(trueLabel);
            ilGenerator.Emit(OpCodes.Ldc_I4, 1);
            ilGenerator.Emit(OpCodes.Ret);            
        }

		public void Save()
		{
			m_TypeBuilder.CreateType();
			m_AssemblyBuilder.Save();
		}
	}
}
