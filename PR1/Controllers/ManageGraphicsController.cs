using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PR1.Data;
using PR1.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PR1.Controllers
{
    [Authorize(Roles = "manager")]
    public class ManageGraphicsController : Controller
    {
        private readonly AppDbContext _context;

        public ManageGraphicsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
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

            // Темпоральный код для проверки данных
            foreach (var ps in productSales)
            {
                Console.WriteLine($"Product: {ps.ProductName}, Quantity: {ps.TotalQuantity}");
            }

            var viewModel = new ManageGraphicsViewModel
            {
                TotalSales = totalSales,
                ProductSales = productSales
            };

            return View(viewModel);
        }
    }

    public class ManageGraphicsViewModel
    {
        public int TotalSales { get; set; }
        public List<ProductSales> ProductSales { get; set; }
    }

    public class ProductSales
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }
}