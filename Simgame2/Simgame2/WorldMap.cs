using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Simgame2
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class WorldMap : Microsoft.Xna.Framework.GameComponent
    {
        private const int mapCellScale = 5;
        private const int mapHeightScale = 2;

        class Node
        {
            public int left;
            public int upper;
            public int width;
            public int height;

            public BoundingBox boundingBox;

            public int depth;

            public Node(int left, int upper, int width, int height, int depth)
            {
                this.depth = depth;

                this.left = left;
                this.upper = upper;
                this.width = width;
                this.height = height;


                Vector3[] bbPoints = new Vector3[2];
                bbPoints[0] = new Vector3(left * mapCellScale, -1, -upper * mapCellScale);
                bbPoints[1] = new Vector3((left + width) * mapCellScale, 999, -(upper + height) * mapCellScale - 1);
                boundingBox = BoundingBox.CreateFromPoints(bbPoints);
            }




            public override string ToString()
            {
                return "(" + left + ", " + upper + ", " + (left+width) + ", " + (upper+height) +")" ;
            }

            


            public Node A;
            public Node B;
        }


        public WorldMap(Game game, int mapWidth, int mapHeight)
            : base(game)
        {
            this.width = mapWidth;
            this.height = mapHeight;

            this.heightMap = new int[width * height];
            generateMap();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }



        public int getCellAdress(int x, int y)
        {
            return (y * this.width) + x;
        }

        public int getCell(int x, int y)
        {
            if (x < 0 || y < 0 || x > this.width || y > this.height)
            {
                return 0;
            }

            return this.heightMap[getCellAdress(x, y)];
        }

        public void setCell(int x, int y, int value)
        {
            this.heightMap[getCellAdress(x, y)] = value;
        }


        public void lowerCellHeight(int x, int y, ushort amount)
        {
            // TODO implement
        }

        public void raiseCellHeight(int x, int y, ushort amount)
        {
            // TODO implement
        }

        public void generateMap()
        {
            Random rand = new Random();
            // TODO implement better map generation

            

            // create water border
            for (int x = 0; x < this.width; x++)
            {
                setCell(x, 0, 0);
                setCell(x, height - 1, 0);

            }
            for (int y = 0; y < this.height; y++)
            {
                setCell(0, y, 0);
                setCell(this.width - 1, y, 0);
            }

            // create beach border
            for (int x = 1; x < this.width-1; x++)
            {
                setCell(x, 1, 1);
                setCell(x, height - 2, 1);

            }
            for (int y = 1; y < this.height-1; y++)
            {
                setCell(1, y, 1);
                setCell(this.width - 2, y, 1);
            }

            int modifier;
            // create land mass
            for (int y = 2; y < this.height-2; y++)
            {
                modifier = 1;
                for (int x = 2; x < this.width-2; x++)
                {
                    //int randval = rand.Next(0, 100);

                    modifier += rand.Next(-2, 2);
                    if (modifier < 1) modifier = 1;
                    if (modifier > 30) modifier = 30;

                    setCell(x, y, modifier);
                }
            }

            GenerateSearchTree();
        }


        private void GenerateSearchTree()
        {
            root = GenerateTree(new Node(0, 0, this.width, this.height, 0),false);
        }

        private Node GenerateTree(Node current, bool direction)
        {
            // stop condition
       //     if (current.width <= 10 && current.height <= 10) { return current; }

            current.A = current.B = null;
            if (direction) // vertical division
            {
                
                if (current.width > 10)
                {
                    current.A = GenerateTree(new Node(current.left, current.upper, current.width / 2, current.height, current.depth + 1), !direction);
                    current.B = GenerateTree(new Node(current.left + (current.width / 2), current.upper, current.width / 2, current.height, current.depth + 1), !direction);
                }
            }
            else // horizontal division
            {
                if (current.height > 10)
                {
                    current.A = GenerateTree(new Node(current.left, current.upper, current.width, current.height / 2, current.depth + 1), !direction);
                    current.B = GenerateTree(new Node(current.left, current.upper + (current.height / 2), current.width, current.height / 2, current.depth + 1), !direction);

                }
            }

            return current;

        }


        private void debugout(string text)
        {
            System.Diagnostics.Debug.Write(text);
        }

        private void debugOutLn(string text)
        {
           // System.Diagnostics.Debug.WriteLine(text);
        }

        public void debugPrintTree()
        {
            System.Diagnostics.Debug.WriteLine("--------------------------------------------------------------------------");
            System.Diagnostics.Debug.WriteLine("Printing tree");

            Queue<Node> nodes = new Queue<Node>();
            nodes.Enqueue(root);
            int depth = 0;

            while (nodes.Count > 0)
            {
                Node n = nodes.Dequeue();

                if (n.A != null)
                {
                    nodes.Enqueue(n.A);
                }

                if (n.B != null)
                {
                    nodes.Enqueue(n.B);
                }

                if (depth != n.depth)
                {
                    System.Diagnostics.Debug.WriteLine("");
                    depth = n.depth;
                }

                System.Diagnostics.Debug.Write(n.ToString());

            }

            System.Diagnostics.Debug.WriteLine("--------------------------------------------------------------------------");

        }




        public bool useTree = true;

        public void GenerateView()
        {
            //TODO speed up methode


            int regionWidth = 0;
            int regionHeight = 0;

            int regionLeft = int.MaxValue;
            int regionRight = int.MinValue;
            int regionUp = int.MaxValue;
            int regionDown = int.MinValue;





            // look for edge corners of the visual region
            Queue<Node> expandNode = new Queue<Node>();

            expandNode.Enqueue(root);



            Node currentNode;
            while (expandNode.Count > 0)
            {
                currentNode = expandNode.Dequeue();

                //  debugOutLn("expanding node: " + currentNode.ToString());

                if (currentNode.A == null && currentNode.B == null)
                {
                    //  debugOutLn("reached leaf: " + currentNode.ToString());
                    if (currentNode.left < regionLeft) { regionLeft = currentNode.left; }
                    if (currentNode.left + currentNode.width > regionRight) { regionRight = currentNode.left + currentNode.width; }
                    if (currentNode.upper < regionUp) { regionUp = currentNode.upper; }
                    if (currentNode.upper + currentNode.height > regionDown) { regionDown = currentNode.upper + currentNode.height; }

                    //  debugOutLn("region lurd: " + regionLeft + ", " + regionUp + ", " + regionRight + ", " + regionDown);
                }
                else
                {
                    if (currentNode.A != null &&
                        IsBoxInFrustum(currentNode.A.boundingBox.Min, currentNode.A.boundingBox.Max, frustum))
                    {
                        //  debugOutLn("adding A");
                        expandNode.Enqueue(currentNode.A);
                    }

                    if (currentNode.B != null &&
                        IsBoxInFrustum(currentNode.B.boundingBox.Min, currentNode.B.boundingBox.Max, frustum))
                    {
                        //   debugOutLn("adding B");
                        expandNode.Enqueue(currentNode.B);
                    }



                }
            }

           



            


         //   debugOutLn("done");
     //       debugOutLn("region lurd: " + regionLeft + ", " + regionUp + ", " + regionRight + ", " + regionDown);

            regionLeft -= 2; if (regionLeft < 0) regionLeft = 0;
            regionRight += 2; if (regionRight > this.width) regionRight = this.width;
            regionUp -= 2; if (regionUp < 0) regionUp = 0;
            regionDown += 2; if (regionDown > this.height) regionDown = this.height;

            regionWidth = regionRight - regionLeft;
            regionHeight = regionDown - regionUp;


            Vector3[] curners = frustum.GetCorners();
            debugOutLn("frustum corners:");
            foreach (Vector3 c in curners)
            {
                debugOutLn(c.ToString());
            }

            debugOutLn("final region lurd: " + regionLeft + ", " + regionUp + ", " + regionRight + ", " + regionDown);

            vertices = new VertexPositionNormalColored[regionWidth * regionHeight];
            for (int x = regionLeft; x < regionRight; x++)
            {
                for (int y = regionUp; y < regionDown; y++)
                {
                    int adress = (x - regionLeft) + (y - regionUp) * regionWidth;

                    vertices[adress].Position = new Vector3(x * mapCellScale, getCell(x, y) * mapHeightScale, -y * mapCellScale);
                    
                    if (getCell(x, y) == 0)
                        vertices[adress].Color = Color.Blue;
                    else if (getCell(x, y) < 3)
                        vertices[adress].Color = Color.Green;
                    else if (getCell(x, y) < 7)
                        vertices[adress].Color = Color.Brown;
                    else
                        vertices[adress].Color = Color.White;
                }
            }




            indices = new Int16[(regionWidth - 1) * (regionHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < regionHeight - 1; y++)
            {
                for (int x = 0; x < regionWidth - 1; x++)
                {
                    Int16 lowerLeft = (Int16)(x + y * regionWidth);
                    Int16 lowerRight = (Int16)((x + 1) + y * regionWidth);
                    Int16 topLeft = (Int16)(x + (y + 1) * regionWidth);
                    Int16 topRight = (Int16)((x + 1) + (y + 1) * regionWidth);

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }


            vertices = CalculateNormals(vertices, indices);

        }






        bool IsBoxInFrustum(Vector3 bMin, Vector3 bMax, BoundingFrustum Frustum)
        {
            Vector3 NearPoint;

            Plane[] plane = new Plane[6];
            plane[0] = frustum.Bottom;
            plane[1] = frustum.Far;
            plane[2] = frustum.Left;
            plane[3] = frustum.Near;
            plane[4] = frustum.Right;
            plane[5] = frustum.Top;
                
            //plane[0].Normal.X
            
            for (int i = 0; i < 6; i++)
            {
                if (plane[i].Normal.X > 0.0f)
                {
                    if (plane[i].Normal.Y > 0.0f)//
                    {
                        if (plane[i].Normal.Z > 0.0f)
                        {
                            NearPoint.X = bMin.X; NearPoint.Y = bMin.Y; NearPoint.Z = bMin.Z;
                        }
                        else
                        {
                            NearPoint.X = bMin.X; NearPoint.Y = bMin.Y; NearPoint.Z = bMax.Z;
                        }
                    }
                    else
                    {
                        if (plane[i].Normal.Z > 0.0f)
                        {
                            NearPoint.X = bMin.X; NearPoint.Y = bMax.Y; NearPoint.Z= bMin.Z;
                        }
                        else
                        {
                            NearPoint.X = bMin.X; NearPoint.Y = bMax.Y; NearPoint.Z = bMax.Z;
                        }
                    }
                }
                else
                {
                    if (plane[i].Normal.Y > 0.0f)
                    {
                        if (plane[i].Normal.Z > 0.0f)
                        {
                            NearPoint.X = bMax.X; NearPoint.Y = bMin.Y; NearPoint.Z = bMin.Z;
                        }
                        else
                        {
                            NearPoint.X = bMax.X; NearPoint.Y = bMin.Y; NearPoint.Z = bMax.Z;
                        }
                    }
                    else
                    {
                        if (plane[i].Normal.Z > 0.0f)
                        {
                            NearPoint.X = bMax.X; NearPoint.Y = bMax.Y; NearPoint.Z = bMin.Z;
                        }
                        else
                        {
                            NearPoint.X = bMax.X; NearPoint.Y = bMax.Y; NearPoint.Z = bMax.Z;
                        }
                    }
                }

                // near extreme point is outside, and thus
                // the AABB is totally outside the polyhedron

                float dotProduct;
                Vector3.Dot(ref plane[i].Normal, ref NearPoint, out dotProduct);
                if (dotProduct + plane[i].D > 0)
                    return false;
                
            }
            return true;
        }











        /*
        public void GenerateView()
        {
            //TODO speed up methode


            int regionWidth = 0;
            int regionHeight = 0;

            int regionLeft = int.MaxValue;
            int regionRight = int.MinValue;
            int regionUp = int.MaxValue;
            int regionDown = int.MinValue;




            // look for upper left corner of the visual region
            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    // four point of the square cell
                    Vector3 v1 = new Vector3(x, getCell(x, y), -y);
                    Vector3 v2 = new Vector3(x+1, getCell(x, y), -y);
                    Vector3 v3 = new Vector3(x, getCell(x, y), -(y+1));
                    Vector3 v4 = new Vector3(x+1, getCell(x, y), -(y+1));
                    if (frustum.Contains(v1) == ContainmentType.Contains ||
                        frustum.Contains(v2) == ContainmentType.Contains ||
                        frustum.Contains(v3) == ContainmentType.Contains ||
                        frustum.Contains(v4) == ContainmentType.Contains)
                    {
                        if (x < regionLeft) { regionLeft = x;  }
                        if (x > regionRight) { regionRight = x; }
                        if (y < regionUp) { regionUp = y; }
                        if (y > regionDown) { regionDown = y; }
                    }
                }
            }





            regionLeft -= 2; if (regionLeft < 0) regionLeft = 0;
            regionRight += 2; if (regionRight > this.width) regionRight = this.width;
            regionUp -= 2; if (regionUp < 0) regionUp = 0;
            regionDown += 2; if (regionDown > this.height) regionDown = this.height;

            regionWidth = regionRight - regionLeft;
            regionHeight = regionDown - regionUp;

            vertices = new VertexPositionNormalColored[regionWidth * regionHeight];
            for (int x = regionLeft; x < regionRight; x++)
            {
                for (int y = regionUp; y < regionDown; y++)
                {
                    int adress = (x - regionLeft) + (y - regionUp) * regionWidth;

                    vertices[adress].Position = new Vector3(x, getCell(x, y), -y);
                    
                    if (getCell(x, y) == 0)
                        vertices[adress].Color = Color.Blue;
                    else if (getCell(x, y) < 10)
                        vertices[adress].Color = Color.Green;
                    else if (getCell(x, y) < 25)
                        vertices[adress].Color = Color.Brown;
                    else
                        vertices[adress].Color = Color.White;
                }
            }




            indices = new Int16[(regionWidth - 1) * (regionHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < regionHeight - 1; y++)
            {
                for (int x = 0; x < regionWidth - 1; x++)
                {
                    Int16 lowerLeft = (Int16)(x + y * regionWidth);
                    Int16 lowerRight = (Int16)((x + 1) + y * regionWidth);
                    Int16 topLeft = (Int16)(x + (y + 1) * regionWidth);
                    Int16 topRight = (Int16)((x + 1) + (y + 1) * regionWidth);

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }


            vertices = CalculateNormals(vertices, indices);

        }
         
        */






        /*
                private void SetUpVertices()
                {
                    vertices = new VertexPositionNormalColored[this.width * this.height];
                    for (int x = 0; x < this.width; x++)
                    {
                        for (int y = 0; y < this.height; y++)
                        {
                            vertices[x + y * this.width].Position = new Vector3(x, getCell(x, y), -y);

                            if (getCell(x,y) == 0)
                                vertices[x + y * this.width].Color = Color.Blue;
                            else if (getCell(x, y) < 10)
                                vertices[x + y * this.width].Color = Color.Green;
                            else if (getCell(x, y) < 25)
                                vertices[x + y * this.width].Color = Color.Brown;
                            else
                                vertices[x + y * this.width].Color = Color.White;

                        }
                    }
                }
                */
      /*  
        private void SetUpIndices()
        {
            indices = new Int16[(this.width - 1) * (this.height - 1) * 6];
            int counter = 0;
            for (ushort y = 0; y < this.height - 1; y++)
            {
                for (ushort x = 0; x < this.width - 1; x++)
                {
                    Int16 lowerLeft = (Int16)(x + y * this.width);
                    Int16 lowerRight = (Int16)((x + 1) + y * this.width);
                    Int16 topLeft = (Int16)(x + (y + 1) * this.width);
                    Int16 topRight = (Int16)((x + 1) + (y + 1) * this.width);

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }*/


        private VertexPositionNormalColored[] CalculateNormals(VertexPositionNormalColored[] vertices, Int16[] indices)
        {
            for (long i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (long i = 0; i < indices.Length / 3; i++)
            {
                long index1 = indices[i * 3];
                long index2 = indices[i * 3 + 1];
                long index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (long i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }


        public int width { get; set; }
        public int height { get; set; }

        private int[] heightMap;

        public VertexPositionNormalColored[] vertices;
        public Int16[] indices;

        public const int WaterLevel = 0;

        public BoundingFrustum frustum;

        // search tree
        private Node root;

    }
}
