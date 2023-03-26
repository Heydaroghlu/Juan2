using Core.Entities;
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Newtonsoft.Json;
using Service.Services.Interfaces;
using Service.ViewModels;

namespace App.Controllers
{
    public class OrderController : Controller
    {

        DataContext _context;
        UserManager<AppUser> _userManager;
        IEmailService _emailService;
        IWebHostEnvironment _env;
        public OrderController(DataContext context, UserManager<AppUser> userManager, IEmailService emailService, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _env = env;
        }

        public IActionResult Checkout()
        {
            CheckoutViewModel checkoutItemVM = new CheckoutViewModel();

            AppUser member = null;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                member = _userManager.Users.FirstOrDefault(x => x.UserName == User.Identity.Name && !x.IsAdmin);
            }

            if (member == null)
            {
                string basketItemStr = HttpContext.Request.Cookies["basketItemList"];

                if (!string.IsNullOrWhiteSpace(basketItemStr))
                {
                    checkoutItemVM.basketItemsVM = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketItemStr);
                    foreach (var item in checkoutItemVM.basketItemsVM)
                    {
                        Product product = _context.Products.Include(x => x.Sizes).ThenInclude(x => x.Size)
                            .Include(x => x.ProductImages).FirstOrDefault(x => x.Id == item.ProductId);

                        if (product != null)
                        {
                            item.Name = product.Name;
                            item.Price = product.CostPrice;
                            item.PosterImage = product.ProductImages.FirstOrDefault(x => x.IsPoster == true)?.Image;
                        }
                        checkoutItemVM.TotalAmount += item.Price;
                    }
                }
            }
            else
            {
                checkoutItemVM.basketItemsVM = _context.BasketItems.Include(x => x.Product).Where(x => x.AppUserId == member.Id).Select(x => new BasketItemViewModel()
                {
                    ProductId = x.ProductId,
                    Name = x.Product.Name,
                    Count = x.Count,
                    PosterImage = x.Product.ProductImages.FirstOrDefault(ps => ps.IsPoster == true).Image,
                    Price = x.Price,
                    //TotalPrice 
                }).ToList();


                foreach (var item in checkoutItemVM.basketItemsVM)
                {
                    checkoutItemVM.TotalAmount += item.Price;
                }
            }

            return View(checkoutItemVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel checkoutVM)
        {

            checkoutVM.basketItemsVM = _getBasketItem();

            if (!ModelState.IsValid) return View(checkoutVM);

            AppUser member = null;

            if (User.Identity.IsAuthenticated)
            {
                member = _userManager.Users.FirstOrDefault(x => x.UserName == User.Identity.Name && !x.IsAdmin);
            }

            Order order = new Order()
            {
                Address = checkoutVM.Addresses,
                FullName = checkoutVM.FullName,
                Phone = checkoutVM.Phone,
                CreatedAt = DateTime.UtcNow.AddHours(4),
                OrderItems = new List<OrderItem>(),
                Email = checkoutVM.Email,
                
                Status = Core.Enums.OrderStatus.Gözləmədə,
            };

            string basketItemsStr;

            List<BasketItemViewModel> basketItemsVM = new List<BasketItemViewModel>();

            if (member != null)
            {
                order.AppUserId = member.Id;
                order.FullName = member.FullName;
                basketItemsVM = _context.BasketItems.Where(x => x.AppUserId == member.Id).Select(x => new BasketItemViewModel
                {
                    ProductId = x.Product.Id,
                    Count = x.Count,
                    Name = x.Product.Name,
                    Price = x.Price,
                    PosterImage = x.Product.ProductImages.FirstOrDefault(x=>x.IsPoster==true).Image,
                    //ProdName = x.Product.Name,
                    //SizeName = pro.uct.ProductSizes.FirstOrDefault(x => x.Size.Name == x.Size.Name).Size.Name,
                    //ColorId = item.ColorId,
                    //ColorName = item.ColorName,
                    //Count = item.Count,
                    //CostPrice = item.SalePrice,
                    //Discount = item.Discount,
                }).ToList();
            }
            else
            {
                basketItemsStr = HttpContext.Request.Cookies["basketItemList"];

                if (basketItemsStr != null)
                {
                    basketItemsVM = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketItemsStr);
                }
            }

