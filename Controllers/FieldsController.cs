using EnergomeraTest.Models;
using EnergomeraTest.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergomeraTest.Controllers
{
    [ApiController]
    [Route("api/fields")]
    public class FieldsController(KmlDataService dataService, GeodeticCalculator calculator) : ControllerBase
    {
        private readonly KmlDataService _dataService = dataService;
        private readonly GeodeticCalculator _calculator = calculator;

        [HttpGet]
        public IActionResult GetAllFields()
        {
            var fields = _dataService.GetAllFields();
            var response = fields.Select(f => new FieldResponse
            {
                Id = f.Id,
                Name = f.Name,
                Size = f.Area / 10000, // Конвертация в гектары
                Locations = new LocationResponse
                {
                    Center = [f.Centroid.Y, f.Centroid.X],
                    Polygon = [.. f.Geometry.ExteriorRing.Coordinates.Select(c => new[] { c.Y, c.X })]
                }
            });
            return Ok(response);
        }

        [HttpGet("{id}/size")]
        public IActionResult GetFieldSize(string id)
        {
            var field = _dataService.GetFieldById(id);
            if (field == null) return NotFound();
            return Ok(new { size = Math.Round(field.Area / 10000, 2) });
        }

        [HttpGet("{id}/distance")]
        public IActionResult CalculateDistance(string id, [FromQuery] double lat, [FromQuery] double lng)
        {
            var field = _dataService.GetFieldById(id);
            if (field == null) return NotFound();

            var point = new NetTopologySuite.Geometries.Point(lng, lat);
            double distance = _calculator.CalculateDistance(field.Centroid, point);
            return Ok(new { distance = Math.Round(distance, 2) });
        }

        [HttpGet("contains")]
        public IActionResult ContainsPoint([FromQuery] double lat, [FromQuery] double lng)
        {
            var point = new NetTopologySuite.Geometries.Point(lng, lat);
            var field = _dataService.GetFieldContainingPoint(point);

            if (field == null) return Ok(false);

            return Ok(new { id = field.Id, name = field.Name });
        }
    }
}