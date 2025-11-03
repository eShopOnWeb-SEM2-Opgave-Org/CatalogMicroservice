using Microsoft.AspNetCore.Mvc;
using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Common.Models;
using Microsoft.AspNetCore.Cors;

namespace CatalogMicroservice.API.Controllers;

[ApiController]
[Route("api/catalog-brands")]
[EnableCors("default-policy")]
public class CatalogBrandsController : ControllerBase
{
    private readonly ICatalogBrandService _service;

    public CatalogBrandsController(ICatalogBrandService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogBrand>>> GetAll(CancellationToken ct)
    {
        IEnumerable<CatalogBrand> brands = await _service.GetBrandsAsync(ct);
        return Ok(brands);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogBrand>> GetById(int id, CancellationToken ct)
    {
        CatalogBrand? brand = await _service.GetBrandByIdAsync(id, ct);
        return brand is null ? NoContent() : Ok(brand);
    }
}
