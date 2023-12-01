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
		private const float SPEED_MODIFIER = 10;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			GetNode<Sprite2D>("Sprite2D").Visible = false;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (destination == null) return;

			// Check if at destination
			if (GlobalPosition.DistanceTo(destination.GlobalPosition) < SPEED_MODIFIER)
			{
				if (destination.CanLandPlane())
				{
					destination.LandPlane(this);
				}
			}
			else
			{
				// Update position and rotation
				var direction = (destination.GlobalPosition - GlobalPosition).Normalized();
				GlobalPosition += direction * (float)Speed * (float)delta * SPEED_MODIFIER;
				UpdateRotation();
			}
		}


		public void Land(Airport airport)
		{
			origin = airport;
			destination = null;
			GetNode<Sprite2D>("Sprite2D").Visible = false;
		}

		public void TakeOff()
		{
			destination = currentConnection.GetOther(origin);
			UpdateRotation();
			GetNode<Sprite2D>("Sprite2D").Visible = true;
		}

		public void Unload()
		{
			passengers.ForEach(p =>
			{
				p.MarkConnectionComplete(currentConnection);
				p.SetCurrentAirport(origin);
			});
			passengers.Clear();
		}

		public void SetConnection(Connection connection)
		{
			currentConnection = connection;
			GD.Print($"Plane {Name} set connection from {connection.PortA.AirportName} to {connection.PortB.AirportName}");
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

			passengers.AddRange(travellers);
			return new List<Traveller>();
		}

		private void UpdateRotation()
		{
			if (destination == null) return;

			var direction = (destination.GlobalPosition - GlobalPosition).Normalized();
			var angle = direction.Angle();

			// Offset by 90 degrees since the sprite is facing up
			Rotation = angle + Mathf.Pi / 2;
		}
	}
}
