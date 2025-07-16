using Microsoft.AspNetCore.Mvc;
using HMCSnacks.Data;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class LocationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LocationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("GetCitiesByStateId/{stateId}")]
    public IActionResult GetCitiesByStateId(int stateId)
    {
        var cities = _context.Cities
            .Where(c => c.StateId == stateId && c.IsActive)
            .Select(c => new { c.Id, c.CityName })
            .ToList();

        return Ok(cities);
    }
}

