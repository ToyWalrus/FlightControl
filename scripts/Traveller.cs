using System;

namespace FlightControl
{
	public class Traveller
	{
		public Airport Destination { get; private set; }

		private Route route;
		private Airport currentAirport;
		public event EventHandler OnArrival;

		public Traveller(Airport origin, Airport destination)
		{
			SetCurrentAirport(origin);
			CalculateRouteTo(destination);
		}

		public void CalculateRouteTo(Airport destination)
		{
			Destination = destination;
			route = RouteCalculator.CalculateShortestRoute(currentAirport, destination);
		}

		public void SetCurrentAirport(Airport airport)
		{
			currentAirport = airport;
			if (currentAirport == Destination)
			{
				OnArrival?.Invoke(this, EventArgs.Empty);
			}
		}

		public void MarkConnectionComplete(Connection connection)
		{
			route.MarkAsTravelled(connection);
		}

		public Airport GetNextAirport()
		{
			return route?.GetNextAirport(currentAirport);
		}
	}
}
