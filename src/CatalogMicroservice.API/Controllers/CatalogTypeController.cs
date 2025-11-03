using Microsoft.AspNetCore.Mvc;
using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Common.Models;
using Microsoft.AspNetCore.Cors;

namespace CatalogMicroservice.API.Controllers;

[ApiController]
[Route("api/catalog-types")]
[EnableCors("default-policy")]
public class CatalogTypeController : ControllerBase
{
    private readonly ICatalogTypeService _service;

    public CatalogTypeController(ICatalogTypeService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogType>>> GetAll(CancellationToken ct)
    {
        IEnumerable<CatalogType> types = await _service.GetCatalogTypesAsync(ct);
        return Ok(types);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogType>> GetById(int id, CancellationToken ct)
    {
        CatalogType? type = await _service.GetCatalogTypeAsync(id, ct);
        return type is null ? NoContent() : Ok(type);
    }
}
