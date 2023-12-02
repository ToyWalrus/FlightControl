using System;
using System.Collections.Generic;
using Godot;

namespace FlightControl
{
	public partial class Game : Node2D
	{
		[Signal]
		public delegate void OnTripsCompletedUpdatedEventHandler(string count);

		// in seconds
		[Export]
		public double travellerCreationInterval = 5;

		public int TripsCompleted { get; private set; }
		public List<Plane> Planes { get; set; }
		public List<Airport> Airports { get; set; }
		public List<Traveller> Travellers { get; set; }

		private double travellerCreationCounter = 0;

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
			TravellerCreationTick(delta);
		}

		public void AddPlane(Plane plane)
		{
			Planes.Add(plane);
		}

		public void AddAirport(Airport airport)
		{
			Airports.Add(airport);
		}

		public void CreateTraveller(Airport origin, Airport destination)
		{
			var traveller = new Traveller(origin, destination);
			traveller.OnArrival += OnTravellerArrived;

			origin.AddTraveller(traveller);
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
					var addedToA = airport.AddConnection(connection);
					var addedToB = connection.GetOther(airport).AddConnection(connection);
					connectionAdded = addedToA && addedToB;

					if (connectionAdded)
					{
						break;
					}
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
			// GD.Print($"Traveller arrived at {traveller.Destination.AirportName}");
			Travellers.Remove((Traveller)sender);
			TripsCompleted++;

			EmitSignal(SignalName.OnTripsCompletedUpdated, TripsCompleted.ToString());
		}

		private void TravellerCreationTick(double delta)
		{
			travellerCreationCounter -= delta;
			if (travellerCreationCounter <= 0)
			{
				travellerCreationCounter = travellerCreationInterval;
				CreateTraveller();
			}
		}

		private void CreateTraveller()
		{
			var randomOrigin = Airports[GD.RandRange(0, Airports.Count - 1)];
			var randomDestination = Airports[GD.RandRange(0, Airports.Count - 1)];
			while (randomDestination == randomOrigin)
			{
				randomDestination = Airports[GD.RandRange(0, Airports.Count - 1)];
			}

			CreateTraveller(randomOrigin, randomDestination);
		}
	}
}
