using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simgame2.Simulation.Events
{
    public interface EventReceiver
    {
        bool OnEvent(Simulation.Event ReceivedEvent);
    }
}
