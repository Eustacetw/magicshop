using Microsoft.AspNetCore.Mvc;
using System.Linq;
using _1131740.Data;
using _1131740.Models;
using Microsoft.AspNetCore.Http; 
namespace _1131740.Controllers
{
    public class AccountController : Controller
    {
        private readonly CmsContext _context;

        public AccountController(CmsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                var member = _context.Members
                    .FirstOrDefault(m => m.Username == model.Username && m.Password == model.Password);

                if (member != null)
                {
                    
                    HttpContext.Session.SetString("Username", member.Username);

                    
                    return RedirectToAction("Product", "Shop");
                }
                else
                {
                    ViewBag.ErrorMessage = "帳號或密碼錯誤，請重新輸入！";
                }
            }
            return View(model);
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
               
                bool isExist = _context.Members.Any(m => m.Username == model.Username);

                if (isExist)
                {
                    ViewBag.ErrorMessage = "這個帳號已經被註冊過了，請換一個！";
                    return View(model);
                }

                
                var newMember = new Member
                {
                    Username = model.Username,
                    Password = model.Password,
                    Name = model.Name
                };

                
                _context.Members.Add(newMember);
                _context.SaveChanges(); 

                
                TempData["SuccessMessage"] = "🎉 註冊成功！請使用新帳號登入。";
                return RedirectToAction("Login");
            }

            return View(model);
        }
    }
}