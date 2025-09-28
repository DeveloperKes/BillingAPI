using Billing.Domain.DTOs;
using Billing.Infrastructure.Repositories;

namespace Billing.Application.Services;

public class InvoiceService(IInvoiceRepository invoiceRepository) : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;
    public async Task<ResponseDto<PagedResultDto<InvoiceDto>>> GetAllInvoicesAsync()
    {
        return await _invoiceRepository.GetAllInvoicesAsync();
    }
    public async Task<ResponseDto<InvoiceDto?>> GetInvoiceByIdAsync(int id)
    {
        return await _invoiceRepository.GetInvoiceByIdAsync(id);
    }

}
