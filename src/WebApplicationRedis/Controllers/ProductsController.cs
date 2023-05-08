using Microsoft.AspNetCore.Mvc;
using WebApplicationRedis.InputModels.ProductInputModels;
using WebApplicationRedis.Services.Contracts;

namespace WebApplicationRedis.Controllers
{
    [ApiController]
    public class ProductsController : ControllerBase
    {
        #region Fields
        private readonly IProductServices _productServices;

        #endregion Fields

        #region Ctor

        public ProductsController(IProductServices productServices)
        {
            this._productServices = productServices;
        }

        #endregion Ctor

        /// <summary>
        /// Get Product List
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/products")]
        public async ValueTask<IActionResult> GetProducts()
        {
            var productVMs = await _productServices.GetProductsAsync();

            return Ok(productVMs);
        }

        /// <summary>
        /// Create Product
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/product")]
        public async ValueTask<IActionResult> CreateProduct(CreateProductInputModel command)
        {
            int productId = await _productServices.CreateProductAsync(command);

            return CreatedAtRoute(nameof(GetProduct), new { productId = productId }, new { ProductId = productId });
        }

        /// <summary>
        /// Get Product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/product/{productId:int}", Name = nameof(GetProduct))]
        public async ValueTask<IActionResult> GetProduct(int productId)
        {
            var productVM = await _productServices.GetProductAsync(productId);

            return Ok(productVM);
        }

        /// <summary>
        /// Update Product
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("/api/v1/product/{productId:int}")]
        public async Task<IActionResult> UpdateProduct(int productId, UpdateProductInputModel command)
        {
            if (productId != command.ProductId)
                return BadRequest("Bad Request Message");

            await _productServices.UpdateProductAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Delete Product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpDelete("/api/v1/product/{productId:int}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            await _productServices.DeleteProductAsync(productId);

            return NoContent();
        }

    }
}
