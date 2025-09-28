using Billing.Domain.DTOs;

namespace Billing.Infrastructure.Repositories;

public interface IProductRepository
{
    Task<ResponseDto<PagedResultDto<ProductDto>>> GetAllProductsAsync();
    Task<ResponseDto<ProductDto?>> GetProductByIdAsync(int id);
}
