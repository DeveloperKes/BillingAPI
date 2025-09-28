namespace Billing.Domain.DTOs;

public class CreateClientDto
{
    public string BusinessName { get; set; } = string.Empty;
    public int ClientTypeId { get; set; } = 0;
    public string Reference { get; set; } = string.Empty;
}
