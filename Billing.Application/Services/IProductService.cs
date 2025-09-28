using Billing.Domain.DTOs;

namespace Billing.Application.Services;

public interface IProductService
{
    Task<ResponseDto<PagedResultDto<ProductDto>>> GetAllProductsAsync();
    Task<ResponseDto<ProductDto?>> GetProductByIdAsync(int id);
}
