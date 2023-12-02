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
				for (int i = 0; i < airports.Count; i++)
				{
					var airport = airports[i];
					var plane = PlaneScene.Instantiate<Plane>();
					plane.Name = $"Plane {i + 1}";
					plane.GlobalPosition = airport.GlobalPosition;
					airport.LandPlane(plane);
					game.AddChild(plane);

					for (int j = i + 1; j < airports.Count; j++)
					{
						var otherAirport = airports[j];
						var con = new Connection
						{
							PortA = airport,
							PortB = otherAirport
						};
						game.AddConnection(con);
						game.AddChild(con);
					}
				}
			}
		}
	}
}
