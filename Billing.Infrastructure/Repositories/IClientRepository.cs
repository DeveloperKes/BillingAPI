using Billing.Domain.DTOs;

namespace Billing.Infrastructure.Repositories;

public interface IClientRepository
{
    Task<ResponseDto<PagedResultDto<ClientDto>>> GetAllClientsAsync();

    Task<ResponseDto<ClientDto?>> GetClientByIdAsync(int id);

    //Task<int> CreateClientAsync(CreateClientDto createClientDto);

}
