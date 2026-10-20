using _1131740.Models;
using _1131740.Data;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace _1131740.Controllers
{
    public class CustomerManagerController : Controller
    {
        private readonly CmsContext _context;
        private readonly string _orderFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "orders.json");

        public CustomerManagerController(CmsContext context)
        {
            _context = context;
        }

        private List<OrderLog> GetOrderHistory()
        {
            if (!System.IO.File.Exists(_orderFilePath)) return new List<OrderLog>();
            var json = System.IO.File.ReadAllText(_orderFilePath);
            return JsonSerializer.Deserialize<List<OrderLog>>(json) ?? new List<OrderLog>();
        }

        private void SaveOrderHistory(List<OrderLog> history)
        {
            var json = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_orderFilePath, json);
        }

        
        public async Task<IActionResult> Customer()
        {
            var members = await _context.Members.ToListAsync();
            return View(members);
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(string username, string password, string name, int balance)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return RedirectToAction("Customer");
            }

            var newMember = new Member
            {
                Username = username,
                Password = password,
              
                Name = string.IsNullOrWhiteSpace(name) ? "未命名" : name,
                Balance = balance,
                TotalConsumed = 0
            };

            _context.Members.Add(newMember);
            await _context.SaveChangesAsync();
            return RedirectToAction("Customer");
        }

        
        public async Task<IActionResult> Edit(string id)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Username == id);
            if (member == null) return NotFound();
            return View(member);
        }

        
        [HttpPost]
        public async Task<IActionResult> Edit(Member updatedMember)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Username == updatedMember.Username);
            if (member != null)
            {
                member.Password = updatedMember.Password;
                member.Name = updatedMember.Name;
                member.Balance = updatedMember.Balance;
                member.TotalConsumed = updatedMember.TotalConsumed;

                _context.Members.Update(member);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Customer");
        }

        
        public async Task<IActionResult> Delete(string id)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Username == id);
            if (member != null)
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Customer");
        }

        
        public async Task<IActionResult> Details(string id)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Username == id);
            if (member == null) return NotFound();

            
            var allOrders = GetOrderHistory();
            var customerOrders = allOrders.Where(o => o.Username == id).OrderByDescending(o => o.OrderDate).ToList();

            ViewBag.Member = member; 
            return View(customerOrders);
        }

        
        [HttpPost]
        public IActionResult Fulfill(string orderNo, string username)
        {
            var allOrders = GetOrderHistory();
            var order = allOrders.FirstOrDefault(o => o.OrderNo == orderNo);

            if (order != null)
            {
                order.Status = "發貨成功"; 
                SaveOrderHistory(allOrders);
            }

            return RedirectToAction("Details", new { id = username });
        }

        
        [HttpPost]
        public IActionResult DeleteOrder(string orderNo, string username)
        {
            var allOrders = GetOrderHistory();
            var orderToRemove = allOrders.FirstOrDefault(o => o.OrderNo == orderNo);

            if (orderToRemove != null)
            {
                allOrders.Remove(orderToRemove);
                SaveOrderHistory(allOrders); 
            }

            return RedirectToAction("Details", new { id = username });
        }

        
        [HttpPost]
        public async Task<IActionResult> RefundOrder(string orderNo, string username, int amount)
        {
            try
            {
                
                var member = await _context.Members.FirstOrDefaultAsync(m => m.Username == username);
                if (member == null)
                {
                    return NotFound(new { success = false, message = "找不到該名顧客" });
                }

                
                var allOrders = GetOrderHistory();
                var order = allOrders.FirstOrDefault(o => o.OrderNo == orderNo);

                if (order != null)
                {
                    
                    if (!order.Status.Contains("退款"))
                    {
                        
                        member.Balance = (member.Balance ?? 0) + amount;
                        member.TotalConsumed = (member.TotalConsumed ?? 0) - amount;
                        if (member.TotalConsumed < 0) member.TotalConsumed = 0; 

                        
                        order.Status = "已退款";

                        
                        _context.Members.Update(member);
                        await _context.SaveChangesAsync();
                        SaveOrderHistory(allOrders);
                    }
                }

                
                return Ok(new
                {
                    success = true,
                    message = "退款成功",
                    newBalance = member.Balance 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}