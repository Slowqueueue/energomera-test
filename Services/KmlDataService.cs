using EnergomeraTest.Models;
using NetTopologySuite.Geometries;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Collections.Concurrent;
using System.Drawing;

namespace EnergomeraTest.Services
{
    public class KmlDataService
    {
        private readonly ConcurrentDictionary<string, Field> _fields;

        public KmlDataService(IConfiguration config)
        {
            var basePath = Directory.GetCurrentDirectory();
            var fieldsPath = Path.Combine(basePath, config["KmlPaths:Fields"]!);
            var centroidsPath = Path.Combine(basePath, config["KmlPaths:Centroids"]!);

            _fields = LoadData(fieldsPath, centroidsPath);
        }

        private ConcurrentDictionary<string, Field> LoadData(string fieldsPath, string centroidsPath)
        {
            var fields = ParseFields(fieldsPath);
            var centroids = ParseCentroids(centroidsPath);
            var calculator = new GeodeticCalculator();
            var result = new ConcurrentDictionary<string, Field>();

            Parallel.ForEach(fields, field =>
            {
                if (centroids.TryGetValue(field.Key, out var centroid))
                {
                    result[field.Key] = new Field
                    {
                        Id = field.Key,
                        Name = field.Value.Name,
                        Geometry = field.Value.Geometry,
                        Centroid = centroid,
                        Area = calculator.CalculateArea(field.Value.Geometry)
                    };
                }
            });

            return result;
        }

        private Dictionary<string, (string Name, NetTopologySuite.Geometries.Polygon Geometry)> ParseFields(string path)
        {
            using var stream = File.OpenRead(path);
            var kml = KmlFile.Load(stream);
            var result = new Dictionary<string, (string, NetTopologySuite.Geometries.Polygon)>();
            var factory = new GeometryFactory(new PrecisionModel(), 4326); // SRID=4326

            foreach (var placemark in kml.Root.Flatten().OfType<Placemark>())
            {
                var id = placemark.ExtendedData?.Data
                    .FirstOrDefault(d => d.Name.Equals("id", StringComparison.OrdinalIgnoreCase))?.Value;

                if (string.IsNullOrEmpty(id) || placemark.Geometry is not SharpKml.Dom.Polygon polygon)
                    continue;

                var coordinates = polygon.OuterBoundary.LinearRing.Coordinates
                    .Select(c => new Coordinate(c.Longitude, c.Latitude)).ToArray();

                result[id] = (
                    placemark.Name,
                    factory.CreatePolygon(coordinates)
                );
            }
            return result;
        }

        private Dictionary<string, NetTopologySuite.Geometries.Point> ParseCentroids(string path)
        {
            using var stream = File.OpenRead(path);
            var kml = KmlFile.Load(stream);
            var result = new Dictionary<string, NetTopologySuite.Geometries.Point>();
            var factory = new GeometryFactory(new PrecisionModel(), 4326); // SRID=4326

            foreach (var placemark in kml.Root.Flatten().OfType<Placemark>())
            {
                string? id = null;
                if (placemark.ExtendedData != null)
                {
                    var idData = placemark.ExtendedData.Data
                        .FirstOrDefault(d => d.Name.Equals("id", StringComparison.OrdinalIgnoreCase));

                    id = idData?.Value;
                }

                if (string.IsNullOrEmpty(id))
                {
                    id = placemark.Name;
                }

                if (string.IsNullOrEmpty(id) || placemark.Geometry is not SharpKml.Dom.Point point)
                    continue;

                var coordinate = new Coordinate(
                    point.Coordinate.Longitude,
                    point.Coordinate.Latitude
                );

                var ntsPoint = factory.CreatePoint(coordinate);
                ntsPoint.SRID = 4326;

                if (!result.ContainsKey(id))
                {
                    result.Add(id, ntsPoint);
                }
            }
            return result;
        }

        public IEnumerable<Field> GetAllFields() => _fields.Values;

        public Field? GetFieldById(string id) =>
            _fields.TryGetValue(id, out var field) ? field : null;

        public Field? GetFieldContainingPoint(NetTopologySuite.Geometries.Point point) =>
            _fields.Values.FirstOrDefault(f => f.Geometry.Contains(point));
    }
}