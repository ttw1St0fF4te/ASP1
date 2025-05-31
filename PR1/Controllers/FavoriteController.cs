using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PR1.Data;
using PR1.Models;

namespace PR1.Controllers;

[Authorize]
public class FavoriteController : Controller
{
    private readonly AppDbContext _context;

    public FavoriteController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
        if (user == null)
        {
            return NotFound();
        }

        // Ищем существующую запись в избранном
        var existing = await _context.Favorites
            .FirstOrDefaultAsync(f => 
                f.UserId == userId && 
                f.ProductId == productId);

        if (existing == null)
        {
            // Добавляем новую запись
            _context.Favorites.Add(new Favorite
            {
                UserId = userId,
                ProductId = productId
            });
        }
        else
        {
            // Удаляем существующую запись
            _context.Favorites.Remove(existing);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Favorite");
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
        if (user == null)
        {
            return NotFound();
        }

        // Получаем избранное пользователя
        var favorites = await _context.Favorites
            .Include(f => f.Product)
            .Where(f => f.UserId == userId)
            .ToListAsync();

        return View(favorites);
    }
}