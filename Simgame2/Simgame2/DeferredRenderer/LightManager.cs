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
    public class LightManager
    {
        //Depth Writing Shader
        Effect depthWriter;

        //Directional Lights
        List<DirectionalLight> directionalLights;

        //Spot Lights
        List<SpotLight> spotLights;

        //Point Lights
        List<PointLight> pointLights;

        #region Get Functions

        //Get Directional Lights
        public List<DirectionalLight> getDirectionalLights() { return directionalLights; }

        //Get Spot Lights
        public List<SpotLight> getSpotLights() { return spotLights; }

        //Get Point Lights
        public List<PointLight> getPointLights() { return pointLights; }

        #endregion

        //Constructor
        public LightManager(ContentManager Content)
        {
            //Initialize Directional Lights
            directionalLights = new List<DirectionalLight>();

            //Initialize Spot Lights
            spotLights = new List<SpotLight>();

            //Initialize Point Lights
            pointLights = new List<PointLight>();

            //Load the Depth Writing Shader
            depthWriter = Content.Load<Effect>("Deferred/DepthWriter");
            depthWriter.CurrentTechnique = depthWriter.Techniques[0];


        }

        //Add a Directional Light
        public void AddLight(DirectionalLight Light)
        {
            directionalLights.Add(Light);
        }

        //Add a Spot Light
        public void AddLight(SpotLight Light)
        {
            spotLights.Add(Light);
        }

        //Remove a Directional Light
        public void RemoveLight(DirectionalLight Light)
        {
            directionalLights.Remove(Light);
        }

        //Remove a Spot Light
        public void RemoveLight(SpotLight Light)
        {
            spotLights.Remove(Light);
        }


        //Add a Point Light
        public void AddLight(PointLight Light)
        {
            pointLights.Add(Light);
        }

        //Remove a Point Light
        public void RemoveLight(PointLight Light)
        {
            pointLights.Remove(Light);
        }


        //Draw Shadow Maps
        public void DrawShadowMaps(GraphicsDevice GraphicsDevice, List<Model> Models)
        {
            //Set States
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            //Foreach SpotLight with Shadows
            foreach (SpotLight Light in spotLights)
            {
                //Update it
                Light.Update();

                //Draw it's Shadow Map
                if (Light.getIsWithShadows()) DrawShadowMap(GraphicsDevice, Light, Models);
            }

            //Foreach PointLight with Shadows
            foreach (PointLight Light in pointLights)
            {
                //Draw it's Shadow Map
                if (Light.getIsWithShadows()) DrawShadowMap(GraphicsDevice, Light, Models);
            }



        }

        //Draw a Shadow Map for a Spot Light
        void DrawShadowMap(GraphicsDevice GraphicsDevice, SpotLight Light, List<Model> Models)
        {
            //Set Light's Target onto the Graphics Device
            GraphicsDevice.SetRenderTarget(Light.getShadowMap());

            //Clear Target
            GraphicsDevice.Clear(Color.Transparent);

            //Set global Effect parameters
            depthWriter.Parameters["View"].SetValue(Light.getView());
            depthWriter.Parameters["Projection"].SetValue(Light.getProjection());
            depthWriter.Parameters["LightPosition"].SetValue(Light.getPosition());
            depthWriter.Parameters["DepthPrecision"].SetValue(Light.getFarPlane());

            //Draw Models
            DrawModels(GraphicsDevice, Models);
        }

        //Draw Models
        void DrawModels(GraphicsDevice GraphicsDevice, List<Model> Models)
        {
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
                        depthWriter.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);

                        //Apply Effect
                        depthWriter.CurrentTechnique.Passes[0].Apply();

                        //Draw
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        }


        //Draw a Shadow Map for a Point Light
        void DrawShadowMap(GraphicsDevice GraphicsDevice, PointLight Light, List<Model> Models)
        {
            //Initialize View Matrices Array
            Matrix[] views = new Matrix[6];

            //Create View Matrices
            views[0] = Matrix.CreateLookAt(Light.getPosition(), Light.getPosition() + Vector3.Forward, Vector3.Up);
            views[1] = Matrix.CreateLookAt(Light.getPosition(), Light.getPosition() + Vector3.Backward, Vector3.Up);
            views[2] = Matrix.CreateLookAt(Light.getPosition(), Light.getPosition() + Vector3.Left, Vector3.Up);
            views[3] = Matrix.CreateLookAt(Light.getPosition(), Light.getPosition() + Vector3.Right, Vector3.Up);
            views[4] = Matrix.CreateLookAt(Light.getPosition(), Light.getPosition() + Vector3.Down, Vector3.Forward);
            views[5] = Matrix.CreateLookAt(Light.getPosition(), Light.getPosition() + Vector3.Up, Vector3.Backward);

            //Create Projection Matrix
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90.0f), 1.0f, 1.0f, Light.getRadius());

            //Set Global Effect Values
            depthWriter.Parameters["Projection"].SetValue(projection);
            depthWriter.Parameters["LightPosition"].SetValue(Light.getPosition());
            depthWriter.Parameters["DepthPrecision"].SetValue(Light.getRadius());

            #region Forward

            GraphicsDevice.SetRenderTarget(Light.getShadowMap(), CubeMapFace.PositiveZ);

            //Clear Target
            GraphicsDevice.Clear(Color.Transparent);

            //Set global Effect parameters
            depthWriter.Parameters["View"].SetValue(views[0]);

            //Draw Models
            DrawModels(GraphicsDevice, Models);

            #endregion

            #region Backward

            GraphicsDevice.SetRenderTarget(Light.getShadowMap(), CubeMapFace.NegativeZ);

            //Clear Target
            GraphicsDevice.Clear(Color.Transparent);

            //Set global Effect parameters
            depthWriter.Parameters["View"].SetValue(views[1]);

            //Draw Models
            DrawModels(GraphicsDevice, Models);

            #endregion

            #region Left

            GraphicsDevice.SetRenderTarget(Light.getShadowMap(), CubeMapFace.NegativeX);

            //Clear Target
            GraphicsDevice.Clear(Color.Transparent);

            //Set global Effect parameters
            depthWriter.Parameters["View"].SetValue(views[2]);

            //Draw Models
            DrawModels(GraphicsDevice, Models);

            #endregion

            #region Right

            GraphicsDevice.SetRenderTarget(Light.getShadowMap(), CubeMapFace.PositiveX);

            //Clear Target
            GraphicsDevice.Clear(Color.Transparent);

            //Set global Effect parameters
            depthWriter.Parameters["View"].SetValue(views[3]);

            //Draw Models
            DrawModels(GraphicsDevice, Models);

            #endregion

            #region Down

            GraphicsDevice.SetRenderTarget(Light.getShadowMap(), CubeMapFace.NegativeY);

            //Clear Target
            GraphicsDevice.Clear(Color.Transparent);

            //Set global Effect parameters
            depthWriter.Parameters["View"].SetValue(views[4]);

            //Draw Models
            DrawModels(GraphicsDevice, Models);

            #endregion

            #region Up

            GraphicsDevice.SetRenderTarget(Light.getShadowMap(), CubeMapFace.PositiveY);

            //Clear Target
            GraphicsDevice.Clear(Color.Transparent);

            //Set global Effect parameters
            depthWriter.Parameters["View"].SetValue(views[5]);

            //Draw Models
            DrawModels(GraphicsDevice, Models);

            #endregion

        }


    }
}
