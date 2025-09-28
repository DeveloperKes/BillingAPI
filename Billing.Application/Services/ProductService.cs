using Billing.Domain.DTOs;
using Billing.Infrastructure.Repositories;

namespace Billing.Application.Services;

public class ProductService(IProductRepository productRepository) : IProductService
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ResponseDto<PagedResultDto<ProductDto>>> GetAllProductsAsync()
    {
        return await _productRepository.GetAllProductsAsync();
    }

    public async Task<ResponseDto<ProductDto?>> GetProductByIdAsync(int id)
    {
        return await _productRepository.GetProductByIdAsync(id);
    }
}
