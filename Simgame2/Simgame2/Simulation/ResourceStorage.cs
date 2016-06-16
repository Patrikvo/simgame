using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simgame2.Simulation
{
    public class ResourceStorage
    {
        
        public const int ResourceCount = 26;

        public const int metalOffset = 12;  // ore + metalOffset = metal
        public const int OreCount = 12;
        public const int OreStart = 2;
        public enum Resource
        {
            NOTHING, ELECTRICITY,
            OreIron, OreCopper, OreAluminium, OreLithium, OreTitanium, OreNickel, OreSilver, OreTungsten, OrePlatinum, OreGold, OreLead, OreUranium,
            MetalIron, MetalCopper, MetalAluminium, MetalLithium, MetalTitanium, MetalNickel, MetalSilver, MetalTungsten, MetalPlatinum, MetalGold, MetalLead, MetalUranium
        };

        public ResourceStorage()
        {
            ResourcesAvailable = new float[ResourceCount];
            ResourcesMaxStorage = new float[ResourceCount];

            ResouceDisplayName = new string[ResourceCount];
            PrepareResourceList();




        }

        public float GetResourceAvailable(Resource resource)
        {
            return ResourcesAvailable[(int)resource];
        }

        public float GetResourceMaxStorageAmount(Resource resource)
        {
            return ResourcesMaxStorage[(int)resource];
        }

        public string GetResourceName(Resource resource)
        {
            return ResouceDisplayName[(int)resource];
        }


        // return true if whole amount can be added, otherwise false.
        public bool AddResourceAmount(Resource resource, float amount)
        {
            int resourceNum = (int) resource;
            ResourcesAvailable[resourceNum] += amount;
            if (ResourcesAvailable[resourceNum] > ResourcesMaxStorage[resourceNum])
            {
                ResourcesAvailable[resourceNum] = ResourcesMaxStorage[resourceNum];
                return false;
            }
            return true;
        }

        public bool CanExtractResourceAmount(Resource resource, float amount)
        {
            int resourceNum = (int)resource;
            return ResourcesAvailable[resourceNum] >= amount;
        }

        public bool ExtractResourceAmount(Resource resource, float amount)
        {

            if (CanExtractResourceAmount(resource, amount))
            {
                int resourceNum = (int)resource;
                ResourcesAvailable[resourceNum] -= amount;
                if (ResourcesAvailable[resourceNum] < 0) { ResourcesAvailable[resourceNum] = 0.0f; }
                return true;
            }
            return false;
        }

        public string GetResourceString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 1; i < ResouceDisplayName.Length; i++)
            {
                sb.Append(ResouceDisplayName[i]);
                sb.Append(" ");
                sb.Append(ResourcesAvailable[i]);
                sb.Append(@"/");
                sb.Append(ResourcesMaxStorage[i]);
                sb.AppendLine();
            }


            return sb.ToString().Trim();
        }

        private void PrepareResourceList()
        {
            ResouceDisplayName[(int)Resource.NOTHING]       = "NOTHING";
            ResourcesAvailable[(int)Resource.NOTHING]       = 0;
            ResourcesMaxStorage[(int)Resource.NOTHING]      = 0;

            ResouceDisplayName[(int)Resource.ELECTRICITY]   = "Electricity";
            ResourcesAvailable[(int)Resource.ELECTRICITY]   = 0;
            ResourcesMaxStorage[(int)Resource.ELECTRICITY]  = 1000;



            ResouceDisplayName[(int)Resource.OreIron]       = "Iron ore";
            ResourcesAvailable[(int)Resource.OreIron]       = 0;
            ResourcesMaxStorage[(int)Resource.OreIron]      = 1000;

            ResouceDisplayName[(int)Resource.OreCopper]     = "Copper ore";
            ResourcesAvailable[(int)Resource.OreCopper]     = 0;
            ResourcesMaxStorage[(int)Resource.OreCopper]    = 1000;

            ResouceDisplayName[(int)Resource.OreAluminium]  = "Aluminium ore";
            ResourcesAvailable[(int)Resource.OreAluminium] = 0;
            ResourcesMaxStorage[(int)Resource.OreAluminium] = 1000;

            ResouceDisplayName[(int)Resource.OreLithium]    = "Lithium ore";
            ResourcesAvailable[(int)Resource.OreLithium] = 0;
            ResourcesMaxStorage[(int)Resource.OreLithium] = 1000;

            ResouceDisplayName[(int)Resource.OreTitanium]   = "Titanium ore";
            ResourcesAvailable[(int)Resource.OreTitanium] = 0;
            ResourcesMaxStorage[(int)Resource.OreTitanium] = 1000;

            ResouceDisplayName[(int)Resource.OreNickel]     = "Nickel ore";
            ResourcesAvailable[(int)Resource.OreNickel] = 0;
            ResourcesMaxStorage[(int)Resource.OreNickel] = 1000;

            ResouceDisplayName[(int)Resource.OreSilver]     = "Silver ore";
            ResourcesAvailable[(int)Resource.OreSilver] = 0;
            ResourcesMaxStorage[(int)Resource.OreSilver] = 1000;

            ResouceDisplayName[(int)Resource.OreTungsten]   = "Tungsten ore";
            ResourcesAvailable[(int)Resource.OreTungsten] = 0;
            ResourcesMaxStorage[(int)Resource.OreTungsten] = 1000;

            ResouceDisplayName[(int)Resource.OrePlatinum]   = "Platinum ore";
            ResourcesAvailable[(int)Resource.OrePlatinum] = 0;
            ResourcesMaxStorage[(int)Resource.OrePlatinum] = 1000;

            ResouceDisplayName[(int)Resource.OreGold]       = "Gold ore";
            ResourcesAvailable[(int)Resource.OreGold] = 0;
            ResourcesMaxStorage[(int)Resource.OreGold] = 1000;

            ResouceDisplayName[(int)Resource.OreLead]       = "Lead ore";
            ResourcesAvailable[(int)Resource.OreLead] = 0;
            ResourcesMaxStorage[(int)Resource.OreLead] = 1000;
            
            ResouceDisplayName[(int)Resource.OreUranium]    = "Uranium ore";
            ResourcesAvailable[(int)Resource.OreUranium] = 0;
            ResourcesMaxStorage[(int)Resource.OreUranium] = 1000;



            ResouceDisplayName[(int)Resource.MetalIron]      = "Iron";
            ResourcesAvailable[(int)Resource.MetalIron]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalIron]      = 1000;

            ResouceDisplayName[(int)Resource.MetalCopper]    = "Copper";
            ResourcesAvailable[(int)Resource.MetalCopper]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalCopper]      = 1000;

            ResouceDisplayName[(int)Resource.MetalAluminium] = "Aluminium";
            ResourcesAvailable[(int)Resource.MetalAluminium]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalAluminium]      = 1000;

            ResouceDisplayName[(int)Resource.MetalLithium]   = "Lithium";
            ResourcesAvailable[(int)Resource.MetalLithium]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalLithium]      = 1000;

            ResouceDisplayName[(int)Resource.MetalTitanium]  = "Titanium";
            ResourcesAvailable[(int)Resource.MetalTitanium]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalTitanium]      = 1000;

            ResouceDisplayName[(int)Resource.MetalNickel]    = "Nickel";
            ResourcesAvailable[(int)Resource.MetalNickel]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalNickel]      = 1000;

            ResouceDisplayName[(int)Resource.MetalSilver]    = "Silver";
            ResourcesAvailable[(int)Resource.MetalSilver]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalSilver]      = 1000;

            ResouceDisplayName[(int)Resource.MetalTungsten]  = "Tungsten";
            ResourcesAvailable[(int)Resource.MetalTungsten]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalTungsten]      = 1000;

            ResouceDisplayName[(int)Resource.MetalPlatinum]  = "Platinum";
            ResourcesAvailable[(int)Resource.MetalPlatinum]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalPlatinum]      = 1000;

            ResouceDisplayName[(int)Resource.MetalGold]      = "Gold";
            ResourcesAvailable[(int)Resource.MetalGold]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalGold]      = 1000;

            ResouceDisplayName[(int)Resource.MetalLead]      = "Lead";
            ResourcesAvailable[(int)Resource.MetalLead]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalLead]      = 1000;

            ResouceDisplayName[(int)Resource.MetalUranium]   = "Uranium";
            ResourcesAvailable[(int)Resource.MetalUranium]      = 0;
            ResourcesMaxStorage[(int)Resource.MetalUranium]      = 1000;

        }



        private float[] ResourcesAvailable;
        private float[] ResourcesMaxStorage;
        private string[] ResouceDisplayName;
    }
}
