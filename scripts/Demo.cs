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
				var plane3 = PlaneScene.Instantiate<Plane>();
				plane1.Name = "Plane 1";
				plane2.Name = "Plane 2";
				plane3.Name = "Plane 3";
				plane1.GlobalPosition = airports[0].GlobalPosition;
				plane2.GlobalPosition = airports[1].GlobalPosition;
				plane3.GlobalPosition = airports[0].GlobalPosition;

				game.AddChild(plane1);
				game.AddChild(plane2);
				game.AddChild(plane3);

				airports[0].LandPlane(plane1);
				airports[1].LandPlane(plane2);
				airports[0].LandPlane(plane3);

				var con1 = new Connection
				{
					PortA = airports[0],
					PortB = airports[1]
				};
				var con2 = new Connection
				{
					PortA = airports[1],
					PortB = airports[2]
				};
				var con3 = new Connection
				{
					PortA = airports[2],
					PortB = airports[0]
				};
				var con4 = new Connection
				{
					PortA = airports[0],
					PortB = airports[3]
				};
				var con5 = new Connection
				{
					PortA = airports[3],
					PortB = airports[2]
				};


				game.AddConnection(con1);
				game.AddConnection(con2);
				game.AddConnection(con3);
				game.AddConnection(con4);
				game.AddConnection(con5);

				game.AddChild(con1);
				game.AddChild(con2);
				game.AddChild(con3);
				game.AddChild(con4);
				game.AddChild(con5);
			}
		}
	}
}
