using Godot;
using System.Collections.Generic;
using System.Linq;

namespace FlightControl.Systems
{
    public partial class AirportSpawnSystem : Node2D
    {
        [Export]
        public int initialAirportCount = 3;

        private readonly List<Airport> airports = new();
        private readonly HashSet<Airport> visibleAirports = new();
        private float timeSinceLastSpawn = 0;

        public override void _Ready()
        {
            airports.AddRange(GetTree().GetNodesInGroup("Airports").Cast<Airport>());
            foreach (var airport in airports)
            {
                airport.Visible = false;
            }

            var rand = new System.Random();
            for (int i = 0; i < initialAirportCount; i++)
            {
                var airport = airports[rand.Next(airports.Count)];
                while (visibleAirports.Contains(airport))
                {
                    airport = airports[rand.Next(airports.Count)];
                }
                airport.Visible = true;
                visibleAirports.Add(airport);
            }
        }

        
    }
}
