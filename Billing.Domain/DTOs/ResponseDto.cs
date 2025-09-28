namespace Billing.Domain.DTOs;

public class ResponseDto<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
