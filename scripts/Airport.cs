using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FlightControl
{
    public partial class Airport : Node2D
    {
        [Export]
        public string AirportName { get; set; }

        [Export]
        public int TravellerMax { get; set; }
        public int TravellerCount { get => travellers.Count; }

        [Export]
        public int PlaneMax { get; set; }
        public int PlaneCount { get => planes.Count; }

        public System.Collections.ObjectModel.ReadOnlyCollection<Connection> Connections
        {
            get => connections.AsReadOnly();
        }

        private readonly Dictionary<Airport, List<Traveller>> travellerDestinations = new();
        private readonly Dictionary<Plane, double> planeWaitCounter = new();
        private readonly List<Connection> connections = new();
        private readonly List<Traveller> travellers = new();
        private readonly List<Plane> planes = new();

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            UnloadReadyPlanes();
            UpdatePlaneWaitCounter(delta);
            LoadAndDepartReadyPlanes();
        }

        public void AddConnection(Connection connection)
        {
            connections.Add(connection);
        }

        public bool CanAddTraveller()
        {
            return TravellerCount < TravellerMax;
        }

        public void AddTraveller(Traveller traveller)
        {
            travellers.Add(traveller);
            UpdateDestinations();
        }

        public bool CanLandPlane()
        {
            return PlaneCount < PlaneMax;
        }

        public void LandPlane(Plane plane)
        {
            plane.Land(this);
            planes.Add(plane);

            GD.Print($"{plane.Name} arrived at {AirportName}");
        }

        public bool CanUnloadPlane(Plane plane)
        {
            if (plane.PassengerCount == 0) return false;
            if (TravellerCount + plane.PassengerCount <= TravellerMax) return true;
            return TravellerCount + plane.GetPassengersNotAtFinalDestination(this).Count() <= TravellerMax;
        }

        public void UnloadPlane(Plane plane)
        {
            var waitingTravellers = plane.GetPassengersNotAtFinalDestination(this);

            // Unload passengers
            plane.Unload();

            // Add waiting travellers to airport
            travellers.AddRange(waitingTravellers);

            // Update destinations after travellers list changed
            UpdateDestinations();
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
            return connections.Find(c => c.HasConnection(destination));
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
                    planeWaitCounter.Add(plane, plane.MinWaitTime);
                }
            }
        }

        private void UnloadReadyPlanes()
        {
            foreach (var plane in planes)
            {
                if (CanUnloadPlane(plane))
                {
                    UnloadPlane(plane);
                }
            }
        }

        private void LoadAndDepartReadyPlanes()
        {
            var oldPlaneList = new List<Plane>(planes);
            foreach (var plane in oldPlaneList)
            {
                if (planeWaitCounter.ContainsKey(plane) && planeWaitCounter[plane] <= 0)
                {
                    LoadPlane(plane);
                    DepartPlane(plane);
                }
            }
        }

        private void LoadPlane(Plane plane)
        {
            var topDestination = GetTopDestinations(1).FirstOrDefault();
            if (topDestination == null) return;

            var travellersToLoad = travellerDestinations[topDestination];
            var travellersStillWaiting = plane.Load(travellersToLoad);

            if (travellersToLoad.Count > 0)
            {
                plane.SetConnection(GetConnectionTo(topDestination));
                travellers.RemoveAll(t => travellersToLoad.Contains(t) && !travellersStillWaiting.Contains(t));
                UpdateDestinations();
            }
        }

        private void DepartPlane(Plane plane)
        {
            if (plane.PassengerCount == 0) return;

            plane.TakeOff();
            planes.Remove(plane);
            planeWaitCounter.Remove(plane);

            GD.Print($"{plane.Name} departed from {AirportName}");
        }

        public static bool operator ==(Airport a, Airport b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            return a?.Equals(b) ?? false;
        }

        public static bool operator !=(Airport a, Airport b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return AirportName == ((Airport)obj).AirportName;
        }

        public override int GetHashCode()
        {
            return AirportName.GetHashCode();
        }
    }
}