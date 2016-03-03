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


namespace Simgame2.Simulation
{
    
    public class SimulationBuildingEnity : SimulationEntity
    {
        
        public SimulationBuildingEnity()
        {

            ResouceAmount = new float[ResourceStorage.ResourceCount];
            ResourceConsumptionPerSecond = new float[ResourceStorage.ResourceCount];
            ResourceMaxAmount = new float[ResourceStorage.ResourceCount];
            ConsumingResource = new bool[ResourceStorage.ResourceCount];
            ConvertsTo = new ResourceStorage.Resource[ResourceStorage.ResourceCount];
            ProduceRate = new float[ResourceStorage.ResourceCount];
            ResourceOutput = new float[ResourceStorage.ResourceCount];
        }



        public void AddResource(ResourceStorage.Resource res, float amount)
        {
            ResouceAmount[(int)res] = clamp( ResouceAmount[(int)res] + amount);
        }

        public float GetResource(ResourceStorage.Resource res)
        {
            return ResouceAmount[(int)res];
        }

        public float GetMaxResourceAmount(ResourceStorage.Resource res)
        {
            return ResourceMaxAmount[(int)res];
        }

        public float GetResourceConsumption(ResourceStorage.Resource res)
        {
            return ResourceConsumptionPerSecond[(int)res];
        }

        public float GetAvailableOutResource(ResourceStorage.Resource res)
        {
            return ResourceOutput[(int)res];
        }

        public float ExtractOutResource(ResourceStorage.Resource res, float amount)
        {
            float output = clamp(amount, ResourceOutput[(int)res]);
            ResourceOutput[(int)res] = clamp(ResourceOutput[(int)res] - output);
            return output;
        }


        public override void Update(GameTime gameTime)
        {
            float timeDelta = gameTime.ElapsedGameTime.Milliseconds / 1000;
            for (int i = 0; i < ResourceStorage.ResourceCount; i++)
            {
                if (ConsumingResource[i] && ResouceAmount[i] > 0) 
                {
                    ResouceAmount[i] = clamp( ResouceAmount[i] - (ResourceConsumptionPerSecond[i] * timeDelta));
                    ResourceOutput[(int)ConvertsTo[i]] = clamp(ResourceOutput[(int)ConvertsTo[i]] + ProduceRate[(int)ConvertsTo[i]] * timeDelta, ResourceMaxAmount[(int)ConvertsTo[i]]);
                }

            }
        }


        protected float clamp(float val)
        {
            if (val < 0){ return 0; }
            else { return val; }
        }

        protected float clamp(float val, float max)
        {
            if (val < 0) { return 0; }
            if (val > max) { return max;  }
            return val;
        }

        protected float[] ResouceAmount;
        protected float[] ResourceConsumptionPerSecond;
        protected float[] ResourceMaxAmount;

        protected ResourceStorage.Resource[] ConvertsTo;
        protected float[] ProduceRate;

        protected bool[] ConsumingResource;

        protected float[] ResourceOutput;


        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < ResourceStorage.ResourceCount; i++)
            {
                sb.Append((ResourceStorage.Resource)(i)); sb.Append(": "); sb.Append((int)ResourceOutput[i]);
                sb.Append("/"); sb.Append(ResourceMaxAmount[i]); sb.Append(" @ ");
                sb.Append(ProduceRate[i]);
                sb.AppendLine();
            }


            return sb.ToString();
        }


    }
}
