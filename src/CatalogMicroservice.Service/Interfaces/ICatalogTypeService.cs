using CatalogMicroservice.Common.Models;

namespace CatalogMicroservice.Service.Interfaces;

public interface ICatalogTypeService
{
    Task<CatalogType?> GetCatalogTypeAsync(int typeId, CancellationToken token = default);
    Task<IEnumerable<CatalogType>> GetCatalogTypesAsync(CancellationToken token = default);
}
