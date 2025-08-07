namespace EnergomeraTest.Models
{
    public class LocationResponse
    {
        public required double[] Center { get; set; }
        public required double[][] Polygon { get; set; }
    }
}