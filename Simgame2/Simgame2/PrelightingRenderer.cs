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
using System.IO;


namespace Simgame2
{

    public class PrelightingRenderer : Microsoft.Xna.Framework.GameComponent
    {
        // Normal, depth, and light map render targets
        public RenderTarget2D depthTarg;
        public RenderTarget2D normalTarg;
        public RenderTarget2D lightTarg;

        // Depth/normal effect and light mapping effect
        Effect depthNormalEffect;
        Effect lightingEffect;

        // Point light (sphere) mesh
        Model lightMesh;

        // List of models, lights, and the camera
     //   public List<CModel> Models { get; set; }
        public List<Entity> entities;
        public List<PPPointLight> Lights { get; set; }
        public Camera Camera { get; set; }

        public GraphicsDevice graphicsDevice;
        int viewWidth = 0, viewHeight = 0;





        public PrelightingRenderer(Game game, Effect effect, GraphicsDevice device, List<Entity> entities)
            : base(game)
        {
            viewWidth = device.Viewport.Width;
            viewHeight = device.Viewport.Height;

            // Create the three render targets
            depthTarg = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
            normalTarg = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            lightTarg = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

            // Load effects
            depthNormalEffect = game.Content.Load<Effect>("PPDepthNormal");
            lightingEffect = game.Content.Load<Effect>("PPLight");
            this.effect = game.Content.Load<Effect>("PrelightEffects");
            // Set effect parameters to light mapping effect
            lightingEffect.Parameters["viewportWidth"].SetValue(viewWidth);
            lightingEffect.Parameters["viewportHeight"].SetValue(viewHeight);

            // Load point light mesh and set light mapping effect to it
            lightMesh = game.Content.Load<Model>("PPLightMesh");
            lightMesh.Meshes[0].MeshParts[0].Effect = lightingEffect;
            

            this.graphicsDevice = device;
            this.entities = entities;

            this.Lights = new List<PPPointLight>()
{
new PPPointLight(new Vector3(500, 50, -500), Color.Red * .85f,
100),
new PPPointLight(new Vector3(550, 50, -550), Color.Blue * .85f,
100),
new PPPointLight(new Vector3(450, 50, -450), Color.Green * .85f,
100)
, new PPPointLight(new Vector3(600, 100, -600), Color.White * .85f, 200000)
};



            // original code: 
            this.PlayerCamera = ((Game1)game).PlayerCamera;
          //  this.effect = effect;
            this.device = device;
            AmbientLightLevel = 0.5f;
            SunLightDirection = new Vector3(-0.5f, -1, -0.5f);
        }


        public void drawDepthNormalMap(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            // Set the render targets to 'slots' 1 and 2
            graphicsDevice.SetRenderTargets(normalTarg, depthTarg);
            // Clear the render target to 1 (infinite depth)
            graphicsDevice.Clear(Color.White);
            // Draw each model with the PPDepthNormal effect


            Matrix worldMatrix = Matrix.Identity;
            this.depthNormalEffect.Parameters["World"].SetValue(worldMatrix);
            this.depthNormalEffect.Parameters["View"].SetValue(currentViewMatrix);
            this.depthNormalEffect.Parameters["Projection"].SetValue(projectionMatrix);

            BlendState stored = this.device.BlendState;
            this.device.BlendState = BlendState.Opaque;

            this.depthNormalEffect.CurrentTechnique.Passes[0].Apply();
            foreach (EffectPass pass in this.depthNormalEffect.CurrentTechnique.Passes)
            {
                this.graphicsDevice.SetVertexBuffer(this.terrainVertexBuffer);
                this.graphicsDevice.Indices = this.terrainIndexBuffer;



                int noVertices = this.terrainVertexBuffer.VertexCount;
                int noTriangles = this.terrainIndexBuffer.IndexCount / 3;
                this.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noVertices, 0, noTriangles);
            }

            foreach (Entity e in entities)
            {
                //if (frustum.Contains(e.boundingBox) != ContainmentType.Disjoint)
                if (e.IsVisible)
                {
                 //   e.CacheEffects();
                    e.SetModelEffect(depthNormalEffect, false);
                    e.HideBillboard = true;
                    e.Draw(PlayerCamera.viewMatrix, PlayerCamera.GetCameraPostion());
                    e.HideBillboard = false;
                   // e.RestoreEffects();
                }
            }

            this.device.BlendState = stored;

            // Un-set the render targets
            graphicsDevice.SetRenderTargets(null);

            
            

        }

      


