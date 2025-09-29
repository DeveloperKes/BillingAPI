using Billing.Domain.DTOs;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace Billing.Infrastructure.Repositories;

public class ProductRepository(string connectionString) : IProductRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<ResponseDto<PagedResultDto<ProductDto>>> GetAllProductsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "sp_GetProducts",
            commandType: CommandType.StoredProcedure
            );
        var responseHeader = await multi.ReadFirstOrDefaultAsync<ResponseHeaderDto>();
        var products = responseHeader?.Code == 0 ? await multi.ReadAsync<ProductDto>() : [];
        return new ResponseDto<PagedResultDto<ProductDto>>
        {
            Code = responseHeader?.Code ?? 500,
            Message = responseHeader?.Message ?? "Internal server error",
            Data = new PagedResultDto<ProductDto>
            {
                Count = responseHeader?.Data ?? 0,
                Results = products
            }
        };
    }

    public async Task<ResponseDto<ProductDto?>> GetProductByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "sp_GetProducts",
            new { Id = id },
            commandType: CommandType.StoredProcedure
            );
        var responseHeader = await multi.ReadFirstOrDefaultAsync<ResponseHeaderDto>();
        var product = responseHeader?.Code == 0
            ? await multi.ReadFirstOrDefaultAsync<ProductDto>()
            : null;
        return new ResponseDto<ProductDto?>
        {
            Code = responseHeader?.Code ?? 500,
            Message = responseHeader?.Message ?? "Internal server error",
            Data = product
        };
    }
}
