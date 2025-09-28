using Billing.Domain.DTOs;

namespace Billing.Application.Services;

public interface IClientService
{
    Task<ResponseDto<PagedResultDto<ClientDto>>> GetAllClientsAsync();
    Task<ResponseDto<ClientDto?>> GetClientByIdAsync(int id);
}
