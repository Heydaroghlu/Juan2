using Core.Entities;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.ViewModels;
using System.Linq.Expressions;

namespace App.Controllers
{
    public class ProductController : Controller
    {

        readonly IProductRepository _productRepository;
        readonly IProductCommentRepository _productCommentRepository;
        public ProductController(IProductRepository productRepository, IProductCommentRepository productCommentRepository)
        {
            _productRepository = productRepository;
            _productCommentRepository = productCommentRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string? word = null)
        {
            ICollection<Product> products = await _productRepository.GetAllAsync(x => true, false);
            if (word != null)
            {
                products = await _productRepository.GetAllAsync(x => x.Name.Contains(word), false);
            }
            PagenatedListDto<Product> list  = PagenatedListDto<Product>.Save(products.AsQueryable(), page,1);
            return View(list);
        }   
        public async Task<IActionResult> Detail(int id)
        {
            ProductViewModel model = new()
            {
                Product = await _productRepository.GetAsync(x => x.Id == id, false, "ProductImages", "ProductComments"),
                ProductComment = new ProductComment()
            };

            return View(model);
        }
       
        public IActionResult Wishlist()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> sendComment(ProductComment comment)
        {

            if(comment.Text == null || comment.FullName == null)
            {
                ModelState.AddModelError("","xeta bas verdi");
                return RedirectToAction("detail",comment.ProductId);
            }

            await _productCommentRepository.AddAsync(comment);
            await _productCommentRepository.CommitAsync();

            return RedirectToAction("detail", comment.ProductId);
        }


    }
}
