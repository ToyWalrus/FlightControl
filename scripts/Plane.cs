using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FlightControl
{
	public partial class Plane : Node2D
	{
		[Export]
		public double Speed { get; set; } = 5;

		[Export]
		public int PassengerMax { get; set; }
		public int PassengerCount { get => passengers.Count; }

		[Export]
		public Connection Connection { get; set; }

		// in seconds
		[Export]
		public double MaxWaitTime { get; set; } = 3;

		private Airport currentAirport;
		private Airport destination;
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
				Land(destination);
			}
			else
			{
				// Update position and rotation
				var direction = (destination.GlobalPosition - GlobalPosition).Normalized();
				GlobalPosition += direction * (float)Speed * (float)delta * SPEED_MODIFIER;
				UpdateRotation();
			}
		}

		public bool IsFull()
		{
			return PassengerCount == PassengerMax;
		}

		public void SetConnection(Connection connection, bool portAIsCurrent = true)
		{
			Connection = connection;
			connection.AddPlane(this);

			Land(portAIsCurrent ? connection.PortA : connection.PortB);
		}

		public Airport GetDestination()
		{
			return Connection.GetOther(currentAirport);
		}

		public IEnumerable<Traveller> Unload()
		{
			passengers.ForEach(p =>
			{
				p.MarkConnectionComplete(Connection);
				p.SetCurrentAirport(currentAirport);
			});

			var unloaded = passengers.Where(p => !p.HasArrivedAtDestination).ToList();

			passengers.Clear();

			return unloaded;
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

		public void TakeOff()
		{
			destination = GetDestination();
			currentAirport = null;
			UpdateRotation();
			SetPlaneColorIndicator();
			GetNode<Sprite2D>("Sprite2D").Visible = true;
		}

		private void Land(Airport airport)
		{
			airport.PlaneArrived(this);
			currentAirport = airport;
			destination = null;
			GlobalPosition = airport.GlobalPosition;
			GetNode<Sprite2D>("Sprite2D").Visible = false;
		}

		private void UpdateRotation()
		{
			if (destination == null) return;

			var direction = (destination.GlobalPosition - GlobalPosition).Normalized();
			var angle = direction.Angle();

			// Offset by 90 degrees since the sprite is facing up
			Rotation = angle + Mathf.Pi / 2;
		}

		private void SetPlaneColorIndicator()
		{
			var sprite = GetNode<Sprite2D>("Sprite2D");
			var material = sprite.Material as ShaderMaterial;

			if (PassengerCount == 0)
			{
				material.SetShaderParameter("color", Colors.White);
				return;
			}

			var emptyColor = Colors.Green;
			var fullColor = Colors.Red;
			var percentFull = (float)PassengerCount / PassengerMax;

			var r = Mathf.Lerp(emptyColor.R, fullColor.R, percentFull);
			var g = Mathf.Lerp(emptyColor.G, fullColor.G, percentFull);
			var b = Mathf.Lerp(emptyColor.B, fullColor.B, percentFull);

			material.SetShaderParameter("color", new Color(r, g, b));
		}
	}
}
