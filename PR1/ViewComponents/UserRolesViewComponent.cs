using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PR1.Data;
using PR1.Models;

namespace PR1.ViewComponents;

public class UserRolesViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public UserRolesViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(int? selectedRoleId = null)
    {
        var roles = await _context.UserRoles.ToListAsync();
        ViewBag.SelectedRoleId = selectedRoleId;
        return View(roles);
    }
}
