using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HA1_Assembly
{
    class SceneManager
    {
        private List<Object> m_DrawableObjects;
        private List<Object> m_CollidableObjects;
        private List<Object> m_MovableObjects;

        public List<Object> DrawableObjects { get { return m_DrawableObjects; } }
        public List<Object> MovableObjects { get { return m_DrawableObjects; } }
        public List<Object> CollidableObjects { get { return m_DrawableObjects; } }

        public SceneManager()
        {
            m_DrawableObjects = new List<Object>();
            m_CollidableObjects = new List<Object>();
            m_MovableObjects = new List<Object>();
        }

        public void ParseObjects(List<Object> a_ObjectList, List<GameType> a_GameTypes)
        {
            //Loop through the objects and sort them according to their behaviour
            foreach ( Object sortableObject in a_ObjectList)
            {
                Type objectType = sortableObject.GetType();

                foreach (GameType gameType in a_GameTypes)
                {
                    if (objectType.Name == gameType.Name)
                    {
                        Boolean found;
                        //See if the current type is drawable
                        gameType.Behaviors.TryGetValue( "Drawable", out found );
                        if (found == true)
                        {
                            m_DrawableObjects.Add(sortableObject);
                        }
                        //See if the current type is collidable
                        gameType.Behaviors.TryGetValue("Collidable", out found);
                        if (found == true)
                        {
                            m_CollidableObjects.Add(sortableObject);
                        }
                        //See if the current type is movable
                        gameType.Behaviors.TryGetValue("Movable", out found);
                        if (found == true)
                        {
                            m_MovableObjects.Add(sortableObject);
                        }

                        break;
                    }
                }
            }
        }
    }
}
