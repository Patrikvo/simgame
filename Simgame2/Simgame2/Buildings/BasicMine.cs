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
           
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {
            
            this.AddTexture(this.Game.Content.Load<Texture2D>("BasicMineTex"));
            this.scale = scale;
            this.rotation = rotation;
            this.location = location;
            this.LoadModel("BasicMine", effect);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (HasMouseFocus)
            {
                UpdateStatusScreen();
            }
        }

        public void Place(WorldMap map, Vector3 location, bool flatten)
        {
            this.location = location;
            this.PlaceBuilding(map, flatten);
            
        }


        WorldMap.ResourceCell resourceCell;

        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            base.Draw(currentViewMatrix, cameraPosition);
            if (this.HasMouseFocus)
            {
                statusBillboard.Draw(this.game.PlayerCamera);
            }
        }

        SpriteBatch spriteBatch;
        public void UpdateStatusScreen()
        {
            if (this.resourceCell == null)
            {
                resourceCell = this.game.worldMap.GetResourceFromWorldCoor(location.X, -location.Z);
            }
            // TODO modifyvto allow custom string and images.
            if (statusBillboard.BillboardBackGroundTexture != null)
            {
                float electicAvail = GetSimEntity().GetAvailableOutResource(Simulation.SimulationEnity.Resource.ORE);
                float electricMax = GetSimEntity().GetMaxResourceAmount(Simulation.SimulationEnity.Resource.ORE);

                spriteBatch = new SpriteBatch(this.game.device);


                RenderTarget2D target = new RenderTarget2D(this.game.device, 400, 400);
                this.game.device.SetRenderTarget(target);// Now the spriteBatch will render to the RenderTarget2D

                this.game.device.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(statusBillboard.BillboardBackGroundTexture, new Rectangle(0, 0, 400, 400), Color.White);

                StringBuilder sb = new StringBuilder();
                sb.Append("Ore: ");
                sb.Append(electicAvail.ToString("0.0"));
                sb.Append("/");
                sb.Append(electricMax);

                spriteBatch.DrawString(this.game.font, resourceCell.ToString(), new Vector2(20, 20), Color.Black);//Do your stuff here

                spriteBatch.End();

                this.game.device.SetRenderTarget(null);//This will set the spriteBatch to render to the screen again.

                this.statusBillboard.SetTexture((Texture2D)target);

            }

        }

        public override Simulation.SimulationEnity GetSimEntity()
        {
            if (basicMineSim == null)
            {
                basicMineSim = new BasicMineSim();
            }

            return basicMineSim;
        }

        public Effect effect { get; set; }

        public static Vector3 StandardScale = new Vector3(5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);

        private BasicMineSim basicMineSim;


        private class BasicMineSim : Simulation.SimulationEnity
        {
            public BasicMineSim()
            {
                ProduceRate[(int)Resource.ORE] = 0.01f;
                ResourceMaxAmount[(int)Resource.ORE] = 1000;
            }


            public override void Update(GameTime gameTime)
            {
                float timeDelta = gameTime.ElapsedGameTime.Milliseconds;
                for (int i = 0; i < ResourceCount; i++)
                {
                    ResourceOutput[(int)Resource.ORE] = clamp(ResourceOutput[(int)Resource.ORE] +
                        (ProduceRate[(int)Resource.ORE] * timeDelta), ResourceMaxAmount[(int)Resource.ORE]);
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
