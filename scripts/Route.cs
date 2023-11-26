using System.Collections.Generic;

namespace FlightControl
{
    // Temporary class to hold data for a connection between two airports.
    // Each traveller will have a single Route that they need to complete.
    public class Route
    {
        public System.Collections.ObjectModel.ReadOnlyCollection<Connection> Connections
        {
            get => _connections.AsReadOnly();
        }
        private List<Connection> _connections = new();

        public bool HasArrived { get => _connections.Count == 0; }

        public void SetConnections(List<Connection> connections)
        {
            _connections = connections;
        }

        public void MarkAsTravelled(Connection connection)
        {
            _connections.Remove(connection);
        }

        public Airport GetNextAirport(Airport currentAirport)
        {
            if (currentAirport == null)
            {
                return _connections[0].PortA;
            }

            foreach (Connection connection in _connections)
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