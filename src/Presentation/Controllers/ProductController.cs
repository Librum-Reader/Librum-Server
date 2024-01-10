using Application.Common.DTOs.Product;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("products")]
public class ProductController(IProductService productService) : ControllerBase
{
    private IProductService ProductService { get; } = productService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductOutDto>>> GetTags()
    {
        var result = await ProductService.GetAllProductsAsync();
        return Ok(result);
    }
}