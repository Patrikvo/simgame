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
            this.AddTexture("CubeTex");
            this.LoadModel("Cube"); //, this.effect);
            this.MaxSpeed = 10.0f;
            Velocity = new Vector3(0.0f, 0.0f, -1.0f);
            CorrectBoundingBox = true;
            this.Type = EntityTypes.MOVER;
            GetSimEntity();
            moverSim.MovementState = MoverSim.UnitMovementState.IDLE;
           
        }

        

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.moverSim.MovementState == MoverSim.UnitMovementState.MOVING)
            {
                this.location = this.location + (Velocity * (MaxSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000));
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

            if (this.moverSim.MovementState == MoverSim.UnitMovementState.MOVING)
            {
                this.moverSim.UpdatePath();
            }
            // TODO set location, update distace, check if distance < minDistance, then change target
            if (this.HasMouseFocus)
            {
                Console.WriteLine(this.moverSim.ToString());
            }

        }

        private void turnToTarget()
        {
            if (TargetLocation != Vector3.Zero)
            {
                Vector3 newForwardUnit = Vector3.Normalize(TargetLocation - this.location);
                this.Velocity = newForwardUnit;

                double r = Math.Sqrt(this.Velocity.X*this.Velocity.X + this.Velocity.Y*this.Velocity.Y + this.Velocity.Z*this.Velocity.Z);
               // double t = Math.Atan(this.Velocity.Y/this.Velocity.X);
                double p = Math.Acos(-this.Velocity.Z / r);
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
                        TargetLocation = this.worldMap.entities[i].location;
                        
                    }
                }
                else
                {
                    
                }
            }

            TargetDistance = distance;
        }


        public Simulation.SimulationEntity GetSimEntity()
        {
            if (moverSim == null)
            {

                moverSim = new MoverSim(this);

            }

            return moverSim;
        }

        private MoverSim moverSim;

        public Vector3 Velocity { get; set; }

        public Vector3 TargetLocation { get; set; }
        public float TargetDistance { get; set; }

        public float MaxSpeed { get; set; }

               



        public class MoverSim : Simulation.SimulationEntity
        {
            public const float StopDistance = 20;


            public MoverSim(MoverEntity mover) : base()
            {
                this.mover = mover;
                this.MovementState = UnitMovementState.IDLE;
                this.PayloadState = UnitPayloadState.EMPTY;
                ReachedGoal = true;
                RefreshTarget = true;
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
            }


            public float DistanceToTarget()
            {
                return Vector3.Distance(GetLocation(), this.mover.TargetLocation);
            }

            public float DistanceToTarget(Vector3 target)
            {
                return Vector3.Distance(GetLocation(), target);
            }

            public Vector3 GetLocation() { return this.mover.location; }
            public void SetTarget(Vector3 targetLoc) 
            {
                Vector2 loc = mover.worldMap.getCellAdressFromWorldCoor(GetLocation().X, -GetLocation().Z);
                Vector2 goalLoc = mover.worldMap.getCellAdressFromWorldCoor(targetLoc.X, -targetLoc.Z);

                pathfinder = new PathFinder(mover.worldMap.GetCellTravelResistance, mover.worldMap.mapNumCellsPerRow, mover.worldMap.mapNumCellPerColumn,
                    (int)loc.X, (int)loc.Y, (int)goalLoc.X, (int)goalLoc.Y);

                if (pathfinder.Path != null)
                {
                    this.Path = new Vector2[pathfinder.Path.Length-1];
                    for (int i = 0; i < pathfinder.Path.Length-1; i++)
                    {
                        this.Path[i] = new Vector2(pathfinder.Path[i+1].First, pathfinder.Path[i+1].Second);
                        
                    //    mover.worldMap.AddEntity(mover.entityFactory.CreateMiniMover(new Vector3(pathfinder.Path[i + 1].First * 5, 20, -pathfinder.Path[i + 1].Second * 5)));

                    }
                    CurrentPathStep = 0; 
                    this.mover.TargetLocation = new Vector3(Path[CurrentPathStep].X*5, 12, -Path[CurrentPathStep].Y*5);
                    ReachedGoal = false;
                }
                else
                {
                    this.Path = null;
                }
            }

            int CurrentPathStep;

            public bool RefreshTarget;

            public bool UpdatePath()
            {
                if (DistanceToTarget() < Entities.MoverEntity.MoverSim.StopDistance)
                {
                    CurrentPathStep++;
                    if (CurrentPathStep >= this.Path.Length)
                    {
                        this.MovementState = UnitMovementState.IDLE;
                        ReachedGoal = true;
                        return false;
                    }
                    else
                    {
                        this.mover.TargetLocation = new Vector3(Path[CurrentPathStep].X*5, 12, -Path[CurrentPathStep].Y*5);
                    }
                }
                
                return true;
            }




            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("MoverUnit: ");
                sb.Append(" movestate: " + this.MovementState);
                sb.Append(" loadState: " + this.PayloadState);
                return sb.ToString();
            }



            public UnitMovementState MovementState
            {
                get { return _MovementState; }
                set { _MovementState = value; }
            }

            public bool ReachedGoal;

            private UnitMovementState _MovementState;
         
            public UnitPayloadState PayloadState;
            public MoverEntity mover;
            public enum UnitMovementState { IDLE, MOVING };
            public enum UnitPayloadState { EMPTY, LOADED, LOADING, UNLOADING };
            PathFinder pathfinder;
            private Vector2[] Path;

        }



        
    }
}
