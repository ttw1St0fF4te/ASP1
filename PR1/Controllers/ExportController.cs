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
using iText.IO.Font;
using iText.Kernel.Font;
using Microsoft.AspNetCore.Hosting; // Добавьте этот namespace


namespace PR1.Controllers
{
    [Authorize(Roles = "manager")]
    public class ExportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment; // Для доступа к файлам

        public ExportController(AppDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
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

                    // ШАГ 1: Загрузка кириллического шрифта
                    string fontPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Fonts", "times.ttf");
                    PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

                    // ШАГ 2: Используем шрифт во всех текстовых элементах
                    // Заголовок
                    document.Add(new Paragraph("Отчет о продажах")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(20)
                        .SetFont(font)); // Устанавливаем шрифт

                    // Общие продажи
                    document.Add(new Paragraph($"Всего продано: {viewModel.TotalSales}")
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFontSize(12)
                        .SetFont(font)); // Устанавливаем шрифт

                    // Описание
                    document.Add(new Paragraph("Этот отчет содержит информацию о продажах товаров. В таблице ниже представлены данные о количестве проданных единиц по каждому товару.")
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFontSize(12)
                        .SetMarginBottom(20)
                        .SetFont(font)); // Устанавливаем шрифт

                    // Таблица
                    var table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
                    
                    // Заголовки таблицы
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Товар").SetFont(font)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Количество").SetFont(font)));

                    // Данные таблицы
                    foreach (var ps in viewModel.ProductSales)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(ps.ProductName).SetFont(font)));
                        table.AddCell(new Cell().Add(new Paragraph(ps.TotalQuantity.ToString()).SetFont(font)));
                    }

                    document.Add(table);
                }

                return File(memoryStream.ToArray(), "application/pdf", "Отчет о продажах.pdf");
            }
        }
    }
}
