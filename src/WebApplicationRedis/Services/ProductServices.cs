using WebApplicationRedis.Domain;
using WebApplicationRedis.Domain.Dtos;
using WebApplicationRedis.Domain.Entitys;
using WebApplicationRedis.InputModels.ProductInputModels;
using WebApplicationRedis.Services.Contracts;
using WebApplicationRedis.ViewModels.ProductViewModels;

namespace WebApplicationRedis.Services
{
    public class ProductServices : IProductServices
    {
        private readonly IProductWriteRepository _productWriteRepository;
        private readonly IProductReadRepository _productReadRepository;
        private readonly IRedisCacheRepository _redisCacheRepository;

        public ProductServices(IProductWriteRepository productWriteRepository,
            IProductReadRepository productReadRepository,
            IRedisCacheRepository redisCacheRepository)
        {
            this._productWriteRepository = productWriteRepository;
            this._productReadRepository = productReadRepository;
            this._redisCacheRepository = redisCacheRepository;
        }

        #region Implement

        public async Task<ProductViewModel> GetProductAsync(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("Product Id Is Invalid");

            string cacheKey = "getProductsAsync";//you can get from confige files
            int cacheTimeOut = 180;//you can get from confige files

            var cacheResult = await GetAsync<ProductViewModel>(cacheKey);
            if (cacheResult != null)
                return cacheResult;

            var productDto = await _productReadRepository.GetProductAsync(productId).ConfigureAwait(false);
            if (productDto == null)
                return new ProductViewModel();

            var productViewModel = CreateProductViewModelFromProductDto(productDto);

            await SetAsync(cacheKey, productViewModel, cacheTimeOut).ConfigureAwait(false);

            return productViewModel;
        }

        public async Task<IEnumerable<ProductViewModel>> GetProductsAsync()
        {
            string cacheKey = "getProductsAsync";
            int cacheTimeOut = 180;

            var cacheResult = await GetAsync<IEnumerable<ProductViewModel>>(cacheKey);

            if (cacheResult != null)
                return cacheResult;

            var productDtos = await _productReadRepository.GetProductsAsync().ConfigureAwait(false);

            if (productDtos == null || productDtos.Count() == 0)
                return Enumerable.Empty<ProductViewModel>();

            var productViewModels = CreateProductViewModelsFromProductDtos(productDtos);

            await SetAsync(cacheKey, productViewModels, cacheTimeOut).ConfigureAwait(false);

            return productViewModels;
        }

        public async Task<int> CreateProductAsync(CreateProductInputModel inputModel)
        {
            ValidateProductName(inputModel.ProductName);

            ValidateProductTitle(inputModel.ProductTitle);

            var productEntoty = CreateProductEntityFromInputModel(inputModel);

            return await _productWriteRepository.CreateProductAsync(productEntoty).ConfigureAwait(false);
        }

        public async Task UpdateProductAsync(UpdateProductInputModel inputModel)
        {
            if (inputModel.ProductId <= 0)
                throw new ArgumentException("ProductId Is Invalid.");

            ValidateProductName(inputModel.ProductName);

            ValidateProductTitle(inputModel.ProductTitle);

            await IsExistProduct(inputModel.ProductId).ConfigureAwait(false);

            var productEntoty = CreateProductEntityFromInputModel(inputModel);

            await _productWriteRepository.UpdateProductAsync(productEntoty).ConfigureAwait(false);
        }

        public async Task DeleteProductAsync(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("ProductId Is Invalid.");

            string cacheKey = "getProductsAsync";

            await IsExistProduct(productId).ConfigureAwait(false);

            await _productWriteRepository.DeleteProductAsync(productId).ConfigureAwait(false);

            DeleteCache(cacheKey);
        }

        #endregion Implement

        #region Cache

        private void DeleteCache(string key)
           => _redisCacheRepository.Delete(key);

        private async Task SetAsync<T>(string key, T result, int cacheTimeOut)
            => await _redisCacheRepository
                 .SetAsync(key, result, TimeSpan.FromMinutes(cacheTimeOut));

        private async Task<T> GetAsync<T>(string cacheKey)
            => await _redisCacheRepository
                .GetAsync<T>(cacheKey);

        #endregion Cache


        #region Private

        private async Task IsExistProduct(int productId)
        {
            var isExistProduct = await _productReadRepository.IsExistProductAsync(productId).ConfigureAwait(false);
            if (isExistProduct == false)
                throw new ArgumentException("ProductId Is Not Found.");
        }

        private Product CreateProductEntityFromInputModel(CreateProductInputModel inputModel)
            => new Product(inputModel.ProductName, inputModel.ProductTitle, inputModel.ProductDescription, inputModel.MainImageName, inputModel.MainImageTitle, inputModel.MainImageUri, inputModel.IsExisting, inputModel.IsFreeDelivery, inputModel.Weight);

        private Product CreateProductEntityFromInputModel(UpdateProductInputModel inputModel)
            => new Product(inputModel.ProductId, inputModel.ProductName, inputModel.ProductTitle, inputModel.ProductDescription, inputModel.MainImageName, inputModel.MainImageTitle, inputModel.MainImageUri, inputModel.IsExisting, inputModel.IsFreeDelivery, inputModel.Weight);

        private ProductViewModel CreateProductViewModelFromProductDto(ProductDto dto)
            => new ProductViewModel()
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                ProductTitle = dto.ProductTitle,
                ProductDescription = dto.ProductDescription,
                MainImageName = dto.MainImageName,
                MainImageTitle = dto.MainImageTitle,
                MainImageUri = dto.MainImageUri,
                IsExisting = dto.IsExisting,
                IsFreeDelivery = dto.IsFreeDelivery,
                Weight = dto.Weight
            };

        private IEnumerable<ProductViewModel> CreateProductViewModelsFromProductDtos(IEnumerable<ProductDto> dtos)
        {
            ICollection<ProductViewModel> productViewModels = new List<ProductViewModel>();

            foreach (var ProductDto in dtos)
                productViewModels.Add(
                     new ProductViewModel()
                     {

                         ProductId = ProductDto.ProductId,
                         ProductName = ProductDto.ProductName,
                         ProductTitle = ProductDto.ProductTitle,
                         ProductDescription = ProductDto.ProductDescription,
                         MainImageName = ProductDto.MainImageName,
                         MainImageTitle = ProductDto.MainImageTitle,
                         MainImageUri = ProductDto.MainImageUri,
                         IsExisting = ProductDto.IsExisting,
                         IsFreeDelivery = ProductDto.IsFreeDelivery,
                         Weight = ProductDto.Weight
                     });


            return (IEnumerable<ProductViewModel>)productViewModels;
        }

        private void ValidateProductName(string productName)
        {
            if (string.IsNullOrEmpty(productName) || string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException(nameof(productName), "Product Name cannot be nul.l");
        }

        private void ValidateProductTitle(string productTitle)
        {
            if (string.IsNullOrEmpty(productTitle) || string.IsNullOrWhiteSpace(productTitle))
                throw new ArgumentException(nameof(productTitle), "Product Title cannot be null.");
        }

        #endregion Private
    }
}
