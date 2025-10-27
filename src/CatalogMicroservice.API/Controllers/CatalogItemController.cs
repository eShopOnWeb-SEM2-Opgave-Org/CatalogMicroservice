using Microsoft.AspNetCore.Mvc;
using CatalogMicroservice.Infrastructure.Interfaces;
using CatalogMicroservice.Common.Models;

namespace CatalogMicroservice.API.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogItemController : ControllerBase
{
    private readonly ICatalogItemRepository _repository;

    public CatalogItemController(ICatalogItemRepository repository) => _repository = repository;

    [HttpGet("item/page")]
    public async Task<ActionResult<IEnumerable<CatalogItem>>> GetPage(
        // en ide til at lave pagination og filtrering, her fra starten, skal nok ændres ERH
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? brandId = null,
        [FromQuery] int? typeId = null,
        CancellationToken ct = default)
    {
        if (pageNo < 1 || pageSize < 1) return BadRequest("pageNo og pageSize skal være >= 1.");

        var items = await _repository.GetItemPageAsync(pageNo, pageSize, brandId, typeId, ct);
        return Ok(items);
    }

    [HttpGet("item/{id:int}")]
    public async Task<ActionResult<CatalogItem>> GetById(int id, CancellationToken ct)
    {
        var item = await _repository.GetItemAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("item")]
    public async Task<ActionResult<CatalogItem>> Create([FromBody] CreateCatalogItem dto, CancellationToken ct)
    {
        if (dto is null) return BadRequest("Body mangler.");

        var created = await _repository.CreateItemAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("item/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CatalogItem model, CancellationToken ct)
    {
        if (model is null || model.Id != id)
            return BadRequest("Id i route og body skal matche.");

        var existing = await _repository.GetItemAsync(id, ct);
        if (existing is null) return NotFound();

        await _repository.UpdateItemAsync(model, ct);
        return NoContent();
    }

    [HttpDelete("item/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _repository.GetItemAsync(id, ct);
        if (existing is null) return NotFound();

        await _repository.DeleteItemAsync(id, ct);
        return NoContent();
    }
}
