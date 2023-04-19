using WebApplicationRedis.InputModels.ProductInputModels;
using WebApplicationRedis.ViewModels.ProductViewModels;

namespace WebApplicationRedis.Services.Contracts
{
    public interface IProductServices
    {
        Task<int> CreateProductAsync(CreateProductInputModel inputModel);

        Task UpdateProductAsync(UpdateProductInputModel inputModel);

        Task DeleteProductAsync(int productId);

        Task<ProductViewModel> GetProductAsync(int productId);

        Task<IEnumerable<ProductViewModel>> GetProductsAsync();
    }
}
