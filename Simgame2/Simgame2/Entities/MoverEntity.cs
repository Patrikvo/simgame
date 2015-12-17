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
            this.LoadModel("Cube"); //, this.effect);
            CorrectBoundingBox = true;
            this.Type = EntityTypes.MOVER;
           
        }

        

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.movement.moverSim.MovementState == Movement.MoverSim.UnitMovementState.MOVING)
            {
                this.location = this.location + (this.movement.Velocity * (this.movement.MaxSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000));
                float height = this.worldMap.getCellHeightFromWorldCoor(location.X, -location.Z);
                if (height < WorldMap.waterHeight)
                {
                    height = WorldMap.waterHeight;
                }


                this.location = new Vector3(location.X, height, location.Z);
            }
            this.UpdateBoundingBox();

            turnToTarget();
            OrientateConformTerrain();
            
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

        private void turnToTarget()
        {
            if (this.movement.TargetLocation != Vector3.Zero)
            {
                Vector3 newForwardUnit = Vector3.Normalize(this.movement.TargetLocation - this.location);
                this.movement.Velocity = newForwardUnit;

                double r = Math.Sqrt(this.movement.Velocity.X * this.movement.Velocity.X + this.movement.Velocity.Y * this.movement.Velocity.Y + this.movement.Velocity.Z * this.movement.Velocity.Z);
               // double t = Math.Atan(this.Velocity.Y/this.Velocity.X);
                double p = Math.Acos(-this.movement.Velocity.Z / r);
                rotation = new Vector3(rotation.X, (float)p, rotation.Z);

            }
        }

        private void OrientateConformTerrain()
        {
 

            int h1 = this.worldMap.getExactHeightFromWorldCoor(this.corners[7].X, -this.corners[7].Z);
            int h2 = this.worldMap.getExactHeightFromWorldCoor(this.corners[3].X, -this.corners[3].Z);
            int h3 = this.worldMap.getExactHeightFromWorldCoor(this.corners[2].X, -this.corners[2].Z);

            float l12 = Vector3.Distance(this.corners[7], this.corners[3]);
            float l23 = Vector3.Distance(this.corners[3], this.corners[2]);

          //  float angle1 = (float)Math.Asin(clamp((h1 - h2) / l12));

          //  float angle2 = (float)Math.Asin(clamp((h2 - h3) / l23));

            float angle1 = (float)Math.Atan(Math.Abs((h1 - h2)) / (l12 * Math.Sqrt(2)));
            float angle2 = (float)Math.Atan(Math.Abs((h2 - h3)) / (l23 * Math.Sqrt(2)));


            //rotation = new Vector3(angle1, rotation.Y, -angle2);
            rotation = new Vector3(angle1, rotation.Y, angle2);



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

            for (int i = 0; i < this.worldMap.entities.Count; i++)
            {
                if (this.worldMap.entities[i].Type == target)
                {
                    float dist = this.DistanceTo(this.worldMap.entities[i]);
                    if (dist < distance)
                    {
                        distance = dist;
                        this.movement.TargetLocation = this.worldMap.entities[i].location;
                        
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
