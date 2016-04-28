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


namespace Simgame2.Renderer
{

    public class PrelightingRenderer : Microsoft.Xna.Framework.GameComponent
    {
        public MainLightSource SunLight;

        //shadow map

        // Position and target of the shadowing light
     //   public Vector3 ShadowLightPosition { get; set; }
     //   public Vector3 ShadowLightTarget { get; set; }

        // Shadow depth target and depth-texture effect
        public RenderTarget2D shadowDepthTarg;
        Effect shadowDepthEffect;

        // Depth texture parameters
        int shadowMapSize = 4096; // 2048;
        public int shadowFarPlane = 750;

        // Shadow light view and projection
        Matrix shadowView, shadowProjection;

        // Shadow properties
        public bool DoShadowMapping { get; set; }
        public float ShadowMult { get; set; }
        public float ShadowBias { get; set; }
        public float NormalBias { get; set; }

        // Normal, depth, and light map render targets
        public RenderTarget2D depthTarg;
        public RenderTarget2D normalTarg;
        public RenderTarget2D lightTarg;

        // Depth/normal effect and light mapping effect
        Effect depthNormalEffect;
        Effect lightingEffect;

        // Point light (sphere) mesh
        Model lightMesh;

        // diffuse lightdirection
   //     public Vector3 LightDirection;


        // List of models, lights, and the camera
     //   public List<CModel> Models { get; set; }
        public List<Entity> entities;
        public List<PPPointLight> Lights { get; set; }
        public Camera Camera { get; set; }

        public GraphicsDevice graphicsDevice;
        int viewWidth = 0, viewHeight = 0;

        public int DrawnLights { get; set; }


//        private Game1 game;

        Model sun;


        protected GameSession.GameSession RunningGameSession;

