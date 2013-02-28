using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.CodeDom.Compiler;

namespace HA1_Assembly
{
	public class PropertyField
	{
		public string PropertyName;
		public Type PropertyType;
	}

	public class CustomAssemblyBuilder
	{
		private string assemblyName;
		private AssemblyBuilder assemblyBuilder;
		private ModuleBuilder moduleBuilder;

		public CustomAssemblyBuilder( string assemblyName )
		{
			this.assemblyName = assemblyName;

			AssemblyName an = new AssemblyName(assemblyName);
			assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
			moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, string.Format("{0}.dll", assemblyName), false);
		}

		public void Save()
		{
			assemblyBuilder.Save( string.Format( "{0}.dll", assemblyName ) );
		}

		public void CreateType(string name, Dictionary<string, PropertyField> properties)
		{
			Type myType = CreateType(assemblyBuilder, name, properties);

			/*System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
			parameters.GenerateExecutable = false;
			parameters.OutputAssembly = "AutoGen.dll";

			CompilerResults r = CodeDomProvider.CreateProvider("C#").CompileAssemblyFromSource(parameters, "public class B {public static int k=7;}");

			//verify generation
			Console.WriteLine(Assembly.LoadFrom("AutoGen.dll").GetType("B").GetField("k").GetValue(null));*/
		}

		public Type CreateType(AssemblyBuilder assemblyBuilder, string typeName, Dictionary<string, PropertyField> properties)
		{
            TypeBuilder tb = CreateTypeBuilder( typeName);
			ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            if (properties != null)
            {
                foreach (PropertyField field in properties.Values)
                {
                    CreateProperty(tb, field.PropertyName, field.PropertyType);
                }
            }

			Type objectType = tb.CreateType();
			return objectType;
		}

        public TypeBuilder CreateType( string name)
        {
            TypeBuilder typeBuilder = CreateTypeBuilder(name);

            return typeBuilder;
        }


        public ILGenerator CreateFunction(TypeBuilder typeBuilder, string name, MethodAttributes methodAttributes, Type returnType, Type[] parameters)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(name, methodAttributes, returnType, parameters);
            ILGenerator ilGenerator = methodBuilder.GetILGenerator();
            
            return ilGenerator;
        }

		private TypeBuilder CreateTypeBuilder( string typeName)
		{
			TypeBuilder tb = moduleBuilder.DefineType(typeName
								, TypeAttributes.Public |
								TypeAttributes.Class
								//TypeAttributes.AutoClass |
								//TypeAttributes.AnsiClass |
								//TypeAttributes.BeforeFieldInit |
								//TypeAttributes.AutoLayout
								, null);

			return tb;
		}

		private void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
		{
			FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

			PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
			MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
			ILGenerator getIl = getPropMthdBldr.GetILGenerator();

			getIl.Emit(OpCodes.Ldarg_0);
			getIl.Emit(OpCodes.Ldfld, fieldBuilder);
			getIl.Emit(OpCodes.Ret);

			MethodBuilder setPropMthdBldr =	tb.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propertyType });

			ILGenerator setIl = setPropMthdBldr.GetILGenerator();
			Label modifyProperty = setIl.DefineLabel();
			Label exitSet = setIl.DefineLabel();

			setIl.MarkLabel(modifyProperty);
			setIl.Emit(OpCodes.Ldarg_0);
			setIl.Emit(OpCodes.Ldarg_1);
			setIl.Emit(OpCodes.Stfld, fieldBuilder);

			setIl.Emit(OpCodes.Nop);
			setIl.MarkLabel(exitSet);
			setIl.Emit(OpCodes.Ret);

			propertyBuilder.SetGetMethod(getPropMthdBldr);
			propertyBuilder.SetSetMethod(setPropMthdBldr);
		}
	}
}
