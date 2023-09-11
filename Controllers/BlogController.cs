using AdminBlog.Filter;
using BlogAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace BlogAdmin.Controllers
{
 
    public class BlogController : Controller
    {
        private readonly ILogger<BlogController> _logger;
        private readonly BlogContext _blogContext;

        public BlogController(ILogger<BlogController> logger, BlogContext context)
        {
            _logger = logger;
            _blogContext = context;
        }


        public IActionResult Index()
        {
            var list = _blogContext.Blog.ToList();

            return View(list);
        }

        public IActionResult Publish(int Id)
        {
            var blog = _blogContext.Blog.Find(Id);
            blog.IsPublish = true;
            _blogContext.Update(blog);
            _blogContext.SaveChanges(); 
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteBlog(int? Id)
        {
            var blog  = await _blogContext.Blog.FindAsync(Id);
            _blogContext.Remove(blog);
            _blogContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Blog()
        {
            ViewBag.Categories = _blogContext.Category.Select(w =>
                new SelectListItem
                {
                    Text = w.Name,
                    Value = w.Id.ToString()
                }
            ).ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Save(Blog model)
        {
            if (model != null)
            {
                var file = Request.Form.Files.First();
                string savePath = Path.Combine("C:", "Users", "ardatunca.alev", "source", "repos", "BlogProje", "wwwroot", "images");
                var fileName = $"{DateTime.Now:MMddHHmmss}.{file.FileName.Split(".").Last()}";
                var fileUrl = Path.Combine(savePath, fileName);
                using (var fileStream = new FileStream(fileUrl, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                model.ImagePath = fileName;
                model.AuthorId = (int)HttpContext.Session.GetInt32("id");
                await _blogContext.AddAsync(model);
                await _blogContext.SaveChangesAsync();
                return Json(true);

            }

            return Json(false);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
