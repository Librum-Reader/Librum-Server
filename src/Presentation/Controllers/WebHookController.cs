using Application.Common.DTOs.Product;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Presentation.Controllers;

[ApiController]
[Route("webhooks")]
public class WebHookController(IConfiguration configuration, 
                              ILogger<BookController> logger, 
                              IProductService productService,
                              IUserService userService) : ControllerBase
{
    private ILogger<BookController> Logger { get; } = logger;
    private IUserService UserService { get; } = userService;
    private string WebhookSecret { get; } = configuration["StripeWebhookSecret"];

    [HttpPost("stripe")]
    public async Task<IActionResult> Stripe()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json,
                                                          Request.Headers["Stripe-Signature"], 
                                                          "whsec_83273595ebb73eab88c16c396f64c00265f8711c11b9d8c5d70563df9a23933e",
                                                          300,
                                                          (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
                                                          false);
            switch (stripeEvent.Type)
            {
                case Events.ProductCreated:
                    await CreateProduct(stripeEvent.Data.Object as Product);
                    break;
                case Events.ProductDeleted:
                    await DeleteProduct(stripeEvent.Data.Object as Product);
                    break;
                case Events.ProductUpdated:
                    await UpdateProduct(stripeEvent.Data.Object as Product);
                    break;
                case Events.PriceCreated:
                case Events.PriceUpdated:
                    await AddPriceToProduct(stripeEvent.Data.Object as Price);
                    break;
                case Events.CustomerCreated:
                    await AddCustomerIdToUser(stripeEvent.Data.Object as Customer);
                    break;
                case Events.CustomerSubscriptionCreated:
                case Events.CustomerSubscriptionUpdated:
                    await AddTierToCustomer(stripeEvent.Data.Object as Subscription);
                    break;
                case Events.CustomerSubscriptionDeleted:
                    await RemoveTierFromCustomer(stripeEvent.Data.Object as Subscription);
                    break;
                default:
                    const string message = "Unhandled Stripe Event Type";
                    Logger.LogWarning("Unhandled Stripe Event Type");
                    return StatusCode(500, message);
            }

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest();
        }
    }

    private async Task AddTierToCustomer(Subscription subscription)
    {
        var customerId = subscription.CustomerId;
        var productId = subscription.Items.Data[0].Price.ProductId;
        
        await UserService.AddTierToUser(customerId, productId);
    }
    
    private async Task RemoveTierFromCustomer(Subscription subscription)
    {
        var customerId = subscription.CustomerId;

        await UserService.ResetUserToFreeTier(customerId);
    }

    private async Task AddCustomerIdToUser(Customer customer)
    {
        var email = customer.Email;
        var customerId = customer.Id;
        
        await UserService.AddCustomerIdToUser(email, customerId);
    }

    private async Task UpdateProduct(Product product)
    {
        ProductForUpdateDto productUpdateDto = new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            BookStorageLimit = long.Parse(product.Metadata["bookStorageLimit"]),
            AiRequestLimit = int.Parse(product.Metadata["aiRequestLimit"])
        };
        foreach(var feature in product.Features)
            productUpdateDto.Features.Add(feature.Name);

        await productService.UpdateProductAsync(productUpdateDto);
    }

    private async Task DeleteProduct(Product product)
    {
        await productService.DeleteProductAsync(product.Id);
    }

    private async Task AddPriceToProduct(Price price)
    {
        var product = price.ProductId;
        var priceId = price.Id;
        var amount = (int)price.UnitAmount!.Value;
        
        await productService.AddPriceToProductAsync(product, priceId, amount);
    }

    private async Task CreateProduct(Product product)
    {
        ProductInDto productInDto = new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            BookStorageLimit = long.Parse(product.Metadata["bookStorageLimit"]),
            AiRequestLimit = int.Parse(product.Metadata["aiRequestLimit"]),
            LiveMode = product.Livemode
        };
        foreach(var feature in product.Features)
            productInDto.Features.Add(feature.Name);

        await productService.CreateProductAsync(productInDto);
    }
}