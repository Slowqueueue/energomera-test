using System.Drawing;
using NetTopologySuite.Geometries;

namespace EnergomeraTest.Models
{
    public class Field
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required Polygon Geometry { get; set; }
        public required NetTopologySuite.Geometries.Point Centroid { get; set; }
        public double Area { get; set; }
    }
}