using CatalogMicroservice.Common.Models;

namespace CatalogMicroservice.Infrastructure.Interfaces;

public interface ICatalogBrandRepository
{
    Task<CatalogBrand?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CatalogBrand>> GetBrandsAsync(CancellationToken cancellationToken = default);
}
