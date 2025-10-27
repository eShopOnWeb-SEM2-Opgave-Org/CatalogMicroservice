using Microsoft.AspNetCore.Mvc;
using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Common.Models;

namespace CatalogMicroservice.API.Controllers;

[ApiController]
[Route("api/catalog-items")]
public class CatalogItemController : ControllerBase
{
    private readonly ICatalogItemService _service;

    public CatalogItemController(ICatalogItemService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogItem>>> GetPage(
        // en ide til at lave pagination og filtrering, her fra starten, skal nok ændres ERH
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? brandId = null,
        [FromQuery] int? typeId = null,
        CancellationToken ct = default)
    {
        if (pageNo < 1 || pageSize < 1)
            return BadRequest("pageNo og pageSize skal være >= 1.");

        IEnumerable<CatalogItem> items = await _service.GetItemsAsync(pageNo, pageSize, brandId, typeId, ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogItem>> GetById(int id, CancellationToken ct)
    {
        CatalogItem? item = await _service.GetItemAsync(id, ct);
        return item is null ? NoContent() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CatalogItem>> Create([FromBody] CreateCatalogItem dto, CancellationToken ct)
    {
        if (dto is null)
            return BadRequest("Missing Model");

        CatalogItem? created = await _service.CreateItemAsync(dto, ct);
        if (created is null)
            return Problem("failed to create new item");

        return Created($"api/catalog-item/{created.Id}", created);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCatalogItem model, CancellationToken ct)
    {
        if (model is null)
            return BadRequest("Missing model");

        CatalogItem? existing = await _service.GetItemAsync(model.Id, ct);
        if (existing is null) return NotFound();

        await _service.UpdateItemAsync(model, ct);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        CatalogItem? existing = await _service.GetItemAsync(id, ct);
        if (existing is null) return NotFound();

        await _service.DeleteItemAsync(id, ct);
        return Ok();
    }
}
