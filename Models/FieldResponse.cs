namespace EnergomeraTest.Models
{
    public class FieldResponse
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public double Size { get; set; }
        public required LocationResponse Locations { get; set; }
    }
}