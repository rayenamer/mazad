using Backend.Api.Domain;
using Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/annonces")]
public class AnnoncesController : ControllerBase
{
    private readonly AnnonceService _service;

    public AnnoncesController(AnnonceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AnnonceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<List<AnnonceDto>> Lister([FromQuery] string? statut, [FromQuery] string? q)
    {
        var annonces = _service.Lister(statut, q);

        return Ok(annonces);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AnnonceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AnnonceDetailDto> Obtenir(Guid id)
    {
        var annonce = _service.Obtenir(id);

        return Ok(annonce);
    }

    [HttpPost("{id:guid}/encheres")]
    [ProducesResponseType(typeof(AnnonceDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<AnnonceDetailDto> PlacerEnchere(Guid id, [FromBody] CreerEnchereDto? dto)
    {
        var annonce = _service.PlacerEnchere(id, dto);

        return CreatedAtAction(nameof(Obtenir), new { id }, annonce);
    }
}