using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PR1.Data;
using PR1.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PR1.Controllers
{
    [Authorize(Roles = "manager")]
    public class ExportController : Controller
    {
        private readonly AppDbContext _context;

        public ExportController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ExportSalesReport()
        {
            var totalSales = await _context.OrderItems
                .SumAsync(oi => oi.Quantity);

            var productSales = await _context.OrderItems
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new ProductSales
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .ToListAsync();

            var viewModel = new ManageGraphicsViewModel
            {
                TotalSales = totalSales,
                ProductSales = productSales
            };

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new Document(pdf);
                    document.Add(new Paragraph("Отчет по продажам").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));

                    document.Add(new Paragraph($"Общее количество продаж: {viewModel.TotalSales}"));

                    var table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
                    table.AddHeaderCell("Товар");
                    table.AddHeaderCell("Количество");

                    foreach (var ps in viewModel.ProductSales)
                    {
                        table.AddCell(ps.ProductName);
                        table.AddCell(ps.TotalQuantity.ToString());
                    }

                    document.Add(table);
                }

                return File(memoryStream.ToArray(), "application/pdf", "SalesReport.pdf");
            }
        }
    }
}
