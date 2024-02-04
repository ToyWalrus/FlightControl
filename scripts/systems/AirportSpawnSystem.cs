using Godot;
using System.Collections.Generic;

using TinyJson;

namespace FlightControl.Systems
{
    public class AirportSpawnSystem
    {

        // Data from https://github.com/mwgg/Airports
        private static List<AirportInfo> airports;
        public static void LoadAirports()
        {
            var file = FileAccess.Open("res://data/airports.json", FileAccess.ModeFlags.Read);
            var jsonString = file.GetAsText();
            airports = jsonString.FromJson<List<AirportInfo>>();
            file.Close();
        }

        public static List<AirportInfo> Airports
        {
            get
            {
                if (airports == null)
                {
                    LoadAirports();
                }
                return airports;
            }
        }

        public class AirportInfo
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
