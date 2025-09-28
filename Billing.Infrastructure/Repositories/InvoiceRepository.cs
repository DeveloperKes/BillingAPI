using Billing.Domain.DTOs;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Billing.Infrastructure.Repositories;

public class InvoiceRepository(string connectionString) : IInvoiceRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<ResponseDto<PagedResultDto<InvoiceDto>>> GetAllInvoicesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "sp_GetInvoices",
            commandType: CommandType.StoredProcedure
            );
        var responseHeader = await multi.ReadFirstOrDefaultAsync<ResponseHeaderDto>();
        var invoices = responseHeader?.Code == 0 ? await multi.ReadAsync<InvoiceDto>() : [];
        return new ResponseDto<PagedResultDto<InvoiceDto>>
        {
            Code = responseHeader?.Code ?? 500,
            Message = responseHeader?.Message ?? "Internal server error",
            Data = new PagedResultDto<InvoiceDto>
            {
                Count = responseHeader?.Data ?? 0,
                Results = invoices
            }
        };
    }

    public async Task<ResponseDto<InvoiceDto?>> GetInvoiceByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "sp_GetInvoices",
            new { Id = id },
            commandType: CommandType.StoredProcedure
            );
        var responseHeader = await multi.ReadFirstOrDefaultAsync<ResponseHeaderDto>();
        var invoice = responseHeader?.Code == 0
            ? await multi.ReadFirstOrDefaultAsync<InvoiceDto>()
            : null;
        return new ResponseDto<InvoiceDto?>
        {
            Code = responseHeader?.Code ?? 500,
            Message = responseHeader?.Message ?? "Internal server error",
            Data = invoice
        };
    }
}