using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.Xna.Framework;

namespace HA1_Assembly
{
    public class QuadTreeNode
    {
        public QuadTreeNode(Rectangle a_BoundingRectangle, List<Tuple<Rectangle, int>> a_Rectangles, int a_Depth)
        {
            hasChilds = false;
            boundingRectangle = a_BoundingRectangle;
            rectangles = a_Rectangles;
            //ITems per node == 4
            //Max depth == 10
            if ( a_Rectangles.Count > 3 && a_Depth < 10)
            {
                hasChilds = true;
                Rectangle topLeft, topRight, bottomLeft, bottomRight;
                int halfWidth = boundingRectangle.Width / 2;
                int halfHeight = boundingRectangle.Height / 2;
                topLeft = new Rectangle(a_BoundingRectangle.X, a_BoundingRectangle.Y, halfWidth, halfHeight);
                topRight = new Rectangle( a_BoundingRectangle.X + halfWidth, a_BoundingRectangle.Y, halfWidth, halfHeight );
                bottomLeft = new Rectangle(a_BoundingRectangle.X, a_BoundingRectangle.Y + halfHeight, halfWidth, halfHeight );
                bottomRight = new Rectangle( a_BoundingRectangle.X + halfWidth, a_BoundingRectangle.Y + halfHeight, halfWidth, halfHeight );

                List<Tuple<Rectangle, int>> tlRectangles = new List<Tuple<Rectangle, int>>();
                List<Tuple<Rectangle, int>> trRectangles = new List<Tuple<Rectangle, int>>();
                List<Tuple<Rectangle, int>> blRectangles = new List<Tuple<Rectangle, int>>();
                List<Tuple<Rectangle, int>> brRectangles = new List<Tuple<Rectangle, int>>();

                foreach (Tuple<Rectangle, int> pair in a_Rectangles)
                {
                    if (pair.Item1.Intersects(topLeft) == true)
                        tlRectangles.Add(pair);
                    if (pair.Item1.Intersects(topRight) == true)
                        trRectangles.Add(pair);
                    if (pair.Item1.Intersects(bottomLeft) == true)
                        blRectangles.Add(pair);
                    if (pair.Item1.Intersects(bottomRight) == true)
                        brRectangles.Add(pair);
                }
                
                childNodes = new QuadTreeNode[4];
                //Left top
                childNodes[0] = new QuadTreeNode(topLeft, tlRectangles, a_Depth + 1);
                //Right top
                childNodes[1] = new QuadTreeNode(topRight, trRectangles, a_Depth + 1);
                //Left bottom
                childNodes[2] = new QuadTreeNode(bottomLeft, blRectangles, a_Depth + 1);
                //Right bottom
                childNodes[3] = new QuadTreeNode(bottomRight, brRectangles, a_Depth + 1);
            }
        }

