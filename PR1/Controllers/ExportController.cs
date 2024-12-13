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

                    // Adding the title
                    document.Add(new Paragraph("Sales Report")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(20));

                    // Adding the total sales
                    document.Add(new Paragraph($"Total Sales: {viewModel.TotalSales}")
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFontSize(12));

                    // Adding explanatory text
                    document.Add(new Paragraph("This report provides information about product sales. The table below contains data on the number of units sold for each product.")
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFontSize(12)
                        .SetMarginBottom(20));

                    // Creating the table
                    var table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Product")));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Quantity")));

                    foreach (var ps in viewModel.ProductSales)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(ps.ProductName)));
                        table.AddCell(new Cell().Add(new Paragraph(ps.TotalQuantity.ToString())));
                    }

                    // Adding the table to the document
                    document.Add(table);
                }

                return File(memoryStream.ToArray(), "application/pdf", "SalesReport.pdf");
            }
        }
    }
}
