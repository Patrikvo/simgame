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

namespace Simgame2
{
    public class EntityBuilding: Entity
    {
        public EntityBuilding(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.IsTransparant = false;
            statusBillboard = new Billboard(RuningGameSession);
            statusBillboard.Show = false;
            this.CanBeCommanded = true;
            
        }

        public bool PlaceBuilding(LODTerrain.LODTerrain map, bool flattening)
        //public bool PlaceBuilding(WorldMap map, bool flattening)
        {
            bool res = PlaceBuilding(map, (int)this.location.X, (int)this.location.Z, flattening);
            if (flattening)
            {
                statusBillboard.loadTexture(this.RunningGameSession.Content.Load<Texture2D>("GUI\\Bilboard"), this.location);
                statusBillboard.offset = surroundingCicle;
                statusBillboard.Show = true;
            }
            return res;
        }
        
        public bool PlaceBuilding(LODTerrain.LODTerrain map, int x, int y, bool flattening)
        //public bool PlaceBuilding(WorldMap map, int x, int y, bool flattening)
        {

            this.location = new Vector3(x, 12, y);
            BoundingBox bbox = this.UpdateBoundingBox();



            float MinX = float.MaxValue;
            float MaxX = float.MinValue;
            float MinZ = float.MaxValue;
            float MaxZ = float.MinValue;

            float Y = float.MaxValue;

            foreach (Vector3 v in corners)
            {
                if (v.X < MinX) { MinX = v.X; }
                if (v.X > MaxX) { MaxX = v.X; }

                if (v.Z < MinZ) { MinZ = v.Z; }
                if (v.Z > MaxZ) { MaxZ = v.Z; }


                if (v.Y < Y) { Y = v.Y; }
            }

            int height;
            if (flattening)
            {
                height = map.levelTerrainWorldCoor(MinX, MinZ, MaxX, MaxZ);
            }
            else
            {
                height = map.getCellHeightFromWorldCoor((MinX + MaxX) / 2, ((MinZ + MaxZ) / 2));
            }
            this.location = new Vector3(location.X, height, location.Z);

            this.boundingBox = this.UpdateBoundingBox();

            map.AddEntity(this); // .entities.Add(this);

            // surrounding circle.
            float w = bbox.Max.X - bbox.Min.X;
            float h = bbox.Max.Z - bbox.Min.Z;
            if (w > h) 
            {
                surroundingCicle = w / 2;
            }
            else
            {
                surroundingCicle = h / 2;
            }

            return true;
        }

        public void RemoveBuilding(LODTerrain.LODTerrain map)
        //public void RemoveBuilding(WorldMap map)
        {
            map.RemoveEntity(this);
        }


        


        public virtual Simulation.SimulationBuildingEnity GetSimEntity()
        {
            return null;
        }

        private float surroundingCicle;

        protected Billboard statusBillboard;
    }
}
