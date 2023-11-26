using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FlightControl
{
    public partial class Plane : Node2D
    {
        [Export]
        public double Speed { get; set; }

        [Export]
        public int PassengerMax { get; set; }
        public int PassengerCount { get => passengers.Count; }

        [Export]
        public double MinWaitTime { get; set; }

        private Airport origin;
        private Airport destination;
        private Connection currentConnection;
        private readonly List<Traveller> passengers = new();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            // If currentDestination != null, move towards it
            // If at currentDestination, check airport.CanUnloadPlane(this) every X seconds
            // If true, airport.UnloadPlane(this)
        }

        public void Land()
        {
            origin = destination;
            destination = null;
        }

        public void TakeOff()
        {
            destination = currentConnection.GetOther(origin);
        }

        public void Unload()
        {
            passengers.ForEach(p => p.MarkConnectionComplete(currentConnection));
            passengers.Clear();
        }

        public void SetConnection(Connection connection)
        {
            currentConnection = connection;
        }

        public IEnumerable<Traveller> GetPassengersNotAtFinalDestination(Airport currentAirport)
        {
            return passengers.Where(p => p.Destination != currentAirport);
        }

        public IEnumerable<Traveller> Load(List<Traveller> travellers)
        {
            if (PassengerCount + travellers.Count > PassengerMax)
            {
                var cutoffIndex = PassengerMax - PassengerCount;
                passengers.AddRange(travellers.GetRange(0, cutoffIndex));
                return travellers.GetRange(cutoffIndex, travellers.Count - cutoffIndex);
            }
            else
            {
                passengers.AddRange(travellers);
                return new List<Traveller>();
            }
        }
    }
}