        public PrelightingRenderer(GameSession.GameSession RunningGameSession, Effect effect, GraphicsDevice device, List<Entity> entities)
            : base(RunningGameSession.game)
        {
 

            viewWidth = device.Viewport.Width;
            viewHeight = device.Viewport.Height;
//            this.game = (Game1)game;

            this.RunningGameSession = RunningGameSession;

            this.PlayerCamera = this.RunningGameSession.PlayerCamera;
            this.device = device;
            AmbientLightLevel = 0.2f;

            // Create the three render targets
            depthTarg = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
            normalTarg = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            lightTarg = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            //shadowDepthTarg = new RenderTarget2D(device, shadowMapSize, shadowMapSize, false, SurfaceFormat.Single, DepthFormat.Depth24);
            shadowDepthTarg = new RenderTarget2D(device, shadowMapSize, shadowMapSize, false, SurfaceFormat.Single, DepthFormat.Depth24);
            
            


            // Load effects
            depthNormalEffect = this.RunningGameSession.Content.Load<Effect>("PPDepthNormal");
            lightingEffect = this.RunningGameSession.Content.Load<Effect>("PPLight");
            this.effect = this.RunningGameSession.Content.Load<Effect>("PrelightEffects");
            shadowDepthEffect = this.RunningGameSession.Content.Load<Effect>("ShadowDepthEffect");


            // Set effect parameters to light mapping effect
            lightingEffect.Parameters["viewportWidth"].SetValue(viewWidth);
            lightingEffect.Parameters["viewportHeight"].SetValue(viewHeight);

            // Load point light mesh and set light mapping effect to it
            lightMesh = this.RunningGameSession.Content.Load<Model>("Models/PPLightMesh");
            lightMesh.Meshes[0].MeshParts[0].Effect = lightingEffect;

     //       sun =  game.Content.Load<Model>("PPLightMesh");
    //        sun.Meshes[0].MeshParts[0].Effect = this.effect.Clone();

            
            //shadow map
            this.shadowFarPlane = (int)PlayerCamera.DrawDistance;
            shadowDepthEffect.Parameters["FarPlane"].SetValue(shadowFarPlane);

            this.graphicsDevice = device;
            this.entities = entities;


            SunLight = new MainLightSource();

          
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


            // draw terrain
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




        public void drawLightMap(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition, BoundingFrustum frustum)
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
            DrawnLights = 0;
            foreach (PPPointLight light in Lights)
            {
                // Set the light's parameters to the effect
                light.SetEffectParameters(lightingEffect);

                // Calculate the world * view * projection matrix and set it to
                // the effect
                Matrix wvp = (Matrix.CreateScale(light.Attenuation) * Matrix.CreateTranslation(light.Position)) * viewProjection;
                lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

                // skip all invisible lights.
                if (frustum.Contains(light.GetBoundingSphere()) == ContainmentType.Disjoint)
                {
                    continue;
                }
                DrawnLights++;
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


        public void drawShadowDepthMap()
        {
            // Calculate view and projection matrices for the "light"
            // shadows are being calculated for
            shadowView = Matrix.CreateLookAt(PlayerCamera.GetCameraPostion() + SunLight.ShadowLightPosition, PlayerCamera.GetCameraPostion() + SunLight.ShadowLightTarget, Vector3.Up);
       
            shadowProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1, 1, shadowFarPlane);

                // Set render target
            graphicsDevice.SetRenderTarget(shadowDepthTarg);
            // Clear the render target to 1 (infinite depth)
            graphicsDevice.Clear(Color.White);
            // Draw each model with the ShadowDepthEffect effect

            BlendState stored = this.device.BlendState;
            this.device.BlendState = BlendState.Opaque;
            this.device.RasterizerState = RasterizerState.CullNone;

            this.shadowDepthEffect.Parameters["World"].SetValue(Matrix.Identity);
            this.shadowDepthEffect.Parameters["View"].SetValue(shadowView);
            this.shadowDepthEffect.Parameters["Projection"].SetValue(shadowProjection);
            this.shadowDepthEffect.Parameters["LightViewProj"].SetValue(CreateLightViewProjectionMatrix());
            this.shadowDepthEffect.Parameters["FarPlane"].SetValue(shadowFarPlane);
            this.shadowDepthEffect.Parameters["LightPosition"].SetValue(SunLight.ShadowLightPosition);
            // LightPosition



            this.shadowDepthEffect.CurrentTechnique.Passes[0].Apply();
            foreach (EffectPass pass in this.shadowDepthEffect.CurrentTechnique.Passes)
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
              //  if (e.IsVisible)
                {
                    //   e.CacheEffects();
                    e.SetModelEffect(shadowDepthEffect, false);
                    e.HideBillboard = true;
                    //e.Draw(shadowView,shadowProjection, ShadowLightPosition);
                    e.DrawShadow(shadowView, shadowProjection, SunLight.ShadowLightPosition);

                    e.HideBillboard = false;
                    // e.RestoreEffects();
                }
            }

            if (stored != null)
            {
                this.device.BlendState = stored;
            }

            // Un-set the render targets
            graphicsDevice.SetRenderTarget(null);
        }






        


        public void Initialize(int mapWidth, int mapHeigth)
        {

            base.Initialize();
            PresentationParameters pp = device.PresentationParameters;
            refractionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            reflectionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            cloudsRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, pp.DepthStencilFormat);

            cloudStaticMap = CreateStaticMap(256);

            fullScreenVertices = SetUpFullscreenVertices();

                                        // LR    UD   roll
            SunLight.SetLightDirection(1.0f, 2.5f, 0.0f);
            SunLight.Color = Color.White;
            SunLight.Power = 0.7f;



            this.SunLight.ShadowLightPosition = new Vector3(-500, 200, -500);

 
            this.SunLight.ShadowLightTarget = new Vector3(500, 100, 500);

            this.DoShadowMapping = true;
            this.ShadowMult = 0.3f;
            this.ShadowBias = 1.0f / 200.0f;


            this.NormalBias = 0.4f;


            this.Lights = new List<PPPointLight>()
            {
                new PPPointLight(SunLight.ShadowLightPosition, Color.Red * .85f, 100),
                new PPPointLight(SunLight.ShadowLightTarget, Color.Blue * .85f, 100),
                new PPPointLight(new Vector3(450, 50, 450), Color.Green * .85f, 100), 
                new PPPointLight(new Vector3(1000, 500, 1000), Color.White * .85f, 500)
            };
         
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


     //   public Vector3 SunLightDirection { get; set; }


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



