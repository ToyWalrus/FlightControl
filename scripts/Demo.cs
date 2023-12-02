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
			var connections = new List<Connection>();

			airports.AddRange(GetTree().GetNodesInGroup("Airports").AsEnumerable().Cast<Airport>());
			connections.AddRange(GetTree().GetNodesInGroup("Connections").AsEnumerable().Cast<Connection>());


			foreach (Airport airport in airports)
			{
				game.AddAirport(airport);
			}

			foreach (Connection connection in connections)
			{
				game.AddConnection(connection);

				var plane = PlaneScene.Instantiate<Plane>();
				plane.SetConnection(connection);
				plane.Name = $"{connection.PortA.AirportName} to {connection.PortB.AirportName}";

				game.AddChild(plane);
			}
		}
	}
}
