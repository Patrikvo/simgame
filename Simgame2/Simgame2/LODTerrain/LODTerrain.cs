using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simgame2.Renderer; 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;



namespace Simgame2.LODTerrain
{
    public class LODTerrain
    {


        Game game;

        public LODTerrain(Game1 game, int mapNumCellsPerRow, int mapNumCellPerColumn, Effect effect, GraphicsDevice device)
        {


            this.game = game;

            playerLight = new PPPointLight(new Vector3(0, 0, 0), Color.White* 0.50f, 100);
            this.playerCamera = game.PlayerCamera;

            modelEffect = game.Content.Load<Effect>("PrelightEffects");
        //    modelEffect = game.Content.Load<Effect>("PPModel");

            

     //       if (LoadMap(@"c:\temp\map.dat") != true)
      //      {
                CreateNewMap(mapNumCellsPerRow, mapNumCellPerColumn, device);
      //      }

            prelightRender = new PrelightingRenderer(game, effect, device, entities);

       //     prelightRender.Lights.Add(playerLight);


            this.Initialize();
        }

        public void Initialize()
        {
            prelightRender.Initialize(this.mapNumCellsPerRow * LODTerrain.mapCellScale, this.mapNumCellPerColumn * LODTerrain.mapCellScale);

            prelightRender.SetUpWaterVertices(10000, 10000, mapCellScale, waterHeight);
        }


        public void CreateNewMap(int mapNumCellsPerRow, int mapNumCellPerColumn, GraphicsDevice device)
        {
            entities = new List<Entity>();
            this.mapNumCellsPerRow = mapNumCellsPerRow;
            this.mapNumCellPerColumn = mapNumCellPerColumn;

//            this.heightMap = new int[mapNumCellsPerRow * mapNumCellPerColumn];
  //          this.textureMap = new Vector4[mapNumCellsPerRow * mapNumCellPerColumn];
    //        this.sector = new int[mapNumCellsPerRow * mapNumCellPerColumn];

            //_quadTree = new QuadTree(new Vector4(mapNumCellsPerRow, mapNumCellPerColumn, this.minHeight, this.maxHeight), device);
            _quadTree = new QuadTree(new Vector4(mapNumCellsPerRow, mapNumCellPerColumn, this.minHeight, this.maxHeight), device);
      //      _quadTree.MinimumDepth = 0;

    //        generateMap();

     //       PrecalculateVertices();
      //      GenerateMovementMap();
        }












        public void Draw(Camera PlayerCamera, GameTime gameTime)
        {
            //_quadTree.Draw(gameTime);

            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;

        //    BoundingFrustum frustum = new BoundingFrustum(PlayerCamera.viewMatrix * PlayerCamera.projectionMatrix);
         //   this.frustum = frustum;

            this.frustum = playerCamera.GetFrustrum();

            prelightRender.terrainVertexBuffer = _quadTree.GetVertexBuffer();
            prelightRender.terrainIndexBuffer = _quadTree.GetIndexBuffer();



       //     GenerateView(PlayerCamera.GetCameraPostion());
            FindVisibleEnities();

            // water
            prelightRender.drawDepthNormalMap(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());
            prelightRender.drawLightMap(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion(), this.frustum);

            this.prelightRender.DrawRefractionMap(PlayerCamera, waterHeight, mapHeightScale);
            prelightRender.DrawReflectionMap(PlayerCamera, waterHeight, mapHeightScale, this.entities, this.frustum);

            if (prelightRender.DoShadowMapping) { prelightRender.drawShadowDepthMap(); }
            


            // skybox
            prelightRender.GeneratePerlinNoise(time);

            prelightRender.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            prelightRender.DrawSkyDome(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());

            prelightRender.DrawTerrain(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());
            prelightRender.DrawWater(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion(), time);

            enitiesDrawn = 0;
            foreach (Entity e in entities)
            {
                if (e.IsVisible)
                {
                    enitiesDrawn++;
                    //  e.ShowBoundingBox = true;

                    e.SetModelEffect(modelEffect, false);
                    e.Draw(PlayerCamera.viewMatrix, PlayerCamera.GetCameraPostion());
                }
            }


            //prelightRender.DrawSun(playerCamera.viewMatrix, playerCamera.projectionMatrix, playerCamera.GetCameraPostion());
        //    prelightRender.DrawSun(playerCamera.viewMatrix, playerCamera.BigProjectionMatrix, playerCamera.GetCameraPostion());


            prelightRender.DrawNormal(playerCamera.viewMatrix, playerCamera.projectionMatrix, playerCamera.GetCameraPostion(), new Vector3(1600, 50, 500), prelightRender.SunLight.GetRotationMatrix());
          //  prelightRender.DrawNormal(playerCamera.viewMatrix, playerCamera.projectionMatrix, playerCamera.GetCameraPostion(), new Vector3(1500, 100, 500), new Vector3(0,0,1));
            //prelightRender.DrawNormal(playerCamera.viewMatrix, playerCamera.projectionMatrix, playerCamera.GetCameraPostion(), new Vector3(1450, 100, 500), new Vector3(0, 0, -1));


        //    prelightRender.DrawTriangle(playerCamera.viewMatrix, playerCamera.BigProjectionMatrix, playerCamera.GetCameraPostion());
            
        //    ((Game1)base.Game).debugImg = MiniMap(PlayerCamera.GetCameraPostion());
            //  ((Game1)base.Game).debugImg = prelightRender.lightTarg;
            //  ((Game1)base.Game).debugImg = prelightRender.depthTarg;
              ((Game1)game).debugImg = prelightRender.normalTarg;
            // shadowDepthTarg

        }

