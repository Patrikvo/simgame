using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simgame2.Simulation
{
    public abstract class Event
    {
        public Event(Entity source, Entity target, bool isBroadcast)
        {
            this.IsBroadcast = isBroadcast;
            this.SourceEntity = source;
            this.TargetEntity = target;
        }


        public bool IsBroadcast { get; set; }

        public Entity SourceEntity { get; private set; }

        public Entity TargetEntity { get; private set; }

    }
}