        public Matrix CreateLightViewProjectionMatrix()
        {
            /*
            // Matrix with that will rotate in points the direction of the light
            Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
                                                       -SunLight.GetLightDirection(),
                                                       Vector3.Up);
            
            // Get the corners of the frustum
            Vector3[] frustumCorners = PlayerCamera.GetFrustrum().GetCorners();

            // Transform the positions of the corners into the direction of the light
            for (int i = 0; i < frustumCorners.Length; i++)
            {
                frustumCorners[i] = Vector3.Transform(frustumCorners[i], lightRotation);
            }

            // Find the smallest box around the points
            BoundingBox lightBox = BoundingBox.CreateFromPoints(frustumCorners);

            Vector3 boxSize = lightBox.Max - lightBox.Min;
            Vector3 halfBoxSize = boxSize * 0.5f;

            // The position of the light should be in the center of the back
            // pannel of the box. 
            Vector3 lightPosition = lightBox.Min + halfBoxSize;
            lightPosition.Z = lightBox.Min.Z;

            // We need the position back in world coordinates so we transform 
            // the light position by the inverse of the lights rotation
            lightPosition = Vector3.Transform(lightPosition,
                                              Matrix.Invert(lightRotation));



            

            // Create the view matrix for the light
            Matrix lightView = Matrix.CreateLookAt(lightPosition,
                                                   lightPosition - SunLight.GetLightDirection(),
                                                   Vector3.Up);



            // Create the projection matrix for the light
            // The projection is orthographic since we are using a directional light
            Matrix lightProjection = Matrix.CreateOrthographic(boxSize.X, boxSize.Y,
                                                               -boxSize.Z, boxSize.Z);
            */
            // ////////////////////////

            //BoundingSphere sphere = BoundingSphere.CreateFromFrustum(PlayerCamera.GetFrustrum());
            BoundingSphere sphere = new BoundingSphere(PlayerCamera.GetCameraPostion(), shadowFarPlane);

            float ExtraBackup = 100.0f;
            const float NearClip = 1.0f;

            float backupDist = ExtraBackup + NearClip + sphere.Radius;
            Vector3 shadowCamPos = sphere.Center + (SunLight.GetInvertedLightDirection() * backupDist);
            Matrix shadowViewMatrix = Matrix.CreateLookAt(shadowCamPos, sphere.Center, Vector3.Up);

            float bounds = sphere.Radius * 2.0f;
            float farClip = ExtraBackup + sphere.Radius *2;
            Matrix shadowProjMatrix = Matrix.CreateOrthographic(bounds, bounds, NearClip, farClip);

            Matrix shadowMatrix = shadowViewMatrix * shadowProjMatrix;


          
            Vector3 shadowOrigin = Vector3.Transform(Vector3.Zero, shadowMatrix);
            shadowOrigin *= (shadowMapSize / 2.0f);
            Vector3 roundedOrigin = new Vector3((float)Math.Round(shadowOrigin.X), (float)Math.Round(shadowOrigin.Y), (float)Math.Round(shadowOrigin.Z));
            Vector3 rounding = roundedOrigin - shadowOrigin;
            rounding /= (shadowMapSize / 2.0f);

            Matrix roundMatrix = Matrix.CreateTranslation(rounding.X, rounding.Y, rounding.Z);
            shadowMatrix *= roundMatrix;




            return shadowMatrix;


            /*
            lightView = Matrix.CreateLookAt(SunLight.ShadowLightPosition, SunLight.ShadowLightTarget, new Vector3(0, 1, 0));
            lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 5f, 100f);

            return lightView * lightProjection;*/
        }



        #region   MODEL


        public void DrawModel(Model model, Texture2D[] textures, bool IsTransparant, bool CanPlace, bool HasFocus, Matrix WorldMatrix, Matrix ViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition, string Technique)
        {

            Matrix[] ModelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(ModelTransforms);

            int meshCount = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques[Technique];

                    currentEffect.Parameters["World"].SetValue(ModelTransforms[mesh.ParentBone.Index] * WorldMatrix);
                    currentEffect.Parameters["View"].SetValue(ViewMatrix);
                    currentEffect.Parameters["Projection"].SetValue(projectionMatrix);

                    currentEffect.Parameters["xIsTransparant"].SetValue(IsTransparant);
                    if (CanPlace)
                    {
                        currentEffect.Parameters["xTransparantColor"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 0.5f));
                    }
                    else
                    {
                        currentEffect.Parameters["xTransparantColor"].SetValue(new Vector4(1.0f, 0.0f, 0.0f, 0.5f));
                    }

