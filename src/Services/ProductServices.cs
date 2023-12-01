using Microsoft.Extensions.Options;
using WebApplicationRedis.Domain;
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

            var product = await _productReadRepository.GetProductAsync(productId).ConfigureAwait(false);
            if (product == null)
                return new ProductViewModel();

            var productViewModel = CreateProductViewModelFromProduct(product);

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

            var products = await _productReadRepository.GetProductsAsync().ConfigureAwait(false);

            if (products == null || products.Count() == 0)
                return Enumerable.Empty<ProductViewModel>();

            var productViewModels = CreateProductViewModelsFromProducts(products);

            await SetInToCacheAsync(cacheKey, productViewModels, cacheTimeOut).ConfigureAwait(false);

            return productViewModels;
        }

        public async Task<int> CreateProductAsync(CreateProductInputModel inputModel)
        {
            if (inputModel == null)
                throw new NullReferenceException("Product Id Is Invalid");

            ValidateProductName(inputModel.ProductName);

            ValidateProductTitle(inputModel.ProductTitle);

            var product= CreateProductEntityFromInputModel(inputModel);

            int productId = await _productWriteRepository.CreateProductAsync(product).ConfigureAwait(false);

            product.setProductId(productId);

            string cacheKey = string.Format(_redisSettingOption.ProductKey, productId);
            int cacheTimeOut = _redisSettingOption.CacheTimeOut;

            await SetInToCacheAsync(cacheKey, product, cacheTimeOut).ConfigureAwait(false);

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

        private ProductViewModel CreateProductViewModelFromProduct(Product product)
            => new ProductViewModel()
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductTitle = product.ProductTitle,
                ProductDescription = product.ProductDescription,
                MainImageName = product.MainImageName,
                MainImageTitle = product.MainImageTitle,
                MainImageUri = product.MainImageUri,
                IsExisting = product.IsExisting,
                IsFreeDelivery = product.IsFreeDelivery,
                Weight = product.Weight
            };

        private IEnumerable<ProductViewModel> CreateProductViewModelsFromProducts(IEnumerable<Product> products)
        {
            ICollection<ProductViewModel> productViewModels = new List<ProductViewModel>();

            foreach (var product in products)
                productViewModels.Add(
                     new ProductViewModel()
                     {
                         ProductId = product.ProductId,
                         ProductName = product.ProductName,
                         ProductTitle = product.ProductTitle,
                         ProductDescription = product.ProductDescription,
                         MainImageName = product.MainImageName,
                         MainImageTitle = product.MainImageTitle,
                         MainImageUri = product.MainImageUri,
                         IsExisting = product.IsExisting,
                         IsFreeDelivery = product.IsFreeDelivery,
                         Weight = product.Weight
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
