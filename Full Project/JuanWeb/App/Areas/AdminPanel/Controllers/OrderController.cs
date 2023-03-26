using Core.Entities;
using Core.Enums;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Microsoft.EntityFrameworkCore;

namespace App.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    [Authorize("Admin")]

    public class OrderController : Controller
    {

        DataContext _context;
        IWebHostEnvironment _env;
        IHttpContextAccessor _accessor;

        public OrderController(DataContext context, IWebHostEnvironment env, IHttpContextAccessor accessor)
        {
            _context = context;
            _env = env;
            _accessor = accessor;
        }
        public IActionResult Index(int page = 1, string word = null, OrderStatus orderStatus = OrderStatus.Gözləmədə)
        {
            var query = _context.Orders.AsQueryable();

            if (orderStatus == OrderStatus.Qəbul)
            {
                query = query.Where(x => x.Status == OrderStatus.Qəbul);
            }
            if (orderStatus == OrderStatus.İmtina)
            {
                query = query.Where(x => x.Status == OrderStatus.İmtina);
            }
            if (orderStatus == OrderStatus.Gözləmədə)
            {
                query = query.Where(x => x.Status == OrderStatus.Gözləmədə);
            }

            ViewBag.PageSize = 8;
            ViewBag.Word = word;
            TempData["Word"] = word;
            TempData["PageSize"] = 8;
            TempData["OrderStatus"] = orderStatus;


            return View(query.ToList());
        }

        public IActionResult Edit(int id)
        {
            Order order = _context.Orders.Include(x => x.AppUser).Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.ProductImages).
                Include(x => x.AppUser).Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.Sizes).ThenInclude(x => x.Size).
                FirstOrDefault(x => x.Id == id);

            if (order == null) return RedirectToAction("notfound", "pages");

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Accepted(int id)
        {
            Order order = _context.Orders.FirstOrDefault(x => x.Id == id);

            if (order == null) return RedirectToAction("notfound", "pages");

            order.Status = (OrderStatus)2;
            _context.SaveChanges();

            TempData["Success"] = "Order is Accessed";

            return RedirectToAction("index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Canceled(int id)
        {
            Order order = _context.Orders.FirstOrDefault(x => x.Id == id);

            if (order == null) return RedirectToAction("notfound", "pages");

            order.Status = (OrderStatus)3;
            _context.SaveChanges();

            TempData["Success"] = "Order is Canceled";

            return RedirectToAction("index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Pending(int id)
        {
            Order order = _context.Orders.FirstOrDefault(x => x.Id == id);

            if (order == null) return RedirectToAction("notfound", "pages");

            order.Status = (OrderStatus)1;
            _context.SaveChanges();

            TempData["Success"] = "Order is Pending";

            return RedirectToAction("index");
        }

        //public IActionResult ExportToExcel()
        //{

        //    var workbook = new XLWorkbook();

        //    var worksheet = workbook.Worksheets.Add("Orders");

        //    int row = 2;

        //    var orders = _context.Orders.ToList();

        //    worksheet.Cell(1, 1).Value = "Email";
        //    worksheet.Cell(1, 2).Value = "FullName";
        //    worksheet.Cell(1, 3).Value = "Address";
        //    worksheet.Cell(1, 4).Value = "Aparment";
        //    worksheet.Cell(1, 5).Value = "City";
        //    worksheet.Cell(1, 6).Value = "Phone";
        //    worksheet.Cell(1, 7).Value = "Status";
        //    worksheet.Cell(1, 8).Value = "TotalAmount";
        //    worksheet.Cell(1, 9).Value = "OrderCode";

        //    foreach (var item in orders)
        //    {
        //        worksheet.Cell(row, 1).Value = item.Email;
        //        worksheet.Cell(row, 2).Value = item.FullName;
        //        worksheet.Cell(row, 3).Value = item.Addresses;
        //        worksheet.Cell(row, 4).Value = item.Aparment;
        //        worksheet.Cell(row, 5).Value = item.City;
        //        worksheet.Cell(row, 6).Value = item.Phone;
        //        worksheet.Cell(row, 7).Value = item.Status;
        //        worksheet.Cell(row, 8).Value = item.TotalAmount;
        //        worksheet.Cell(row, 9).Value = item.OrderCode;
        //        row++;
        //    }

        //    var stream = new MemoryStream();
        //    workbook.SaveAs(stream);
        //    var content = stream.ToArray();
        //    stream.Close();
        //    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
        //}

    }
}
