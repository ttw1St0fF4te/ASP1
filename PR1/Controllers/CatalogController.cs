using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using PR1.Data;

public class CatalogController : Controller
{
    private readonly AppDbContext _context;

    public CatalogController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchTerm = "", string sortBy = "", string sortOrder = "asc")
    {
        var productsQuery = _context.Products.AsQueryable();

        // Поиск по названию (без учета регистра)
        if (!string.IsNullOrEmpty(searchTerm))
        {
            productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));
        }

        // Сортировка
        switch (sortBy.ToLower())
        {
            case "price":
                productsQuery = sortOrder == "desc" 
                    ? productsQuery.OrderByDescending(p => p.Price)
                    : productsQuery.OrderBy(p => p.Price);
                break;
            case "date":
                productsQuery = sortOrder == "desc" 
                    ? productsQuery.OrderByDescending(p => p.Id)
                    : productsQuery.OrderBy(p => p.Id);
                break;
            case "name":
                productsQuery = sortOrder == "desc" 
                    ? productsQuery.OrderByDescending(p => p.Name)
                    : productsQuery.OrderBy(p => p.Name);
                break;
            default:
                productsQuery = productsQuery.OrderBy(p => p.Id);
                break;
        }

        var products = await productsQuery.ToListAsync();

        // Передаем параметры в представление
        ViewBag.SearchTerm = searchTerm;
        ViewBag.SortBy = sortBy;
        ViewBag.SortOrder = sortOrder;
        ViewBag.HasResults = products.Any();
        ViewBag.IsSearching = !string.IsNullOrEmpty(searchTerm);

        return View(products);
    }
}