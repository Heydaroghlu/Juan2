using Core.Entities;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Service.CustomExceptions;
using Service.HelperServices.Interfaces;

namespace App.Areas.AdminPanel.Controllers
{
    [Area("Adminpanel")]
    public class BlogController : Controller
    {
        readonly IFileService _fileService;
        readonly ILocalService _localService;
        readonly IWebHostEnvironment _webHost;
        readonly IBlogRepository _blogRepository;
        public BlogController(IFileService fileService, ILocalService localService, IWebHostEnvironment webHost, IBlogRepository blogRepository)
        {
            _fileService = fileService;
            _localService = localService;
            _webHost = webHost;
            _blogRepository = blogRepository;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _blogRepository.GetAllAsync(x => x.IsDeleted == false);
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            if(blog.Text == null)
            {
                ModelState.AddModelError("Text", "Required");
                return View();
            }

            blog.CreatedAt = DateTime.UtcNow.AddHours(4);
            await _blogRepository.AddAsync(blog);
            await _blogRepository.CommitAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _blogRepository.GetAsync(x => x.Id == id);
            if (entity == null) return RedirectToAction("notfound", "error");
            return View(entity);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Blog entity)
        {
            var existEntity = await _blogRepository.GetAsync(x => x.Id == entity.Id);

            if (existEntity == null)
            {
                return RedirectToAction("notfound", "error");
            }

            existEntity.Text = entity.Text;
            existEntity.UpdatedAt = DateTime.UtcNow.AddHours(4);

            return RedirectToAction("index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _blogRepository.GetAsync(x => x.Id == id);
            if (entity == null)
            {
                return RedirectToAction("notfound", "error");
            }
            entity.IsDeleted = true;
            await _blogRepository.CommitAsync();
            return Ok();
        }



    }
}
