using Billing.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    private readonly IInvoiceService _invoiceService = invoiceService;
    private readonly ILogger<InvoicesController> _logger;

    [HttpGet]
    public async Task<IActionResult> GetAllInvoicesAsync()
    {
        var response = await _invoiceService.GetAllInvoicesAsync();
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
    public async Task<IActionResult> GetInvoiceByIdAsync(int id)
    {
        var response = await _invoiceService.GetInvoiceByIdAsync(id);
        if (response.Code == 5)
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
