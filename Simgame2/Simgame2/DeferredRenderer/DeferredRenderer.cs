using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;



namespace Simgame2.DeferredRenderer
{
    public class DeferredRenderer
    {
        //Clear Shader
        Effect Clear;
        //GBuffer Shader
        Effect GBuffer;
        //Directional Light Shader
        Effect directionalLight;
        //Point Light Shader
        Effect pointLight;
        //Spot Light Shader
        Effect spotLight;
        //Composition Shader
        Effect compose;
        //LightMap BlendState
        BlendState LightMapBS;
        //GBuffer Targets
        RenderTargetBinding[] GBufferTargets;
        //GBuffer Texture Size
        Vector2 GBufferTextureSize;
        //Light Map Target
        RenderTarget2D LightMap;
        //Fullscreen Quad
        FullscreenQuad fsq;
        //Point Light Geometry
        Model pointLightGeometry;
        //Spot Light Geometry
        Model spotLightGeometry;



        // modifications
        public VertexBuffer terrainVertexBuffer;
        public IndexBuffer terrainIndexBuffer;
        public Texture2D[] Textures;



        //Get GBuffer
        public RenderTargetBinding[] getGBuffer() { return GBufferTargets; }

        //Constructor
        public DeferredRenderer(GraphicsDevice GraphicsDevice, ContentManager Content, int Width, int Height)
        {
            //Load Clear Shader
            Clear = Content.Load<Effect>("Deferred/Clear");
            Clear.CurrentTechnique = Clear.Techniques[0];

            //Load GBuffer Shader
            GBuffer = Content.Load<Effect>("Deferred/GBuffer");
            GBuffer.CurrentTechnique = GBuffer.Techniques[0];

            //Load Directional Light Shader
            directionalLight = Content.Load<Effect>("Deferred/DirectionalLight");
            directionalLight.CurrentTechnique = directionalLight.Techniques[0];

            //Load Point Light Shader
            pointLight = Content.Load<Effect>("Deferred/PointLight");
            pointLight.CurrentTechnique = pointLight.Techniques[0];

            //Load Spot Light Shader
            spotLight = Content.Load<Effect>("Deferred/SpotLight");
            spotLight.CurrentTechnique = spotLight.Techniques[0];

            //Load Composition Shader
            compose = Content.Load<Effect>("Deferred/Composition");
            compose.CurrentTechnique = compose.Techniques[0];

            //Create LightMap BlendState
            LightMapBS = new BlendState();
            LightMapBS.ColorSourceBlend = Blend.One;
            LightMapBS.ColorDestinationBlend = Blend.One;
            LightMapBS.ColorBlendFunction = BlendFunction.Add;
            LightMapBS.AlphaSourceBlend = Blend.One;
            LightMapBS.AlphaDestinationBlend = Blend.One;
            LightMapBS.AlphaBlendFunction = BlendFunction.Add;

            //Set GBuffer Texture Size
            GBufferTextureSize = new Vector2(Width, Height);

            //Initialize GBuffer Targets Array
            GBufferTargets = new RenderTargetBinding[3];

            //Intialize Each Target of the GBuffer
            GBufferTargets[0] = new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, Width, Height, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8));
            GBufferTargets[1] = new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, Width, Height, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8));
            GBufferTargets[2] = new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, Width, Height, false, SurfaceFormat.Vector2, DepthFormat.Depth24Stencil8));

            //Initialize LightMap
            LightMap = new RenderTarget2D(GraphicsDevice, Width, Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            //Create Fullscreen Quad
            fsq = new FullscreenQuad(GraphicsDevice);


            //Load Point Light Geometry
//            pointLightGeometry = Content.Load<Model>("PointLightGeometry");   // quick fix
            pointLightGeometry = Content.Load<Model>("PPLightMesh");


            // PPLightMesh
            //Load Spot Light Geometry
//            spotLightGeometry = Content.Load<Model>("SpotLightGeometry");     // quick fix
            spotLightGeometry = Content.Load<Model>("PPLightMesh");

        }


        //GBuffer Creation
        void MakeGBuffer(GraphicsDevice GraphicsDevice, List<Model> Models, Camera Camera)
        {
            //Set Depth State
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Set up global GBuffer parameters
            GBuffer.Parameters["View"].SetValue(Camera.viewMatrix);
            GBuffer.Parameters["Projection"].SetValue(Camera.projectionMatrix);

            //Draw Each Model
            foreach (Model model in Models)
            {
                //Get Transforms
                Matrix[] transforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(transforms);

                //Draw Each ModelMesh
                foreach (ModelMesh mesh in model.Meshes)
                {
                    //Draw Each ModelMeshPart
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        //Set Vertex Buffer
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);

                        //Set Index Buffer
                        GraphicsDevice.Indices = part.IndexBuffer;

                        //Set World
                        GBuffer.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);

                        //Set WorldIT GBuffer.Parameters["WorldViewIT"].SetValue(Matrix.Transpose(Matrix.Invert(transforms[mesh.ParentBone.Index] * Camera.View)));
                        //Set Albedo Texture
                        GBuffer.Parameters["Texture"].SetValue(part.Effect.Parameters["Texture"].GetValueTexture2D());

                        //Set Normal Texture GBuffer.Parameters["NormalMap"].SetValue(part.Effect.Parameters["NormalMap"].GetValueTexture2D());
                        //Set Specular Texture GBuffer.Parameters["SpecularMap"].SetValue(part.Effect.Parameters["SpecularMap"].GetValueTexture2D());
                        //Apply Effect
                        GBuffer.CurrentTechnique.Passes[0].Apply();

                        //Draw
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
            //Set RenderTargets off
            GraphicsDevice.SetRenderTargets(null);
        }


        //GBuffer Creation
        void MakeTerrainGBuffer(GraphicsDevice GraphicsDevice, Matrix currentViewMatrix, Matrix projectionMatrix)
        {
            Matrix worldMatrix = Matrix.Identity;
            //Set Depth State
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Set up global GBuffer parameters
            GBuffer.Parameters["View"].SetValue(currentViewMatrix);
            GBuffer.Parameters["Projection"].SetValue(projectionMatrix);

            // draw terrain
            //Set Vertex Buffer
            GraphicsDevice.SetVertexBuffer(this.terrainVertexBuffer);

            //Set Index Buffer
            GraphicsDevice.Indices = this.terrainIndexBuffer;

            GBuffer.Parameters["World"].SetValue(worldMatrix);

            //Set WorldIT 
            GBuffer.Parameters["WorldViewIT"].SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix * currentViewMatrix)));
            //GBuffer.Parameters["WorldViewIT"].SetValue(Matrix.Transpose(Matrix.Invert(transforms[mesh.ParentBone.Index] * Camera.View)));
            //Set Albedo Texture
            GBuffer.Parameters["Texture"].SetValue(Textures[0]);

            //Set Normal Texture GBuffer.Parameters["NormalMap"].SetValue(part.Effect.Parameters["NormalMap"].GetValueTexture2D());
            //Set Specular Texture GBuffer.Parameters["SpecularMap"].SetValue(part.Effect.Parameters["SpecularMap"].GetValueTexture2D());
            //Apply Effect
            GBuffer.CurrentTechnique.Passes[0].Apply();

            //Draw
            int noVertices = this.terrainVertexBuffer.VertexCount;
            int noTriangles = this.terrainIndexBuffer.IndexCount / 3;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noVertices, 0, noTriangles);


            //Set RenderTargets off
            GraphicsDevice.SetRenderTargets(null);
        }


        //Clear GBuffer
        void ClearGBuffer(GraphicsDevice GraphicsDevice)
        {
            //Set to ReadOnly depth for now...
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            //Set GBuffer Render Targets
            GraphicsDevice.SetRenderTargets(GBufferTargets);

            //Set Clear Effect
            Clear.CurrentTechnique.Passes[0].Apply();

            //Draw
            fsq.Draw(GraphicsDevice);
        }

        //Draw Scene Deferred
        public void Draw(GraphicsDevice GraphicsDevice, List<Model> Models, LightManager Lights, Camera Camera, RenderTarget2D Output)
        {
            //Set States
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            //Clear GBuffer
            ClearGBuffer(GraphicsDevice);

            //Make GBuffer
        //    MakeGBuffer(GraphicsDevice, Models, Camera);
            MakeTerrainGBuffer(GraphicsDevice, Camera.viewMatrix, Camera.projectionMatrix);

            //Make LightMap
            MakeLightMap(GraphicsDevice, Lights, Camera);

            //Make Final Rendered Scene
            MakeFinal(GraphicsDevice, Output);
        }







        //Debug
        public void Debug(GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch)
        {
            //Begin SpriteBatch
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);

            //Width + Height
            int width = 128;
            int height = 128;

            //Set up Drawing Rectangle
            Rectangle rect = new Rectangle(0, 0, width, height);

            //Draw GBuffer 0
            spriteBatch.Draw((Texture2D)GBufferTargets[0].RenderTarget, rect, Color.White);

            //Draw GBuffer 1
            rect.X += width;
            spriteBatch.Draw((Texture2D)GBufferTargets[1].RenderTarget, rect, Color.White);

            //Draw GBuffer 2
            rect.X += width;
            spriteBatch.Draw((Texture2D)GBufferTargets[2].RenderTarget, rect, Color.White);

            //End SpriteBatch
            spriteBatch.End();
        }



        //Light Map Creation
        void MakeLightMap(GraphicsDevice GraphicsDevice, LightManager Lights, Camera Camera)
        {
            //Set LightMap Target
            GraphicsDevice.SetRenderTarget(LightMap);

            //Clear to Transperant Black
            GraphicsDevice.Clear(Color.Transparent);

            //Set States
            GraphicsDevice.BlendState = LightMapBS;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            #region Set Global Samplers

            //GBuffer 1 Sampler
            GraphicsDevice.Textures[0] = GBufferTargets[0].RenderTarget;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            //GBuffer 2 Sampler
            GraphicsDevice.Textures[1] = GBufferTargets[1].RenderTarget;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            //GBuffer 3 Sampler
            GraphicsDevice.Textures[2] = GBufferTargets[2].RenderTarget;
            GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;

            //SpotLight Cookie Sampler
            GraphicsDevice.SamplerStates[3] = SamplerState.LinearClamp;

            //ShadowMap Sampler
            GraphicsDevice.SamplerStates[4] = SamplerState.PointClamp;

            #endregion

            //Calculate InverseView
            Matrix InverseView = Matrix.Invert(Camera.viewMatrix);

            //Calculate InverseViewProjection
            Matrix InverseViewProjection = Matrix.Invert(Camera.viewMatrix * Camera.projectionMatrix);

            //Set Directional Lights Globals
            directionalLight.Parameters["InverseViewProjection"].SetValue(InverseViewProjection);
            directionalLight.Parameters["inverseView"].SetValue(InverseView);
            directionalLight.Parameters["CameraPosition"].SetValue(Camera.GetCameraPostion());
            directionalLight.Parameters["GBufferTextureSize"].SetValue(GBufferTextureSize);

            //Set the Directional Lights Geometry Buffers
            fsq.ReadyBuffers(GraphicsDevice);

            //Draw Directional Lights
            foreach (DirectionalLight light in Lights.getDirectionalLights())
            {
                //Set Directional Light Parameters
                directionalLight.Parameters["L"].SetValue(Vector3.Normalize(light.getDirection()));
                directionalLight.Parameters["LightColor"].SetValue(light.getColor());
                directionalLight.Parameters["LightIntensity"].SetValue(light.getIntensity());

                //Apply
                directionalLight.CurrentTechnique.Passes[0].Apply();

                //Draw
                fsq.JustDraw(GraphicsDevice);
            }

            //Set Spot Lights Globals
            spotLight.Parameters["View"].SetValue(Camera.viewMatrix);
            spotLight.Parameters["inverseView"].SetValue(InverseView);
            spotLight.Parameters["Projection"].SetValue(Camera.projectionMatrix);
            spotLight.Parameters["InverseViewProjection"].SetValue(InverseViewProjection);
            spotLight.Parameters["CameraPosition"].SetValue(Camera.GetCameraPostion());
            spotLight.Parameters["GBufferTextureSize"].SetValue(GBufferTextureSize);

            //Set Spot Lights Geometry Buffers
            GraphicsDevice.SetVertexBuffer(spotLightGeometry.Meshes[0].MeshParts[0].VertexBuffer, spotLightGeometry.Meshes[0].MeshParts[0].VertexOffset);
            GraphicsDevice.Indices = spotLightGeometry.Meshes[0].MeshParts[0].IndexBuffer;

            //Draw Spot Lights
            foreach (SpotLight light in Lights.getSpotLights())
            {
                //Set Attenuation Cookie Texture and SamplerState
                GraphicsDevice.Textures[3] = light.getAttenuationTexture();

                //Set ShadowMap and SamplerState
                GraphicsDevice.Textures[4] = light.getShadowMap();

                //Set Spot Light Parameters
                spotLight.Parameters["World"].SetValue(light.getWorld());
                spotLight.Parameters["LightViewProjection"].SetValue(light.getView() * light.getProjection());
                spotLight.Parameters["LightPosition"].SetValue(light.getPosition());
                spotLight.Parameters["LightColor"].SetValue(light.getColor());
                spotLight.Parameters["LightIntensity"].SetValue(light.getIntensity());
                spotLight.Parameters["S"].SetValue(light.getDirection());
                spotLight.Parameters["LightAngleCos"].SetValue(light.LightAngleCos());
                spotLight.Parameters["LightHeight"].SetValue(light.getFarPlane());
                spotLight.Parameters["Shadows"].SetValue(light.getIsWithShadows());
                spotLight.Parameters["shadowMapSize"].SetValue(light.getShadowMapResoloution());
                spotLight.Parameters["DepthPrecision"].SetValue(light.getFarPlane());
                spotLight.Parameters["DepthBias"].SetValue(light.getDepthBias());

                #region Set Cull Mode

                //Calculate L
                Vector3 L = Camera.GetCameraPostion() - light.getPosition();

                //Calculate S.L
                float SL = Math.Abs(Vector3.Dot(L, light.getDirection()));

                //Check if SL is within the LightAngle, if so then draw the BackFaces, if not //then draw the FrontFaces
                if (SL < light.LightAngleCos())
                    GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                else
                    GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

                #endregion

                //Apply
                spotLight.CurrentTechnique.Passes[0].Apply();

                //Draw
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                spotLightGeometry.Meshes[0].MeshParts[0].NumVertices,
                spotLightGeometry.Meshes[0].MeshParts[0].StartIndex,
                spotLightGeometry.Meshes[0].MeshParts[0].PrimitiveCount);
            }


            //Set Point Lights Geometry Buffers
            GraphicsDevice.SetVertexBuffer(pointLightGeometry.Meshes[0].MeshParts[0].VertexBuffer, pointLightGeometry.Meshes[0].MeshParts[0].VertexOffset);
            GraphicsDevice.Indices = pointLightGeometry.Meshes[0].MeshParts[0].IndexBuffer;

            //Set Point Lights Globals
            pointLight.Parameters["inverseView"].SetValue(InverseView);
            pointLight.Parameters["View"].SetValue(Camera.viewMatrix);
            pointLight.Parameters["Projection"].SetValue(Camera.projectionMatrix);
            pointLight.Parameters["InverseViewProjection"].SetValue(InverseViewProjection);
            pointLight.Parameters["CameraPosition"].SetValue(Camera.GetCameraPostion());
            pointLight.Parameters["GBufferTextureSize"].SetValue(GBufferTextureSize);

            //Draw Point Lights without Shadows
            foreach (PointLight light in Lights.getPointLights())
            {
                //Set Point Light Sampler
                GraphicsDevice.Textures[4] = light.getShadowMap();
                GraphicsDevice.SamplerStates[4] = SamplerState.PointWrap;

                //Set Point Light Parameters
                pointLight.Parameters["World"].SetValue(light.World());
                pointLight.Parameters["LightPosition"].SetValue(light.getPosition());
                pointLight.Parameters["LightRadius"].SetValue(light.getRadius());
                pointLight.Parameters["LightColor"].SetValue(light.getColor());
                pointLight.Parameters["LightIntensity"].SetValue(light.getIntensity()); ;
                pointLight.Parameters["Shadows"].SetValue(light.getIsWithShadows());
                pointLight.Parameters["DepthPrecision"].SetValue(light.getRadius());
                pointLight.Parameters["DepthBias"].SetValue(light.getDepthBias());
                pointLight.Parameters["shadowMapSize"].SetValue(light.getShadowMapResoloution());

                //Set Cull Mode
                Vector3 diff = Camera.GetCameraPostion() - light.getPosition();
                float CameraToLight = (float)Math.Sqrt((float)Vector3.Dot(diff, diff)) / 100.0f;

                //If the Camera is in the light, render the backfaces, if it's out of the //light, render the frontfaces
                if (CameraToLight <= light.getRadius())
                    GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
                else if (CameraToLight > light.getRadius())
                    GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                //Apply
                pointLight.CurrentTechnique.Passes[0].Apply();

                //Draw
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                pointLightGeometry.Meshes[0].MeshParts[0].NumVertices,
                pointLightGeometry.Meshes[0].MeshParts[0].StartIndex,
                pointLightGeometry.Meshes[0].MeshParts[0].PrimitiveCount);
            }



            //Set States Off
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }




        //Composition
        void MakeFinal(GraphicsDevice GraphicsDevice, RenderTarget2D Output)
        {
            //Set Composition Target
            GraphicsDevice.SetRenderTarget(Output);

            //Clear
            GraphicsDevice.Clear(Color.Transparent);

            //Set Textures
            GraphicsDevice.Textures[0] = GBufferTargets[0].RenderTarget;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            GraphicsDevice.Textures[1] = LightMap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            //Set Effect Parameters
            compose.Parameters["GBufferTextureSize"].SetValue(GBufferTextureSize);

            //Apply
            compose.CurrentTechnique.Passes[0].Apply();

            //Draw
            fsq.Draw(GraphicsDevice);
        }




    }
}
