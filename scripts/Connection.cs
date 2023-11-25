using System;
using Godot;

namespace FlightControl
{
    public class Connection
    {
        [Export]
        public Airport PortA { get; set; }

        [Export]
        public Airport PortB { get; set; }

        public static int CalculateCost(Airport a, Airport b)
        {
            return 0;
        }
    }
}