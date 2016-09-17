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
    public class Melter:  EntityBuilding
    {

        public Melter(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.Type = EntityTypes.MELTER;
            this.CurrentState = States.UNDER_CONSTRUCTION;
        }

        public override bool OnEvent(Simulation.Event ReceivedEvent)
        {
            base.OnEvent(ReceivedEvent);
            // TODO IMPLEMENT
            /*
            if (ReceivedEvent is Simulation.Events.BuildingContructionDoneEvent)
            {
                Simulation.Events.BuildingContructionDoneEvent e = (Simulation.Events.BuildingContructionDoneEvent)ReceivedEvent;

                Console.WriteLine("Saw New Building");

                return true;
            }
            */

            return false;
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {

            this.AddTexture("BasicMelterTex");  // base texture

            this.scale = scale;
            this.Rotation = rotation;
            this.location = location;
            this.LoadModel("Models/BasicMelter");

           // rotorTransform = this.model.Bones["Rotor"].Transform;  // Rotor start position
        }

        public override void Update(GameTime gameTime)
        {
          //  rotorRotation += (float)((rotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2 * Math.PI);

            base.Update(gameTime);

            if (HasMouseFocus)
            {
                UpdateStatusScreen();
            }

            if (!IsGhost)
            {
                GetSimEntity().Update(gameTime);
            }
        }


        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            base.Draw(currentViewMatrix, cameraPosition);

            /*
            
            if (ShowBoundingBox)
            {
                DrawBoundingBox(currentViewMatrix, cameraPosition);
            }
            */
            

            if (HasMouseFocus && ! HideBillboard)
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
                //float electicAvail = GetSimEntity().GetAvailableOutResource(Simulation.SimulationEnity.Resource.METAL);
               // float electricMax = GetSimEntity().GetMaxResourceAmount(Simulation.SimulationEnity.Resource.METAL);

                spriteBatch = new SpriteBatch(this.device);


                RenderTarget2D target = new RenderTarget2D(this.device, 400, 400);
                this.device.SetRenderTarget(target);// Now the spriteBatch will render to the RenderTarget2D

                this.device.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(statusBillboard.BillboardBackGroundTexture, new Rectangle(0, 0, 400, 400), Color.White);

                StringBuilder sb = new StringBuilder();
                sb.Append("Metal: ");
             //   sb.Append(electicAvail.ToString("0.0"));
                sb.Append("/");
            //    sb.Append(electricMax);
                sb.AppendLine();
                sb.Append(this.getAltitude());

                spriteBatch.DrawString(this.font, sb.ToString(), new Vector2(20, 20), Color.Black);//Do your stuff here

                spriteBatch.End();

                this.device.SetRenderTarget(null);//This will set the spriteBatch to render to the screen again.

                this.statusBillboard.SetTexture((Texture2D)target);

            }

        }


        public override Simulation.SimulationBuildingEnity GetSimEntity()
        {
            if (melterSim == null)
            {
                melterSim = new MelterSim(this);
            }

            return melterSim;
        }


    //    public Effect effect { get; set; }

        public static Vector3 StandardScale = new Vector3(10, 10, 10);    // (5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);



        private MelterSim melterSim;

        private class MelterSim : Simulation.SimulationBuildingEnity
        {
            private Melter parent;
            public MelterSim(Melter Parent)
            {
                this.parent = Parent;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreAluminium] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalAluminium] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreCopper] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalCopper] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreGold] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalGold] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreIron] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalIron] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreLead] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalLead] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreLithium] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalLithium] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreNickel] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalNickel] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OrePlatinum] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalPlatinum] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreSilver] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalSilver] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreTitanium] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalTitanium] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreTungsten] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalTungsten] = 1;

                ResourceConsumptionPerSecond[(int)Simulation.ResourceStorage.Resource.OreUranium] = 1;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.MetalUranium] = 1;


            }


            public override void Update(GameTime gameTime)
            {
                float timeDelta = gameTime.ElapsedGameTime.Milliseconds;
                for (int i = Simulation.ResourceStorage.OreStart; i < Simulation.ResourceStorage.OreCount + Simulation.ResourceStorage.OreStart; i++)
                {
                    if (this.parent.RunningGameSession.AvailableResources.GetResourceAvailable((Simulation.ResourceStorage.Resource)i) >=
                    ResourceConsumptionPerSecond[i]) 
                    { 
                        // required resource is available

                        if (this.parent.RunningGameSession.AvailableResources.GetResourceAvailable((Simulation.ResourceStorage.Resource)(i+12))
                            < this.parent.RunningGameSession.AvailableResources.GetResourceMaxStorageAmount((Simulation.ResourceStorage.Resource)(i + 12)))
                        {
                            // sufficient space to store product
                            this.parent.RunningGameSession.AvailableResources.ExtractResourceAmount((Simulation.ResourceStorage.Resource)i, ResourceConsumptionPerSecond[i]);
                            this.parent.RunningGameSession.AvailableResources.AddResourceAmount((Simulation.ResourceStorage.Resource)(i + 12), ProduceRate[i + 12]);


                        }



                    }
                        
                        
                        
                        
                        //, (ProduceRate[i] / 100 * timeDelta));


         //           ResourceOutput[(int)Resource.METAL] = clamp(ResourceOutput[(int)Resource.METAL] +
          //              (ProduceRate[(int)Resource.METAL] * timeDelta), ResourceMaxAmount[(int)Resource.METAL]);
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