        public void GenerateAssembly(ILGenerator a_ILGenerator, Label a_FalseLabel, Label a_TrueLabel)
        {
            //if (rectangles.Count == 0)
            //    return;

            Label endLabel = a_ILGenerator.DefineLabel();
            //Write assembly
            //Load PlayerX onto stack
            a_ILGenerator.Emit(OpCodes.Ldloc_1);
            //Load playerWidth
            a_ILGenerator.Emit(OpCodes.Ldloc_3);
            //Add player x and width
            a_ILGenerator.Emit(OpCodes.Add);
            //Load bounding rectangle X
            a_ILGenerator.Emit(OpCodes.Ldc_I4, boundingRectangle.X);
            //Check if the x + width < bouding rectangle.X if so jump to endlabel
            a_ILGenerator.Emit(OpCodes.Blt, endLabel);
            //Load PlayerX onto stack
            a_ILGenerator.Emit(OpCodes.Ldloc_1);
            //Load bounding rectangle.X onto stack
            a_ILGenerator.Emit(OpCodes.Ldc_I4, boundingRectangle.X);
            //Load bounding rectangle.width onto stack
            a_ILGenerator.Emit(OpCodes.Ldc_I4, boundingRectangle.Width);
            //Add rectanlge.X + rectangle.Width
            a_ILGenerator.Emit(OpCodes.Add);
            ////Check if player.X > rectangle.X + rectangle.Width if so jump to endlabel
            a_ILGenerator.Emit(OpCodes.Bgt, endLabel);

            //Load PlayerX onto stack
            a_ILGenerator.Emit(OpCodes.Ldloc_2);
            //Load playerWidth
            a_ILGenerator.Emit(OpCodes.Ldloc, 4);
            //Add player Y and Height
            a_ILGenerator.Emit(OpCodes.Add);
            //Load bounding rectangle Y
            a_ILGenerator.Emit(OpCodes.Ldc_I4, boundingRectangle.Y);
            //Check if the Y + Height < bounding rectangle.Y if so jump to endlabel
            a_ILGenerator.Emit(OpCodes.Blt, endLabel);
            //Load PlayerX onto stack
            a_ILGenerator.Emit(OpCodes.Ldloc_2);
            //Load bounding rectangle.Y onto stack
            a_ILGenerator.Emit(OpCodes.Ldc_I4, boundingRectangle.Y);
            //Load bounding rectangle.Height onto stack
            a_ILGenerator.Emit(OpCodes.Ldc_I4, boundingRectangle.Height);
            //Add rectanlge.Y + rectangle.Height
            a_ILGenerator.Emit(OpCodes.Add);
            //Check if player.Y > rectangle.Y + rectangle.Height if so jump to endlabel
            a_ILGenerator.Emit(OpCodes.Bgt, endLabel);

            if (hasChilds == true)
            {
                childNodes[0].GenerateAssembly(a_ILGenerator, endLabel, a_TrueLabel);
                childNodes[1].GenerateAssembly(a_ILGenerator, endLabel, a_TrueLabel);
                childNodes[2].GenerateAssembly(a_ILGenerator, endLabel, a_TrueLabel);
                childNodes[3].GenerateAssembly(a_ILGenerator, endLabel, a_TrueLabel);
            }
            else
            {
                MethodInfo checkMethod = typeof(Rectangle).GetMethod("Intersects", new Type[] { typeof(Rectangle) }, null);                 
                foreach (Tuple<Rectangle, int> pair in rectangles)
                {
                    Label endRectangleLabel = a_ILGenerator.DefineLabel();
                    //Push hash onto the stack
                    //New method ( extracted rectangle check )
                    //Load PlayerX onto stack
                    a_ILGenerator.Emit(OpCodes.Ldloc_1);
                    //Load PlayerWidth onto stack
                    a_ILGenerator.Emit(OpCodes.Ldloc_3);
                    //Add PlayerX to PlayerWidth, which ends up on the stack
                    a_ILGenerator.Emit(OpCodes.Add);
                    //Load hard coded rectangle X onto stack
                    a_ILGenerator.Emit(OpCodes.Ldc_I4, pair.Item1.X);
                    //Check if PlayerX + PlayerW >= Rectangle X
                    a_ILGenerator.Emit(OpCodes.Blt, endRectangleLabel);
                    //Load PlayerX onto stack
                    a_ILGenerator.Emit(OpCodes.Ldloc_1);
                    //Load ObjectX onto stack
                    a_ILGenerator.Emit(OpCodes.Ldc_I4, pair.Item1.X);
                    //Load Object width onto stack
                    a_ILGenerator.Emit(OpCodes.Ldc_I4, pair.Item1.Width);
                    //Add the objectX to object Width, which ends up on the stack
                    a_ILGenerator.Emit(OpCodes.Add);
                    //Check if PlayerX <= ObjectX + ObjectWidth
                    a_ILGenerator.Emit(OpCodes.Bgt, endRectangleLabel);

                    //Load PlayerX onto stack
                    a_ILGenerator.Emit(OpCodes.Ldloc_2);
                    //Load PlayerHeight onto stack
                    a_ILGenerator.Emit(OpCodes.Ldloc, 4);
                    //Add PlayerX to PlayerHeight, which ends up on the stack
                    a_ILGenerator.Emit(OpCodes.Add);
                    //Load hard coded rectangle Y onto stack
                    a_ILGenerator.Emit(OpCodes.Ldc_I4, pair.Item1.Y);
                    //Check if PlayerX + PlayerW >= Rectangle Y
                    a_ILGenerator.Emit(OpCodes.Blt, endRectangleLabel);
                    //Load PlayerX onto stack
                    a_ILGenerator.Emit(OpCodes.Ldloc_2);
                    //Load ObjectX onto stack
                    a_ILGenerator.Emit(OpCodes.Ldc_I4, pair.Item1.Y);
                    //Load Object Height onto stack
                    a_ILGenerator.Emit(OpCodes.Ldc_I4, pair.Item1.Height);
                    //Add the objectX to object Height, which ends up on the stack
                    a_ILGenerator.Emit(OpCodes.Add);
                    //Check if PlayerX <= ObjectX + ObjectWidth
                    a_ILGenerator.Emit(OpCodes.Bgt, endRectangleLabel);
                    a_ILGenerator.Emit(OpCodes.Ldc_I4, pair.Item2);
                    a_ILGenerator.Emit(OpCodes.Br, a_TrueLabel);
                    a_ILGenerator.MarkLabel(endRectangleLabel);                  

                    //Old method
                    //a_ILGenerator.Emit(OpCodes.Ldc_I4, rect.X); //Load arguments for object rectangle onto stack
                    //a_ILGenerator.Emit(OpCodes.Ldc_I4, rect.Y);
                    //a_ILGenerator.Emit(OpCodes.Ldc_I4, rect.Width);
                    //a_ILGenerator.Emit(OpCodes.Ldc_I4, rect.Height);
                    ////Create new rectangle
                    //a_ILGenerator.Emit(OpCodes.Newobj, rect.GetType().GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }));
                    ////Store the rectangle in some local position
                    //a_ILGenerator.Emit(OpCodes.Stloc_0);
                    ////Load the address of the rectangle
                    //a_ILGenerator.Emit(OpCodes.Ldloca, 0);
                    ////Load player rectangle by value
                    //a_ILGenerator.Emit(OpCodes.Ldarg_1);
                    ////Call intersection function
                    //a_ILGenerator.EmitCall(OpCodes.Call, checkMethod, null);
                    ////If we intersect jump to true label
                    //a_ILGenerator.Emit(OpCodes.Brtrue, a_TrueLabel);                    
                }
            }

            a_ILGenerator.MarkLabel(endLabel);

        }

