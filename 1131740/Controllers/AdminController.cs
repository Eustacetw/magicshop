using Microsoft.AspNetCore.Mvc;
using _1131740.Data;
using _1131740.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace _1131740.Controllers
{
    public class AdminController : Controller
    {
        private readonly CmsContext _context;

        public AdminController(CmsContext context)
        {
            _context = context;
        }

        
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Username") == "admin";
        }

        
        public async Task<IActionResult> Admin()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Admin");
        }

        
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Admin");
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return RedirectToAction("Admin"); 
            }
            return View(product); 
        }

        
        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Products.Update(product); 
                await _context.SaveChangesAsync(); 
                return RedirectToAction("Admin", "Admin"); 
            }
            return View(product); 
        }
    }
}