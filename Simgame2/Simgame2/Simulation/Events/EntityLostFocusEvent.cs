using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simgame2.Simulation.Events
{
    class EntityLostFocusEvent: Event
    {
        public EntityLostFocusEvent(Entity source)
            : base(source, null, true)
        {
        }
    }
}
