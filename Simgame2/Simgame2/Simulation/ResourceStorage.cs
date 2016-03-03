using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simgame2.Simulation
{
    public class ResourceStorage
    {
        
        public const int ResourceCount = 26;

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
            PrepareResourceNameList();




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

        

        private void PrepareResourceNameList()
        {
            ResouceDisplayName[(int)Resource.NOTHING]       = "NOTHING";
            ResouceDisplayName[(int)Resource.ELECTRICITY]   = "Electricity";
            ResouceDisplayName[(int)Resource.OreIron]       = "Iron ore";
            ResouceDisplayName[(int)Resource.OreCopper]     = "Copper ore";
            ResouceDisplayName[(int)Resource.OreAluminium]  = "Aluminium ore";
            ResouceDisplayName[(int)Resource.OreLithium]    = "Lithium ore";
            ResouceDisplayName[(int)Resource.OreTitanium]   = "Titanium ore";
            ResouceDisplayName[(int)Resource.OreNickel]     = "Nickel ore";
            ResouceDisplayName[(int)Resource.OreSilver]     = "Silver ore";
            ResouceDisplayName[(int)Resource.OreTungsten]   = "Tungsten ore";
            ResouceDisplayName[(int)Resource.OrePlatinum]   = "Platinum ore";
            ResouceDisplayName[(int)Resource.OreGold]       = "Gold ore";
            ResouceDisplayName[(int)Resource.OreLead]       = "Lead ore";
            ResouceDisplayName[(int)Resource.OreUranium]    = "Uranium ore";

            ResouceDisplayName[(int)Resource.MetalIron]      = "Iron";
            ResouceDisplayName[(int)Resource.MetalCopper]    = "Copper";
            ResouceDisplayName[(int)Resource.MetalAluminium] = "Aluminium";
            ResouceDisplayName[(int)Resource.MetalLithium]   = "Lithium";
            ResouceDisplayName[(int)Resource.MetalTitanium]  = "Titanium";
            ResouceDisplayName[(int)Resource.MetalNickel]    = "Nickel";
            ResouceDisplayName[(int)Resource.MetalSilver]    = "Silver";
            ResouceDisplayName[(int)Resource.MetalTungsten]  = "Tungsten";
            ResouceDisplayName[(int)Resource.MetalPlatinum]  = "Platinum";
            ResouceDisplayName[(int)Resource.MetalGold]      = "Gold";
            ResouceDisplayName[(int)Resource.MetalLead]      = "Lead";
            ResouceDisplayName[(int)Resource.MetalUranium]   = "Uranium";
        }

        private float[] ResourcesAvailable;
        private float[] ResourcesMaxStorage;
        private string[] ResouceDisplayName;
    }
}
