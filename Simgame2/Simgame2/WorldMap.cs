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

    public class WorldMap : Microsoft.Xna.Framework.GameComponent
    {
        public const int mapCellScale = 5;
        public const int mapHeightScale = 2;

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
            entities = new List<Entity>();

            this.width = mapWidth;
            this.height = mapHeight;

            this.heightMap = new int[width * height];
            generateMap();

            lookingAt = new Vector3(float.MinValue);
            
        }


        public override void Initialize()
        {
            base.Initialize();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }


        public int getCellFromWorldCoor(float wx, float wz)
        {
            float wy = wz;  // transform z in worldcoor (3D) to y in mapcoor (2D)

            int x = (int)Math.Floor(wx / mapCellScale);
            int y = (int)Math.Floor(wy / mapCellScale);

            return getCell(x, y);
        }


        public int levelTerrainWorldCoor(float wx_min, float wz_min, float wx_max, float wz_max)
        {
            // transform z in worldcoor (3D) to y in mapcoor (2D)
            int xMin = (int)Math.Floor(wx_min / mapCellScale);
            int yMin = (int)Math.Floor(wz_min / mapCellScale);
            int xMax = (int)Math.Floor(wx_max / mapCellScale);
            int yMax = (int)Math.Floor(wz_max / mapCellScale);


            if (yMin > yMax)
            {
                int temp = yMin;
                yMin = yMax;
                yMax = temp;
            }

            if (xMin > xMax)
            {
                int temp = xMin;
                xMin = xMax;
                xMax = temp;
            }

            int altitude;

            int averageAltitude = 0;
            int cnt = 0;
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    averageAltitude += getCell(x, y);
                    cnt++;
                }
            }

            altitude = averageAltitude / cnt;

            for (int x = xMin - 2; x <= xMax + 2; x++)
            {
                for (int y = yMin - 2; y <= yMax + 2; y++)
                {
                    setCell(x, y, altitude);
                }
            }

            return altitude*mapHeightScale;
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


        private int maxHeight = 30;
        private int minHeight = 1;
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

            // create land mass
            float value;
            for (int y = 2; y < this.height-2; y++)
            {
                for (int x = 2; x < this.width-2; x++)
                {
                    //value = (getCell(x - 1, y - 1) + getCell(x - 1, y) + getCell(x, y - 1)) / 3;

                    value = (getCell(x - 1, y - 1) + getCell(x - 1, y) + getCell(x, y - 1));
                    value += getCell(x - 2, y - 2) + getCell(x - 1, y - 2) + getCell(x, y - 2);
                    value += getCell(x - 1, y - 2) + getCell(x, y - 2);
                    value = value / 8;

         //           value += (float)(( Math.Sin((x + SimplexNoise.Noise.Generate(x * 5, y * 5) / 2) * 50)) * 2);
         //           value = (float)Math.Ceiling(value);
                    if (rand.Next(100) < 50)
                    {
                        value += (float)((Math.Sin((x + SimplexNoise.Noise.Generate(x * 5, y * 5) / 2) * 50)) * 2);
                        value = (float)Math.Ceiling(value);
                    }
                    if (value < minHeight)
                    {
                        value = minHeight;
                    }
                    if (value > maxHeight)
                    {
                        value = maxHeight;
                    }

                    setCell(x, y, (int)(value));

                }
            }



            // Add lake
            int lakex = 15;
            int lakey = 15;
            int lakeWidth = 6;
            int lakeHeight = 8;


            for (int y = lakey-(lakeWidth/2); y < lakey+(lakeWidth/2); y++)
            {
                for (int x = lakex-(lakeHeight/2); x < lakex+(lakeHeight/2); x++)
                {
                    setCell(x, y, 0);
                }
            }




            // TODO add flat plains, rivers, lakes


            GenerateSearchTree();
        }


        private void GenerateSearchTree()
        {
            root = GenerateTree(new Node(0, 0, this.width, this.height, 0),false);
        }

        private Node GenerateTree(Node current, bool direction)
        {

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

        public void GenerateView(Vector3 cameraPosition)
        {

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

                if (currentNode.A == null && currentNode.B == null)
                {
                    if (currentNode.left < regionLeft) { regionLeft = currentNode.left; }
                    if (currentNode.left + currentNode.width > regionRight) { regionRight = currentNode.left + currentNode.width; }
                    if (currentNode.upper < regionUp) { regionUp = currentNode.upper; }
                    if (currentNode.upper + currentNode.height > regionDown) { regionDown = currentNode.upper + currentNode.height; }
                }
                else
                {
                    if (currentNode.A != null &&
                        IsBoxInFrustum(currentNode.A.boundingBox.Min, currentNode.A.boundingBox.Max, frustum))
                    {
                        expandNode.Enqueue(currentNode.A);
                    }

                    if (currentNode.B != null &&
                        IsBoxInFrustum(currentNode.B.boundingBox.Min, currentNode.B.boundingBox.Max, frustum))
                    {
                        expandNode.Enqueue(currentNode.B);
                    }
                }
            }

           


            regionLeft -= 2; if (regionLeft < 0) regionLeft = 0;
            regionRight += 2; if (regionRight > this.width) regionRight = this.width;
            regionUp -= 2; if (regionUp < 0) regionUp = 0;
            regionDown += 2; if (regionDown > this.height) regionDown = this.height;

            regionWidth = regionRight - regionLeft;
            regionHeight = regionDown - regionUp;


            vertices = new VertexMultitextured[regionWidth * regionHeight];
            for (int x = regionLeft; x < regionRight; x++)
            {
                for (int y = regionUp; y < regionDown; y++)
                {
                    int adress = (x - regionLeft) + (y - regionUp) * regionWidth;

                   
                    vertices[adress].Position = new Vector3(x * mapCellScale, getCell(x, y) * mapHeightScale, -y * mapCellScale);

                    vertices[adress].TextureCoordinate.X = (float)x % textureSize;
                    vertices[adress].TextureCoordinate.Y = (float)y % textureSize;
                    

                    vertices[adress].TexWeights.X = MathHelper.Clamp(1.0f - Math.Abs(getCell(x,y) - 0) / 8.0f, 0, 1);
                    vertices[adress].TexWeights.Y = MathHelper.Clamp(1.0f - Math.Abs(getCell(x,y) - 2) / 6.0f, 0, 1);
                    vertices[adress].TexWeights.Z = MathHelper.Clamp(1.0f - Math.Abs(getCell(x,y) - 4) / 6.0f, 0, 1);
                    vertices[adress].TexWeights.W = MathHelper.Clamp(1.0f - Math.Abs(getCell(x,y) - 10) / 6.0f, 0, 1);

                    float total = vertices[adress].TexWeights.X;
                    total += vertices[adress].TexWeights.Y;
                    total += vertices[adress].TexWeights.Z;
                    total += vertices[adress].TexWeights.W;
 
                    vertices[adress].TexWeights.X /= total;
                    vertices[adress].TexWeights.Y /= total;
                    vertices[adress].TexWeights.Z /= total;
                    vertices[adress].TexWeights.W /= total;


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
            CopyToTerrainBuffers();
        }



        private void CopyToTerrainBuffers()
        {

            terrainVertexBuffer = new VertexBuffer(device, typeof(VertexMultitextured), vertices.Length, BufferUsage.WriteOnly);

            terrainVertexBuffer.SetData(vertices);

            terrainIndexBuffer = new IndexBuffer(device, typeof(Int16), indices.Length, BufferUsage.WriteOnly);
            terrainIndexBuffer.SetData(indices);


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




        



        //private VertexPositionNormalColored[] CalculateNormals(VertexPositionNormalColored[] vertices, Int16[] indices)
        private VertexMultitextured[] CalculateNormals(VertexMultitextured[] vertices, Int16[] indices)
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

        float FOGNEAR = 100.0f;
        float FOGFAR = 250.0f;
        Color FOGCOLOR = new Color(100,100, 70);


        public void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            
            DrawSkyDome(currentViewMatrix, cameraPosition);

            BoundingFrustum frustum = new BoundingFrustum(currentViewMatrix * projectionMatrix);
            this.frustum = frustum;

            GenerateView(cameraPosition);


            Matrix worldMatrix = Matrix.Identity;
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(currentViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);

            effect.Parameters["xTexture"].SetValue(this.groundTexture);


            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Parameters["xTexture0"].SetValue(this.sandTexture);
            effect.Parameters["xTexture1"].SetValue(this.groundTexture);
            effect.Parameters["xTexture2"].SetValue(this.rockTexture);
            effect.Parameters["xTexture3"].SetValue(this.snowTexture);

            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xAmbient"].SetValue(0.4f);
            effect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));


            //FOG

            effect.Parameters["FogColor"].SetValue(FOGCOLOR.ToVector4());
            effect.Parameters["FogNear"].SetValue(FOGNEAR);
            effect.Parameters["FogFar"].SetValue(FOGFAR);
            effect.Parameters["cameraPos"].SetValue(cameraPosition);
            


            effect.CurrentTechnique.Passes[0].Apply();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                device.SetVertexBuffer(terrainVertexBuffer);
                device.Indices = terrainIndexBuffer;



                int noVertices = terrainVertexBuffer.VertexCount;
                int noTriangles = terrainIndexBuffer.IndexCount / 3;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noVertices, 0, noTriangles);
            }

            enitiesDrawn = 0;
            foreach (Entity e in entities)
            {
                if (frustum.Contains(e.boundingBox) != ContainmentType.Disjoint)
                {
                    enitiesDrawn++;
                    e.Draw(currentViewMatrix);
                }
            }


            


        }


        private void DrawSkyDome(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            device.DepthStencilState = DepthStencilState.None;

            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(100) * Matrix.CreateTranslation(cameraPosition);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture0"].SetValue(cloudMap);
                    currentEffect.Parameters["xEnableLighting"].SetValue(false);

                    // FOG

                    effect.Parameters["FogColor"].SetValue(FOGCOLOR.ToVector4());
                    effect.Parameters["FogNear"].SetValue(FOGNEAR);
                    effect.Parameters["FogFar"].SetValue(FOGFAR);
                    effect.Parameters["cameraPos"].SetValue(cameraPosition);

                }
                mesh.Draw();
            }

            device.DepthStencilState = DepthStencilState.Default;
        }


        public void updateCameraRay(Ray ray)
        {
            this.cameraRay = ray;
        }



        int enitiesDrawn;
        
        
        public string GetStats()
        {
            int maxVertices = width * height * 2;
            int maxIndices = maxVertices * 3;
            return "vertices: " + terrainVertexBuffer.VertexCount + "/" + maxVertices + " - indices: " + terrainIndexBuffer.IndexCount + "/" + maxVertices +
               Environment.NewLine + " - entities: " + enitiesDrawn + "/" + entities.Count;
        }
        



        public int width { get; set; }
        public int height { get; set; }

        private int[] heightMap;

        //public VertexPositionNormalColored[] vertices;
        public VertexMultitextured[] vertices;

        public Texture2D groundTexture { get; set; }
        public float textureSize { get; set; }

        public Texture2D sandTexture { get; set; }
        public Texture2D rockTexture { get; set; }
        public Texture2D snowTexture { get; set; }



        public GraphicsDevice device { get; set; }

        public Int16[] indices;
        private VertexBuffer terrainVertexBuffer;
        private IndexBuffer terrainIndexBuffer;

        public const int WaterLevel = 0;

        public BoundingFrustum frustum;
        public Effect effect { get; set; }
        public Matrix projectionMatrix { get; set; }

        // search tree
        private Node root;


        // sky dome

        public Texture2D cloudMap;
        public Model skyDome;


        public List<Entity> entities;


        // selection

        public Vector3 lookingAt;
        public Ray cameraRay;
        public Texture2D selectionTexture;















        // Copyright 2001 softSurfer, 2012 Dan Sunday
        // This code may be freely used, distributed and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.


        // Assume that classes are already given for the objects:
        //    Point and Vector with
        //        coordinates {float x, y, z;}
        //        operators for:
        //            == to test  equality
        //            != to test  inequality
        //            (Vector)0 =  (0,0,0)         (null vector)
        //            Point   = Point ± Vector
        //            Vector =  Point - Point
        //            Vector =  Scalar * Vector    (scalar product)
        //            Vector =  Vector * Vector    (cross product)
        //    Line and Ray and Segment with defining  points {Point P0, P1;}
        //        (a Line is infinite, Rays and  Segments start at P0)
        //        (a Ray extends beyond P1, but a  Segment ends at P1)
        //    Plane with a point and a normal {Point V0; Vector  n;}
        //    Triangle with defining vertices {Point V0, V1, V2;}
        //    Polyline and Polygon with n vertices {int n;  Point *V;}
        //        (a Polygon has V[n]=V[0])
        //===================================================================


        const float SMALL_NUM = 0.00000001f; // anything that avoids division overflow
        // dot product (3D) which allows vector operations in arguments
        //#define dot(u,v)   ((u).x * (v).x + (u).y * (v).y + (u).z * (v).z)



        // intersect3D_RayTriangle(): find the 3D intersection of a ray with a triangle
        //    Input:  a ray R, and a triangle T
        //    Output: *I = intersection point (when it exists)
        //    Return: -1 = triangle is degenerate (a segment or point)
        //             0 =  disjoint (no intersect)
        //             1 =  intersect in unique point I1
        //             2 =  are in the same plane
        int intersect3D_RayTriangle(Vector3[] R, Vector3[] T, Vector3 I)
        {
            Vector3 u, v, n;              // triangle vectors
            Vector3 dir, w0, w;           // ray vectors
            float r, a, b;              // params to calc ray-plane intersect

            // get triangle edge vectors and plane normal
            u = T[1] - T[0];
            v = T[2] - T[0];
            n = Vector3.Cross(u,v);              // cross product
            if (n == Vector3.Zero)             // triangle is degenerate
                return -1;                  // do not deal with this case

            dir = R[1]- R[0];              // ray direction vector
            w0 = R[0] - T[0];
            a = - Vector3.Dot(n, w0);
            b = Vector3.Dot(n, dir);
            if (Math.Abs(b) < SMALL_NUM)
            {     // ray is  parallel to triangle plane
                if (a == 0)                 // ray lies in triangle plane
                    return 2;
                else return 0;              // ray disjoint from plane
            }

            // get intersect point of ray with triangle plane
            r = a / b;
            if (r < 0.0)                    // ray goes away from triangle
                return 0;                   // => no intersect
            // for a segment, also test if (r > 1.0) => no intersect

            I = R[0] + r * dir;            // intersect point of ray and plane

            // is I inside T?
            float uu, uv, vv, wu, wv, D;
            uu = Vector3.Dot(u, u);
            uv = Vector3.Dot(u, v);
            vv = Vector3.Dot(v, v);
            w = I - T[0];
            wu = Vector3.Dot(w, u);
            wv = Vector3.Dot(w, v);
            D = uv * uv - uu * vv;

            // get and test parametric coords
            float s, t;
            s = (uv * wv - vv * wu) / D;
            if (s < 0.0 || s > 1.0)         // I is outside T
                return 0;
            t = (uv * wu - uu * wv) / D;
            if (t < 0.0 || (s + t) > 1.0)  // I is outside T
                return 0;

            return 1;                       // I is in T
        }

    }
}
