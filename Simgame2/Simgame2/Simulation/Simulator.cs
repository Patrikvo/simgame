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
        protected GameSession.GameSession RunningGameSession;

        public Simulator(GameSession.GameSession RunningGameSession)
        {
            EventQueue = new Queue<Event>();
            SimEntities = new List<Simulation.Events.EventReceiver>();
            this.RunningGameSession = RunningGameSession;

            MapModified = false;
        }

        public Boolean MapModified;

        public void Update(GameTime gameTime) 
        {
            while (EventQueue.Count > 0)
            {
                Event currentEvent = EventQueue.Dequeue();

                foreach (Events.EventReceiver e in SimEntities)
                {
                    if (e.OnEvent(currentEvent) == true)
                    {
                        if (!currentEvent.IsBroadcast)
                        {
                            break;
                        }
                    }
                }


            }



/*
            foreach (SimulationEntity e in SimEntities)
            {
                if (e != null)
                {
                    e.Update(gameTime);



                    //if (e is Entities.MoverEntity.MoverSim)
                    //{
                    //    Entities.MoverEntity.MoverSim s = (Entities.MoverEntity.MoverSim)e;
                    //    if (s.ManualControl == true) { continue; }
                    //    if (MapModified) { s.RefreshTarget = true; }

                    //    if (s.MovementState == Entities.MoverEntity.MoverSim.UnitMovementState.IDLE)
                    //    {
                    //        if (s.PayloadState == Entities.MoverEntity.MoverSim.UnitPayloadState.EMPTY)
                    //        {
                    //            Vector3 target = SetTargetNearest(s, Entity.EntityTypes.BASIC_MINE);

                    //            if (target != Vector3.Zero && s.DistanceToTarget(target) > Entities.MoverEntity.MoverSim.StopDistance)
                    //            {
                    //                s.SetTarget(target);
                    //                s.MovementState = Entities.MoverEntity.MoverSim.UnitMovementState.MOVING;
                    //            }
                    //        }
                    //        else{
                    //            Vector3 target = SetTargetNearest(s, Entity.EntityTypes.MELTER);
                    //            if (target != Vector3.Zero){
                    //                s.SetTarget(target);
                    //                s.MovementState = Entities.MoverEntity.MoverSim.UnitMovementState.MOVING;
                    //            }
                    //        }
                    //    }
                    //   // else
                    //   // {
                    //     //   if (s.MovementState == Entities.MoverEntity.MoverSim.UnitMovementState.MOVING)
                    //      //  {
                    //            if (s.ReachedGoal) // .DistanceToTarget() < 20)
                    //            {
                    //                if (s.PayloadState == Entities.MoverEntity.MoverSim.UnitPayloadState.EMPTY)
                    //                {
                    //                    s.PayloadState = Entities.MoverEntity.MoverSim.UnitPayloadState.LOADED;
                    //                    s.MovementState = Entities.MoverEntity.MoverSim.UnitMovementState.IDLE;
                    //                }
                    //                else
                    //                {
                    //                    s.PayloadState = Entities.MoverEntity.MoverSim.UnitPayloadState.EMPTY;
                    //                    s.MovementState = Entities.MoverEntity.MoverSim.UnitMovementState.IDLE;
                    //                }
                    //            }
                    //    //    }
                    //  //  }



                    //}


                }
            }
 * */
            MapModified = false;

        }

        public void AddEntity(Simulation.Events.EventReceiver s)
        {
            SimEntities.Add(s);
        }

        public void RemoveEntity(Simulation.Events.EventReceiver s)
        {
            SimEntities.Remove(s);
        }





        public Vector3 SetTargetNearest(Entities.MoverEntity.MoverSim unit,  Entity.EntityTypes targetType)
        {
            float distance = float.MaxValue;
            Vector3 TargetLocation = Vector3.Zero;
            for (int i = 0; i < this.RunningGameSession.LODMap.entities.Count; i++)
            //for (int i = 0; i < this.game.worldMap.entities.Count; i++)
            {
                if (this.RunningGameSession.LODMap.entities[i].Type == targetType && this.RunningGameSession.LODMap.entities[i].IsGhost == false)
                //if (this.game.worldMap.entities[i].Type == targetType && this.game.worldMap.entities[i].IsGhost == false)
                {
                    //float dist = Vector3.Distance(unit.GetLocation(), this.game.worldMap.entities[i].location);
                    float dist = Vector3.Distance(unit.GetLocation(), this.RunningGameSession.LODMap.entities[i].location);
                    if (dist < distance)
                    {
                        distance = dist;
                        //TargetLocation = this.game.worldMap.entities[i].location;
                        TargetLocation = this.RunningGameSession.LODMap.entities[i].location;

                    }
                }
            }

            return TargetLocation;
        }




        private List<Simulation.Events.EventReceiver> SimEntities;




        public void AddEvent(Event newEvent)
        {
            this.EventQueue.Enqueue(newEvent);
        }


        private Queue<Event> EventQueue;



    }
}
