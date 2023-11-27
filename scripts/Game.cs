using System;
using System.Collections.Generic;
using Godot;

namespace FlightControl
{
    public partial class Game : Node2D
    {
        public int TripsCompleted { get; private set; }
        public List<Plane> Planes { get; set; }
        public List<Airport> Airports { get; set; }
        public List<Traveller> Travellers { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Planes = new();
            Airports = new();
            Travellers = new();
            TripsCompleted = 0;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            // Create new traveller every X seconds
        }

        public void AddPlane(Plane plane)
        {
            Planes.Add(plane);
        }

        public void AddAirport(Airport airport)
        {
            Airports.Add(airport);
        }

        public void AddTraveller(Traveller traveller)
        {
            traveller.OnArrival += OnTravellerArrived;
            Travellers.Add(traveller);
        }

        public void AddConnection(Connection connection)
        {
            bool connectionAdded = false;
            foreach (Airport airport in Airports)
            {
                if (connection.HasConnection(airport))
                {
                    // Add connection to both airports
                    airport.AddConnection(connection);
                    connection.GetOther(airport).AddConnection(connection);
                    connectionAdded = true;
                    break;
                }
            }

            if (connectionAdded)
            {
                foreach (var traveller in Travellers)
                {
                    // Update route for all travellers; new path may be shorter
                    traveller.CalculateRouteTo(traveller.Destination);
                }
            }
        }

        private void OnTravellerArrived(object sender, EventArgs e)
        {
            var traveller = (Traveller)sender;
            GD.Print($"Traveller arrived at {traveller.Destination.AirportName}");
            Travellers.Remove(traveller);
            TripsCompleted++;
        }
    }
}