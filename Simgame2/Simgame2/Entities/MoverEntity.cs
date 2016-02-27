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

namespace Simgame2.Entities
{
    public class MoverEntity : Entity
    {
        public MoverEntity(Game game)
            : base(game)
        {
            this.movement = new Movement(this);
            this.AddTexture("CubeTex");
            this.LoadModel("Models/Cube"); //, this.effect);
            CorrectBoundingBox = true;
            this.Type = EntityTypes.MOVER;
           
        }

        

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.movement.moverSim.MovementState == Movement.MoverSim.UnitMovementState.MOVING)
            {
                this.location = this.location + (this.movement.Velocity * (this.movement.MaxSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000));
                //float height = this.worldMap.getCellHeightFromWorldCoor(location.X, -location.Z);
                float height = this.LODMap.getCellHeightFromWorldCoor(location.X, location.Z);
                
                if (height < LODTerrain.LODTerrain.waterHeight)
                //if (height < WorldMap.waterHeight)
                {
                    //height = WorldMap.waterHeight;
                    height = LODTerrain.LODTerrain.waterHeight;
                }


                this.location = new Vector3(location.X, height, location.Z);
            }
            this.UpdateBoundingBox();

            this.movement.turnToTarget();
            this.movement.OrientateConformTerrain();
            
            this.UpdateBoundingBox();

            if (this.movement.moverSim.MovementState == Movement.MoverSim.UnitMovementState.MOVING)
            {
                this.movement.moverSim.UpdatePath();
            }
            // TODO set location, update distace, check if distance < minDistance, then change target
            if (this.HasMouseFocus)
            {
                Console.WriteLine(this.movement.moverSim.ToString());
            }

        }


        float clamp(float a)
        {
            if (a > 1)
            {
                a = 1;
            }
            if (a < -1)
            {
                a = -1;
            }
            return a;
        }

        public void SetTargetNearest(EntityTypes target)
        {
            float distance = float.MaxValue;

            for (int i = 0; i < this.LODMap.entities.Count; i++)
            //for (int i = 0; i < this.worldMap.entities.Count; i++)
            {
                if (this.LODMap.entities[i].Type == target)
                //if (this.worldMap.entities[i].Type == target)
                {
                    float dist = this.DistanceTo(this.LODMap.entities[i]);
                    //float dist = this.DistanceTo(this.worldMap.entities[i]);
                    if (dist < distance)
                    {
                        distance = dist;
                        //this.movement.TargetLocation = this.worldMap.entities[i].location;
                        this.movement.TargetLocation = this.LODMap.entities[i].location;
                        
                    }
                }
                else
                {
                    
                }
            }

            this.movement.TargetDistance = distance;
        }


        public Movement movement;
        
    }
}
