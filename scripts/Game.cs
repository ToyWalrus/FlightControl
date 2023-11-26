using System;
using System.Collections.Generic;
using Godot;

namespace FlightControl
{
    public partial class Game : Node2D
    {
        public List<Plane> Planes { get; set; }
        public List<Airport> Airports { get; set; }
        public List<Traveller> Travellers { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Planes = new();
            Airports = new();
            Travellers = new();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            // Create new traveller every X seconds
        }
    }
}