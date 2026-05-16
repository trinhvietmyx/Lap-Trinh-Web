using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab_01.Models;
using Lab_01.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab_01.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("product/category/{category?}")]
        public async Task<IActionResult> List(string? category, int page = 1)
        {
            int pageSize = 9;
            
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category!.Name == category);
            }

            int totalProducts = await query.CountAsync();
            var productsOnPage = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentCategory = category;
            ViewBag.Categories = await _context.Categories.Select(c => c.Name).ToListAsync();

            return View(productsOnPage);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
