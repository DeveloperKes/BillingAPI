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
    public async Task<ResponseDto<PagedResultDto<InvoiceDto?>>> GetInvoiceByNumberAsync(int number)
    {
        return await _invoiceRepository.GetInvoiceByNumberAsync(number);
    }
    public async Task<ResponseDto<PagedResultDto<InvoiceDto?>>> GetInvoiceByClientAsync(int clientId)
    {
        return await _invoiceRepository.GetInvoiceByClientAsync(clientId);
    }
    public async Task<ResponseDto<int>> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto)
    {
        return await _invoiceRepository.CreateInvoiceAsync(createInvoiceDto);
    }

}
