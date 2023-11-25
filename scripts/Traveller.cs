using System;
using Godot;


namespace FlightControl
{
    public class Traveller
    {
        [Export]
        public Airport Destination { get; set; }

        private event EventHandler Arrived;

        public Traveller() { }
        public Traveller(EventHandler onArrival)
        {
            Arrived += onArrival;
        }

        public void Arrive()
        {
            Arrived?.Invoke(this, EventArgs.Empty);
        }
    }
}