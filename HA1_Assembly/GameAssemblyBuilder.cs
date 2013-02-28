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

        public void GenerateGameObjects( )
        {

        }

        public void GenerateInitializeFunction()
        {
            // TODO: generate the initialize function with all objects that need to be initialized with basic properties
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
