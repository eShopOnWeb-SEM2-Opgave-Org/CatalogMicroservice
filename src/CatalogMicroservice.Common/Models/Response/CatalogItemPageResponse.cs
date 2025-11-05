
namespace CatalogMicroservice.Common.Models.Responses;

public class CatalogItemPageResponse
{
  public required IEnumerable<CatalogItem> CatalogItems { get; init; }

  public required int PageCount { get; init; }
}
