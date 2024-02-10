using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FlightControl
{
	[Tool]
	public partial class Airport : Area2D
	{
		[Signal]
		public delegate void OnTravellerCountChangedEventHandler();
		[Signal]
		public delegate void OnClickAirportEventHandler(Airport airport);

		[Export]
		public string AirportName { get; set; }

		[Export]
		public int TravellerMax { get; set; }
		public int TravellerCount { get => travelers.Count; }

		private readonly Dictionary<Airport, List<Traveller>> travellerDestinations = new();
		private readonly Dictionary<Plane, double> planeWaitCounter = new();
		private readonly HashSet<Connection> connections = new();
		private readonly List<Traveller> travelers = new();
		private readonly HashSet<Plane> planes = new();

		public override void _Ready()
		{
			SetAirportLabelText();
			// GetNode<AirportPropertiesPopup>("AirportProperties").SetAirport(this);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			// to update the name of the airport in the editor
			if (Engine.IsEditorHint())
			{
				SetAirportLabelText();
			}

			UpdatePlaneWaitCounter(delta);
			LoadAndDepartReadyPlanes();
		}

		public IEnumerable<Connection> GetConnections()
		{
			return connections;
		}

		public bool AddConnection(Connection connection)
		{
			return connections.Add(connection);
		}

		public bool RemoveConnection(Connection connection)
		{
			return connections.Remove(connection);
		}

		public bool IsConnectedTo(Airport airport)
		{
			return connections.Any(c => c.HasConnection(airport));
		}

		public void AddTraveller(Traveller traveller)
		{
			travelers.Add(traveller);
			UpdateDestinations();
		}

		public void RemoveTraveller(Traveller traveller)
		{
			travelers.Remove(traveller);
			UpdateDestinations();
		}

		public void PlaneArrived(Plane plane)
		{
			// GD.Print($"{plane.Name} arrived at {AirportName}");
			planes.Add(plane);
			UnloadPlane(plane);
		}

		public IEnumerable<KeyValuePair<Airport, int>> GetTopFinalDestinations(int count)
		{
			var travelerFinalDestinations = new Dictionary<Airport, int>();

			foreach (var traveller in travelers)
			{
				var finalDestination = traveller.Destination;
				if (finalDestination == null) continue;

				if (travelerFinalDestinations.ContainsKey(finalDestination))
				{
					travelerFinalDestinations[finalDestination]++;
				}
				else
				{
					travelerFinalDestinations.Add(finalDestination, 1);
				}
			}

			return travelerFinalDestinations
					.OrderByDescending(kvp => kvp.Value)
					.Select(kvp => new KeyValuePair<Airport, int>(kvp.Key, kvp.Value))
					.Take(count);
		}

		public IEnumerable<Airport> GetTopDestinations(int count)
		{
			return travellerDestinations
					.OrderByDescending(kvp => kvp.Value.Count)
					.Select(kvp => kvp.Key)
					.Take(count);
		}

		public IEnumerable<KeyValuePair<Airport, int>> GetDestinations()
		{
			return travellerDestinations
					.OrderByDescending(kvp => kvp.Value.Count)
					.Select(kvp => new KeyValuePair<Airport, int>(kvp.Key, kvp.Value.Count));
		}

		public Connection GetConnectionTo(Airport destination)
		{
			return connections.FirstOrDefault(c => c.HasConnection(destination));
		}

		private void UnloadPlane(Plane plane)
		{
			// Add unloaded travellers to airport
			travelers.AddRange(plane.Unload());

			// Update destinations after travellers list changed
			UpdateDestinations();
		}

		private void UpdateDestinations()
		{
			travellerDestinations.Clear();

			foreach (var traveller in travelers)
			{
				var nextDestination = traveller.GetNextAirport();
				if (nextDestination == null) continue;

				if (travellerDestinations.ContainsKey(nextDestination))
				{
					travellerDestinations[nextDestination].Add(traveller);
				}
				else
				{
					travellerDestinations.Add(nextDestination, new List<Traveller> { traveller });
				}
			}

			EmitSignal(SignalName.OnTravellerCountChanged);
		}

		private void UpdatePlaneWaitCounter(double delta)
		{
			foreach (var plane in planes)
			{
				if (planeWaitCounter.ContainsKey(plane))
				{
					planeWaitCounter[plane] -= delta;
				}
				else
				{
					planeWaitCounter.Add(plane, plane.MaxWaitTime);
				}
			}
		}

		private void LoadAndDepartReadyPlanes()
		{
			var oldPlaneList = new List<Plane>(planes);
			foreach (var plane in oldPlaneList)
			{
				if (planeWaitCounter.ContainsKey(plane))
				{
					LoadPlane(plane);
				}
				if (planeWaitCounter[plane] <= 0 || plane.IsFull())
				{
					DepartPlane(plane);
				}
			}
		}

		private void LoadPlane(Plane plane)
		{
			var planeDestination = plane.GetDestination();
			if (planeDestination == null)
			{
				GD.Print($"{plane.Name} has no destination");
				return;
			}
			if (!travellerDestinations.ContainsKey(planeDestination)) return;

			var travellersToLoad = travellerDestinations[planeDestination];
			var travellersStillWaiting = plane.Load(travellersToLoad);

			if (travellersToLoad.Count > 0)
			{
				travelers.RemoveAll(t => travellersToLoad.Contains(t) && !travellersStillWaiting.Contains(t));
				UpdateDestinations();
			}
		}

		private void DepartPlane(Plane plane)
		{
			plane.TakeOff();
			planes.Remove(plane);
			planeWaitCounter.Remove(plane);
			// GD.Print($"{plane.Name} departed from {AirportName}");
		}

		public override void _InputEvent(Viewport _, InputEvent @event, int __)
		{
			if (@event.IsActionPressed("click"))
			{
				EmitSignal(SignalName.OnClickAirport, this);
			}

			else if (@event.IsActionPressed("right_click"))
			{
				GD.Print($"Right click on {AirportName}");

				// var popup = GetNode<AirportPropertiesPopup>("AirportProperties");
				// popup.Position = (Vector2I)GetViewport().GetMousePosition();
				// popup.Popup();
			}
		}

		private void SetAirportLabelText()
		{
			GetNode<Label>("AirportNameLabel").Text = AirportName;
		}
	}
}