        public int CollisionCheck(Rectangle a_PlayerRectangle)
        {
            if (a_PlayerRectangle.Intersects(boundingRectangle) == true)
            {
                int returnValue = 0;
                if (hasChilds == true)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        returnValue = childNodes[i].CollisionCheck(a_PlayerRectangle);
                        if (returnValue != 0)
                        {
                            return returnValue;
                        }
                    }
                }
                else
                {
                    foreach (Tuple<Rectangle, int> pair in rectangles)
                    {
                        if (a_PlayerRectangle.Intersects(pair.Item1) == true)
                        {
                            return pair.Item2;
                        }
                    }   
                }
                
            }
            return 0;
        }

        //Members
        public Rectangle boundingRectangle;
        List<Tuple<Rectangle, int>> rectangles;
        QuadTreeNode[] childNodes;
        bool hasChilds;
        int depth;
    }

    public class AssemblyQuadTree
    {
        public AssemblyQuadTree(Rectangle a_ScreenRectangle, List<Tuple<Rectangle, int>> a_Rectangles)
        {
            rootNode = new QuadTreeNode(a_ScreenRectangle, a_Rectangles, 0 );
        }

        public QuadTreeNode rootNode;

        public void GenerateAssembly(ILGenerator a_ILGenerator, Label a_FalseLabel, Label a_TrueLabel)
        {
            rootNode.GenerateAssembly(a_ILGenerator, a_FalseLabel, a_TrueLabel);
        }

        public int CheckForCollision(Rectangle a_PlayerRectangle)
        {
            return rootNode.CollisionCheck(a_PlayerRectangle);
        }
    }
}
