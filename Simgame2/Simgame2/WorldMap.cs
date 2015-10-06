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

        

        

        public WorldMap(Game1 game, int mapNumCellsPerRow, int mapNumCellPerColumn, Effect effect, GraphicsDevice device)
            : base(game)
        {
            entities = new List<Entity>();

            this.mapNumCellsPerRow = mapNumCellsPerRow;
            this.mapNumCellPerColumn = mapNumCellPerColumn;
            this.effect = effect;
            this.device = device;

            this.heightMap = new int[mapNumCellsPerRow * mapNumCellPerColumn];
            this.textureMap = new Vector4[mapNumCellsPerRow * mapNumCellPerColumn];
            this.sector = new int[mapNumCellsPerRow * mapNumCellPerColumn];

            this.textureSize = 512;

            generateMap();

            PrecalculateVertices();



        //    lookingAt = new Vector3(float.MinValue);

            this.game = game;

            this.Initialize();
        }



        public override void Initialize()
        {
            base.Initialize();


            PresentationParameters pp = device.PresentationParameters;
            refractionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);
            reflectionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            cloudsRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            cloudStaticMap = CreateStaticMap(256);

            BackDropRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            SetUpWaterVertices(1000, 1000);

            fullScreenVertices = SetUpFullscreenVertices();
            fullScreenVertexDeclaration = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

        }


        public void LoadSkyDome(Model dome){
            this.skyDome = dome;
            this.skyDome.Meshes[0].MeshParts[0].Effect = this.effect.Clone();
        }


        public int getAltitude(float x, float z)
        {
            float widthOffset = this.mapNumCellsPerRow / 2;
            float heightOffset = this.mapNumCellPerColumn / 2;

            return getCellHeightFromWorldCoor(x + widthOffset, -(z+heightOffset));
        }


        public int getMapWidth() { return this.mapNumCellsPerRow * WorldMap.mapCellScale; }
        public int getMapHeight() { return this.mapNumCellPerColumn * WorldMap.mapCellScale; }



        public bool Collides(BoundingBox box)
        {
            foreach (Entity e in this.entities)
            {
                if (!e.boundingBox.Equals(box))
                {
                    if (e.boundingBox.Intersects(box) || e.boundingBox.Contains(box) == ContainmentType.Contains || box.Contains(e.boundingBox) == ContainmentType.Contains)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }


        public Vector2 getCellAdressFromWorldCoor(float wx, float wz)
        {
            float wy = wz;  // transform z in worldcoor (3D) to y in mapcoor (2D)

            int x = (int)Math.Floor(wx / mapCellScale);
            int y = (int)Math.Floor(wy / mapCellScale);

            return new Vector2(x,y);
        }


        public int getCellHeightFromWorldCoor(float wx, float wz)
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



        public int getCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.mapNumCellsPerRow || y >= this.mapNumCellPerColumn)
            {
                return 0;
            }

            return this.heightMap[getCellAdress(x, y)];
        }

        


        public Vector4 getCellTexWeights(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.mapNumCellsPerRow || y >= this.mapNumCellPerColumn)
            {
                return new Vector4(0);
            }

            return this.textureMap[getCellAdress(x, y)];
        }

        public void setCell(int x, int y, int value)
        {
            this.heightMap[getCellAdress(x, y)] = value;
            if (globalVertices != null)
            {
                globalVertices[getCellAdress(x, y)].Position = new Vector3(x * mapCellScale, getCell(x, y) * mapHeightScale, -y * mapCellScale);
            }
        }

        public void setCell(int x, int y, int value, Vector4 textureWeights)
        {
            setCell(x, y, value);
            this.textureMap[getCellAdress(x, y)] = new Vector4(textureWeights.X, textureWeights.Y, textureWeights.Z, textureWeights.W);
        }

        public void setCell(int x, int y, int value, float tex_x, float tex_y, float tex_z, float tex_w)
        {
            setCell(x, y, value);
            this.textureMap[getCellAdress(x, y)] = new Vector4(tex_x, tex_y, tex_z, tex_w);
        }

        public void setCell(int x, int y, int value, float tex_x, float tex_y, float tex_z, float tex_w, int sector)
        {
            setCell(x, y, value);
            this.textureMap[getCellAdress(x, y)] = new Vector4(tex_x, tex_y, tex_z, tex_w);
            this.sector[getCellAdress(x, y)] = sector;
        }

        public void SetResources(ResourceCell[] resources)
        {
            this.resources = resources;
        }

        public ResourceCell GetResource(float x, float y)
        {
            return this.resources[this.sector[getCellAdress((int)x, (int)y)]];
        }

        public ResourceCell GetResourceFromWorldCoor(float wx, float wy)
        {
            Vector2 add =  getCellAdressFromWorldCoor(wx, wy);
            return GetResource(add.X, add.Y);
        }


        public void Draw(Camera PlayerCamera, GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;

            BoundingFrustum frustum = new BoundingFrustum(PlayerCamera.viewMatrix * PlayerCamera.projectionMatrix);
            this.frustum = frustum;

            GenerateView(PlayerCamera.GetCameraPostion());


            // water
            DrawRefractionMap(PlayerCamera);
            DrawReflectionMap(PlayerCamera);


            // skybox
            GeneratePerlinNoise(time);

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);


            DrawSkyDome(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());
            

            DrawTerrain(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());
            DrawWater(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion(), time);

            enitiesDrawn = 0;
            foreach (Entity e in entities)
            {
                if (frustum.Contains(e.boundingBox) != ContainmentType.Disjoint)
                {
                    enitiesDrawn++;
                  //  e.ShowBoundingBox = true;
                    e.Draw(PlayerCamera.viewMatrix, PlayerCamera.GetCameraPostion());
                }
            }



            game.debugImg = MiniMap(PlayerCamera.GetCameraPostion());

        }


        public Entity FindEntityAt(Ray ray)
        {
            
            Entity e = null;
            for (int i = 0; i < this.entities.Count; i++)
            {
                if (frustum.Contains(entities[i].boundingBox) != ContainmentType.Disjoint) 
                { 
                    if (RayIntersectsModel(ray, entities[i].model,entities[i].GetWorldMatrix(),entities[i].GetBoneTransforms()))
                    {
                        e =  entities[i];
                        break;
                    }
                }
            }

            return e;
        }


        private static bool RayIntersectsModel(Ray ray, Model model,
            Matrix worldTransform, Matrix[] absoluteBoneTransforms)
        {
            // Each ModelMesh in a Model has a bounding sphere, so to check for an
            // intersection in the Model, we have to check every mesh.
            foreach (ModelMesh mesh in model.Meshes)
            {
                // the mesh's BoundingSphere is stored relative to the mesh itself.
                // (Mesh space). We want to get this BoundingSphere in terms of world
                // coordinates. To do this, we calculate a matrix that will transform
                // from coordinates from mesh space into world space....
                Matrix world = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;

                // ... and then transform the BoundingSphere using that matrix.
                BoundingSphere sphere = TransformBoundingSphere(mesh.BoundingSphere, world);

                // now that the we have a sphere in world coordinates, we can just use
                // the BoundingSphere class's Intersects function. Intersects returns a
                // nullable float (float?). This value is the distance at which the ray
                // intersects the BoundingSphere, or null if there is no intersection.
                // so, if the value is not null, we have a collision.
                if (sphere.Intersects(ray) != null)
                {
                    return true;
                }
            }

            // if we've gotten this far, we've made it through every BoundingSphere, and
            // none of them intersected the ray. This means that there was no collision,
            // and we should return false.
            return false;
        }


        private static BoundingSphere TransformBoundingSphere(BoundingSphere sphere, Matrix transform)
        {
            BoundingSphere transformedSphere;

            // the transform can contain different scales on the x, y, and z components.
            // this has the effect of stretching and squishing our bounding sphere along
            // different axes. Obviously, this is no good: a bounding sphere has to be a
            // SPHERE. so, the transformed sphere's radius must be the maximum of the 
            // scaled x, y, and z radii.

            // to calculate how the transform matrix will affect the x, y, and z
            // components of the sphere, we'll create a vector3 with x y and z equal
            // to the sphere's radius...
            Vector3 scale3 = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);

            // then transform that vector using the transform matrix. we use
            // TransformNormal because we don't want to take translation into account.
            scale3 = Vector3.TransformNormal(scale3, transform);

            // scale3 contains the x, y, and z radii of a squished and stretched sphere.
            // we'll set the finished sphere's radius to the maximum of the x y and z
            // radii, creating a sphere that is large enough to contain the original 
            // squished sphere.
            transformedSphere.Radius = Math.Max(scale3.X, Math.Max(scale3.Y, scale3.Z));

            // transforming the center of the sphere is much easier. we can just use 
            // Vector3.Transform to transform the center vector. notice that we're using
            // Transform instead of TransformNormal because in this case we DO want to 
            // take translation into account.
            transformedSphere.Center = Vector3.Transform(sphere.Center, transform);

            return transformedSphere;
        }


        private bool IsMousePointerInBox(Vector3 vec, Vector3 min, Vector3 max)
        {
            float minx = Math.Min(min.X, max.X);
            float maxx = Math.Max(min.X, max.X);

            float minz = Math.Min(min.Z, max.Z);
            float maxz = Math.Max(min.Z, max.Z);

            if (vec.X >= minx && vec.X <= maxx && vec.Z >= minz && vec.Z <= maxz) { return true;  }
            return false;


        }




        public void AddEntity(Entity entity)
        {
            entity.FOGNEAR = this.FOGNEAR;
            entity.FOGFAR = this.FOGFAR;
            entity.FOGCOLOR = this.FOGCOLOR;
            this.entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            this.entities.Remove(entity);
        }


        public string GetStats()
        {
            int maxVertices = mapNumCellsPerRow * mapNumCellPerColumn * 2;
            int maxIndices = maxVertices * 3;
            return "vertices: " + terrainVertexBuffer.VertexCount + "/" + maxVertices + " - indices: " + terrainIndexBuffer.IndexCount + "/" + maxVertices +
               Environment.NewLine + " - entities: " + enitiesDrawn + "/" + entities.Count;
        }


       public  int[] pointX;
          public  int[] pointY;
          public int[] pointAltitude;

          public Texture2D MiniMap(Vector3 cameraPosition)
        {
            Texture2D Minimap = new Texture2D(device, this.mapNumCellsPerRow , this.mapNumCellPerColumn, false, SurfaceFormat.Color);

            Color[] noisyColors = new Color[this.mapNumCellsPerRow * this.mapNumCellPerColumn];
            int r, g, b;


            for (int x = 0; x < mapNumCellsPerRow; x++)
            {
                for (int y = 0; y < mapNumCellPerColumn; y++)
                {
                    r = heightMap[x + y * mapNumCellsPerRow] * 40;
                    b = g = r;
                   
                    

                    // rbg must be int (range 0-255) or floats (range 0-1);
                    noisyColors[x + y * mapNumCellsPerRow] = new Color(r, g, b);

                }
            }

            if (pointX != null)
            {
                for (int i = 0; i < pointX.Length; i++)
                {
                    noisyColors[pointX[i] + pointY[i] * mapNumCellsPerRow] = new Color(255, 0, 0);
                }

            }

            int xx = (int)Math.Floor(cameraPosition.X / mapCellScale);
            int yy = (int)Math.Floor(-cameraPosition.Z / mapCellScale);
            noisyColors[(int)(xx + yy * mapNumCellsPerRow)] = new Color(0, 255, 0); //  BUG boundery condition
            noisyColors[(int)(xx+1 + yy * mapNumCellsPerRow)] = new Color(0, 255, 0); //  BUG boundery condition
            noisyColors[(int)(xx + (yy+1) * mapNumCellsPerRow)] = new Color(0, 255, 0); //  BUG boundery condition
            noisyColors[(int)(xx+1 + (yy+1) * mapNumCellsPerRow)] = new Color(0, 255, 0); //  BUG boundery condition
            //  cameraPosition
                Minimap.SetData(noisyColors);
            
            return Minimap;
        }



        #region PrivateMembers

        // Private 
        //==============================

        private int getCellAdress(int x, int y)
        {
            return (y * this.mapNumCellsPerRow) + x;
        }

        





        private int maxHeight = 10;
        private int minHeight = -1;
        private void generateMap()
        {
            //WorldGenerator.generateBasicMap(this, this.width, this.height, this.minHeight, this.maxHeight);
            WorldGenerator.generateRegionMap(this, this.mapNumCellsPerRow, this.mapNumCellPerColumn, this.minHeight, this.maxHeight);
            
            GenerateSearchTree();
        }


        private void GenerateSearchTree()
        {
            root = GenerateTree(new Node(0, 0, this.mapNumCellsPerRow, this.mapNumCellPerColumn, 0),false);
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

        private void debugPrintTree()
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





        private void GenerateView(Vector3 cameraPosition)
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
            regionRight += 2; if (regionRight > this.mapNumCellsPerRow) regionRight = this.mapNumCellsPerRow;
            regionUp -= 2; if (regionUp < 0) regionUp = 0;
            regionDown += 2; if (regionDown > this.mapNumCellPerColumn) regionDown = this.mapNumCellPerColumn;

            regionWidth = regionRight - regionLeft;
            regionHeight = regionDown - regionUp;


            vertices = new VertexMultitextured[regionWidth * regionHeight];
            float mapCellScaleDivTextureSize = mapCellScale / textureSize;


            for (int x = regionLeft; x < regionRight; x++)
            {

                for (int y = regionUp; y < regionDown; y++)
                {
                    int adress = (x - regionLeft) + (y - regionUp) * regionWidth;
                  
/*
                    vertices[adress].Position = new Vector3(x * mapCellScale, getCell(x, y) * mapHeightScale, -y * mapCellScale);

                  //  vertices[adress].TextureCoordinate.X = ((float)(x * mapCellScale) ) / textureSize;
                   // vertices[adress].TextureCoordinate.Y = ((float)(y * mapCellScale) ) / textureSize;

                    vertices[adress].TextureCoordinate.X = ((float)(x * mapCellScaleDivTextureSize));
                    vertices[adress].TextureCoordinate.Y = ((float)(y * mapCellScaleDivTextureSize));
  

                    vertices[adress].TexWeights = getCellTexWeights(x,y);*/


                    vertices[adress] = globalVertices[getCellAdress(x, y)];
                }
            }


            updateIndices(regionWidth, regionHeight);

            
            

       //     SetUpWaterVertices(1000,1000);

        //    waterVertexDeclaration = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());

            //  vertices = CalculateNormals(vertices, indices);
            CopyToTerrainBuffers();


        }

        VertexMultitextured[] globalVertices;
        private void PrecalculateVertices() 
        {
            globalVertices = new VertexMultitextured[this.heightMap.Length];

            float mapCellScaleDivTextureSize = mapCellScale / textureSize;
            for (int x = 0; x < mapNumCellsPerRow; x++)
            {

                for (int y = 0; y < mapNumCellPerColumn; y++)
                {
                    int adress = getCellAdress(x, y); // x + y * mapNumCellsPerRow;


                    globalVertices[adress].Position = new Vector3(x * mapCellScale, getCell(x, y) * mapHeightScale, -y * mapCellScale);

                    globalVertices[adress].TextureCoordinate.X = ((float)(x * mapCellScaleDivTextureSize));
                    globalVertices[adress].TextureCoordinate.Y = ((float)(y * mapCellScaleDivTextureSize));


                    globalVertices[adress].TexWeights = getCellTexWeights(x, y);

                    globalVertices[adress].Normal = new Vector3(0, 0, 0);

                }
            }



            for (int x = 0; x < mapNumCellsPerRow-1; x++)
            {
                for (int y = 0; y < mapNumCellPerColumn-1; y++)
                {
                    long index1 = getCellAdress(x, y);
                    long index2 = getCellAdress(x+1, y+1);
                    long index3 = getCellAdress(x, y + 1);

                    Vector3 side1 = globalVertices[index1].Position - globalVertices[index3].Position;
                    Vector3 side2 = globalVertices[index1].Position - globalVertices[index2].Position;
                    Vector3 normal = Vector3.Cross(side1, side2);

                    globalVertices[index1].Normal += normal;
                    globalVertices[index2].Normal += normal;
                    globalVertices[index3].Normal += normal;

                    //index1 = getCellAdress(x, y);
                    index2 = getCellAdress(x + 1, y);
                    index3 = getCellAdress(x+1, y + 1);

                    side1 = globalVertices[index1].Position - globalVertices[index3].Position;
                    side2 = globalVertices[index1].Position - globalVertices[index2].Position;
                    normal = Vector3.Cross(side1, side2);

                    globalVertices[index1].Normal += normal;
                    globalVertices[index2].Normal += normal;
                    globalVertices[index3].Normal += normal;


                }
            }


            for (long i = 0; i < globalVertices.Length; i++)
                globalVertices[i].Normal.Normalize();


        }




        private int lastRegionWidth = -1;
        private int lastRegionHeight = -1;
        private void updateIndices(int regionWidth, int regionHeight) 
        {
            if (indices == null || regionWidth != lastRegionWidth || regionHeight != lastRegionHeight) 
            {
                indices = new Int16[(regionWidth - 1) * (regionHeight - 1) * 6];
                int counter = 0;
                int row = 0;
                for (int y = 0; y < regionHeight - 1; y++)
                {
                    for (int x = 0; x < regionWidth - 1; x++)
                    {
                        /*  Int16 lowerLeft = (Int16)(x + y * regionWidth);
                          Int16 lowerRight = (Int16)((x + 1) + y * regionWidth);
                          Int16 topLeft = (Int16)(x + (y + 1) * regionWidth);
                          Int16 topRight = (Int16)((x + 1) + (y + 1) * regionWidth);*/


                        Int16 lowerLeft = (Int16)(x + row);
                        Int16 lowerRight = (Int16)(lowerLeft + 1);
                        Int16 topLeft = (Int16)(lowerLeft + regionWidth);
                        Int16 topRight = (Int16)(lowerLeft + 1 + regionWidth);


                        indices[counter++] = topLeft;
                        indices[counter++] = lowerRight;
                        indices[counter++] = lowerLeft;

                        indices[counter++] = topLeft;
                        indices[counter++] = topRight;
                        indices[counter++] = lowerRight;
                    }
                    row += regionWidth;
                }
            }
        }



        private void CopyToTerrainBuffers()
        {

            if (terrainVertexBuffer == null || terrainVertexBuffer.VertexCount != vertices.Length)
            {
                terrainVertexBuffer = new VertexBuffer(device, typeof(VertexMultitextured), vertices.Length, BufferUsage.WriteOnly);
            }
            
            terrainVertexBuffer.SetData(vertices);

            if (terrainIndexBuffer == null || terrainIndexBuffer.IndexCount != indices.Length)
            {
                terrainIndexBuffer = new IndexBuffer(device, typeof(Int16), indices.Length, BufferUsage.WriteOnly);
            }
            terrainIndexBuffer.SetData(indices);


        }



        private bool IsBoxInFrustum(Vector3 bMin, Vector3 bMax, BoundingFrustum Frustum)
        {
            Vector3 NearPoint;

            Plane[] plane = new Plane[6];
            plane[0] = frustum.Bottom;
            plane[1] = frustum.Far;
            plane[2] = frustum.Left;
            plane[3] = frustum.Near;
            plane[4] = frustum.Right;
            plane[5] = frustum.Top;
                
            
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

        float FOGNEAR = 250.0f;
        float FOGFAR = 300.0f;
        Color FOGCOLOR = new Color(100,100, 100);

        
       // Color FOGCOLOR = new Color(0.3f, 0.3f, 0.8f, 1.0f);

        private void DrawTerrain(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
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
            effect.Parameters["xAmbient"].SetValue(0.8f);
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

        private void DrawSkyDome(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            device.DepthStencilState = DepthStencilState.None;

            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(300) * Matrix.CreateTranslation(cameraPosition);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(cloudMap);
                    currentEffect.Parameters["xEnableLighting"].SetValue(false);

                    // FOG

                    effect.Parameters["FogColor"].SetValue(FOGCOLOR.ToVector4());
                    effect.Parameters["FogNear"].SetValue(FOGNEAR);
                    effect.Parameters["FogFar"].SetValue(FOGFAR);
                    effect.Parameters["cameraPos"].SetValue(cameraPosition);

                }
                mesh.Draw();
            }
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
        }

        private Texture2D CreateStaticMap(int resolution)
        {
            Random rand = new Random();
            Color[] noisyColors = new Color[resolution * resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                {
                    noisyColors[x + y * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));
                    
                }

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
            effect.Parameters["xTexture0"].SetValue(cloudStaticMap);
            effect.Parameters["xOvercast"].SetValue(0.9f);
            effect.Parameters["xTime"].SetValue(time / 8000.0f);
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
        private RenderTarget2D refractionRenderTarget;
        private Texture2D refractionMap;
        private RenderTarget2D reflectionRenderTarget;
        private Texture2D reflectionMap;
        public Matrix reflectionViewMatrix;
        private Vector3 windDirection = new Vector3(1, 0, 0);


        public Texture2D waterBumpMap;


        private Plane CreatePlane(float height, Vector3 planeNormalDirection, Matrix currentViewMatrix, bool clipSide)
        {
            planeNormalDirection.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
            if (clipSide)
            {
                planeCoeffs *= -1;
            }

	        Plane finalPlane = new Plane(planeCoeffs);

            return finalPlane;
        }


        private void DrawRefractionMap(Camera PlayerCamera)
        {
            Plane refractionPlane = CreatePlane(waterHeight + (1.5f * mapHeightScale), new Vector3(0, -1, 0), PlayerCamera.viewMatrix, false);

            effect.Parameters["ClipPlane0"].SetValue(new Vector4(refractionPlane.Normal, refractionPlane.D));
            effect.Parameters["Clipping"].SetValue(true);    // Allows the geometry to be clipped for the purpose of creating a refraction map
            device.SetRenderTarget(refractionRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawTerrain(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());
            device.SetRenderTarget(null);
            effect.Parameters["Clipping"].SetValue(false);   // Make sure you turn it back off so the whole scene doesnt keep rendering as clipped
            refractionMap = refractionRenderTarget;
        }





        private void DrawReflectionMap(Camera PlayerCamera)
        {
            Plane reflectionPlane = CreatePlane(waterHeight - (0.5f * mapHeightScale), new Vector3(0, -1, 0), reflectionViewMatrix, true);
            
            effect.Parameters["ClipPlane0"].SetValue(new Vector4(reflectionPlane.Normal, reflectionPlane.D));

            effect.Parameters["Clipping"].SetValue(true);    // Allows the geometry to be clipped for the purpose of creating a refraction map
            device.SetRenderTarget(reflectionRenderTarget);


            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);


            DrawSkyDome(reflectionViewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());
            DrawTerrain(reflectionViewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());

            effect.Parameters["Clipping"].SetValue(false);

            device.SetRenderTarget(null);

            reflectionMap = reflectionRenderTarget;

            
        }






        private void DrawWater(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition, float time)
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
            effect.Parameters["xWaveLength"].SetValue(0.01f);
            effect.Parameters["xWaveHeight"].SetValue(0.03f);

            effect.Parameters["xCamPos"].SetValue(cameraPosition);

            effect.Parameters["FogColor"].SetValue(FOGCOLOR.ToVector4());
            effect.Parameters["FogNear"].SetValue(FOGNEAR);
            effect.Parameters["FogFar"].SetValue(FOGFAR);
            


            effect.Parameters["xTime"].SetValue(time);
            effect.Parameters["xWindForce"].SetValue(0.0002f);
            effect.Parameters["xWindDirection"].SetValue(windDirection);




            effect.CurrentTechnique.Passes[0].Apply();


            device.SetVertexBuffer(waterVertexBuffer);

            int noVertices = waterVertexBuffer.VertexCount;
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, noVertices / 3);




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

       





        private int enitiesDrawn;
        
        



        private Game1 game;

        private int mapNumCellsPerRow { get; set; }
        private int mapNumCellPerColumn { get; set; }

        private int[] heightMap;
        private Vector4[] textureMap;

        // todo provide access to the resources each cell had a sector, and each sector has a resourcesCell
        private int[] sector;
        private ResourceCell[] resources;

        //public VertexPositionNormalColored[] vertices;
        private VertexMultitextured[] vertices;

        public Texture2D groundTexture { get; set; }
        public float textureSize { get; set; }

        public Texture2D sandTexture { get; set; }
        public Texture2D rockTexture { get; set; }
        public Texture2D snowTexture { get; set; }



        private GraphicsDevice device { get; set; }

        private Int16[] indices;
        private VertexBuffer terrainVertexBuffer;
        private IndexBuffer terrainIndexBuffer;

        public const int WaterLevel = 0;

        private BoundingFrustum frustum;

        private Effect effect { get; set; }
    //    private Matrix projectionMatrix { get; set; }

        // search tree
        private Node root;


        // sky dome

        private Texture2D cloudMap;
        private Model skyDome;


        private List<Entity> entities;

        

        // selection

    //    private Vector3 lookingAt;
        public Texture2D selectionTexture;



        private VertexBuffer waterVertexBuffer;
      //  private VertexDeclaration waterVertexDeclaration;
        private RenderTarget2D BackDropRenderTarget;



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
                return "(" + left + ", " + upper + ", " + (left + width) + ", " + (upper + height) + ")";
            }




            public Node A;
            public Node B;
        }



        public class ResourceCell
        {
            // amount in percent
            public float Iron;
            public float Copper;
            public float Aluminium;
            public float Lithium;
            public float Titanium;
            public float Nickel;
            public float Silver;
            public float Tungsten;
            public float Platinum;
            public float Gold;
            public float Lead;
            public float Uranium;

            public void Randomize(Random rnd)
            {
                //Random rnd = new Random();
                Iron = rnd.Next(100);
                Copper = rnd.Next(100);
                Aluminium = rnd.Next(100);
                Lithium = rnd.Next(100);
                Titanium = rnd.Next(100);
                Nickel = rnd.Next(100);
                Silver = rnd.Next(100);
                Tungsten = rnd.Next(100);
                Platinum = rnd.Next(100);
                Gold = rnd.Next(100);
                Lead = rnd.Next(100);
                Uranium = rnd.Next(100);
                Normalize();
            }

            public void Normalize()
            {
                float val = Iron + Copper + Aluminium + Lithium + Titanium + Nickel + Silver + Tungsten + Platinum + Gold + Lead + Uranium;

                Iron = Iron / val * 100;
                Copper = Copper / val * 100;
                Aluminium = Aluminium / val * 100;
                Lithium = Lithium / val * 100;
                Titanium = Titanium / val * 100;
                Nickel = Nickel / val * 100;
                Silver = Silver / val * 100;
                Tungsten = Tungsten / val * 100;
                Platinum = Platinum / val * 100;
                Gold = Gold / val * 100;
                Lead = Lead / val * 100;
                Uranium = Uranium / val * 100;
            }



            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append("Iron: "); sb.Append((int)(Iron * 1)); sb.AppendLine();
                sb.Append("Copper: "); sb.Append((int)(Copper * 1)); sb.AppendLine();
                sb.Append("Aluminium: "); sb.Append((int)(Aluminium * 1)); sb.AppendLine();
                sb.Append("Lithium: "); sb.Append((int)(Lithium * 1)); sb.AppendLine();
                sb.Append("Titanium: "); sb.Append((int)(Titanium * 1)); sb.AppendLine();
                sb.Append("Nickel: "); sb.Append((int)(Nickel * 1)); sb.AppendLine();
                sb.Append("Silver: "); sb.Append((int)(Silver * 1)); sb.AppendLine();
                sb.Append("Tungsten: "); sb.Append((int)(Tungsten * 1)); sb.AppendLine();
                sb.Append("Platinum: "); sb.Append((int)(Platinum * 1)); sb.AppendLine();
                sb.Append("Gold: "); sb.Append((int)(Gold * 1)); sb.AppendLine();
                sb.Append("Lead: "); sb.Append((int)(Lead * 1)); sb.AppendLine();
                sb.Append("Uranium: "); sb.Append((int)(Uranium * 1)); sb.AppendLine();

                return sb.ToString();
            }

        }


        /*
         
Iron
Copper
Aluminium
Lithium
Titanium
Nickel
Silver
Tungsten
Platinum
Gold
Lead
Uranium
          
         * */

        #endregion PrivateMembers

    }
}
