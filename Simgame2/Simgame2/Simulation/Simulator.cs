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
    public class Simulator
    {
        public Simulator(Game1 game)
        {
            SimEntities = new List<SimulationEntity>();
            this.game = game;
        }


        public void Update(GameTime gameTime) 
        {
            foreach (SimulationEntity e in SimEntities)
            {
                if (e != null)
                {
                    e.Update(gameTime);

                    if (e is Entities.MoverEntity.MoverSim)
                    {
                        Entities.MoverEntity.MoverSim s = (Entities.MoverEntity.MoverSim)e;

                        if (s.MovementState == Entities.MoverEntity.MoverSim.UnitMovementState.IDLE)
                        {
                            if (s.PayloadState == Entities.MoverEntity.MoverSim.UnitPayloadState.EMPTY)
                            {
                                Vector3 target = SetTargetNearest(s, Entity.EntityTypes.BASIC_MINE);
                                if (target != Vector3.Zero){
                                    s.SetTarget(target);
                                    s.MovementState = Entities.MoverEntity.MoverSim.UnitMovementState.MOVING;
                                }
                            }
                            else{
                                Vector3 target = SetTargetNearest(s, Entity.EntityTypes.MELTER);
                                if (target != Vector3.Zero){
                                    s.SetTarget(target);
                                    s.MovementState = Entities.MoverEntity.MoverSim.UnitMovementState.MOVING;
                                }
                            }
                        }
                        else
                        {
                            if (s.MovementState == Entities.MoverEntity.MoverSim.UnitMovementState.MOVING)
                            {
                                if (s.ReachedGoal) // .DistanceToTarget() < 20)
                                {
                                    if (s.PayloadState == Entities.MoverEntity.MoverSim.UnitPayloadState.EMPTY)
                                    {
                                        s.PayloadState = Entities.MoverEntity.MoverSim.UnitPayloadState.LOADED;
                                        s.MovementState = Entities.MoverEntity.MoverSim.UnitMovementState.IDLE;
                                    }
                                    else
                                    {
                                        s.PayloadState = Entities.MoverEntity.MoverSim.UnitPayloadState.EMPTY;
                                        s.MovementState = Entities.MoverEntity.MoverSim.UnitMovementState.IDLE;
                                    }
                                }
                            }
                        }



                    }


                }
            }
        }

        public void AddEntity(SimulationEntity s)
        {
            SimEntities.Add(s);
        }

        public void RemoveEntity(SimulationEntity s)
        {
            SimEntities.Remove(s);
        }





        public Vector3 SetTargetNearest(Entities.MoverEntity.MoverSim unit,  Entity.EntityTypes targetType)
        {
            float distance = float.MaxValue;
            Vector3 TargetLocation = Vector3.Zero;
            for (int i = 0; i < this.game.worldMap.entities.Count; i++)
            {
                if (this.game.worldMap.entities[i].Type == targetType && this.game.worldMap.entities[i].IsGhost == false)
                {
                    float dist = Vector3.Distance(unit.GetLocation(), this.game.worldMap.entities[i].location);

                    if (dist < distance)
                    {
                        distance = dist;
                        TargetLocation = this.game.worldMap.entities[i].location;

                    }
                }
            }

            return TargetLocation;
        }


       

        private List<SimulationEntity> SimEntities;
        private Game1 game;
    }
}
