namespace Billing.Domain.DTOs;

public class ResponseHeaderDto
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? Data { get; set; }
}
