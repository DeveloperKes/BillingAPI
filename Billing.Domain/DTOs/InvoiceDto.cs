namespace Billing.Domain.DTOs;

public class InvoiceDto
{
    public int Id { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public ClientDto Client { get; set; } = new ClientDto();
    public int InvoiceNumber { get; set; } = 0;
    public int TotalItems { get; set; } = 0;
    public decimal SubTotalAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;

}
