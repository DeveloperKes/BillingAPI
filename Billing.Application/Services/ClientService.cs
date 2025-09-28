using Billing.Domain.DTOs;
using Billing.Infrastructure.Repositories;

namespace Billing.Application.Services;

public class ClientService(IClientRepository clientRepository) : IClientService
{
    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task<ResponseDto<PagedResultDto<ClientDto>>> GetAllClientsAsync()
    {
        return await _clientRepository.GetAllClientsAsync();
    }

    public async Task<ResponseDto<ClientDto?>> GetClientByIdAsync(int id)
    {
        return await _clientRepository.GetClientByIdAsync(id);
    }
}
