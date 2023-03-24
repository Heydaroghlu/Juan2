using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            //Login register bir yerdedi
            return View();
        }
        public IActionResult Register()
        {
            //Login register bir yerdedi
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }
    }
}
