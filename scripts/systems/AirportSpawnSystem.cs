using Godot;
using System.Collections.Generic;
using System.Text.Json;

namespace FlightControl.Systems
{
    public class AirportSpawnSystem
    {
        // Data from https://github.com/mwgg/Airports
        private static List<AirportJSON> airports;
        public static void LoadAirports()
        {
            var file = FileAccess.Open("res://data/airports.json", FileAccess.ModeFlags.Read);
            var jsonString = file.GetAsText();
            airports = JsonSerializer.Deserialize<List<AirportJSON>>(jsonString);
            file.Close();
        }

        private class AirportJSON
        {
            public string name { get; set; }
            public string icao { get; set; }
            public string iata { get; set; }
            public string country { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
        }
    }
}
