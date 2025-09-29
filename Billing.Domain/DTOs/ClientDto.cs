namespace Billing.Domain.DTOs;

public class ClientDto
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Reference { get; set; } = string.Empty;
}

