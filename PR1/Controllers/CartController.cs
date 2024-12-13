using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PR1.Data;
using PR1.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PR1.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId)
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

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    CartId = cart.Id
                };
                cart.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += 1;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Cart");
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

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    CartItems = new List<CartItem>()
                };
            }

            return View(cart);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
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

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return NotFound();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            cart.CartItems.Remove(cartItem);
            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
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

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                return NotFound();
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow, // Use UTC time
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Order = order
                };
                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
