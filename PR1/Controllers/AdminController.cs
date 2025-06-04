using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PR1.Data;
using PR1.Models;
using System.Text.RegularExpressions;

namespace PR1.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Users Management
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.UserRole)
                .OrderBy(u => u.Id)
                .ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(User user)
        {
            try
            {
                var existingUser = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == user.Id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Validate username
                if (user.Username.Length > 20 || !Regex.IsMatch(user.Username, @"^[a-zA-Z0-9]+$"))
                {
                    TempData["Error"] = "Имя пользователя должно быть до 20 символов и содержать только буквы и цифры";
                    return RedirectToAction(nameof(Users));
                }

                // Validate password
                if (user.Password.Length < 6 || user.Password.Length > 50)
                {
                    TempData["Error"] = "Пароль должен быть от 6 до 50 символов";
                    return RedirectToAction(nameof(Users));
                }

                // Validate email
                if (!string.IsNullOrEmpty(user.Email) && !Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    TempData["Error"] = "Некорректный формат email";
                    return RedirectToAction(nameof(Users));
                }

                // Preserve values that shouldn't be modified
                user.TotalSpent = existingUser.TotalSpent;
                user.WalletBalance = existingUser.WalletBalance;
                user.LoyaltyLevel = existingUser.LoyaltyLevel;

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Users));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            // Validate username
            if (user.Username.Length > 20 || !Regex.IsMatch(user.Username, @"^[a-zA-Z0-9]+$"))
            {
                TempData["Error"] = "Имя пользователя должно быть до 20 символов и содержать только буквы и цифры";
                return RedirectToAction(nameof(Users));
            }

            // Validate password
            if (user.Password.Length < 6 || user.Password.Length > 50)
            {
                TempData["Error"] = "Пароль должен быть от 6 до 50 символов";
                return RedirectToAction(nameof(Users));
            }

            // Validate email
            if (!string.IsNullOrEmpty(user.Email) && !Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                TempData["Error"] = "Некорректный формат email";
                return RedirectToAction(nameof(Users));
            }

            // Set initial values
            user.TotalSpent = null;
            user.WalletBalance = null;
            user.LoyaltyLevel = null;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Users));
        }
        #endregion

        #region Products Management
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .OrderBy(p => p.Id)
                .ToListAsync();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(Product product)
        {
            try
            {
                // Custom validation
                if (string.IsNullOrEmpty(product.Name) || 
                    string.IsNullOrEmpty(product.Category) || 
                    string.IsNullOrEmpty(product.Description) || 
                    string.IsNullOrEmpty(product.Image) || 
                    product.Price <= 0)
                {
                    TempData["Error"] = "Все поля должны быть заполнены, а цена должна быть указана числом больше 0";
                    return RedirectToAction(nameof(Products));
                }

                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Products));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Products.AnyAsync(p => p.Id == product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            // Custom validation
            if (string.IsNullOrEmpty(product.Name) || 
                string.IsNullOrEmpty(product.Category) || 
                string.IsNullOrEmpty(product.Description) || 
                string.IsNullOrEmpty(product.Image) || 
                product.Price <= 0)
            {
                TempData["Error"] = "Все поля должны быть заполнены, а цена должна быть указана числом больше 0";
                return RedirectToAction(nameof(Products));
            }

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Products));
            }
            catch (DbUpdateException ex)
            {
                // Логируем ошибку для диагностики
                Console.WriteLine($"Error creating product: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                TempData["Error"] = "Ошибка при создании товара: " + ex.InnerException?.Message;
                return RedirectToAction(nameof(Products));
            }
        }
        #endregion

        #region Reviews Management
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderBy(r => r.Id)
                .ToListAsync();
            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Reviews));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReview(Review review)
        {
            try
            {
                var existingReview = await _context.Reviews
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == review.Id);

                if (existingReview == null)
                {
                    return NotFound();
                }

                // Validate text
                if (string.IsNullOrEmpty(review.Text))
                {
                    TempData["Error"] = "Текст отзыва не может быть пустым";
                    return RedirectToAction(nameof(Reviews));
                }

                // Validate rating
                if (review.Rating < 1 || review.Rating > 5)
                {
                    TempData["Error"] = "Оценка должна быть от 1 до 5";
                    return RedirectToAction(nameof(Reviews));
                }

                // Preserve values that shouldn't be modified
                review.ProductId = existingReview.ProductId;
                review.UserId = existingReview.UserId;
                review.Date = existingReview.Date;

                _context.Entry(review).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Reviews));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Reviews.AnyAsync(r => r.Id == review.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion

        #region Orders Management
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderBy(o => o.Id)
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Orders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder(Order order)
        {
            try
            {
                var existingOrder = await _context.Orders.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (existingOrder == null)
                {
                    return NotFound();
                }

                // Validate phone number
                if (!string.IsNullOrEmpty(order.CustomerPhone) && 
                    !Regex.IsMatch(order.CustomerPhone, @"^\+7 \([0-9]{3}\) [0-9]{3}-[0-9]{2}-[0-9]{2}$"))
                {
                    TempData["Error"] = "Телефон должен быть в формате +7 (999) 999-99-99";
                    return RedirectToAction(nameof(Orders));
                }

                // Validate delivery address
                if (!string.IsNullOrEmpty(order.DeliveryAddress) && order.DeliveryAddress.Count(c => c == ',') != 4)
                {
                    TempData["Error"] = "Адрес доставки должен содержать все необходимые поля (страна, город, индекс, улица, дом)";
                    return RedirectToAction(nameof(Orders));
                }

                // Preserve values that shouldn't be modified
                order.UserId = existingOrder.UserId;
                order.OrderDate = existingOrder.OrderDate;
                order.FinalAmount = existingOrder.FinalAmount;
                order.TotalAmount = existingOrder.TotalAmount;
                order.WalletEarned = existingOrder.WalletEarned;
                order.WalletUsed = existingOrder.WalletUsed;

                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Orders));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Orders.AnyAsync(o => o.Id == order.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion
    }
}
