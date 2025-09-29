using Billing.Domain.DTOs;
using System.Data;

namespace Billing.Domain.Common.Extensions
{
    public static class DataTableExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<CreateInvoiceDetailDto> details)
        {
            var table = new DataTable();
            table.Columns.Add("IdProducto", typeof(int));
            table.Columns.Add("Cantidad", typeof(int));
            table.Columns.Add("Notas", typeof(string));

            foreach (var detail in details)
            {
                table.Rows.Add(detail.ProductId, detail.Quantity, detail.Notes);
            }

            return table;
        }
    }
}
