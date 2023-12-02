using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FlightControl
{
	[Tool]
	public partial class Airport : Node2D
	{
		[Signal]
		public delegate void OnTravellerCountChangedEventHandler(string count);

		[Export]
		public string AirportName { get; set; }

		[Export]
		public int TravellerMax { get; set; }
		public int TravellerCount { get => travellers.Count; }

		private readonly Dictionary<Airport, List<Traveller>> travellerDestinations = new();
		private readonly Dictionary<Plane, double> planeWaitCounter = new();
		private readonly HashSet<Connection> connections = new();
		private readonly List<Traveller> travellers = new();
		private readonly HashSet<Plane> planes = new();

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
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

		public void AddTraveller(Traveller traveller)
		{
			travellers.Add(traveller);
			UpdateDestinations();
		}

		public void RemoveTraveller(Traveller traveller)
		{
			travellers.Remove(traveller);
			UpdateDestinations();
		}

		public void PlaneArrived(Plane plane)
		{
			// GD.Print($"{plane.Name} arrived at {AirportName}");
			planes.Add(plane);
			UnloadPlane(plane);
		}

		public List<Airport> GetTopDestinations(int count)
		{
			return travellerDestinations
					.OrderByDescending(kvp => kvp.Value.Count)
					.Select(kvp => kvp.Key)
					.Take(count)
					.ToList();
		}

		public Connection GetConnectionTo(Airport destination)
		{
			return connections.FirstOrDefault(c => c.HasConnection(destination));
		}

		private void UnloadPlane(Plane plane)
		{
			// Add unloaded travellers to airport
			travellers.AddRange(plane.Unload());

			// Update destinations after travellers list changed
			UpdateDestinations();
		}

		private void UpdateDestinations()
		{
			travellerDestinations.Clear();

			foreach (var traveller in travellers)
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

			EmitSignal(SignalName.OnTravellerCountChanged, TravellerCount.ToString());
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
				travellers.RemoveAll(t => travellersToLoad.Contains(t) && !travellersStillWaiting.Contains(t));
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
	}
}
