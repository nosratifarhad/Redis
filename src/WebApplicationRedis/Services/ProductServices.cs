using Microsoft.Extensions.Options;
using WebApplicationRedis.Domain;
using WebApplicationRedis.Domain.Dtos;
using WebApplicationRedis.Domain.Entitys;
using WebApplicationRedis.Helpers;
using WebApplicationRedis.InputModels.ProductInputModels;
using WebApplicationRedis.Services.Contracts;
using WebApplicationRedis.ViewModels.ProductViewModels;

namespace WebApplicationRedis.Services
{
    public class ProductServices : IProductServices
    {
        #region Fields

        private readonly RedisSettingOption _redisSettingOption;
        private readonly IProductWriteRepository _productWriteRepository;
        private readonly IProductReadRepository _productReadRepository;
        private readonly IRedisCacheRepository _redisCacheRepository;

        #endregion Fields

        #region Ctor

        public ProductServices(IOptions<RedisSettingOption> options,
            IProductWriteRepository productWriteRepository,
            IProductReadRepository productReadRepository,
            IRedisCacheRepository redisCacheRepository)
        {
            this._redisSettingOption = options.Value;
            this._productWriteRepository = productWriteRepository;
            this._productReadRepository = productReadRepository;
            this._redisCacheRepository = redisCacheRepository;
        }

        #endregion Ctor

        #region Implement

        public async ValueTask<ProductViewModel> GetProductAsync(int productId)
        {
            if (productId <= 0)
                throw new NullReferenceException("Product Id Is Invalid");

            string cacheKey = string.Format(_redisSettingOption.ProductKey, productId);
            int cacheTimeOut = _redisSettingOption.CacheTimeOut;

            var cacheResult = await GetFromCacheAsync<ProductViewModel>(cacheKey);
            if (cacheResult != null)
                return cacheResult;

            var productDto = await _productReadRepository.GetProductAsync(productId).ConfigureAwait(false);
            if (productDto == null)
                return new ProductViewModel();

            var productViewModel = CreateProductViewModelFromProductDto(productDto);

            await SetInToCacheAsync(cacheKey, productViewModel, cacheTimeOut).ConfigureAwait(false);

            return productViewModel;
        }

        public async ValueTask<IEnumerable<ProductViewModel>> GetProductsAsync()
        {
            string cacheKey = string.Format(_redisSettingOption.ProductKey, "list");
            int cacheTimeOut = _redisSettingOption.CacheTimeOut;

            var cacheResult = await GetFromCacheAsync<IEnumerable<ProductViewModel>>(cacheKey);

            if (cacheResult != null)
                return cacheResult;

            var productDtos = await _productReadRepository.GetProductsAsync().ConfigureAwait(false);

            if (productDtos == null || productDtos.Count() == 0)
                return Enumerable.Empty<ProductViewModel>();

            var productViewModels = CreateProductViewModelsFromProductDtos(productDtos);

            await SetInToCacheAsync(cacheKey, productViewModels, cacheTimeOut).ConfigureAwait(false);

            return productViewModels;
        }

        public async Task<int> CreateProductAsync(CreateProductInputModel inputModel)
        {
            if (inputModel == null)
                throw new NullReferenceException("Product Id Is Invalid");

            ValidateProductName(inputModel.ProductName);

            ValidateProductTitle(inputModel.ProductTitle);

            var productEntoty = CreateProductEntityFromInputModel(inputModel);

            int productId = await _productWriteRepository.CreateProductAsync(productEntoty).ConfigureAwait(false);

            productEntoty.setProductId(productId);

            string cacheKey = string.Format(_redisSettingOption.ProductKey, productId);
            int cacheTimeOut = _redisSettingOption.CacheTimeOut;

            await SetInToCacheAsync(cacheKey, productEntoty, cacheTimeOut).ConfigureAwait(false);

            return productId;
        }

        public async Task UpdateProductAsync(UpdateProductInputModel inputModel)
        {
            if (inputModel.ProductId <= 0)
                throw new NullReferenceException("ProductId Is Invalid.");

            string cacheKey = string.Format(_redisSettingOption.ProductKey, inputModel.ProductId);
            int cacheTimeOut = _redisSettingOption.CacheTimeOut;

            ValidateProductName(inputModel.ProductName);

            ValidateProductTitle(inputModel.ProductTitle);

            await IsExistProduct(inputModel.ProductId).ConfigureAwait(false);

            var productEntoty = CreateProductEntityFromInputModel(inputModel);

            await _productWriteRepository.UpdateProductAsync(productEntoty).ConfigureAwait(false);

            DeleteCache(cacheKey);

            await SetInToCacheAsync(cacheKey, productEntoty, cacheTimeOut).ConfigureAwait(false);
        }

        public async Task DeleteProductAsync(int productId)
        {
            if (productId <= 0)
                throw new NullReferenceException("ProductId Is Invalid.");

            string cacheKey = string.Format(_redisSettingOption.ProductKey, productId);

            await IsExistProduct(productId).ConfigureAwait(false);

            await _productWriteRepository.DeleteProductAsync(productId).ConfigureAwait(false);

            DeleteCache(cacheKey);
        }

        #endregion Implement

        #region [ Cache Private Method ]

        private void DeleteCache(string cacheKey)
           => _redisCacheRepository.Delete(cacheKey);

        private async Task SetInToCacheAsync<T>(string cacheKey, T result, int cacheTimeOut)
            => await _redisCacheRepository
                 .SetAsync(cacheKey, result, TimeSpan.FromMinutes(cacheTimeOut));

        private async Task<T> GetFromCacheAsync<T>(string cacheKey)
            => await _redisCacheRepository
                .GetAsync<T>(cacheKey);

        #endregion [ Cache Private Method ]

        #region Private

        private async Task IsExistProduct(int productId)
        {
            var isExistProduct = await _productReadRepository.IsExistProductAsync(productId).ConfigureAwait(false);
            if (isExistProduct == false)
                throw new NullReferenceException("ProductId Is Not Found.");
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