        public void Update(GameTime gameTime)
        {
            playerLight.Position = new Vector3(playerCamera.GetCameraPostion().X, playerCamera.GetCameraPostion().Y + 20, playerCamera.GetCameraPostion().Z);


         //   _quadTree.View = _camera.View;
          //  _quadTree.Projection = _camera.Projection;
        //    _quadTree.CameraPosition = _camera.Position;
            _quadTree.Update(gameTime, playerCamera);


      //      prelightRender.LightDirection = prelightRender.LightDirection + (Vector3.Up*0.001f);

        }



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

        public Entity FindEntityAt(Ray ray)
        {
            Entity e = null;
            for (int i = 0; i < this.entities.Count; i++)
            {
                if (frustum.Contains(entities[i].boundingBox) != ContainmentType.Disjoint)
                {
                    if (RayIntersectsModel(ray, entities[i].model, entities[i].GetWorldMatrix(), entities[i].GetBoneTransforms()))
                    {
                        e = entities[i];
                        break;
                    }
                }
            }

            return e;
        }

        private void FindVisibleEnities()
        {
            foreach (Entity e in entities)
            {
                if (frustum.Contains(e.boundingBox) != ContainmentType.Disjoint)
                {
                    e.IsVisible = true;
                }
                else
                {
                    e.IsVisible = false;
                }
            }
        }

        public void AddEntity(Entity entity)
        {
            entity.FOGNEAR = PrelightingRenderer.FOGNEAR;
            entity.FOGFAR = PrelightingRenderer.FOGFAR;
            entity.FOGCOLOR = this.prelightRender.FOGCOLOR;
            this.entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            this.entities.Remove(entity);
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

            if (vec.X >= minx && vec.X <= maxx && vec.Z >= minz && vec.Z <= maxz) { return true; }
            return false;


        }



        public Vector2 getCellAdressFromWorldCoor(float wx, float wz)
        {
            float wy = wz;  // transform z in worldcoor (3D) to y in mapcoor (2D)

            int x = (int)Math.Floor(wx / mapCellScale);
            int y = (int)Math.Floor(wy / mapCellScale);

            return new Vector2(x, y);
        }


        public int getCellHeightFromWorldCoor(float wx, float wz)
        {
            float wy = wz;  // transform z in worldcoor (3D) to y in mapcoor (2D)

            int x = (int)Math.Floor(wx / mapCellScale);
            int y = (int)Math.Floor(wy / mapCellScale);

            return getCell(x, y); // *mapHeightScale;
        }

        public int getExactHeightFromWorldCoor(float wx, float wz)
        {
            float wy = wz;  // transform z in worldcoor (3D) to y in mapcoor (2D)

            int x = (int)Math.Floor(wx / mapCellScale);
            int y = (int)Math.Floor(wy / mapCellScale);

            int h1 = getCell(x, y); // *mapHeightScale;
            int h2 = getCell(x, y + 1); // *mapHeightScale;
            int h3 = getCell(x + 1, y + 1); // *mapHeightScale;

            float dx = (wx / mapCellScale) - x;
            float dy = (wy / mapCellScale) - y;

            float dh1 = (h2 - h1) * dy;
            float dh2 = (h3 - h2) * dx;

            float dh = dh1 + dh2 / 2;

            return (int)(h1 + dh);
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

            return altitude; // *mapHeightScale;
        }





