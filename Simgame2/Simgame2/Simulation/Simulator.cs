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
        public Simulator()
        {
            SimEntities = new List<SimulationEnity>();
        }


        public void Update(GameTime gameTime) 
        { 
            foreach (SimulationEnity e in SimEntities)
            {
                if (e != null)
                {
                    e.Update(gameTime);
                }
            }
        }

        public void AddEntity(SimulationEnity s)
        {
            SimEntities.Add(s);
        }

        public void RemoveEntity(SimulationEnity s)
        {
            SimEntities.Remove(s);
        }


        private List<SimulationEnity> SimEntities;
    }
}