        public void drawLightMap(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            // Set the depth and normal map info to the effect
            lightingEffect.Parameters["DepthTexture"].SetValue(depthTarg);
            lightingEffect.Parameters["NormalTexture"].SetValue(normalTarg);


            // Calculate the view * projection matrix
            Matrix viewProjection = currentViewMatrix * projectionMatrix;

            // Set the inverse of the view * projection matrix to the effect
            Matrix invViewProjection = Matrix.Invert(viewProjection);
            lightingEffect.Parameters["InvViewProjection"].SetValue(invViewProjection);

            // Set the render target to the graphics device
            graphicsDevice.SetRenderTarget(lightTarg);

            // Clear the render target to black (no light)
            graphicsDevice.Clear(Color.Black);
         

            // Set render states to additive (lights will add their influences)
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthStencilState = DepthStencilState.None;

            foreach (PPPointLight light in Lights)
            {
                // Set the light's parameters to the effect
                light.SetEffectParameters(lightingEffect);

                // Calculate the world * view * projection matrix and set it to
                // the effect
                Matrix wvp = (Matrix.CreateScale(light.Attenuation) * Matrix.CreateTranslation(light.Position)) * viewProjection;
                lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

                // Determine the distance between the light and camera
                float dist = Vector3.Distance(cameraPosition, light.Position);

                // If the camera is inside the light-sphere, invert the cull mode
                // to draw the inside of the sphere instead of the outside
                if (dist < light.Attenuation) 
                    graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

                // Draw the point-light-sphere
                lightMesh.Meshes[0].Draw();

                // Revert the cull mode
                graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }

            // Revert the blending and depth render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Un-set the render target
            graphicsDevice.SetRenderTarget(null);
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
        public const float FOGNEAR = 550.0f;
        public const float FOGFAR = 600.0f;
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
            this.effect.Parameters["World"].SetValue(worldMatrix);
            this.effect.Parameters["View"].SetValue(currentViewMatrix);
            this.effect.Parameters["Projection"].SetValue(projectionMatrix);

            this.effect.Parameters["xTexture"].SetValue(this.Textures[0]);


            this.effect.CurrentTechnique = this.effect.Techniques["MultiTextured"];
            this.effect.Parameters["xTexture0"].SetValue(this.Textures[1]);
            this.effect.Parameters["xTexture1"].SetValue(this.Textures[2]);
            this.effect.Parameters["xTexture2"].SetValue(this.Textures[3]);
            this.effect.Parameters["xTexture3"].SetValue(this.Textures[4]);



            this.effect.Parameters["xEnableLighting"].SetValue(true);
            this.effect.Parameters["xAmbient"].SetValue(new Vector3(0.5f,0.5f,0.5f));
            this.effect.Parameters["xDiffuseColor"].SetValue(new Vector3(0.15f, 0.15f, 0.15f));

            

            this.effect.Parameters["xLightDirection"].SetValue(SunLightDirection);
            this.effect.Parameters["xLightPos"].SetValue(new Vector3(500, 500, -500));
            this.effect.Parameters["xLightPower"].SetValue(1.0f);



            //FOG

            this.effect.Parameters["FogColor"].SetValue(this.FOGCOLOR.ToVector4());
            this.effect.Parameters["FogNear"].SetValue(this.PlayerCamera.DrawDistance - PrelightingRenderer.FOGDISTANCE);
            this.effect.Parameters["FogFar"].SetValue(this.PlayerCamera.DrawDistance);
            this.effect.Parameters["cameraPos"].SetValue(cameraPosition);



            
                this.effect.Parameters["LightTexture"].SetValue(lightTarg);
            
                this.effect.Parameters["viewportWidth"].SetValue(viewWidth);
            
                this.effect.Parameters["viewportHeight"].SetValue(viewHeight);


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
            effect.Parameters["World"].SetValue(worldMatrix);
            effect.Parameters["View"].SetValue(currentViewMatrix);
            effect.Parameters["xReflectionView"].SetValue(reflectionViewMatrix);
            effect.Parameters["Projection"].SetValue(projectionMatrix);
            effect.Parameters["xReflectionMap"].SetValue(reflectionMap);
            effect.Parameters["xRefractionMap"].SetValue(refractionMap);
            effect.Parameters["xWaterBumpMap"].SetValue(waterBumpMap);
            effect.Parameters["xWaveLength"].SetValue(0.01f);
            effect.Parameters["xWaveHeight"].SetValue(0.03f);

            effect.Parameters["xCamPos"].SetValue(cameraPosition);

            effect.Parameters["FogColor"].SetValue(FOGCOLOR.ToVector4());
            //effect.Parameters["FogNear"].SetValue(FOGNEAR);
            //effect.Parameters["FogFar"].SetValue(FOGFAR);
            effect.Parameters["FogNear"].SetValue(this.PlayerCamera.DrawDistance - PrelightingRenderer.FOGDISTANCE);
            effect.Parameters["FogFar"].SetValue(this.PlayerCamera.DrawDistance);


            effect.Parameters["xTime"].SetValue(time);
            effect.Parameters["xWindForce"].SetValue(0.0002f);
            effect.Parameters["xWindDirection"].SetValue(windDirection);


            this.effect.Parameters["xEnableLighting"].SetValue(true);
            this.effect.Parameters["xAmbient"].SetValue(new Vector3(0.15f, 0.15f, 0.15f));
            this.effect.Parameters["xDiffuseColor"].SetValue(new Vector3(0.15f, 0.15f, 0.15f));
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

            foreach (Entity e in entities)
            {
                //if (frustum.Contains(e.boundingBox) != ContainmentType.Disjoint)
                if (e.IsVisible)
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
                    currentEffect.Parameters["World"].SetValue(worldMatrix);
                    currentEffect.Parameters["View"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["Projection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(this.cloudMap);
                    currentEffect.Parameters["xEnableLighting"].SetValue(false);
                    currentEffect.Parameters["xAmbient"].SetValue(new Vector3(0.15f, 0.15f, 0.15f));
                    currentEffect.Parameters["xDiffuseColor"].SetValue(new Vector3(0.15f, 0.15f, 0.15f));
                    currentEffect.Parameters["xLightDirection"].SetValue(SunLightDirection);

                    // FOG

                    currentEffect.Parameters["FogColor"].SetValue(this.FOGCOLOR.ToVector4());
                    //currentEffect.Parameters["FogNear"].SetValue(Renderer.FOGNEAR);
                    //currentEffect.Parameters["FogFar"].SetValue(Renderer.FOGFAR);
                    currentEffect.Parameters["FogNear"].SetValue(this.PlayerCamera.DrawDistance - PrelightingRenderer.FOGDISTANCE);
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
