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

namespace Simgame2.Buildings
{
    public class BasicMine: EntityBuilding
    {
        public BasicMine(Game game)
            : base(game)
        {
            this.Type = EntityTypes.BASIC_MINE;
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {

            this.AddTexture("BasicMineTex");
            this.scale = scale;
            this.rotation = rotation;
            this.location = location;
            this.LoadModel("Models/BasicMine");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (HasMouseFocus)
            {
                UpdateStatusScreen();
            }
        }

 
        ResourceCell resourceCell;

        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            base.Draw(currentViewMatrix, cameraPosition);
            if (this.HasMouseFocus && !HideBillboard)
            {
                statusBillboard.Draw(this.playerCamera);
            }
        }

        SpriteBatch spriteBatch;
        public void UpdateStatusScreen()
        {
            if (this.resourceCell == null)
            {
                resourceCell = this.getResourceCell(location.X, -location.Z);
            }
            // TODO modifyvto allow custom string and images.
            if (statusBillboard.BillboardBackGroundTexture != null)
            {
               // float electicAvail = GetSimEntity().GetAvailableOutResource(Simulation.SimulationEnity.Resource.ORE);
               // float electricMax = GetSimEntity().GetMaxResourceAmount(Simulation.SimulationEnity.Resource.ORE);

                spriteBatch = new SpriteBatch(this.device);


                RenderTarget2D target = new RenderTarget2D(this.device, 400, 600);
                this.device.SetRenderTarget(target);// Now the spriteBatch will render to the RenderTarget2D

                this.device.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(statusBillboard.BillboardBackGroundTexture, new Rectangle(0, 0, 400, 600), Color.White);

            //    StringBuilder sb = new StringBuilder();
             //   sb.Append("Ore: ");
              //  sb.Append(electicAvail.ToString("0.0"));
               // sb.Append("/");
                //sb.Append(electricMax);

                spriteBatch.DrawString(this.font, GetSimEntity().ToString(), new Vector2(20, 20), Color.Black);//Do your stuff here

                spriteBatch.End();

                this.device.SetRenderTarget(null);//This will set the spriteBatch to render to the screen again.

                this.statusBillboard.SetTexture((Texture2D)target);

            }

        }

        public override Simulation.SimulationBuildingEnity GetSimEntity()
        {
            if (basicMineSim == null)
            {
                if (this.resourceCell == null)
                {
                    resourceCell = this.getResourceCell(location.X, location.Z);
                }
                basicMineSim = new BasicMineSim(this);
                
            }

            return basicMineSim;
        }


        public static Vector3 StandardScale = new Vector3(5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);

        private BasicMineSim basicMineSim;


        private class BasicMineSim : Simulation.SimulationBuildingEnity
        {
            private BasicMine mineParent;

            public BasicMineSim(BasicMine mineParent)
            {
                this.mineParent = mineParent;
                ProduceRate[(int)Resource.OreAluminium] = this.mineParent.resourceCell.Aluminium;
                ResourceMaxAmount[(int)Resource.OreAluminium] = 1000;

                ProduceRate[(int)Resource.OreCopper] = this.mineParent.resourceCell.Copper;
                ResourceMaxAmount[(int)Resource.OreCopper] = 1000;

                ProduceRate[(int)Resource.OreGold] = this.mineParent.resourceCell.Gold;
                ResourceMaxAmount[(int)Resource.OreGold] = 1000;

                ProduceRate[(int)Resource.OreIron] = this.mineParent.resourceCell.Iron;
                ResourceMaxAmount[(int)Resource.OreIron] = 1000;

                ProduceRate[(int)Resource.OreLead] = this.mineParent.resourceCell.Lead;
                ResourceMaxAmount[(int)Resource.OreLead] = 1000;

                ProduceRate[(int)Resource.OreLithium] = this.mineParent.resourceCell.Lithium;
                ResourceMaxAmount[(int)Resource.OreLithium] = 1000;

                ProduceRate[(int)Resource.OreNickel] = this.mineParent.resourceCell.Nickel;
                ResourceMaxAmount[(int)Resource.OreNickel] = 1000;

                ProduceRate[(int)Resource.OrePlatinum] = this.mineParent.resourceCell.Platinum;
                ResourceMaxAmount[(int)Resource.OrePlatinum] = 1000;

                ProduceRate[(int)Resource.OreSilver] = this.mineParent.resourceCell.Silver;
                ResourceMaxAmount[(int)Resource.OreSilver] = 1000;

                ProduceRate[(int)Resource.OreTitanium] = this.mineParent.resourceCell.Titanium;
                ResourceMaxAmount[(int)Resource.OreTitanium] = 1000;

                ProduceRate[(int)Resource.OreTungsten] = this.mineParent.resourceCell.Tungsten;
                ResourceMaxAmount[(int)Resource.OreTungsten] = 1000;

                ProduceRate[(int)Resource.OreUranium] = this.mineParent.resourceCell.Uranium;
                ResourceMaxAmount[(int)Resource.OreUranium] = 1000;
            }


            public override void Update(GameTime gameTime)
            {
                float timeDelta = gameTime.ElapsedGameTime.Milliseconds;
                for (int i = 0; i < ResourceCount; i++)
                {
                    ResourceOutput[i] = clamp(ResourceOutput[i] +
                        (ProduceRate[i]/100 * timeDelta), ResourceMaxAmount[i]);
                }
            }


            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                for (int i = 2; i < 14; i++)
                {
                    sb.Append((Resource)(i)); sb.Append(": "); sb.Append((int)ResourceOutput[i]);
                    sb.Append("/"); sb.Append(ResourceMaxAmount[i]); sb.Append(" @ ");
                    sb.Append(ProduceRate[i]);
                    sb.AppendLine();
                }


                return sb.ToString();
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
