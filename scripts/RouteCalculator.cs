using System;
using System.Collections.Generic;
using Godot;

namespace FlightControl
{
    public class RouteCalculator
    {
        public static Route CalculateShortestRoute(Airport start, Airport end)
        {
            var queue = new Queue<Airport>();
            var airportMap = new Dictionary<Airport, Airport>();

            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == end)
                {
                    return ReconstructPath(airportMap, start, end);
                }

                foreach (var connection in current.Connections)
                {
                    var next = connection.GetOther(current);
                    if (!airportMap.ContainsKey(next))
                    {
                        airportMap[next] = current;
                        queue.Enqueue(next);
                    }
                }
            }

            return null; // No path found
        }

        private static Route ReconstructPath(Dictionary<Airport, Airport> airportMap, Airport start, Airport end)
        {
            var path = new List<Connection>();
            var current = end;

            while (current != start)
            {
                path.Add(current.GetConnectionTo(airportMap[current]));
                current = airportMap[current];
            }

            path.Reverse();

            return new Route(path);
        }
    }
}
