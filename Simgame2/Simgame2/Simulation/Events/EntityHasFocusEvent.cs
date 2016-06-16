using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simgame2.Simulation.Events
{
    class EntityHasFocusEvent: Event
    {
        public EntityHasFocusEvent(Entity source)
            : base(source, null, true)
        {
        }
    }
}
