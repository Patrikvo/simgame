﻿using System;
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


namespace Simgame2.Buildings
{
    public class WindTower : EntityBuilding
    {
        public WindTower(Game game)
            : base(game)
        {
            this.Type = EntityTypes.WIND_TOWER;
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {
            this.AddTexture("WindTower_bTex");  // base texture
            this.AddTexture("WindTower_rTex");  // rotor texture
          

            this.scale = scale;
            this.rotation = rotation;
            this.location = location;

            this.LoadModel("WindTower");

            rotorTransform = this.model.Bones["Rotor"].Transform;  // Rotor start position
        }

        public override void Update(GameTime gameTime)
        {
            rotorRotation += (float)((rotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2*Math.PI);

            if (HasMouseFocus)
            {
                UpdateStatusScreen();
            }
            base.Update(gameTime);
        }



        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            this.model.Bones["Rotor"].Transform = Matrix.CreateRotationZ(rotorRotation) * rotorTransform; 
                
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

                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xAmbient"].SetValue(0.4f);
                    currentEffect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));

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
                float electicAvail = GetSimEntity().GetAvailableOutResource(Simulation.SimulationBuildingEnity.Resource.ELECTRICITY);
                float electricMax = GetSimEntity().GetMaxResourceAmount(Simulation.SimulationBuildingEnity.Resource.ELECTRICITY);

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
            if (windTowerSim == null)
            {
                windTowerSim = new WindTowerSim();
            }

            return windTowerSim;
        }


    //    public Effect effect { get; set; }

        public static Vector3 StandardScale = new Vector3(5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);

        private float rotorRotation = 0;
        private float rotationSpeed = 1;
        private Matrix rotorTransform;

        private WindTowerSim windTowerSim;

        private class WindTowerSim : Simulation.SimulationBuildingEnity
        {
            public WindTowerSim()
            {
                ProduceRate[(int)Resource.ELECTRICITY] = 0.01f;
                ResourceMaxAmount[(int)Resource.ELECTRICITY] = 1000;
            }


            public override void Update(GameTime gameTime)
            {
                float timeDelta = gameTime.ElapsedGameTime.Milliseconds;
                for (int i = 0; i < ResourceCount; i++)
                {
                    ResourceOutput[(int)Resource.ELECTRICITY] = clamp(ResourceOutput[(int)Resource.ELECTRICITY] +
                        (ProduceRate[(int)Resource.ELECTRICITY] * timeDelta), ResourceMaxAmount[(int)Resource.ELECTRICITY]);
                }
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
