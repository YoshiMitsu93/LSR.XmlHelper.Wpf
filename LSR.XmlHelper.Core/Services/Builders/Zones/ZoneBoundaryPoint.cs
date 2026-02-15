namespace LSR.XmlHelper.Core.Services.Builders.Zones
{
    public sealed class ZoneBoundaryPoint
    {
        public ZoneBoundaryPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }
}
