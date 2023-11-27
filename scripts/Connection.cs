namespace FlightControl
{
    public class Connection
    {
        public Airport PortA { get; set; }
        public Airport PortB { get; set; }

        public bool HasConnection(Airport a)
        {
            return a == PortA || a == PortB;
        }

        public bool Connects(Airport a, Airport b)
        {
            return (a == PortA && b == PortB) || (a == PortB && b == PortA);
        }

        public Airport GetOther(Airport a)
        {
            if (a == PortA) return PortB;
            if (a == PortB) return PortA;
            return null;
        }

        public static int CalculateCost(Airport a, Airport b)
        {
            return 0;
        }
    }
}