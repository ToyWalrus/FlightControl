using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FlightControl
{
    public partial class Airport : Node2D
    {
        [Export]
        public string AirportName { get; set; }

        [Export]
        public int TravellerMax { get; set; }

        public int TravellerCount { get => Travellers.Count; }

        [Export]
        public int PlaneMax { get; set; }

        [Export]
        public int PlaneCount { get; set; }

        public List<Traveller> Travellers { get; set; }
        public List<Connection> Connections { get; set; }


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Travellers = new();
            Connections = new();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public bool CanUnloadPlane(Plane plane)
        {
            if (TravellerCount + plane.PassengerCount <= TravellerMax) return true;
            return TravellerCount + plane.Passengers.Where(p => p.Destination != this).Count() <= TravellerMax;
        }

        public void UnloadPlane(Plane plane)
        {
            var waitingTravellers = plane.Passengers.Where(p => p.Destination != this);
            var arrivedTravellers = plane.Passengers.Where(p => p.Destination == this);
            plane.Unload();

            Travellers.AddRange(waitingTravellers);

            foreach (var traveller in arrivedTravellers)
            {
                traveller.Arrive();
            }
        }

        public static bool operator ==(Airport a, Airport b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            return a?.Equals(b) ?? false;
        }

        public static bool operator !=(Airport a, Airport b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return AirportName == ((Airport)obj).AirportName;
        }

        public override int GetHashCode()
        {
            return AirportName.GetHashCode();
        }
    }
}