        public ResourceCell GetResourceFromWorldCoor(float wx, float wy)
        {
            Vector2 add = getCellAdressFromWorldCoor(wx, wy);

            return this._quadTree.Vertices.resources[this._quadTree.Vertices.sector[getCellAdress((int)add.X, (int)add.Y)]];
        }


        // worldgenerator interface

        public int getCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.mapNumCellsPerRow || y >= this.mapNumCellPerColumn)
            {
                return 0;
            }

            //return this.heightMap[getCellAdress(x, y)];
            return (int)this._quadTree.Vertices.Vertices[getCellAdress(x, y)].Position.Y;
        }

        public void setCell(int x, int y, int value)
        {
            this._quadTree.Vertices.Vertices[getCellAdress(x, y)].Position = new Vector3(x * mapCellScale, getCell(x, y) * mapHeightScale, -y * mapCellScale);

            //this.heightMap[getCellAdress(x, y)] = value;
       //     if (globalVertices != null)
        //    {
         //       globalVertices[getCellAdress(x, y)].Position = new Vector3(x * mapCellScale, getCell(x, y) * mapHeightScale, -y * mapCellScale);
          //  }
        }


    /*    public void setCell(int x, int y, int value, float tex_x, float tex_y, float tex_z, float tex_w)
        {
            setCell(x, y, value);
            this.textureMap[getCellAdress(x, y)] = new Vector4(tex_x, tex_y, tex_z, tex_w);
        }

        public void setCell(int x, int y, int value, float tex_x, float tex_y, float tex_z, float tex_w, int sector)
        {
            setCell(x, y, value);
            this.textureMap[getCellAdress(x, y)] = new Vector4(tex_x, tex_y, tex_z, tex_w);

            this._quadTree.Vertices.sector[getCellAdress(x, y)] = sector;
            //this.sector[getCellAdress(x, y)] = sector;
        }
*/
        public void SetResources(ResourceCell[] resources)
        {
            this._quadTree.Vertices.resources = resources;
        }

/*
        private Vector4 getCellTexWeights(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.mapNumCellsPerRow || y >= this.mapNumCellPerColumn)
            {
                return new Vector4(0);
            }

            return this.textureMap[getCellAdress(x, y)];
        }*/


        private Vector3 getPlaneNormalFromWorldCoor(float wx, float wz)
        {
            float wy = wz;  // transform z in worldcoor (3D) to y in mapcoor (2D)

            int x = (int)Math.Floor(wx / mapCellScale);
            int y = (int)Math.Floor(wy / mapCellScale);
            
            return PlaneNormals[getCellAdress(x, y)];
        }


        public double GetCellTravelResistance(int x, int y)
        {
            int adress = getCellAdress(x, y);
            return 1;   // TODO fix this
            //return cellTravelResistance[adress];
        }











        private int getCellAdress(int x, int y)
        {
            return (y * this.mapNumCellsPerRow) + x;
        }




        public string GetStats()
        {
            int maxVertices = mapNumCellsPerRow * mapNumCellPerColumn * 2;
            int maxIndices = maxVertices * 3;
            //  return "vertices: " + renderer.terrainVertexBuffer.VertexCount + "/" + maxVertices + " - indices: " + renderer.terrainIndexBuffer.IndexCount + "/" + maxVertices +
            return "vertices: " + prelightRender.terrainVertexBuffer.VertexCount + "/" + maxVertices + " - indices: " + prelightRender.terrainIndexBuffer.IndexCount + "/" + maxVertices +
               Environment.NewLine + " - entities: " + enitiesDrawn + "/" + entities.Count + " - lights: " + prelightRender.DrawnLights + "/" + prelightRender.Lights.Count;
        }



        private QuadTree _quadTree;

        

        public const int mapCellScale = 5;
        public const int mapHeightScale = 2; // 2;
        public const float waterHeight = 2.75f * (float)mapHeightScale;
        public const int textureSize = 512;
        private int maxHeight = 20;
        private int minHeight = -1; //-10;


        public int mapNumCellsPerRow { get; set; }
        public int mapNumCellPerColumn { get; set; }

        public int getMapWidth() { return this.mapNumCellsPerRow * LODTerrain.mapCellScale; }
        public int getMapHeight() { return this.mapNumCellPerColumn * LODTerrain.mapCellScale; }

   //     private int[] heightMap;
  //      private Vector4[] textureMap;

        private double[] cellTravelResistance;

     //   private int[] sector;
 //       private ResourceCell[] resources;



        private Effect modelEffect;

        PPPointLight playerLight;

        Camera playerCamera;

        private BoundingFrustum frustum;

        
        public PrelightingRenderer GetRenderer() { return this.prelightRender; }
        private PrelightingRenderer prelightRender;

        public List<Entity> entities;
        private int enitiesDrawn;



        public int[] pointX;
        public int[] pointY;
        public int[] pointAltitude;

        private Vector3[] PlaneNormals;
     //   VertexMultitextured[] globalVertices;

        // selection
        public Texture2D selectionTexture;

    }
}




