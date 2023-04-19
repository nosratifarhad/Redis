using WebApplicationRedis.Domain.Dtos;

namespace WebApplicationRedis.Domain
{
    public interface IProductReadRepository
    {
        Task<ProductDto> GetProductAsync(int productId);

        Task<IEnumerable<ProductDto>> GetProductsAsync();

        Task<bool> IsExistProductAsync(int productId);
    }
}
