using Billing.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;

        [HttpGet]
        public async Task<IActionResult> GetAllProductsAsync()
        {
            var response = await _productService.GetAllProductsAsync();
            if (response.Code == 204)
            {
                return StatusCode(StatusCodes.Status204NoContent, response);
            }
            if (response.Code == 500)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            var response = await _productService.GetProductByIdAsync(id);
            if (response.Code == 1)
            {
                return StatusCode(StatusCodes.Status404NotFound, response);
            }
            if (response.Code == 500)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            return Ok(response);
        }
    }
}