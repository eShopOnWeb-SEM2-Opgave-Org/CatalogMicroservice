using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Common.Models;
using CatalogMicroservice.Infrastructure.Interfaces;

namespace CatalogMicroservice.Service.Service;

public class CatalogTypeService : ICatalogTypeService
{
    private readonly ICatalogTypeRepository _typeRepository;

    public CatalogTypeService(ICatalogTypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task<CatalogType?> GetCatalogTypeAsync(int typeId, CancellationToken token = default)
    {
        CatalogType? type = await _typeRepository.GetCatalogTypeAsync(typeId, token);
        return type;
    }

    public async Task<IEnumerable<CatalogType>> GetCatalogTypesAsync(CancellationToken token = default)
    {
        IEnumerable<CatalogType> types = await _typeRepository.GetCatalogTypesAsync(token);
        return types;
    }
}
