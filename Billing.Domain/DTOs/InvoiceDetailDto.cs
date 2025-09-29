namespace Billing.Domain.DTOs;

public class InvoiceDetailDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public decimal UnitPrice { get; set; } = 0;
    public decimal SubTotalPrice { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;
}
