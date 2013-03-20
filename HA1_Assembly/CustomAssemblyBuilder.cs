using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.CodeDom.Compiler;
using System.Globalization;

namespace HA1_Assembly
{
	public class PropertyField
	{
		public string PropertyName { get; set; }
		public Type PropertyType { get; set; }
	}

	public class CustomAssemblyBuilder
	{
		private string m_AssemblyName;
		private AssemblyBuilder m_AssemblyBuilder;
		private ModuleBuilder m_ModuleBuilder;

		public CustomAssemblyBuilder(string a_AssemblyName)
		{
			m_AssemblyName = a_AssemblyName;

			AssemblyName an = new AssemblyName(m_AssemblyName);
			m_AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
			m_ModuleBuilder = m_AssemblyBuilder.DefineDynamicModule(m_AssemblyName, string.Format("{0}.dll", m_AssemblyName), true);
		}

		public void Save()
		{
			m_AssemblyBuilder.Save(string.Format("{0}.dll", m_AssemblyName));
		}

		public void CreateType(string a_Name, Dictionary<string, PropertyField> a_Properties)
		{
			Type myType = CreateType(m_AssemblyBuilder, a_Name, a_Properties);

			//verify generation
			//Console.WriteLine(Assembly.LoadFrom("GameTypes.dll").GetType("B").GetField("k").GetValue(null));
		}

		public Type CreateType(AssemblyBuilder a_AssemblyBuilder, string a_TypeName, Dictionary<string, PropertyField> a_Properties)
		{
			TypeBuilder tb = CreateTypeBuilder(a_TypeName);
			ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

			if (a_Properties != null)
            {
				foreach (PropertyField field in a_Properties.Values)
                {
                    CreateProperty(tb, field.PropertyName, field.PropertyType);
                }
            }

			Type objectType = tb.CreateType();
			return objectType;
		}

        public TypeBuilder CreateType(string a_Name)
        {
			TypeBuilder typeBuilder = CreateTypeBuilder(a_Name);
            return typeBuilder;
        }


		public ILGenerator CreateFunction(TypeBuilder a_TypeBuilder, string a_Name, MethodAttributes a_MethodAttributes, Type a_ReturnType, Type[] a_Parameters)
        {
			MethodBuilder methodBuilder = a_TypeBuilder.DefineMethod(a_Name, a_MethodAttributes, a_ReturnType, a_Parameters);
            ILGenerator ilGenerator = methodBuilder.GetILGenerator();
            
            return ilGenerator;
        }

		private TypeBuilder CreateTypeBuilder(string a_TypeName)
		{
			TypeBuilder typeBuilder = m_ModuleBuilder.DefineType(a_TypeName,
										TypeAttributes.Public | TypeAttributes.Class, 
										null);
			return typeBuilder;
		}

		private void CreateProperty(TypeBuilder a_TypeBuilder, string a_PropertyName, Type a_PropertyType)
		{
			FieldBuilder fieldBuilder = a_TypeBuilder.DefineField("_" + a_PropertyName, a_PropertyType, FieldAttributes.Private);

			PropertyBuilder propertyBuilder = a_TypeBuilder.DefineProperty(a_PropertyName, PropertyAttributes.HasDefault, a_PropertyType, null);
			MethodBuilder getPropMthdBldr = a_TypeBuilder.DefineMethod("get_" + a_PropertyName, 
														MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, 
														a_PropertyType, Type.EmptyTypes);

			ILGenerator getIl = getPropMthdBldr.GetILGenerator();

			// Load the current instance onto the stack
			getIl.Emit(OpCodes.Ldarg_0);
			// Loads the value of the field specified as parameter from the object on the stack
			getIl.Emit(OpCodes.Ldfld, fieldBuilder);
			// Returns the method and pushes the get value onto the caller's stack
			getIl.Emit(OpCodes.Ret);

			MethodBuilder setPropMthdBldr = a_TypeBuilder.DefineMethod("set_" + a_PropertyName, 
														MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, 
														null, new[] { a_PropertyType });

			ILGenerator setIl = setPropMthdBldr.GetILGenerator();
			// Load the current instance onto the stack
			setIl.Emit(OpCodes.Ldarg_0);
			// Load the first argument, which is the assignment value onto the stack
			setIl.Emit(OpCodes.Ldarg_1);
			// Store the value currently on the stack in the field specified as parameter in the object on the stack
			setIl.Emit(OpCodes.Stfld, fieldBuilder);
			// Returns the setter
			setIl.Emit(OpCodes.Ret);

			propertyBuilder.SetGetMethod(getPropMthdBldr);
			propertyBuilder.SetSetMethod(setPropMthdBldr);
		}
	}
}
