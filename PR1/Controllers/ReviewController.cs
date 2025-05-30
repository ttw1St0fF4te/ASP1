using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PR1.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PR1.Data;
using PR1.Models;

[Authorize]
public class ReviewController : Controller
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Details", "Product", new { id = model.ProductId });
        }

        // Получаем Id пользователя из Claim
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized(); // или перенаправь на логин
        }

        int userId = int.Parse(userIdClaim.Value);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound();
        }

        var review = new Review
        {
            Text = model.Text,
            Rating = model.Rating,
            ProductId = model.ProductId,
            UserId = user.Id,
            Date = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Product", new { id = model.ProductId });
    }
}