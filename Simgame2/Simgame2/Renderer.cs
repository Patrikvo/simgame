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

    public class Renderer : Microsoft.Xna.Framework.GameComponent
    {
        public Renderer(Game game, Effect effect, GraphicsDevice device)
            : base(game)
        {
            this.PlayerCamera = ((Game1)game).PlayerCamera;
            this.effect = effect;
            this.device = device;
            AmbientLightLevel = 0.8f;
            SunLightDirection = new Vector3(-0.5f, -1, -0.5f);
        }

        public override void Initialize()
        {

            base.Initialize();
            PresentationParameters pp = device.PresentationParameters;
            refractionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            reflectionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            cloudsRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            cloudStaticMap = CreateStaticMap(256);

            fullScreenVertices = SetUpFullscreenVertices();
        }

        private Camera PlayerCamera;

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }


        public const float FOGDISTANCE = 50.0f;
        public const float FOGNEAR = 250.0f;
        public const float FOGFAR = 300.0f;
        public Color FOGCOLOR = new Color(100, 100, 100);
        // Color FOGCOLOR = new Color(0.3f, 0.3f, 0.8f, 1.0f);


        public Vector3 SunLightDirection { get; set; }


        public Effect effect { get; set; }
        public GraphicsDevice device { get; set; }

        private float _AmbientLightLevel;

        public float AmbientLightLevel
        {
            get { return _AmbientLightLevel; }
            set { _AmbientLightLevel = value; }
        }


        public void IncreaseAmbientLightLevel()
        {
            AmbientLightLevel = AmbientLightLevel + 0.1f;
            if (AmbientLightLevel > 1) { AmbientLightLevel = 1.0f; }
        }


        public void DecreaseAmbientLightLevel()
        {
            AmbientLightLevel = AmbientLightLevel - 0.1f;
            if (AmbientLightLevel < 0.1) { AmbientLightLevel = 0.1f; }
        }

        #region TERRAIN
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public VertexBuffer terrainVertexBuffer;
        public IndexBuffer terrainIndexBuffer;

        public Texture2D[] Textures;

        private Int16[] indices;
        public VertexMultitextured[] vertices;


        public void DrawTerrain(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            Matrix worldMatrix = Matrix.Identity;
            this.effect.Parameters["xWorld"].SetValue(worldMatrix);
            this.effect.Parameters["xView"].SetValue(currentViewMatrix);
            this.effect.Parameters["xProjection"].SetValue(projectionMatrix);

            this.effect.Parameters["xTexture"].SetValue(this.Textures[0]);


            this.effect.CurrentTechnique = this.effect.Techniques["MultiTextured"];
            this.effect.Parameters["xTexture0"].SetValue(this.Textures[1]);
            this.effect.Parameters["xTexture1"].SetValue(this.Textures[2]);
            this.effect.Parameters["xTexture2"].SetValue(this.Textures[3]);
            this.effect.Parameters["xTexture3"].SetValue(this.Textures[4]);



            this.effect.Parameters["xEnableLighting"].SetValue(true);
            this.effect.Parameters["xAmbient"].SetValue(AmbientLightLevel);
            this.effect.Parameters["xLightDirection"].SetValue(SunLightDirection);
            this.effect.Parameters["xLightPos"].SetValue(new Vector3(500,500,-500));
            this.effect.Parameters["xLightPower"].SetValue(1.0f);



            //FOG

            this.effect.Parameters["FogColor"].SetValue(this.FOGCOLOR.ToVector4());
            //this.effect.Parameters["FogNear"].SetValue(Renderer.FOGNEAR);
            //this.effect.Parameters["FogFar"].SetValue(Renderer.FOGFAR);
            this.effect.Parameters["FogNear"].SetValue(this.PlayerCamera.DrawDistance - Renderer.FOGDISTANCE);
            this.effect.Parameters["FogFar"].SetValue(this.PlayerCamera.DrawDistance);
            this.effect.Parameters["cameraPos"].SetValue(cameraPosition);



            this.effect.CurrentTechnique.Passes[0].Apply();
            foreach (EffectPass pass in this.effect.CurrentTechnique.Passes)
            {
                this.device.SetVertexBuffer(this.terrainVertexBuffer);
                this.device.Indices = this.terrainIndexBuffer;



                int noVertices = this.terrainVertexBuffer.VertexCount;
                int noTriangles = this.terrainIndexBuffer.IndexCount / 3;
                this.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noVertices, 0, noTriangles);
            }
        }


        public void CopyToTerrainBuffers()
        {

            if (this.terrainVertexBuffer == null || this.terrainVertexBuffer.VertexCount != vertices.Length)
            {
                this.terrainVertexBuffer = new VertexBuffer(this.device, typeof(VertexMultitextured), vertices.Length, BufferUsage.WriteOnly);
            }

            this.terrainVertexBuffer.SetData(vertices);

            if (this.terrainIndexBuffer == null || this.terrainIndexBuffer.IndexCount != this.indices.Length)
            {
                this.terrainIndexBuffer = new IndexBuffer(this.device, typeof(Int16), this.indices.Length, BufferUsage.WriteOnly);
            }
            this.terrainIndexBuffer.SetData(this.indices);


        }


        public void updateIndices(int regionWidth, int regionHeight)
        {
            if (this.indices == null || regionWidth != -1 || regionHeight != -1)
            {
                this.indices = new Int16[(regionWidth - 1) * (regionHeight - 1) * 6];
                int counter = 0;
                int row = 0;
                for (int y = 0; y < regionHeight - 1; y++)
                {
                    for (int x = 0; x < regionWidth - 1; x++)
                    {

                        Int16 lowerLeft = (Int16)(x + row);
                        Int16 lowerRight = (Int16)(lowerLeft + 1);
                        Int16 topLeft = (Int16)(lowerLeft + regionWidth);
                        Int16 topRight = (Int16)(lowerLeft + 1 + regionWidth);


                        this.indices[counter++] = topLeft;
                        this.indices[counter++] = lowerRight;
                        this.indices[counter++] = lowerLeft;

                        this.indices[counter++] = topLeft;
                        this.indices[counter++] = topRight;
                        this.indices[counter++] = lowerRight;
                    }
                    row += regionWidth;
                }
            }
        }




        #endregion

        #region WATER
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        
        private RenderTarget2D refractionRenderTarget;
        private RenderTarget2D reflectionRenderTarget;
        private Texture2D refractionMap;
        private Texture2D reflectionMap;
        private VertexBuffer waterVertexBuffer;
        private Vector3 windDirection = new Vector3(1, 0, 0);
        public Texture2D waterBumpMap;
        public Matrix reflectionViewMatrix;

        public void SetUpWaterVertices(int width, int height, int mapCellScale, float waterHeight)
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


        public void DrawWater(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition, float time)
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
            //effect.Parameters["FogNear"].SetValue(FOGNEAR);
            //effect.Parameters["FogFar"].SetValue(FOGFAR);
            effect.Parameters["FogNear"].SetValue(this.PlayerCamera.DrawDistance - Renderer.FOGDISTANCE);
            effect.Parameters["FogFar"].SetValue(this.PlayerCamera.DrawDistance);


            effect.Parameters["xTime"].SetValue(time);
            effect.Parameters["xWindForce"].SetValue(0.0002f);
            effect.Parameters["xWindDirection"].SetValue(windDirection);


            this.effect.Parameters["xEnableLighting"].SetValue(true);
            this.effect.Parameters["xAmbient"].SetValue(AmbientLightLevel);
            this.effect.Parameters["xLightDirection"].SetValue(SunLightDirection);
            this.effect.Parameters["cameraPos"].SetValue(cameraPosition);

            effect.CurrentTechnique.Passes[0].Apply();


            device.SetVertexBuffer(waterVertexBuffer);

            int noVertices = waterVertexBuffer.VertexCount;
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, noVertices / 3);




        }


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


        public void DrawRefractionMap(Camera PlayerCamera, float waterHeight, int mapHeightScale)
        {
            Plane refractionPlane = this.CreatePlane(waterHeight + (1.5f * mapHeightScale), new Vector3(0, -1, 0), PlayerCamera.viewMatrix, false);

            this.effect.Parameters["ClipPlane0"].SetValue(new Vector4(refractionPlane.Normal, refractionPlane.D));
            this.effect.Parameters["Clipping"].SetValue(true);    // Allows the geometry to be clipped for the purpose of creating a refraction map
            this.device.SetRenderTarget(this.refractionRenderTarget);
            this.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawTerrain(PlayerCamera.viewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());
            this.device.SetRenderTarget(null);
            this.effect.Parameters["Clipping"].SetValue(false);   // Make sure you turn it back off so the whole scene doesnt keep rendering as clipped
            this.refractionMap = this.refractionRenderTarget;
        }

        public void DrawReflectionMap(Camera PlayerCamera, float waterHeight, int mapHeightScale, List<Entity> entities, BoundingFrustum frustum)
        {
            Plane reflectionPlane = this.CreatePlane(waterHeight - (0.5f * mapHeightScale), new Vector3(0, -1, 0), this.reflectionViewMatrix, true);

            this.effect.Parameters["ClipPlane0"].SetValue(new Vector4(reflectionPlane.Normal, reflectionPlane.D));

            this.effect.Parameters["Clipping"].SetValue(true);    // Allows the geometry to be clipped for the purpose of creating a refraction map
            this.device.SetRenderTarget(this.reflectionRenderTarget);


            this.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);


            DrawSkyDome(this.reflectionViewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());
            this.DrawTerrain(this.reflectionViewMatrix, PlayerCamera.projectionMatrix, PlayerCamera.GetCameraPostion());

            foreach (Entity e in  entities)
            {
                if (frustum.Contains(e.boundingBox) != ContainmentType.Disjoint)
                {
                    e.Draw(this.reflectionViewMatrix, PlayerCamera.GetCameraPostion());
                }
            }



            this.effect.Parameters["Clipping"].SetValue(false);

            this.device.SetRenderTarget(null);

            this.reflectionMap = this.reflectionRenderTarget;


        }

        


        #endregion




        #region SKYDOME
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        
        private RenderTarget2D cloudsRenderTarget;
        private Texture2D cloudStaticMap;
        private VertexPositionTexture[] fullScreenVertices;
        private Model skyDome;
        private Texture2D cloudMap;


        public void LoadSkyDome(Model dome)
        {
            this.skyDome = dome;
            this.skyDome.Meshes[0].MeshParts[0].Effect = this.effect.Clone();

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

        public void DrawSkyDome(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            this.device.DepthStencilState = DepthStencilState.None;

            Matrix[] modelTransforms = new Matrix[this.skyDome.Bones.Count];
            this.skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(300) * Matrix.CreateTranslation(cameraPosition);
            foreach (ModelMesh mesh in this.skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(this.cloudMap);
                    currentEffect.Parameters["xEnableLighting"].SetValue(false);
                    currentEffect.Parameters["xAmbient"].SetValue(AmbientLightLevel);
                    currentEffect.Parameters["xLightDirection"].SetValue(SunLightDirection);

                    // FOG

                    currentEffect.Parameters["FogColor"].SetValue(this.FOGCOLOR.ToVector4());
                    //currentEffect.Parameters["FogNear"].SetValue(Renderer.FOGNEAR);
                    //currentEffect.Parameters["FogFar"].SetValue(Renderer.FOGFAR);
                    currentEffect.Parameters["FogNear"].SetValue(this.PlayerCamera.DrawDistance - Renderer.FOGDISTANCE);
                    currentEffect.Parameters["FogFar"].SetValue(this.PlayerCamera.DrawDistance);

                    currentEffect.Parameters["cameraPos"].SetValue(cameraPosition);

                }
                mesh.Draw();
            }
            this.device.BlendState = BlendState.Opaque;
            this.device.DepthStencilState = DepthStencilState.Default;
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


        public void GeneratePerlinNoise(float time)
        {
            this.device.SetRenderTarget(this.cloudsRenderTarget);
            this.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            this.effect.CurrentTechnique = this.effect.Techniques["PerlinNoise"];
            this.effect.Parameters["xTexture0"].SetValue(this.cloudStaticMap);
            this.effect.Parameters["xOvercast"].SetValue(0.9f);
            this.effect.Parameters["xTime"].SetValue(time / 8000.0f);
            this.effect.CurrentTechnique.Passes[0].Apply();
            foreach (EffectPass pass in this.effect.CurrentTechnique.Passes)
            {
                this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, this.fullScreenVertices, 0, 2);
            }


            this.device.SetRenderTarget(null);
            this.cloudMap = this.cloudsRenderTarget;


        }




        #endregion
    }
}
