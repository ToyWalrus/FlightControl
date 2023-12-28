using Godot;
using System;

namespace FlightControl
{
	public partial class ConnectionCreator : Node2D
	{
		[Export]
		public PackedScene ConnectionScene;
		[Export]
		public PackedScene PlaneScene;
		private Airport clickedAirport;

		public override void _Ready()
		{
			if (ConnectionScene == null)
			{
				GD.PushError("Must assign a default Connection scene to ConnectionCreator");
			}
			if (PlaneScene == null)
			{
				GD.PushWarning("Must assign a default Plane scene to ConnectionCreator");
			}

			// Commenting out for now since Demo.cs adds airports to the Game script

			// var airports = GetTree().GetNodesInGroup("Airports");
			// foreach (var airport in airports)
			// {
			// 	NewAirportAdded((Airport)airport);
			// }
		}

		public void NewAirportAdded(Airport airport)
		{
			airport.Connect("OnClickAirport", new Callable(this, nameof(OnClickAirport)));
		}

		private void OnClickAirport(Airport airport)
		{
			if (clickedAirport == null)
			{
				clickedAirport = airport;
				GD.Print($"Set initial airport to {airport.AirportName}");
				// clickedAirport.SetSelected(true);
			}
			else if (clickedAirport != airport)
			{
				if (clickedAirport.IsConnectedTo(airport))
				{
					GD.Print($"Already connected {clickedAirport.AirportName} to {airport.AirportName}");
					return;
				}

				var connection = ConnectionScene.Instantiate<Connection>();
				connection.Name = $"{clickedAirport.AirportName} to {airport.AirportName}";
				connection.PortA = clickedAirport;
				connection.PortB = airport;

				var plane = PlaneScene.Instantiate<Plane>();
				connection.AddPlane(plane);
				AddChild(connection);

				// clickedAirport.SetSelected(false);
				clickedAirport = null;

				GD.Print($"Connected {connection.PortA.AirportName} to {connection.PortB.AirportName}");
			}
		}

		public override void _Process(double delta)
		{
			QueueRedraw();
		}

		public override void _Input(InputEvent @event)
		{
			if (Input.IsActionJustPressed("right_click") && clickedAirport != null)
			{
				// clickedAirport.SetSelected(false);
				clickedAirport = null;

				GD.Print("Cancelled connection creation");
			}
		}

		public override void _Draw()
		{
			if (clickedAirport == null) return;

			var airportPos = clickedAirport.GlobalPosition;
			var mousePos = GetGlobalMousePosition();

			if (ConnectionScene._Bundled.TryGetValue("HighlightColor", out var highlightColor))
			{
				DrawLine(airportPos, mousePos, highlightColor.AsColor(), 2);
			}
			else
			{
				DrawLine(airportPos, mousePos, Colors.White, 2);
			}
		}
	}
}
