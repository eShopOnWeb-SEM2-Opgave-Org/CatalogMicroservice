using Microsoft.AspNetCore.Mvc;
using CatalogMicroservice.Infrastructure.Interfaces;
using CatalogMicroservice.Common.Models;

namespace CatalogMicroservice.API.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogBrandsController : ControllerBase
{
    private readonly ICatalogBrandRepository _repository;

    public CatalogBrandsController(ICatalogBrandRepository repository) => _repository = repository;

    [HttpGet("brand")]
    public async Task<ActionResult<IEnumerable<CatalogBrand>>> GetAll(CancellationToken ct)
    {
        var brands = await _repository.GetBrandsAsync(ct);
        return Ok(brands);
    }

    [HttpGet("brand/{id:int}")]
    public async Task<ActionResult<CatalogBrand>> GetById(int id, CancellationToken ct)
    {
        var brand = await _repository.GetBrandByIdAsync(id, ct);
        return brand is null ? NotFound() : Ok(brand);
    }
}
