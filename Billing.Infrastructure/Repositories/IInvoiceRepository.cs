using Billing.Domain.DTOs;

namespace Billing.Infrastructure.Repositories;

public interface IInvoiceRepository
{
    Task<ResponseDto<PagedResultDto<InvoiceDto>>> GetAllInvoicesAsync();
    Task<ResponseDto<InvoiceDto?>> GetInvoiceByIdAsync(int id);
}
