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



// TODO create door opening animation


namespace Simgame2.Buildings
{
    public class Lander : EntityBuilding
    {

        public Lander(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.Type = EntityTypes.LANDER;
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {
            this.AddTexture("Ship");  // Ship texture
            this.AddTexture("ShipDoor");  // back door texture
          

            this.scale = scale;
            this.Rotation = rotation;
            this.location = location;

            this.LoadModel("Models/Lander");

//            doorTransform = this.model.Bones["Rotor"].Transform;  // Door start position
        }

        public override void Update(GameTime gameTime)
        {
//            rotorRotation += (float)((rotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2*Math.PI);

            if (HasMouseFocus)
            {
                UpdateStatusScreen();
            }
            base.Update(gameTime);
        }



        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
//            this.model.Bones["Rotor"].Transform = Matrix.CreateRotationZ(rotorRotation) * doorTransform; 
                
            Matrix worldMatrix = GetWorldMatrix();
            Matrix[] transforms = GetBoneTransforms();


            int meshNum = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                
                foreach (Effect currentEffect in mesh.Effects)
                {
                    
                    currentEffect.Parameters["View"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["Projection"].SetValue(projectionMatrix);
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];

                    currentEffect.Parameters["xTexture"].SetValue(this.texture[meshNum]);
                    currentEffect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * worldMatrix);

                    currentEffect.Parameters["xIsTransparant"].SetValue(this.IsTransparant);
                    if (CanPlace)
                    {
                        currentEffect.Parameters["xTransparantColor"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 0.5f));
                    }
                    else
                    {
                        currentEffect.Parameters["xTransparantColor"].SetValue(new Vector4(1.0f, 0.0f, 0.0f, 0.5f));
                    }

                    if (HasMouseFocus)
                    {
                        currentEffect.Parameters["xIsTransparant"].SetValue(true);
                        currentEffect.Parameters["xTransparantColor"].SetValue(new Vector4(1.0f, 1.0f, 0.0f, 0.7f));
                    }
                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xAmbient"].SetValue(this.RunningGameSession.LODMap.GetRenderer().AmbientLightLevel);
                    currentEffect.Parameters["LightDirection"].SetValue(this.RunningGameSession.LODMap.GetRenderer().SunLight.GetInvertedLightDirection());

                    currentEffect.Parameters["cameraPos"].SetValue(cameraPosition);
                    currentEffect.Parameters["FogColor"].SetValue(FOGCOLOR.ToVector4());
                    currentEffect.Parameters["FogNear"].SetValue(FOGNEAR);
                    currentEffect.Parameters["FogFar"].SetValue(FOGFAR);


                }
                mesh.Draw();
                meshNum++;
            }

            if (ShowBoundingBox)
            {
                DrawBoundingBox(currentViewMatrix, cameraPosition);
            }

            if (HasMouseFocus && !HideBillboard)
            {
                statusBillboard.Draw(this.playerCamera);
            }
        }


        SpriteBatch spriteBatch;
        public void UpdateStatusScreen()
        {
            // TODO modifyvto allow custom string and images.
            if (statusBillboard.BillboardBackGroundTexture != null)
            {
                float electicAvail = GetSimEntity().GetAvailableOutResource(Simulation.ResourceStorage.Resource.ELECTRICITY);
                float electricMax = GetSimEntity().GetMaxResourceAmount(Simulation.ResourceStorage.Resource.ELECTRICITY);

                spriteBatch = new SpriteBatch(this.device);


                RenderTarget2D target = new RenderTarget2D(this.device, 400, 400);
                this.device.SetRenderTarget(target);// Now the spriteBatch will render to the RenderTarget2D

                this.device.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(statusBillboard.BillboardBackGroundTexture, new Rectangle(0, 0, 400, 400), Color.White);

                StringBuilder sb = new StringBuilder();
                sb.Append("Power: ");
                sb.Append(electicAvail.ToString("0.0"));
                sb.Append("/");
                sb.Append(electricMax);

                spriteBatch.DrawString(this.font, sb.ToString(), new Vector2(20, 20), Color.Black);//Do your stuff here

                spriteBatch.End();

                this.device.SetRenderTarget(null);//This will set the spriteBatch to render to the screen again.

                this.statusBillboard.SetTexture((Texture2D)target);

            }

        }


        public override Simulation.SimulationBuildingEnity GetSimEntity()
        {
            if (landerSim == null)
            {
                landerSim = new LanderSim();
            }

            return landerSim;
        }


    //    public Effect effect { get; set; }

        public static Vector3 StandardScale = new Vector3(5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);

//        private float rotorRotation = 0;
//        private float rotationSpeed = 1;
//        private Matrix doorTransform;

        private LanderSim landerSim;

        private class LanderSim : Simulation.SimulationBuildingEnity
        {
            public LanderSim()
            {
                //                ProduceRate[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] = 0.01f;
                //                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] = 1000;
            }


            public override void Update(GameTime gameTime)
            {
                float timeDelta = gameTime.ElapsedGameTime.Milliseconds;
//                for (int i = 0; i < ResourceCount; i++)
 //               {
                //                   ResourceOutput[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] = clamp(Simulation.ResourceStorage.ResourceOutput[(int)Resource.ELECTRICITY] +
                //                       (ProduceRate[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] * timeDelta), ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.ELECTRICITY]);
 //               }
            }




            //  protected float[] ResouceAmount;
            //   protected float[] ResourceConsumptionPerSecond;
            //  protected float[] ResourceMaxAmount;

            //           protected Resource[] ConvertsTo;
            //          protected float[] ProduceRate;

            //        protected bool[] ConsumingResource;

            //        protected float[] ResourceOutput;

            //      public const int ResourceCount = 3;
            //    public enum Resource { NOTHING, ELECTRICITY, ORE, METAL };

        }

    }
}
