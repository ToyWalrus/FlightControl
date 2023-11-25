using System;
using System.Collections.Generic;
using Godot;

namespace FlightControl
{
    public partial class Plane : Node2D
    {
        [Export]
        public int PassengerMax { get; set; }

        [Export]
        public double Speed { get; set; }

        public List<Traveller> Passengers { get; set; }

        public int PassengerCount { get => Passengers.Count; }

        public Route CurrentRoute { get; set; }

        public bool IsAvailable { get; private set; }

        private Airport currentLocation;
        private Airport currentDestination;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Passengers = new();
            IsAvailable = true;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            // Move plane towards destination
            // If plane is at destination, pause for X seconds
            // After pause, set new destination
            // If no new destination, mark as available
        }

        public void Unload()
        {
            Passengers.Clear();
        }

        public bool CanLoadCount(int count)
        {
            return PassengerCount + count <= PassengerMax;
        }

        public void Load(Traveller traveller)
        {
            Passengers.Add(traveller);
        }

        public void Load(List<Traveller> travellers)
        {
            Passengers.AddRange(travellers);
        }
    }
}