// worldmap:



/*
        


        


        







          #region PrivateMembers

          // Private 
          //==============================


          private Texture2D MiniMap(Vector3 cameraPosition)
        {
          //  Texture2D Minimap = new Texture2D(renderer.device, this.mapNumCellsPerRow, this.mapNumCellPerColumn, false, SurfaceFormat.Color);
            Texture2D Minimap = new Texture2D(this.prelightRender.device, this.mapNumCellsPerRow, this.mapNumCellPerColumn, false, SurfaceFormat.Color);

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








          

        





    
        private void generateMap()
        {
            //WorldGenerator.generateBasicMap(this, this.width, this.height, this.minHeight, this.maxHeight);
            WorldGenerator.generateRegionMap(this, this.mapNumCellsPerRow, this.mapNumCellPerColumn, this.minHeight, this.maxHeight);
            
            GenerateSearchTree();
        }


        private void GenerateMovementMap()
        {
            cellTravelResistance = new double[this.mapNumCellsPerRow * this.mapNumCellPerColumn];
            int adress;
            for (int y = 0; y < this.mapNumCellPerColumn; y++)
            {
                for (int x = 0; x < this.mapNumCellsPerRow; x++)
                {
                    adress = getCellAdress(x, y);

                    // create an inpassable border around the map.
                    if (x == 0 || y == 0 || x == this.mapNumCellsPerRow-1 || y == this.mapNumCellPerColumn-1)
                    {
                        cellTravelResistance[adress] = double.MaxValue;
                        continue;
                    }

                    // traveling through water?
                    if (this.heightMap[adress] <= waterHeight)
                    {
                      //  cellTravelResistance[adress] = TravelResistance_Water;
                        cellTravelResistance[adress] = TravelResistance_Water; // double.MaxValue;
                        continue;
                    }

                    // angle between 1 (flat) and 0 straight edge
                    float angle = -Vector3.Dot(getPlaneNormalFromWorldCoor(x, y), Vector3.Up);

                    if (angle < 0)
                    {
                        Console.WriteLine("negative angle found");
                    }

                    // can't pass if angle is more than ~75°
                    if (angle < 0.4f)
                    {
                        cellTravelResistance[adress] = 70000000.0d;//double.MaxValue;
                        continue;
                    }


                  

                    // otherwise resistance is relative to angle

                    cellTravelResistance[adress] = 1.0d;
                    if (angle < 1) { cellTravelResistance[adress] = 1.0d; }
                    if (angle < 0.9) { cellTravelResistance[adress] = 5.0d; }
                    if (angle < 0.8) { cellTravelResistance[adress] = 7.0d; }
                    if (angle < 0.7) { cellTravelResistance[adress] = 10.0d; }
                    if (angle < 0.6) { cellTravelResistance[adress] = 15.0d; }

                 //   cellTravelResistance[adress] = 100 - (int)Math.Floor((angle-0.5) * 2*100);
               //     cellTravelResistance[adress] = 1;




                }
            }
        }

        



        private const int TravelResistance_Water = 100;

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


          //  renderer.vertices = new VertexMultitextured[regionWidth * regionHeight];
            this.prelightRender.vertices = new VertexMultitextured[regionWidth * regionHeight];


           // float mapCellScaleDivTextureSize = mapCellScale / textureSize;


            for (int x = regionLeft; x < regionRight; x++)
            {

                for (int y = regionUp; y < regionDown; y++)
                {
                    int adress = (x - regionLeft) + (y - regionUp) * regionWidth;

                  //  renderer.vertices[adress] = globalVertices[getCellAdress(x, y)];
                    this.prelightRender.vertices[adress] = globalVertices[getCellAdress(x, y)];
                }
            }


          //  renderer.updateIndices(regionWidth, regionHeight);
            prelightRender.updateIndices(regionWidth, regionHeight);


          //  renderer.CopyToTerrainBuffers();
            prelightRender.CopyToTerrainBuffers();

        }


        


        
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


            PlaneNormals = new Vector3[this.heightMap.Length];
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

                    PlaneNormals[getCellAdress(x, y)] = normal;
                    PlaneNormals[getCellAdress(x, y)].Normalize();

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


        
      

       



       


*/