using Billing.Domain.DTOs;

namespace Billing.Application.Services;

public interface IInvoiceService
{
    Task<ResponseDto<PagedResultDto<InvoiceDto>>> GetAllInvoicesAsync();
    Task<ResponseDto<InvoiceDto?>> GetInvoiceByIdAsync(int id);
    Task<ResponseDto<PagedResultDto<InvoiceDto>>> GetInvoiceByNumberAsync(int number);
    Task<ResponseDto<PagedResultDto<InvoiceDto>>> GetInvoiceByClientAsync(int clientId);

    Task<ResponseDto<int>> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto);
}
