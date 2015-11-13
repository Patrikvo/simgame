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
            ResouceAmount = new float[ResourceCount];
            ResourceConsumptionPerSecond = new float[ResourceCount];
            ResourceMaxAmount = new float[ResourceCount];
            ConsumingResource = new bool[ResourceCount];
            ConvertsTo = new Resource[ResourceCount];
            ProduceRate = new float[ResourceCount];
            ResourceOutput = new float[ResourceCount];
        }

        

        public void AddResource(Resource res, float amount)
        {
            ResouceAmount[(int)res] = clamp( ResouceAmount[(int)res] + amount);
        }

        public float GetResource(Resource res)
        {
            return ResouceAmount[(int)res];
        }

        public float GetMaxResourceAmount(Resource res)
        {
            return ResourceMaxAmount[(int)res];
        }

        public float GetResourceConsumption(Resource res)
        {
            return ResourceConsumptionPerSecond[(int)res];
        }

        public float GetAvailableOutResource(Resource res)
        {
            return ResourceOutput[(int)res];
        }

        public float ExtractOutResource(Resource res, float amount)
        {
            float output = clamp(amount, ResourceOutput[(int)res]);
            ResourceOutput[(int)res] = clamp(ResourceOutput[(int)res] - output);
            return output;
        }


        public override void Update(GameTime gameTime)
        {
            float timeDelta = gameTime.ElapsedGameTime.Milliseconds / 1000;
            for (int i = 0; i < ResourceCount; i++)
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

        protected Resource[] ConvertsTo;
        protected float[] ProduceRate;

        protected bool[] ConsumingResource;

        protected float[] ResourceOutput;

        public const int ResourceCount = 26;
        public enum Resource { NOTHING, ELECTRICITY, 
            OreIron,   OreCopper,   OreAluminium,   OreLithium,   OreTitanium,   OreNickel,   OreSilver,   OreTungsten,   OrePlatinum,   OreGold,   OreLead,   OreUranium,
            MetalIron, MetalCopper, MetalAluminium, MetalLithium, MetalTitanium, MetalNickel, MetalSilver, MetalTungsten, MetalPlatinum, MetalGold, MetalLead, MetalUranium
        };

        //Iron, Copper, Aluminium, Lithium, Titanium, Nickel, Silver, Tungsten, Platinum, Gold, Lead, Uranium


        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < ResourceCount; i++)
            {
                sb.Append((Resource)(i)); sb.Append(": "); sb.Append((int)ResourceOutput[i]);
                sb.Append("/"); sb.Append(ResourceMaxAmount[i]); sb.Append(" @ ");
                sb.Append(ProduceRate[i]);
                sb.AppendLine();
            }


            return sb.ToString();
        }


    }
}