                    if (HasFocus)
                    {
                        currentEffect.Parameters["xIsTransparant"].SetValue(true);
                        currentEffect.Parameters["xTransparantColor"].SetValue(new Vector4(1.0f, 1.0f, 0.0f, 0.7f));
                    }

                    currentEffect.Parameters["xTexture"].SetValue(textures[meshCount]);
                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xAmbient"].SetValue(this.RunningGameSession.LODMap.GetRenderer().AmbientLightLevel);
                    currentEffect.Parameters["LightDirection"].SetValue(this.RunningGameSession.LODMap.GetRenderer().SunLight.GetInvertedLightDirection());

                    currentEffect.Parameters["cameraPos"].SetValue(cameraPosition);
                    currentEffect.Parameters["FogColor"].SetValue(FOGCOLOR.ToVector4());
                    currentEffect.Parameters["FogNear"].SetValue(FOGNEAR);
                    currentEffect.Parameters["FogFar"].SetValue(FOGFAR);

                    currentEffect.Parameters["LightViewProj"].SetValue(this.CreateLightViewProjectionMatrix());


                    this.effect.Parameters["LightTexture"].SetValue(lightTarg);

                    this.effect.Parameters["viewportWidth"].SetValue(viewWidth);

                    this.effect.Parameters["viewportHeight"].SetValue(viewHeight);


                    // shadowmap:
                    if (this.effect.Parameters["DoShadowMapping"] != null)
                        this.effect.Parameters["DoShadowMapping"].SetValue(DoShadowMapping);

                    if (this.effect.Parameters["ShadowMap"] != null)
                        this.effect.Parameters["ShadowMap"].SetValue(shadowDepthTarg);
                    if (this.effect.Parameters["ShadowView"] != null)
                        this.effect.Parameters["ShadowView"].SetValue(shadowView);
                    if (this.effect.Parameters["ShadowProjection"] != null)
                        this.effect.Parameters["ShadowProjection"].SetValue(shadowProjection);

