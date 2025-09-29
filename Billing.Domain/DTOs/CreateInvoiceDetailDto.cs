namespace Billing.Domain.DTOs;

public class CreateInvoiceDetailDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 0;
    public string? Notes { get; set; } = string.Empty;
}
