using System;
using Godot;


namespace FlightControl
{
    public class Traveller
    {
        public Airport Destination { get; private set; }

        private Route route;
        private event EventHandler OnArrival;

        public Traveller(Airport destination)
        {
            SetDestination(destination);
        }

        public Traveller(Airport destination, EventHandler onArrival)
        {
            SetDestination(destination);
            OnArrival += onArrival;
        }

        public void SetDestination(Airport destination)
        {
            Destination = destination;
            // calculate route
        }

        public void MarkConnectionComplete(Connection connection)
        {
            route.MarkAsTravelled(connection);
            if (route.HasArrived)
            {
                OnArrival?.Invoke(this, EventArgs.Empty);
            }
        }

        public Airport GetNextAirport(Airport currentAirport)
        {
            return route.GetNextAirport(currentAirport);
        }
    }
}