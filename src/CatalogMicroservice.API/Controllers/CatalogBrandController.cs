using Microsoft.AspNetCore.Mvc;
using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Common.Models;

namespace CatalogMicroservice.API.Controllers;

[ApiController]
[Route("api/catalog-brands")]
public class CatalogBrandsController : ControllerBase
{
    private readonly ICatalogBrandService _service;

    public CatalogBrandsController(ICatalogBrandService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogBrand>>> GetAll(CancellationToken ct)
    {
        var brands = await _service.GetBrandsAsync(ct);
        return Ok(brands);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogBrand>> GetById(int id, CancellationToken ct)
    {
        var brand = await _service.GetBrandByIdAsync(id, ct);
        return brand is null ? NoContent() : Ok(brand);
    }
}
