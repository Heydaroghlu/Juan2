using Core.Entities;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.CustomExceptions;
using Service.HelperServices.Interfaces;
using System.Drawing;

namespace App.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]

    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ISizeRepository _sizeRepository;
        private readonly IColorRepository _colorRepository;

        readonly ILocalService _localService;
        readonly IFileService _fileService;
        readonly IWebHostEnvironment _webHost;
        public ProductController(IProductRepository productRepository, IColorRepository colorRepository, ISizeRepository sizeRepository,ILocalService localService,IFileService fileService,IWebHostEnvironment webHost)
        {
            _productRepository= productRepository;
            _colorRepository= colorRepository;
            _localService = localService;
            _sizeRepository= sizeRepository;    
            _webHost = webHost;
            _fileService = fileService;
        }
        public async Task<IActionResult> Index(int page=1,int categoryId=0,string? search=null)
        {
            ICollection<Product> products = await _productRepository.GetAllAsync(x => true, false, "ProductImages", "Category");
            
            if (search != null)
            {
                products = await _productRepository.GetAllAsync(x => x.Name.Contains(search), false);
            }
            if (categoryId != 0)
            {
                products=await _productRepository.GetAllAsync(x=>x.CategoryId==categoryId, false);
            }
            return View(products.ToList());
        }
        public async Task<IActionResult> Create()
        {
            var size=await _sizeRepository.GetAllAsync(x => x.IsDeleted == false);
            var color = await _colorRepository.GetAllAsync(x => x.IsDeleted == false);
            ViewBag.SizeIds = size.ToList();
            ViewBag.ColorIds=color.ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            var size = await _sizeRepository.GetAllAsync(x => x.IsDeleted == false);
            var color = await _colorRepository.GetAllAsync(x => x.IsDeleted == false);
            ViewBag.SizeIds = size.ToList();
            ViewBag.ColorIds = color.ToList();
            product.Sizes = new List<ProductSizes>();
            foreach (var sizeId in product.SizeIds)
            {
 
                ProductSizes productSizes = new ProductSizes
                {
                    SizeId = sizeId,
                    Product = product
                };
                product.Sizes.Add(productSizes);
            }
            if (product.PosterImage == null)
            {
                ModelState.AddModelError("PosterImage", "Poster şəkli boş ola bilməz!");
                return View();
            }
            else
            {
                product.ProductImages = new List<ProductImage>();
                try
                {
                    _fileService.CheckFileLenght(product.PosterImage.Length);
                    _fileService.CheckImageFile(product.PosterImage);
                }
                catch (ImageFileLenghtException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                    return View();
                }
                catch (CheckImageFileTypeException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                    return View();
                }
                catch (Exception ex)
                {
                    return View(ex.Message);
                }
               string filename = await _localService.SaveAsync(_webHost.WebRootPath, "uploads/products", product.PosterImage);
                ProductImage productImage = new ProductImage
                {
                    Image = filename,
                    IsPoster = true
                };
                product.ProductImages.Add(productImage);
            }
            if(product.ImageFiles!=null)
            {
                foreach (var item in product.ImageFiles)
                {
                    try
                    {
                        _fileService.CheckFileLenght(product.PosterImage.Length);
                        _fileService.CheckImageFile(product.PosterImage);
                    }
                    catch (ImageFileLenghtException ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                        return View();
                    }
                    catch (CheckImageFileTypeException ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                        return View();
                    }
                    catch (Exception ex)
                    {
                        return View(ex.Message);
                    }
                    string filename = await _localService.SaveAsync(_webHost.WebRootPath, "uploads/products", product.PosterImage);
                    ProductImage productImage = new ProductImage
                    {
                        Image = filename,
                        IsPoster = false
                    };
                    product.ProductImages.Add(productImage);
                }
            }
            if (product.SalePrice < 0)
            {
                ModelState.AddModelError("Price", "Qiymət mənfi ola bilməz");
                return View();

            }
            if (product.CostPrice < 0)
            {
                ModelState.AddModelError("Price", "Qiymət mənfi ola bilməz");
                return View();

            }
            if (product.DiscountPrice < 0)
            {
                ModelState.AddModelError("Price", "Qiymət mənfi ola bilməz");
                return View();

            }
            if (product.InStock < 0)
            {
                ModelState.AddModelError("Price", "Say mənfi ola bilməz");
                return View();

            }
            await _productRepository.AddAsync(product);
            await _productRepository.CommitAsync();
            return RedirectToAction("index");
        }
    }
}
