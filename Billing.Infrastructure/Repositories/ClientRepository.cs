using Billing.Domain.DTOs;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;


namespace Billing.Infrastructure.Repositories;

public class ClientRepository(string connectionString) : IClientRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<ResponseDto<PagedResultDto<ClientDto>>> GetAllClientsAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        using var multi = await connection.QueryMultipleAsync(
            "sp_GetClients",
            commandType: CommandType.StoredProcedure
            );


        var responseHeader = await multi.ReadFirstOrDefaultAsync<ResponseHeaderDto>();

        var clients = responseHeader?.Code == 0 ? await multi.ReadAsync<ClientDto>() : [];

        return new ResponseDto<PagedResultDto<ClientDto>>
        {
            Code = responseHeader?.Code ?? 500,
            Message = responseHeader?.Message ?? "Internal server error",
            Data = new PagedResultDto<ClientDto>
            {
                Count = responseHeader?.Data ?? 0,
                Results = clients
            }
        };
    }

    public async Task<ResponseDto<ClientDto?>> GetClientByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        using var multi = await connection.QueryMultipleAsync(
            "sp_GetClients",
            new { Id = id },
            commandType: CommandType.StoredProcedure
            );

        var responseHeader = await multi.ReadFirstOrDefaultAsync<ResponseHeaderDto>();
        var client = responseHeader?.Code == 0
            ? await multi.ReadFirstOrDefaultAsync<ClientDto>()
            : null;

        return new ResponseDto<ClientDto?>
        {
            Code = responseHeader?.Code ?? 500,
            Message = responseHeader?.Message ?? "Internal server error",
            Data = client
        };
    }
}
