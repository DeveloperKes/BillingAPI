namespace Billing.Domain.DTOs;

public class PagedResultDto<T>
{
    public int Count { get; set; }
    public IEnumerable<T> Results { get; set; } = new List<T>();
}
