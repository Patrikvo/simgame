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

using Simgame2.DeferredRenderer;


namespace Simgame2.LODTerrain
{
    public class LODTerrain
    {
        public enum RENDERER  {PRELIT, DEFERED}


        public RENDERER ActiveRenderer;

        //Light Manager
        LightManager lightManager;

        //Deffered Renderer
        public DeferredRenderer.DeferredRenderer deferredRenderer;

        //SSAO
        SSAO ssao;


        Game game;

        GraphicsDevice device;

        public LODTerrain(Game1 game, int mapNumCellsPerRow, int mapNumCellPerColumn, Effect effect, GraphicsDevice device)
        {
            this.ActiveRenderer = RENDERER.PRELIT;

            this.game = game;

            playerLight = new PPPointLight(new Vector3(0, 0, 0), Color.White* 0.50f, 100);
            this.playerCamera = game.PlayerCamera;

            modelEffect = game.Content.Load<Effect>("PrelightEffects");
           

            CreateNewMap(mapNumCellsPerRow, mapNumCellPerColumn, device);
            this.device = device;


            
            prelightRender = new PrelightingRenderer(game, effect, device, entities);


            //Create Deferred Renderer
            deferredRenderer = new DeferredRenderer.DeferredRenderer(device, game.Content, device.Viewport.Width, device.Viewport.Height);

            //Create SSAO
    //        ssao = new SSAO(device, game.Content, device.Viewport.Width, device.Viewport.Height);

            //Create Light Manager
            lightManager = new LightManager(game.Content);

            Scene = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            //Add a Directional Light
           // lightManager.AddLight(new DeferredRenderer.DirectionalLight(Vector3.Down, Color.White, 0.5f));
            lightManager.AddLight(new DeferredRenderer.DirectionalLight(new Vector3(0.5f,0.7f,0.5f), Color.White, 0.5f));

            lightManager.AddLight(new DeferredRenderer.PointLight(device,new Vector3(1500.0f,100.0f,500.0f), 100.0f, Color.Green.ToVector4(), 0.7f, true, 4096));

                

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

            _quadTree = new QuadTree(new Vector4(mapNumCellsPerRow, mapNumCellPerColumn, this.minHeight, this.maxHeight), device);
        }

        //Scene
        RenderTarget2D Scene;

        public void Draw(Camera PlayerCamera, GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;


            this.frustum = playerCamera.GetFrustrum();


            FindVisibleEnities();

            if (this.ActiveRenderer == RENDERER.PRELIT)
            {

                prelightRender.terrainVertexBuffer = _quadTree.GetVertexBuffer();
                prelightRender.terrainIndexBuffer = _quadTree.GetIndexBuffer();

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



                prelightRender.DrawNormal(playerCamera.viewMatrix, playerCamera.projectionMatrix, playerCamera.GetCameraPostion(), new Vector3(1600, 50, 500), prelightRender.SunLight.GetRotationMatrix());
                ((Game1)game).debugImg = prelightRender.normalTarg;


                
                




            }
            else if (this.ActiveRenderer == RENDERER.DEFERED)
            {

                deferredRenderer.terrainVertexBuffer = _quadTree.GetVertexBuffer();
                deferredRenderer.terrainIndexBuffer = _quadTree.GetIndexBuffer();

                deferredRenderer.Draw(this.device, null, this.lightManager, playerCamera, null);

       //         ssao.Draw(this.device, deferredRenderer.getGBuffer(), Scene, playerCamera, null);

                SpriteBatch b = new SpriteBatch(game.GraphicsDevice);
                deferredRenderer.Debug(game.GraphicsDevice, b);
                


            }

           



            

        }

        public void Update(GameTime gameTime)
        {
            playerLight.Position = new Vector3(playerCamera.GetCameraPostion().X, playerCamera.GetCameraPostion().Y + 20, playerCamera.GetCameraPostion().Z);
            _quadTree.Update(gameTime, playerCamera);
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
        }


        public void SetResources(ResourceCell[] resources)
        {
            this._quadTree.Vertices.resources = resources;
        }



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


        private double[] cellTravelResistance;



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

        // selection
        public Texture2D selectionTexture;

    }
}



