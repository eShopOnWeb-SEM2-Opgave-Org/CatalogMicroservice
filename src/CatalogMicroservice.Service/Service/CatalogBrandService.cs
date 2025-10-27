using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Common.Models;
using CatalogMicroservice.Infrastructure.Interfaces;

namespace CatalogMicroservice.Service.Service;
public class CatalogBrandService : ICatalogBrandService
{
    private readonly ICatalogBrandRepository _brandRepository;

    public CatalogBrandService(ICatalogBrandRepository brandRepository)
    {
        this._brandRepository = brandRepository;
    }

    public async Task<CatalogBrand?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default)
    {
        CatalogBrand? brand = await _brandRepository.GetBrandByIdAsync(brandId, cancellationToken);
        return brand;
    }

    public async Task<IEnumerable<CatalogBrand>> GetBrandsAsync(CancellationToken token = default)
    {
        IEnumerable<CatalogBrand> brands = await _brandRepository.GetBrandsAsync(token);
        return brands;
    }
}
