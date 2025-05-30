using Microsoft.EntityFrameworkCore;
using PR1.Models;

namespace PR1.ViewComponents;

using Microsoft.AspNetCore.Mvc;
using PR1.Data;
using System.Linq;
using System.Threading.Tasks;

public class AverageRatingViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public AverageRatingViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        double averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

        return View(new AverageRatingViewModel
        {
            ProductId = productId,
            AverageRating = Math.Round(averageRating, 1)
        });
    }
}