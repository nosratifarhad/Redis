using WebApplicationRedis.Domain;
using WebApplicationRedis.Domain.Entitys;

namespace WebApplicationRedis.Infra.Repositories.ReadRepositories.ProductReadRepositories
{
    public class ProductReadRepository : IProductReadRepository
    {
        public async Task<Product> GetProductAsync(int productId)
        {
            return await Task.Run(()
            => new Product("inputModel.ProductName", "inputModel.ProductTitle", "inputModel.ProductDescription",
            "inputModel.MainImageName", "inputModel.MainImageTitle", "inputModel.MainImageUri", true,
            true, 0));
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await Task.Run(async () => Enumerable.Empty<Product>());
        }

        public async Task<bool> IsExistProductAsync(int productId)
        {
            return await Task.Run(() => false);
        }
    }
}
