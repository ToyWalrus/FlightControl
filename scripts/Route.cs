using System;
using System.Collections.Generic;
using Godot;

namespace FlightControl
{
    public partial class Route : Node2D
    {
        public System.Collections.ObjectModel.ReadOnlyCollection<Connection> Connections
        {
            get => _connections.AsReadOnly();
        }
        private List<Connection> _connections = new();

        public void SetConnections(List<Connection> connections)
        {
            _connections = connections;
        }

        public void AddConnection(Connection connection)
        {
            _connections.Add(connection);
        }

        public Airport GetNextAirport(Airport currentAirport)
        {
            if (currentAirport == null)
            {
                return _connections[0].PortA;
            }

            foreach (Connection connection in _connections)
            {
                if (connection.PortA == currentAirport)
                {
                    return connection.PortB;
                }
                else if (connection.PortB == currentAirport)
                {
                    return connection.PortA;
                }
            }

            return null;
        }
    }
}