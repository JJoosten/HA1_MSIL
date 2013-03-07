using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HA1_Assembly
{
    class SceneManager
    {
       
        public Dictionary<string, List<Object>> m_GenericObjectLists;

        public SceneManager()
        {
            m_GenericObjectLists = new Dictionary<string, List<Object>>();
        }

        public List<Object> GetObjectList( string a_BehaviourName )
        {
            List<Object> objectList;
            if ( m_GenericObjectLists.TryGetValue( a_BehaviourName, out objectList ) == false )
            {
                Console.Write( string.Format( "Tried to retrieve a list with specific behaviour name that does not exist {0}.\n", a_BehaviourName ) );
            }

            return objectList;
        }

        public List<Object> GetStaticObjectList()
        {
            List<Object> staticList = new List<Object>();            
            List<Object> collidableList = GetObjectList( "Collidable" );
            List<Object> movableList = GetObjectList( "Movable" );

            foreach( Object obj in collidableList )
            {
                if ( movableList.Contains( obj ) == false )
                {
                    staticList.Add( obj );
                }
            }

            return staticList;
        }

        public void ParseObjects(List<Object> a_ObjectList, List<GameType> a_GameTypes, Dictionary<string, List<PropertyField>> a_Behaviours )
        {
            //Loop through all the behaviour types and create lists for them
            foreach (string behaviourName in a_Behaviours.Keys)
            {
                List<Object> objectsList = new List<Object>();
                m_GenericObjectLists.Add(behaviourName, objectsList);
            }

            //Loop through the objects and sort them according to their behaviour
            foreach ( Object sortableObject in a_ObjectList)
            {
                Type objectType = sortableObject.GetType();

                foreach (GameType gameType in a_GameTypes)
                {
                    if (objectType.Name == gameType.Name)
                    {
                        //Loop through all the behaviours this type contains
                        foreach (string behaviourName in gameType.Behaviors.Keys)
                        {
                            //Lookup the generic list for this behaviour name
                            List<Object> list;
                            if (m_GenericObjectLists.TryGetValue(behaviourName, out list) == true)
                            {
                                list.Add(sortableObject);
                            }
                            else
                            {
                                Console.Write(string.Format("Tried looking up a behaviour that didn't exist when the generic object list were created {0}", behaviourName));
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}
