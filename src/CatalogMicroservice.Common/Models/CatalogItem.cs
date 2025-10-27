
namespace CatalogMicroservice.Common.Models;

public class CatalogItem
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required decimal Price { get; set; }
    public string? PictureUri { get; set; }
    public required int CatalogBrandId { get; set; }
    public required int CatalogTypeId { get; set; }
}
