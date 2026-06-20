using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Common;
using WebApp.Model.Product;
using WebApp.Service.Product;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("ProductList")]
        public  async Task<IActionResult> GetProduct()
        {
            try
            {
                var result = await _productService.ProductList();
                return Ok(new ApiResponse(true, null, result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> Add([FromForm] CreateProductRequestModel model)
        {
            try
            {
                var result = await _productService.Add(model);
                return Ok(new ApiResponse(true, "Product added successfully.", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> Update([FromBody] UpdateProductModel model)
        {
            try
            {
                var result = await _productService.Update(model);
                return Ok(new ApiResponse(true, "Product updated successfully.", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }

        [HttpDelete("RemoveProduct")]
        public async Task<IActionResult> Remove(string id)
        {
            try
            {
                var result = await _productService.Delete(Guid.Parse(id));
                return Ok(new ApiResponse(true, "Product deleted successfully.", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }
    }
}
