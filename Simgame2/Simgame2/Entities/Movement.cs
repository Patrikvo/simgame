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
    public class Movement
    {
        public Movement(Entity ParentEntity)
        {
            this.ParentEntity = ParentEntity;
            this.MaxSpeed = 10.0f;
            this.Velocity = new Vector3(0.0f, 0.0f, -1.0f);
            GetSimEntity();
            moverSim.MovementState = MoverSim.UnitMovementState.IDLE;
        }


        public void MoveTo(Entity target)
        {
            this.TargetEntity = target;
            this.MoveTo(target.location);
        }

        public void MoveTo(Vector3 TargetLocation)
        {
            this.TargetLocation = TargetLocation;
        }


        public float DistanceTo(Entity other)
        {
            float distance = float.MaxValue;
            if (other != null)
            {
                distance = Vector3.Distance(other.location, this.Location);
            }
            return distance;
        }

        

        public Vector3 Rotation
        {
            get { return this.ParentEntity.rotation; }
            set { this.ParentEntity.rotation = value; }
        }
      //  private Vector3 _Rotation;


        

        public Vector3 Location
        {
            get { return _Location; }
            set { _Location = value; }
        }
        private Vector3 _Location;



        public void Update(GameTime gameTime)
        {
            if (this.TargetEntity != null && this.TargetEntity.IsMover)
            {
                this.MoveTo(this.TargetEntity.location);
            }
        }


        public Vector3 turnToTarget()
        {
            Vector3 rotation = new Vector3(0,0,0);
            if (this.TargetLocation != Vector3.Zero)
            {
                Vector3 newForwardUnit = Vector3.Normalize(this.TargetLocation - this.ParentEntity.location);
                this.Velocity = newForwardUnit;

                double r = Math.Sqrt(this.Velocity.X * this.Velocity.X + this.Velocity.Y * this.Velocity.Y + this.Velocity.Z * this.Velocity.Z);
                // double t = Math.Atan(this.Velocity.Y/this.Velocity.X);
                double p = Math.Acos(-this.Velocity.Z / r);
                rotation = new Vector3(rotation.X, (float)p, rotation.Z);

            }
            return rotation;
        }



        public void OrientateConformTerrain()
        {

            /*
            int h1 = this.ParentEntity.worldMap.getExactHeightFromWorldCoor(this.ParentEntity.corners[7].X, -this.ParentEntity.corners[7].Z);
            int h2 = this.ParentEntity.worldMap.getExactHeightFromWorldCoor(this.ParentEntity.corners[3].X, -this.ParentEntity.corners[3].Z);
            int h3 = this.ParentEntity.worldMap.getExactHeightFromWorldCoor(this.ParentEntity.corners[2].X, -this.ParentEntity.corners[2].Z);
            */

            int h1 = this.ParentEntity.LODMap.getExactHeightFromWorldCoor(this.ParentEntity.corners[7].X, this.ParentEntity.corners[7].Z);
            int h2 = this.ParentEntity.LODMap.getExactHeightFromWorldCoor(this.ParentEntity.corners[3].X, this.ParentEntity.corners[3].Z);
            int h3 = this.ParentEntity.LODMap.getExactHeightFromWorldCoor(this.ParentEntity.corners[2].X, this.ParentEntity.corners[2].Z);

            float l12 = Vector3.Distance(this.ParentEntity.corners[7], this.ParentEntity.corners[3]);
            float l23 = Vector3.Distance(this.ParentEntity.corners[3], this.ParentEntity.corners[2]);

            float angle1 = (float)Math.Atan(Math.Abs((h1 - h2)) / (l12 * Math.Sqrt(2)));
            float angle2 = (float)Math.Atan(Math.Abs((h2 - h3)) / (l23 * Math.Sqrt(2)));


            this.ParentEntity.rotation = new Vector3(angle1, this.Rotation.Y, -angle2);
            
            

        }


        public bool CollidesWith(Entity other)
        {
            if (!this.ParentEntity.boundingBox.Equals(other.boundingBox))
            {
                if (ParentEntity.boundingBox.Intersects(other.boundingBox) || ParentEntity.boundingBox.Contains(other.boundingBox) == ContainmentType.Contains || other.boundingBox.Contains(ParentEntity.boundingBox) == ContainmentType.Contains)
                {
                    return true;
                }
            }
            return false;
        }


        public Simulation.SimulationEntity GetSimEntity()
        {
            if (moverSim == null)
            {

                moverSim = new MoverSim(this);

            }

            return moverSim;
        }


        public float MaxSpeed { get; set; }

        public Vector3 Velocity { get; set; }

        public Vector3 TargetLocation;
        public float TargetDistance { get; set; }
        public Entity TargetEntity;
        public Entity ParentEntity;

        public MoverSim moverSim;








        public class MoverSim : Simulation.SimulationEntity
        {
            public const float StopDistance = 30;

            public bool ManualControl { get; set; }


            public MoverSim(Movement mover)
                : base()
            {
                this.mover = mover;
                this.MovementState = UnitMovementState.IDLE;
                this.PayloadState = UnitPayloadState.EMPTY;
                ReachedGoal = true;
                RefreshTarget = true;
                ManualControl = false;
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

            public Vector3 GetLocation() { return this.mover.ParentEntity.location; }
            public void SetTarget(Vector3 targetLoc)
            {
                //Vector2 loc = mover.ParentEntity.worldMap.getCellAdressFromWorldCoor(GetLocation().X, -GetLocation().Z);
                Vector2 loc = mover.ParentEntity.LODMap.getCellAdressFromWorldCoor(GetLocation().X, GetLocation().Z);


                //Vector2 goalLoc = mover.ParentEntity.worldMap.getCellAdressFromWorldCoor(targetLoc.X, -targetLoc.Z);
                Vector2 goalLoc = mover.ParentEntity.LODMap.getCellAdressFromWorldCoor(targetLoc.X, targetLoc.Z);

                //pathfinder = new PathFinder(mover.ParentEntity.worldMap.GetCellTravelResistance, mover.ParentEntity.worldMap.mapNumCellsPerRow, mover.ParentEntity.worldMap.mapNumCellPerColumn,
                pathfinder = new PathFinder(mover.ParentEntity.LODMap.GetCellTravelResistance, mover.ParentEntity.LODMap.mapNumCellsPerRow, mover.ParentEntity.LODMap.mapNumCellPerColumn,
                    (int)loc.X, (int)loc.Y, (int)goalLoc.X, (int)goalLoc.Y);

                if (pathfinder.Path != null)
                {
                    this.Path = new Vector2[pathfinder.Path.Length - 1];
                    for (int i = 0; i < pathfinder.Path.Length - 1; i++)
                    {
                        this.Path[i] = new Vector2(pathfinder.Path[i + 1].First, pathfinder.Path[i + 1].Second);

                        //    mover.worldMap.AddEntity(mover.entityFactory.CreateMiniMover(new Vector3(pathfinder.Path[i + 1].First * 5, 20, -pathfinder.Path[i + 1].Second * 5)));

                    }
                    CurrentPathStep = 0;
                    this.mover.TargetLocation = new Vector3(Path[CurrentPathStep].X * 5, 12, Path[CurrentPathStep].Y * 5);
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
                if (DistanceToTarget() < MoverSim.StopDistance)
                {
                    CurrentPathStep++;
                    if (this.Path == null || CurrentPathStep >= this.Path.Length)
                    {
                        this.MovementState = UnitMovementState.IDLE;
                        ReachedGoal = true;
                        return false;
                    }
                    else
                    {
                        this.mover.TargetLocation = new Vector3(Path[CurrentPathStep].X * 5, 12, Path[CurrentPathStep].Y * 5);
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
            public Movement mover;
            public enum UnitMovementState { IDLE, MOVING };
            public enum UnitPayloadState { EMPTY, LOADED, LOADING, UNLOADING };
            PathFinder pathfinder;
            private Vector2[] Path;

        }
    }
}
