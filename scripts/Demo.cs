using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FlightControl
{
    public partial class Demo : Node
    {
        [Export]
        public PackedScene PlaneScene { get; set; }

        public override void _Ready()
        {
            var game = GetNode<Game>("Game");
            var airports = new List<Airport>();
            airports.AddRange(GetTree().GetNodesInGroup("Airports").AsEnumerable().Cast<Airport>());

            foreach (Airport airport in airports)
            {
                game.AddAirport(airport);
            }

            if (airports.Count > 1)
            {
                var plane1 = PlaneScene.Instantiate<Plane>();
                var plane2 = PlaneScene.Instantiate<Plane>();
                plane1.Name = "Plane 1";
                plane2.Name = "Plane 2";
                plane1.GlobalPosition = airports[0].GlobalPosition;
                plane2.GlobalPosition = airports[1].GlobalPosition;

                game.AddChild(plane1);
                game.AddChild(plane2);

                airports[0].LandPlane(plane1);
                airports[1].LandPlane(plane2);

                game.AddConnection(new Connection
                {
                    PortA = airports[0],
                    PortB = airports[1]
                });

                CreateTraveller(airports[0], airports[1]);
            }
        }

        private void CreateTraveller(Airport origin, Airport destination)
        {
            var game = GetNode<Game>("Game");
            var traveller = new Traveller(origin, destination);

            traveller.OnArrival += (_, __) => { CreateTraveller(destination, origin); };

            game.AddTraveller(traveller);
        }

    }
}