using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PR1.Data;
using PR1.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using PR1.Services;

namespace PR1.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public CartController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

            return RedirectToAction("Details", "Product", new { 
                id = productId, 
                inCart = true // Передаем флаг добавления
            });;
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

        // Новый метод для увеличения количества товара
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncreaseQuantity(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Json(new { success = false, message = "Пользователь не авторизован" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден" });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return Json(new { success = false, message = "Корзина не найдена" });
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Товар в корзине не найден" });
            }

            if (cartItem.Quantity >= 10)
            {
                return Json(new { success = false, message = "Уже указано максимальное кол-во позиций на товар (10)" });
            }

            cartItem.Quantity += 1;
            await _context.SaveChangesAsync();

            // Пересчитываем общую сумму корзины
            var totalSum = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            return Json(new { 
                success = true, 
                newQuantity = cartItem.Quantity,
                itemTotal = cartItem.Quantity * cartItem.Product.Price,
                cartTotal = totalSum
            });
        }

        // Новый метод для уменьшения количества товара
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecreaseQuantity(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Json(new { success = false, message = "Пользователь не авторизован" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден" });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return Json(new { success = false, message = "Корзина не найдена" });
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Товар в корзине не найден" });
            }

            if (cartItem.Quantity <= 1)
            {
                return Json(new { success = false, message = "Число товара минимальное, если хотите удалить товар, нажмите на соответствующую кнопку" });
            }

            cartItem.Quantity -= 1;
            await _context.SaveChangesAsync();

            // Пересчитываем общую сумму корзины
            var totalSum = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            return Json(new { 
                success = true, 
                newQuantity = cartItem.Quantity,
                itemTotal = cartItem.Quantity * cartItem.Product.Price,
                cartTotal = totalSum
            });
        }
        
        [HttpGet]
        public async Task<IActionResult> Checkout()
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
                return RedirectToAction("Index");
            }

            var totalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
            
            // Обновляем уровень лояльности пользователя
            var totalSpent = user.TotalSpent ?? 0m;
            var loyaltyLevel = LoyaltyLevels.GetLoyaltyLevel(totalSpent);
            var canUseWallet = !string.IsNullOrEmpty(loyaltyLevel);
            
            var model = new CheckoutViewModel
            {
                Cart = cart,
                TotalAmount = totalAmount,
                UserEmail = user.Email,
                LoyaltyLevel = loyaltyLevel,
                WalletBalance = user.WalletBalance ?? 0m,
                CanUseWallet = canUseWallet,
                PotentialEarnings = canUseWallet ? totalAmount * LoyaltyLevels.GetCashbackPercent(loyaltyLevel) : 0m,
                FinalAmount = totalAmount
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            
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
                TempData["Error"] = "Корзина пуста";
                return RedirectToAction("Index");
            }
            
            if (string.IsNullOrEmpty(user.Email))
            {
                TempData["Error"] = "Для оформления заказа необходимо указать адрес электронной почты в профиле";
                return RedirectToAction("Checkout");
            }

            try
            {
                var totalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
                var walletUsed = 0m;
                var walletEarned = 0m;
                var finalAmount = totalAmount;

                // Работа с виртуальным кошельком
                var loyaltyLevel = LoyaltyLevels.GetLoyaltyLevel(user.TotalSpent ?? 0m);
                if (!string.IsNullOrEmpty(loyaltyLevel))
                {
                    if (model.UseWallet)
                    {
                        // Списываем с кошелька
                        walletUsed = Math.Min(user.WalletBalance ?? 0m, totalAmount);
                        finalAmount = totalAmount - walletUsed;
                        user.WalletBalance = (user.WalletBalance ?? 0m) - walletUsed;
                    }
                    else
                    {
                        // Начисляем на кошелек
                        var cashbackPercent = LoyaltyLevels.GetCashbackPercent(loyaltyLevel);
                        walletEarned = totalAmount * cashbackPercent;
                        user.WalletBalance = (user.WalletBalance ?? 0m) + walletEarned;
                    }
                }

                // Создаем заказ
                var order = new Order
                {
                    UserId = user.Id,
                    OrderDate = DateTime.UtcNow,
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    DeliveryAddress = $"{model.Country}, {model.City}, {model.PostalCode}, {model.Street}, {model.HouseNumber}",
                    TotalAmount = totalAmount,
                    WalletUsed = walletUsed > 0 ? walletUsed : null,
                    WalletEarned = walletEarned > 0 ? walletEarned : null,
                    FinalAmount = finalAmount,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var cartItem in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceAtOrder = cartItem.Product.Price,
                        Order = order
                    };
                    order.OrderItems.Add(orderItem);
                }

                // Обновляем общую сумму потраченных средств пользователя
                user.TotalSpent = (user.TotalSpent ?? 0m) + finalAmount;
                
                // Обновляем уровень лояльности
                user.LoyaltyLevel = LoyaltyLevels.GetLoyaltyLevel(user.TotalSpent ?? 0m);

                _context.Orders.Add(order);
                _context.Carts.Remove(cart);

                await _context.SaveChangesAsync();

                // Отправляем email
                try
                {
                    await _emailService.SendOrderConfirmationAsync(user.Email, order);
                }
                catch (Exception emailEx)
                {
                    // Логируем ошибку отправки email, но не прерываем выполнение
                    // В production лучше использовать ILogger
                    Console.WriteLine($"Ошибка отправки email: {emailEx.Message}");
                }

                TempData["Success"] = "Заказ успешно оформлен! Спасибо за покупку!";
                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Ошибка при оформлении заказа: {ex.Message}");
                TempData["Error"] = "Произошла ошибка при оформлении заказа. Попробуйте еще раз.";
                
                // Возвращаем пользователя на страницу оформления заказа
                model.Cart = cart;
                model.TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
                model.UserEmail = user.Email;
                model.LoyaltyLevel = LoyaltyLevels.GetLoyaltyLevel(user.TotalSpent ?? 0m);
                model.WalletBalance = user.WalletBalance ?? 0m;
                model.CanUseWallet = !string.IsNullOrEmpty(model.LoyaltyLevel);
                model.PotentialEarnings = model.CanUseWallet ? model.TotalAmount * LoyaltyLevels.GetCashbackPercent(model.LoyaltyLevel) : 0m;
                
                return View("Checkout", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == int.Parse(userId));

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Добавить метод для AJAX запроса расчета итоговой суммы
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalculateTotal(bool useWallet)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Json(new { success = false });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return Json(new { success = false });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return Json(new { success = false });
            }

            var totalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
            var loyaltyLevel = LoyaltyLevels.GetLoyaltyLevel(user.TotalSpent ?? 0m);
            var walletBalance = user.WalletBalance ?? 0m;
    
            var walletDeduction = 0m;
            var walletEarnings = 0m;
            var finalAmount = totalAmount;

            if (!string.IsNullOrEmpty(loyaltyLevel))
            {
                if (useWallet)
                {
                    walletDeduction = Math.Min(walletBalance, totalAmount);
                    finalAmount = totalAmount - walletDeduction;
                }
                else
                {
                    var cashbackPercent = LoyaltyLevels.GetCashbackPercent(loyaltyLevel);
                    walletEarnings = totalAmount * cashbackPercent;
                }
            }

            return Json(new
            {
                success = true,
                walletDeduction = walletDeduction,
                walletEarnings = walletEarnings,
                finalAmount = finalAmount
            });
        }
    }
}