using _1131740.Data;
using _1131740.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace _1131740.Controllers
{
    public class ShopController : Controller
    {
        private readonly CmsContext _context;
        private readonly string _orderFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "orders.json");

        public ShopController(CmsContext context)
        {
            _context = context;
        }

        private async Task SyncUserBalanceToSession()
        {
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _context.Members.FirstOrDefaultAsync(m => m.Username == username);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("Balance", user.Balance ?? 0);
                    HttpContext.Session.SetInt32("TotalConsumed", user.TotalConsumed ?? 0);
                }
            }
        }

        private List<OrderLog> GetOrderHistory()
        {
            try
            {
                if (System.IO.File.Exists(_orderFilePath))
                {
                    var json = System.IO.File.ReadAllText(_orderFilePath);
                    return JsonSerializer.Deserialize<List<OrderLog>>(json) ?? new List<OrderLog>();
                }
            }
            catch { }
            return new List<OrderLog>();
        }

        private void SaveOrderHistory(List<OrderLog> history)
        {
            try
            {
                var json = JsonSerializer.Serialize(history);
                System.IO.File.WriteAllText(_orderFilePath, json);
            }
            catch { }
        }

        public async Task<IActionResult> Product()
        {
            await SyncUserBalanceToSession();
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            await SyncUserBalanceToSession();
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public IActionResult AddToCart(CartItem item)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cart = string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            cart.Add(item);
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> Cart()
        {
            await SyncUserBalanceToSession();
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            return View(cart);
        }

        public IActionResult RemoveFromCart(string id)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(cartJson))
            {
                var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                var itemToRemove = cart.FirstOrDefault(c => c.CartItemId == id);
                if (itemToRemove != null)
                {
                    cart.Remove(itemToRemove);
                    HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
                }
            }
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult Checkout(string Pay_zg)
        {
            var username = HttpContext.Session.GetString("Username") ?? "訪客";
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson)) return RedirectToAction("Cart");
            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);

            int totalAmount = cart.Sum(x => x.SubTotal);
            int finalAmount = totalAmount + 35;
            string orderNo = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");

            
            RecordOrderToHistory(cart, orderNo, "速買配超商代碼", finalAmount, "等待發貨", username);

            var customerInfo = cart.First();
            var smilePayData = new Dictionary<string, string>
            {
                { "Dcvc", "YOUR_MERCHANT_ID" }, 
                { "Rvg2c", "1" },
                { "Data_id", orderNo },
                { "Amount", finalAmount.ToString() },
                { "Pay_zg", Pay_zg ?? "4" },
                { "Verify_key", "YOUR_VERIFY_KEY" }, 
                { "Pur_name", customerInfo.RealName ?? "顧客" },
                { "Tel_number", customerInfo.Phone ?? "" },
                { "Mobile_number", customerInfo.Phone ?? "" },
                { "Email", customerInfo.Email ?? "" },
                { "Roturl", $"https://localhost:7198/Shop/PaySuccess?orderNo={orderNo}&amount={finalAmount}&fee=35" }
            };

            ViewBag.SmilePayData = smilePayData;
            ViewBag.SubmitUrl = "https://ssl.smse.com.tw/ezpos/mtmk_utf.asp";

            HttpContext.Session.Remove("Cart");
            return View("RedirectToPay");
        }

        [HttpPost]
        public async Task<IActionResult> PayWithBalance()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "請先登入會員才能使用餘額結帳！";
                return RedirectToAction("Cart");
            }

            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson)) return RedirectToAction("Cart");
            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);

            int totalAmount = cart.Sum(x => x.SubTotal);

            var user = await _context.Members.FirstOrDefaultAsync(m => m.Username == username);
            if (user == null) return RedirectToAction("Cart");

            int currentBalance = user.Balance ?? 0;
            int currentConsumed = user.TotalConsumed ?? 0;

            if (currentBalance < totalAmount)
            {
                TempData["ErrorMessage"] = "您的商城餘額不足！請聯絡客服儲值。";
                return RedirectToAction("Cart");
            }

            user.Balance = currentBalance - totalAmount;
            user.TotalConsumed = currentConsumed + totalAmount;
            _context.Members.Update(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("Balance", user.Balance ?? 0);
            HttpContext.Session.SetInt32("TotalConsumed", user.TotalConsumed ?? 0);

            string orderNo = "BAL" + DateTime.Now.ToString("yyyyMMddHHmmss");

         
            RecordOrderToHistory(cart, orderNo, "商城餘額支付", totalAmount, "等待發貨", username);

            HttpContext.Session.Remove("Cart");
            return RedirectToAction("PaySuccess", new { orderNo = orderNo, amount = totalAmount, fee = 0 });
        }

        public async Task<IActionResult> AddTestMoney()
        {
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _context.Members.FirstOrDefaultAsync(m => m.Username == username);
                if (user != null)
                {
                    user.Balance = (user.Balance ?? 0) + 5000;
                    _context.Members.Update(user);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetInt32("Balance", user.Balance ?? 0);
                }
            }
            return Redirect(Request.Headers["Referer"].ToString() ?? "/Shop/Product");
        }

        public async Task<IActionResult> OrderHistory()
        {
            await SyncUserBalanceToSession();
            var username = HttpContext.Session.GetString("Username");
            var history = GetOrderHistory();

            if (!string.IsNullOrEmpty(username))
            {
                history = history.Where(o => o.Username == username).ToList();
            }
            else
            {
                history = new List<OrderLog>();
            }

            history.Reverse();
            return View(history);
        }

        public async Task<IActionResult> PaySuccess(string orderNo, int amount, int fee)
        {
            await SyncUserBalanceToSession();
            var history = GetOrderHistory();
            var currentOrder = history.FirstOrDefault(o => o.OrderNo == orderNo);

            ViewBag.OrderNo = orderNo;
            ViewBag.Amount = amount;
            ViewBag.Fee = fee;
            ViewBag.ItemSubTotal = amount - fee;

            return View(currentOrder);
        }

        public async Task<IActionResult> Contact()
        {
            await SyncUserBalanceToSession();
            return View();
        }

        private void RecordOrderToHistory(List<CartItem> cart, string orderNo, string method, int total, string status, string username)
        {
            var history = GetOrderHistory();
            var newOrder = new OrderLog
            {
                OrderNo = orderNo,
                OrderDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                PaymentMethod = method,
                TotalAmount = total,
                Status = status,
                Username = username,
                Items = cart
            };

            history.Add(newOrder);
            SaveOrderHistory(history);
        }
    }
}