using CatalogMicroservice.Common.Models;

namespace CatalogMicroservice.Service.Interfaces;
public interface ICatalogBrandService
{
    Task<CatalogBrand?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CatalogBrand>> GetBrandsAsync(CancellationToken token = default);
}