                    if (this.effect.Parameters["ShadowLightPosition"] != null)
                        this.effect.Parameters["ShadowLightPosition"].SetValue(SunLight.ShadowLightPosition);
                    if (this.effect.Parameters["ShadowFarPlane"] != null)
                        this.effect.Parameters["ShadowFarPlane"].SetValue(shadowFarPlane);
                    if (this.effect.Parameters["ShadowMult"] != null)
                        this.effect.Parameters["ShadowMult"].SetValue(ShadowMult);
                    if (this.effect.Parameters["ShadowBias"] != null)
                        this.effect.Parameters["ShadowBias"].SetValue(ShadowBias);
                    if (this.effect.Parameters["NormalBias"] != null)
                        this.effect.Parameters["NormalBias"].SetValue(this.NormalBias);



                }
                mesh.Draw();
                meshCount++;
            }
            //           if (ShowBoundingBox)
            //           {
            //               DrawBoundingBox(currentViewMatrix, cameraPosition);
            //           }

        }



        public void DrawShadow( Model model, Matrix WorldMatrix, Matrix ViewMatrix, Matrix projectionMatrix, string Technique)
        {

            Matrix[] ModelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(ModelTransforms);

            int meshCount = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques[Technique];

                    currentEffect.Parameters["World"].SetValue(ModelTransforms[mesh.ParentBone.Index] * WorldMatrix);
                    currentEffect.Parameters["View"].SetValue(ViewMatrix);
                    currentEffect.Parameters["Projection"].SetValue(projectionMatrix);

                }
                mesh.Draw();
                meshCount++;
            }

        }








        #endregion


        #region TERRAIN
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public VertexBuffer terrainVertexBuffer;
        public IndexBuffer terrainIndexBuffer;

        public Texture2D[] Textures;

        public Int16[] indices;
        public VertexMultitextured[] vertices;


        public void DrawTerrain(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            this.device.SamplerStates[1] = SamplerState.PointClamp;

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
            this.effect.Parameters["xAmbient"].SetValue(new Vector3(AmbientLightLevel, AmbientLightLevel, AmbientLightLevel));
            this.effect.Parameters["xDiffuseColor"].SetValue(SunLight.Color.ToVector3());



            this.effect.Parameters["LightDirection"].SetValue(SunLight.GetInvertedLightDirection());
            this.effect.Parameters["xLightPos"].SetValue(new Vector3(500, 500, 500));
            this.effect.Parameters["xLightPower"].SetValue(SunLight.Power);

            this.effect.Parameters["NormalTexture"].SetValue(normalTarg);


            //FOG

            this.effect.Parameters["FogColor"].SetValue(this.FOGCOLOR.ToVector4());
            this.effect.Parameters["FogNear"].SetValue(this.PlayerCamera.DrawDistance - PrelightingRenderer.FOGDISTANCE);
            this.effect.Parameters["FogFar"].SetValue(this.PlayerCamera.DrawDistance);
            this.effect.Parameters["cameraPos"].SetValue(cameraPosition);



            
                this.effect.Parameters["LightTexture"].SetValue(lightTarg);
            
                this.effect.Parameters["viewportWidth"].SetValue(viewWidth);
            
                this.effect.Parameters["viewportHeight"].SetValue(viewHeight);

            // shadowmap:
                if (this.effect.Parameters["DoShadowMapping"] != null)
                    this.effect.Parameters["DoShadowMapping"].SetValue(DoShadowMapping);
                
                if (this.effect.Parameters["ShadowMap"] != null)
                    this.effect.Parameters["ShadowMap"].SetValue(shadowDepthTarg);
                if (this.effect.Parameters["ShadowView"] != null)
                    this.effect.Parameters["ShadowView"].SetValue(shadowView);
                if (this.effect.Parameters["ShadowProjection"] != null)
                    this.effect.Parameters["ShadowProjection"].SetValue(shadowProjection);

                if (this.effect.Parameters["ShadowLightPosition"] != null)
                    this.effect.Parameters["ShadowLightPosition"].SetValue(SunLight.ShadowLightPosition);
                if (this.effect.Parameters["ShadowFarPlane"] != null)
                    this.effect.Parameters["ShadowFarPlane"].SetValue(shadowFarPlane);
                if (this.effect.Parameters["ShadowMult"] != null)
                    this.effect.Parameters["ShadowMult"].SetValue(ShadowMult);
                if (this.effect.Parameters["ShadowBias"] != null)
                    this.effect.Parameters["ShadowBias"].SetValue(ShadowBias);
                if (this.effect.Parameters["NormalBias"] != null)
                    this.effect.Parameters["NormalBias"].SetValue(this.NormalBias);
            
            

                effect.Parameters["LightViewProj"].SetValue(CreateLightViewProjectionMatrix());


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

   //     public Texture2D WaterTexture;

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

            
     //       effect.Parameters["waterTexWidth"].SetValue( (float)LODTerrain.LODTerrain.mapCellScale / (float)WaterTexture.Width);
     //       effect.Parameters["waterTexHeight"].SetValue((float)LODTerrain.LODTerrain.mapCellScale / (float)WaterTexture.Height);
     //       effect.Parameters["waterTexture"].SetValue(WaterTexture);
         //   effect.Parameters["xCamPos"].SetValue(cameraPosition);

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
            this.effect.Parameters["xDiffuseColor"].SetValue(SunLight.Color.ToVector3());
            this.effect.Parameters["LightDirection"].SetValue(SunLight.GetInvertedLightDirection());
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
                    currentEffect.Parameters["xDiffuseColor"].SetValue(SunLight.Color.ToVector3());
                    currentEffect.Parameters["LightDirection"].SetValue(SunLight.GetInvertedLightDirection());

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
            this.effect.Parameters["xOvercast"].SetValue(0.8f);
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





        public void DrawSun(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            Matrix worldMatrix =
                    Matrix.CreateScale(50.0f) * Matrix.CreateTranslation(SunLight.ShadowLightPosition);

            sun.Meshes[0].MeshParts[0].Effect.Parameters["World"].SetValue(worldMatrix);
            sun.Meshes[0].MeshParts[0].Effect.Parameters["View"].SetValue(currentViewMatrix);
            sun.Meshes[0].MeshParts[0].Effect.Parameters["Projection"].SetValue(projectionMatrix);

            sun.Meshes[0].MeshParts[0].Effect.CurrentTechnique = sun.Meshes[0].MeshParts[0].Effect.Techniques["flatshaded"];


            graphicsDevice.BlendState = BlendState.Opaque;
            //graphicsDevice.DepthStencilState = DepthStencilState .None;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;


            sun.Meshes[0].MeshParts[0].Effect.CurrentTechnique.Passes[0].Apply();
            foreach (EffectPass pass in sun.Meshes[0].MeshParts[0].Effect.CurrentTechnique.Passes)
            {
                sun.Meshes[0].Draw();
            }

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void DrawNormal(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition, Vector3 Position, Matrix Normal)
        {
            int Size = 10;
/*
         //   Matrix worldMatrix = Matrix.Identity;
            //Matrix worldMatrix = Matrix.CreateWorld(Position, Normal, Vector3.Up);
            //Matrix worldMatrix = Matrix.CreateWorld()


            //D3DXVECTOR3 toCam = camPos - spherePos;       == normal
            //D3DXVECTOR3 fwdVector;
            // D3DXVec3Normalize( &fwdVector, &toCam );
            Vector3 fwdVector = new Vector3(Normal.X, Normal.Y, Normal.Z);
            fwdVector.Normalize();

            // D3DXVECTOR3 upVector( 0.0f, 1.0f, 0.0f );
            Vector3 upVector = new Vector3(0.0f, 1.0f, 0.0f);

            // D3DXVECTOR3 sideVector;
            Vector3 sideVector;

            // D3DXVec3CrossProduct( &sideVector, &upVector, &fwdVector );
             sideVector = Vector3.Cross(upVector, fwdVector);

             // D3DXVec3CrossProduct( &upVector, &sideVector, &fwdVector );
             upVector = Vector3.Cross(sideVector, fwdVector);

             // D3DXVec3Normalize( &upVector, &toCam );
             upVector.Normalize();

             // D3DXVec3Normalize( &sideVector, &toCam );
             sideVector.Normalize();

            /*
             D3DXMATRIX orientation( sideVector.x, sideVector.y, sideVector.z, 0.0f,
                         upVector.x,   upVector.y,   upVector.z,   0.0f,
                         fwdVector.x,  fwdVector.y,  fwdVector.z,  0.0f,
                         spherePos.x,  spherePos.y,  spherePos.z,  1.0f );
            /

           //  Matrix worldMatrix = Matrix.CreateWorld(Position, fwdVector, upVector);
             Matrix wm = new Matrix(sideVector.X, sideVector.Y, sideVector.Z, 0.0f,
                                                upVector.X, upVector.Y, upVector.Z, 0.0f,
                                                fwdVector.X, fwdVector.Y, fwdVector.Z, 0.0f,
                                                Position.X, Position.Y, Position.Z, 1.0f);
                                                //Position.X, Position.Y, Position.Z, 1.0f);

             Matrix worldMatrix = Matrix.CreateWorld(Position, fwdVector, upVector);
            */


            Matrix worldMatrix = Normal * Matrix.CreateTranslation(Position);




            graphicsDevice.BlendState = BlendState.Opaque;
            //graphicsDevice.DepthStencilState = DepthStencilState .None;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;


            VertexPositionColored[] _vertices = new VertexPositionColored[36];

            Position = Vector3.Zero;

            // Calculate the position of the vertices on the top face.

            Vector3 topLeftFront = Position + new Vector3(-0.1f, 2.0f, -0.1f) * Size;

            Vector3 topLeftBack = Position + new Vector3(-0.1f, 2.0f, 0.1f) * Size;

            Vector3 topRightFront = Position + new Vector3(0.1f, 2.0f, -0.1f) * Size;

            Vector3 topRightBack = Position + new Vector3(0.1f, 2.0f, 0.1f) * Size;



            // Calculate the position of the vertices on the bottom face.

            Vector3 btmLeftFront = Position + new Vector3(-1.0f, 0.0f, -1.0f) * Size;

            Vector3 btmLeftBack = Position + new Vector3(-1.0f, 0.0f, 1.0f) * Size;

            Vector3 btmRightFront = Position + new Vector3(1.0f, 0.0f, -1.0f) * Size;

            Vector3 btmRightBack = Position + new Vector3(1.0f, 0.0f, 1.0f) * Size;






            // Add the vertices for the FRONT face.

            _vertices[0] = new VertexPositionColored(topLeftFront, Color.Blue);

            _vertices[1] = new VertexPositionColored(btmLeftFront, Color.Blue);

            _vertices[2] = new VertexPositionColored(topRightFront, Color.Blue);

            _vertices[3] = new VertexPositionColored(btmLeftFront, Color.Blue);

            _vertices[4] = new VertexPositionColored(btmRightFront, Color.Blue);

            _vertices[5] = new VertexPositionColored(topRightFront, Color.Blue);



            // Add the vertices for the BACK face.

            _vertices[6] = new VertexPositionColored(topLeftBack, Color.Blue);

            _vertices[7] = new VertexPositionColored(topRightBack, Color.Blue);

            _vertices[8] = new VertexPositionColored(btmLeftBack, Color.Blue);

            _vertices[9] = new VertexPositionColored(btmLeftBack, Color.Blue);

            _vertices[10] = new VertexPositionColored(topRightBack, Color.Blue);

            _vertices[11] = new VertexPositionColored(btmRightBack, Color.Blue);



            // Add the vertices for the TOP face.

            _vertices[12] = new VertexPositionColored(topLeftFront, Color.Red);

            _vertices[13] = new VertexPositionColored(topRightBack, Color.Red);

            _vertices[14] = new VertexPositionColored(topLeftBack, Color.Red);

            _vertices[15] = new VertexPositionColored(topLeftFront, Color.Red);

            _vertices[16] = new VertexPositionColored(topRightFront, Color.Red);

            _vertices[17] = new VertexPositionColored(topRightBack, Color.Red);



            // Add the vertices for the BOTTOM face. 

            _vertices[18] = new VertexPositionColored(btmLeftFront, Color.Blue);

            _vertices[19] = new VertexPositionColored(btmLeftBack, Color.Blue);

            _vertices[20] = new VertexPositionColored(btmRightBack, Color.Blue);

            _vertices[21] = new VertexPositionColored(btmLeftFront, Color.Blue);

            _vertices[22] = new VertexPositionColored(btmRightBack, Color.Blue);

            _vertices[23] = new VertexPositionColored(btmRightFront, Color.Blue);



            // Add the vertices for the LEFT face.

            _vertices[24] = new VertexPositionColored(topLeftFront, Color.Blue);

            _vertices[25] = new VertexPositionColored(btmLeftBack, Color.Blue);

            _vertices[26] = new VertexPositionColored(btmLeftFront, Color.Blue);

            _vertices[27] = new VertexPositionColored(topLeftBack, Color.Blue);

            _vertices[28] = new VertexPositionColored(btmLeftBack, Color.Blue);

            _vertices[29] = new VertexPositionColored(topLeftFront, Color.Blue);



            // Add the vertices for the RIGHT face. 

            _vertices[30] = new VertexPositionColored(topRightFront, Color.Blue);

            _vertices[31] = new VertexPositionColored(btmRightFront, Color.Blue);

            _vertices[32] = new VertexPositionColored(btmRightBack, Color.Blue);

            _vertices[33] = new VertexPositionColored(topRightBack, Color.Blue);

            _vertices[34] = new VertexPositionColored(topRightFront, Color.Blue);

            _vertices[35] = new VertexPositionColored(btmRightBack, Color.Blue);









            this.effect.CurrentTechnique = this.effect.Techniques["flatshaded"];
            this.effect.Parameters["World"].SetValue(worldMatrix);
            this.effect.Parameters["View"].SetValue(currentViewMatrix);
            this.effect.Parameters["Projection"].SetValue(projectionMatrix);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, 12, VertexPositionColor.VertexDeclaration);
            }

          

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
