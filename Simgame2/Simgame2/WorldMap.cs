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

        private Game1 game;

        public WorldMap(Game1 game, int mapWidth, int mapHeight)
            : base(game)
        {
            entities = new List<Entity>();

            this.width = mapWidth;
            this.height = mapHeight;

            this.heightMap = new int[width * height];
            generateMap();

            lookingAt = new Vector3(float.MinValue);

            this.game = game;
                

        }

        public int getMapWidth() { return this.width; }
        public int getMapHeight() {  return this.height; }

        public override void Initialize()
        {
            base.Initialize();


            PresentationParameters pp = device.PresentationParameters;
            refractionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);
            reflectionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            cloudsRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);
            cloudStaticMap = CreateStaticMap(32);

            fullScreenVertices = SetUpFullscreenVertices();
            fullScreenVertexDeclaration = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());
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

            return getCell(x, y) * mapHeightScale;
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


        private int maxHeight = 10;
        private int minHeight = -1;
        public void generateMap()
        {
            //WorldGenerator.generateBasicMap(this, this.width, this.height, this.minHeight, this.maxHeight);
            WorldGenerator.generateRegionMap(this, this.width, this.height, this.minHeight, this.maxHeight);
            
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

            

            SetUpWaterVertices(1000,1000);
            waterVertexDeclaration = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());

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


        public void Draw(Matrix currentViewMatrix, Vector3 cameraPosition, GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;

            

            BoundingFrustum frustum = new BoundingFrustum(currentViewMatrix * projectionMatrix);
            this.frustum = frustum;

            GenerateView(cameraPosition);
            
            
            
            

            // water
       //     DrawRefractionMap(ref currentViewMatrix, ref cameraPosition);
        //    DrawReflectionMap(ref currentViewMatrix, ref cameraPosition);


            // skybox
            

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            GeneratePerlinNoise(time);
            DrawSkyDome(currentViewMatrix, cameraPosition);

            DrawTerrain(ref currentViewMatrix, ref cameraPosition);
        //    DrawWater(ref currentViewMatrix, ref cameraPosition, time);

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

        private void DrawTerrain(ref Matrix currentViewMatrix, ref Vector3 cameraPosition)
        {
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
        }


        
        
        // Skydome
        RenderTarget2D cloudsRenderTarget;
        Texture2D cloudStaticMap;
        VertexPositionTexture[] fullScreenVertices;
        VertexDeclaration fullScreenVertexDeclaration;

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

        private Texture2D CreateStaticMap(int resolution)
        {
            Random rand = new Random();
            Color[] noisyColors = new Color[resolution * resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                    noisyColors[x + y * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));

            Texture2D noiseImage = new Texture2D(device, resolution, resolution, true, SurfaceFormat.Color);
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }

        private VertexPositionTexture[] SetUpFullscreenVertices()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
            vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

            return vertices;
        }


        private void GeneratePerlinNoise(float time)
        {
            device.SetRenderTarget(cloudsRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            effect.CurrentTechnique = effect.Techniques["PerlinNoise"];
            effect.Parameters["xTexture"].SetValue(cloudStaticMap);
            effect.Parameters["xOvercast"].SetValue(1.1f);
            effect.Parameters["xTime"].SetValue(time / 1000.0f);
            effect.CurrentTechnique.Passes[0].Apply();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, fullScreenVertices, 0, 2);
            }


            device.SetRenderTarget(null);
            cloudMap = cloudsRenderTarget;
        }










        // Water

        public const float waterHeight = 2.75f * (float)mapHeightScale;
        RenderTarget2D refractionRenderTarget;
        Texture2D refractionMap;
        RenderTarget2D reflectionRenderTarget;
        Texture2D reflectionMap;
        public Matrix reflectionViewMatrix;
        Vector3 windDirection = new Vector3(1, 0, 0);


        public Texture2D waterBumpMap;


        private Plane CreatePlane(float height, Vector3 planeNormalDirection, Matrix currentViewMatrix, bool clipSide)
        {
            planeNormalDirection.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
            if (clipSide)
                planeCoeffs *= -1;


		// TRY    Vector4 planeCoefficients = new Vector4(planeNormalDirection, - GetWaterHeight() + 0.3f);

            Plane finalPlane = new Plane(planeCoeffs);

            return finalPlane;
        }


        private void DrawRefractionMap(ref Matrix currentViewMatrix, ref Vector3 cameraPosition)
        {
            Plane refractionPlane = CreatePlane(waterHeight + (1.5f * mapHeightScale), new Vector3(0, -1, 0), currentViewMatrix, false);

            effect.Parameters["ClipPlane0"].SetValue(new Vector4(refractionPlane.Normal, refractionPlane.D));
            effect.Parameters["Clipping"].SetValue(true);    // Allows the geometry to be clipped for the purpose of creating a refraction map
            device.SetRenderTarget(refractionRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawTerrain(ref currentViewMatrix, ref cameraPosition);
            device.SetRenderTarget(null);
            effect.Parameters["Clipping"].SetValue(false);   // Make sure you turn it back off so the whole scene doesnt keep rendering as clipped
            refractionMap = refractionRenderTarget;
        }





        private void DrawReflectionMap(ref Matrix currentViewMatrix, ref Vector3 cameraPosition)
        {
            Plane reflectionPlane = CreatePlane(waterHeight - (0.5f * mapHeightScale), new Vector3(0, -1, 0), reflectionViewMatrix, true);
            

            //effect.Parameters["ClipPlane0"].SetValue(new Vector4(reflectionPlane.Normal, reflectionPlane.D));
            effect.Parameters["ClipPlane0"].SetValue(new Vector4(reflectionPlane.Normal, reflectionPlane.D));

            effect.Parameters["Clipping"].SetValue(true);    // Allows the geometry to be clipped for the purpose of creating a refraction map
            device.SetRenderTarget(reflectionRenderTarget);


            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);


            DrawSkyDome(reflectionViewMatrix, cameraPosition);
            DrawTerrain(ref reflectionViewMatrix, ref cameraPosition);


            //device.ClipPlanes[0].IsEnabled = false;
            effect.Parameters["Clipping"].SetValue(false);

            //device.SetRenderTarget(0, null);
            device.SetRenderTarget(null);

            reflectionMap = reflectionRenderTarget;

            this.game.debugImg = reflectionMap;
        }

       




        private void DrawWater(ref Matrix currentViewMatrix, ref Vector3 cameraPosition, float time)
        {
            effect.CurrentTechnique = effect.Techniques["Water"];
            Matrix worldMatrix = Matrix.Identity;
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(currentViewMatrix);
            effect.Parameters["xReflectionView"].SetValue(reflectionViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xReflectionMap"].SetValue(reflectionMap);
            effect.Parameters["xRefractionMap"].SetValue(refractionMap);
            effect.Parameters["xWaterBumpMap"].SetValue(waterBumpMap);
            effect.Parameters["xWaveLength"].SetValue(0.1f*mapHeightScale);
            effect.Parameters["xWaveHeight"].SetValue(0.3f*mapHeightScale);

            effect.Parameters["xCamPos"].SetValue(cameraPosition);

            effect.Parameters["xTime"].SetValue(time);
            effect.Parameters["xWindForce"].SetValue(0.02f);
            effect.Parameters["xWindDirection"].SetValue(windDirection);

            effect.CurrentTechnique.Passes[0].Apply();

     //       foreach (EffectPass pass in effect.CurrentTechnique.Passes)
    //        {
                device.SetVertexBuffer(waterVertexBuffer);

                int noVertices = waterVertexBuffer.VertexCount;
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, noVertices / 3);


   //         }

        }





        private void SetUpWaterVertices(int width, int height)
        {
            width = width * mapCellScale;
            height = height * mapCellScale;

            VertexPositionTexture[] waterVertices = new VertexPositionTexture[6];

            waterVertices[0] = new VertexPositionTexture(new Vector3(-width, waterHeight, height), new Vector2(0, 1));
            waterVertices[2] = new VertexPositionTexture(new Vector3(width, waterHeight, -height), new Vector2(1, 0));
            waterVertices[1] = new VertexPositionTexture(new Vector3(-width, waterHeight, -height), new Vector2(0, 0));

            waterVertices[3] = new VertexPositionTexture(new Vector3(-width, waterHeight, height), new Vector2(0, 1));
            waterVertices[5] = new VertexPositionTexture(new Vector3(width, waterHeight, height), new Vector2(1, 1));
            waterVertices[4] = new VertexPositionTexture(new Vector3(width, waterHeight, -height), new Vector2(1, 0));

            waterVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), waterVertices.Length, BufferUsage.WriteOnly);
            waterVertexBuffer.SetData(waterVertices);
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
        private VertexBuffer waterVertexBuffer;
        private VertexDeclaration waterVertexDeclaration;
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