            foreach (var item in basketItemsVM)
            {
                Product product = _context.Products.Include(x => x.ProductImages).Include(x => x.Sizes).ThenInclude(x => x.Size).FirstOrDefault(x => x.Id == item.ProductId);

                if (product == null)
                {
                    ModelState.AddModelError("", "Not found");
                    return View(checkoutVM);
                }

                _addOrderItem(ref order, product, item);
            }

            if (order.OrderItems.Count == 0)
            {
                ModelState.AddModelError("", "NotFound");
                return View(checkoutVM);
            }

            var lastOrder = _context.Orders.OrderByDescending(x => x.Id).FirstOrDefault();

            _context.Orders.Add(order);
            _context.SaveChanges();

            if (member != null)
            {
                _context.BasketItems.RemoveRange(_context.BasketItems.Where(x => x.AppUserId == member.Id));
                _context.SaveChanges();
            }
            else
            {
                //Response.Cookies.Delete("BasketItem");
            }

            basketItemsVM.Clear();

            basketItemsStr = HttpContext.Request.Cookies["basketItemList"];

            basketItemsStr = JsonConvert.SerializeObject(basketItemsVM);

            HttpContext.Response.Cookies.Append("basketItemList", basketItemsStr);


            string messageBody = string.Format("Your order is pending");

            _emailService.Send(order.Email, "Order Message", messageBody);

            TempData["Success"] = "Thanks for your order";

            return RedirectToAction("index", "home");
        }

        public IActionResult Shipping()
        {
            return View();
        }

        public IActionResult Payment()
        {
            return View();
        }

        private void _addOrderItem(ref Order order, Product product, BasketItemViewModel item)
        {
            product.SellCount = product.SellCount+ 1;


            OrderItem orderItem = new OrderItem()
            {
                ProductId = product.Id,
                ProdName = product.Name,
                Count = item.Count,
                CostPrice = product.CostPrice,
                SalePrice = product.SalePrice,
                OrderId = order.Id,
            };
            order.OrderItems.Add(orderItem);
            order.TotalAmount += (orderItem.SalePrice);
        }
        private List<BasketItemViewModel> _getBasketItem()
        {
            List<BasketItemViewModel> basketItemsVM = new List<BasketItemViewModel>();

            AppUser member = null;

            if (User.Identity.IsAuthenticated)
            {
                member = _userManager.Users.FirstOrDefault(x => x.UserName == User.Identity.Name && !x.IsAdmin);
            }

            if (member == null)
            {
                string basketItemsStr = HttpContext.Request.Cookies["basketItemList"];

                if (!string.IsNullOrWhiteSpace(basketItemsStr))
                {

                    basketItemsVM = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketItemsStr);

                    foreach (var item in basketItemsVM)
                    {

                        Product product = _context.Products.Include(x => x.ProductImages).
                            Include(x => x.Sizes).ThenInclude(x => x.Size).FirstOrDefault(x => x.Id == item.ProductId);


                        if (product != null)
                        {
                            item.Name = product.Name;
                            item.Price = product.SalePrice;
                            item.PosterImage = product.ProductImages.FirstOrDefault(x => x.IsPoster == true)?.Image;
                        }

                    }
                }
            }
            else
            {
                basketItemsVM = _context.BasketItems.Include(x => x.Product).ThenInclude(x => x.ProductImages).
                    Include(x => x.Product).ThenInclude(x => x.Sizes).ThenInclude(x => x.Size).Select(x => new BasketItemViewModel()
                    {
                        Count = x.Count,
                        Price = x.Product.SalePrice,
                        Name = x.Product.Name,
                        PosterImage = x.Product.ProductImages.FirstOrDefault(x => x.IsPoster == true).Image,
                        ProductId = x.Product.Id,
                    }).ToList();
            }

            return basketItemsVM;
        }



        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult Compare()
        {
            return View();
        }
    }
}
