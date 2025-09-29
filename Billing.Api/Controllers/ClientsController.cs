using Billing.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController(IClientService clientService) : ControllerBase
    {
        private readonly IClientService _clientService = clientService;

        [HttpGet]
        public async Task<IActionResult> GetAllClientsAsync()
        {
            var response = await _clientService.GetAllClientsAsync();
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
        public async Task<IActionResult> GetClientByIdAsync(int id)
        {
            var response = await _clientService.GetClientByIdAsync(id);
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
