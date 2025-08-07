using NetTopologySuite.Geometries;
using System.Drawing;

namespace EnergomeraTest.Services
{
    public class GeodeticCalculator
    {
        private const double EarthRadius = 6371000; // Метры

        public double CalculateDistance(NetTopologySuite.Geometries.Point p1, NetTopologySuite.Geometries.Point p2)
        {
            var dLat = ToRadians(p2.Y - p1.Y);
            var dLon = ToRadians(p2.X - p1.X);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(p1.Y)) * Math.Cos(ToRadians(p2.Y)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadius * c;
        }

        public double CalculateArea(Polygon polygon)
        {
            var coords = polygon.ExteriorRing.Coordinates;
            double area = 0;

            for (int i = 0; i < coords.Length - 1; i++)
            {
                var p1 = coords[i];
                var p2 = coords[i + 1];
                area += ToRadians(p2.X - p1.X) *
                       (2 + Math.Sin(ToRadians(p1.Y)) + Math.Sin(ToRadians(p2.Y)));
            }

            area = Math.Abs(area * EarthRadius * EarthRadius / 2);
            return area;
        }

        private static double ToRadians(double degrees) =>
            degrees * Math.PI / 180;
    }
}