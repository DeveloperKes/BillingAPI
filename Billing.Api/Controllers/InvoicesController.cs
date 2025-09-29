using Billing.Application.Services;
using Billing.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    private readonly IInvoiceService _invoiceService = invoiceService;

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

    [HttpGet("reference/{number:int}")]
    public async Task<IActionResult> GetInvoiceByNumberAsync(int number)
    {
        var response = await _invoiceService.GetInvoiceByNumberAsync(number);
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


    [HttpGet("client/{clientId:int}")]
    public async Task<IActionResult> GetInvoiceByClientAsync(int clientId)
    {
        var response = await _invoiceService.GetInvoiceByClientAsync(clientId);
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


    [HttpPost]
    public async Task<IActionResult> CreateInvoiceAsync([FromBody] CreateInvoiceDto createInvoiceDto)
    {
        var response = await _invoiceService.CreateInvoiceAsync(createInvoiceDto);

        if (response.Code == 500)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
        if (response.Code != 0)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}

