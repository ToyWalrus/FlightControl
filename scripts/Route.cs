using System.Collections.Generic;

namespace FlightControl
{
	// Temporary class to hold data for a connection between two airports.
	// Each traveller will have a single Route that they need to complete.
	public class Route
	{
		private readonly List<Connection> connections;

		public Route(List<Connection> connections)
		{
			this.connections = connections;
		}

		public void MarkAsTravelled(Connection connection)
		{
			connections.Remove(connection);
		}

		public Airport GetNextAirport(Airport currentAirport)
		{
			if (currentAirport == null)
			{
				return connections[0].PortA;
			}

			foreach (var connection in connections)
			{
				if (connection.HasConnection(currentAirport))
				{
					return connection.GetOther(currentAirport);
				}
			}

			return null;
		}
	}
}
