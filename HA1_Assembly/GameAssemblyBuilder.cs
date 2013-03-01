using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

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
			MethodBuilder mb = m_TypeBuilder.DefineMethod("Initialize", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), new Type[] { typeof(List<Object>) });

			ILGenerator gen = mb.GetILGenerator();

			Type listType = a_Objects.GetType();
			MethodInfo miGetItem = listType.GetMethod("get_Item");

			gen.Emit(OpCodes.Nop);

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

		public void GenerateDrawFunction()
		{
			ILGenerator ilGenerator = m_AssemblyBuilder.CreateFunction(m_TypeBuilder, "Draw", MethodAttributes.Public, typeof(void), null);
			
			// TODO; generate draw function with all the different game objects that get drawn


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
