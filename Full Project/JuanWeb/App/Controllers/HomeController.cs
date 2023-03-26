using App.Models;
using Core.Entities;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;
using Service.ViewModels;
using System.Diagnostics;

namespace App.Controllers
{
    public class HomeController : Controller
    {
        readonly IBrandRepository _brandRepository;
        readonly IProductRepository _productRepository;
        readonly IProductImagesRepository _productImagesRepository;
        readonly IBlogRepository _blogRepository;
        readonly ISliderService _sliderService;
        readonly ISettingRepository _settingRepository;
        public HomeController(IBrandRepository brandRepository, IProductRepository productRepository, IProductImagesRepository productImagesRepository, IBlogRepository blogRepository, ISliderService sliderService, ISettingRepository settingRepository)
        {
            _brandRepository = brandRepository;
            _productRepository = productRepository;
            _productImagesRepository = productImagesRepository;
            _blogRepository = blogRepository;
            _sliderService = sliderService;
            _settingRepository = settingRepository;
        }
        public async Task<IActionResult> Index()
        {
            var brands = await _brandRepository.GetAllAsync(x => !x.IsDeleted);
            var products = await _productRepository.GetAllAsync(x => !x.IsDeleted, false, "ProductImages");
            var blogs = await _blogRepository.GetAllAsync(x => !x.IsDeleted, false);
            var topSeller = products.OrderBy(x => x.SellCount).Take(10);
            var sliders = await _sliderService.GetAllAsync(x => !x.IsDeleted);
            var settings = await _settingRepository.GetAllAsync(x => !x.IsDeleted);


            HomeViewModel model = new HomeViewModel
            {
                Brands = brands.ToList(),
                Products = products.ToList(),
                Blogs = blogs.Take(5).ToList(),
                TopSeller = topSeller.ToList(),
                Sliders = sliders.ToList(),
                Settings = settings.ToList(),
            };

            return View(model);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}