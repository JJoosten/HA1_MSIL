using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace HA1_Assembly
{
    public class CodeGenerator
    {

        public CodeGenerator()
        {

        }

        public Boolean ParseDataLayout( String a_FileName )
        {
            using (XmlReader xmlReader = XmlReader.Create(a_FileName))
            {
                xmlReader.MoveToContent();

                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        if (xmlReader.Name == "DataLayout")
                        {
                            
                            
                        }
                    }
                }
            }

            return true;
        }
    }
}
