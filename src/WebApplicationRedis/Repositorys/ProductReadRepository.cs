using WebApplicationRedis.Domain;
using WebApplicationRedis.Domain.Dtos;

namespace WebApplicationRedis.Repositorys
{
    public class ProductReadRepository : IProductReadRepository
    {
        public async Task<ProductDto> GetProductAsync(int productId)
        {
            return await Task.Run(() => new ProductDto());
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            return await Task.Run(async () => Enumerable.Empty<ProductDto>());
        }

        public async Task<bool> IsExistProductAsync(int productId)
        {
            return await Task.Run(() => false);
        }
    }
}
