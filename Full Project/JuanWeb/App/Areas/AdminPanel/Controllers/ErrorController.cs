﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace App.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]

    public class ErrorController : Controller
    {
        public new IActionResult NotFound()
        {
           return View();
        }
    }
}
