namespace Billing.Domain.DTOs;

public class CreateInvoiceDto
{
    public int ClientId { get; set; }
    public int InvoiceNumber { get; set; } = 0;
    public List<CreateInvoiceDetailDto> InvoiceDetails { get; set; } = new List<CreateInvoiceDetailDto>();
}
