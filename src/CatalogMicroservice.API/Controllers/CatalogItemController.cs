using Microsoft.AspNetCore.Mvc;
using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Common.Models;
using Microsoft.AspNetCore.Cors;
using CatalogMicroservice.Common.Models.Responses;

namespace CatalogMicroservice.API.Controllers;

[ApiController]
[Route("api/catalog-items")]
[EnableCors("default-policy")]
public class CatalogItemController : ControllerBase
{
    private readonly ICatalogItemService _service;

    public CatalogItemController(ICatalogItemService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<CatalogItemPageResponse>> GetPage(
        // en ide til at lave pagination og filtrering, her fra starten, skal nok ændres ERH
        [FromQuery] int pageNo,
        [FromQuery] int pageSize,
        [FromQuery] int? brandId,
        [FromQuery] int? typeId,
        CancellationToken ct = default)
    {
        IEnumerable<CatalogItem> items = await _service.GetItemsAsync(pageNo, pageSize, brandId, typeId, ct);
        int itemCount = await _service.ItemCountAsync(brandId, typeId, ct);

        var response = new CatalogItemPageResponse
        {
            CatalogItems = items,
            PageCount = (int)Math.Ceiling((double)itemCount / pageSize)
        };

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogItem>> GetById(int id, CancellationToken ct)
    {
        CatalogItem? item = await _service.GetItemAsync(id, ct);
        return item is null ? NoContent() : Ok(item);
    }

    [HttpGet("/count")]
    public async Task<ActionResult<int>> GetItemCountAsync(int? brand, int? type, CancellationToken ct = default)
    {
        int count = await _service.ItemCountAsync(brand, type, ct);
        return Ok(count);
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
