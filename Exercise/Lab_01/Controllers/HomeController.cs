using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Lab_01.Models;

namespace Lab_01.Controllers;

public class HomeController : Controller
{
    private static readonly List<Product> Products = new List<Product>
    {
        new Product { Id = 1, Name = "Váy Quây dài", Price = "600,000đ", OldPrice = "800,000đ", Sale = "-25%", Image = "https://images.unsplash.com/photo-1595777457583-95e059d581b8?auto=format&fit=crop&w=800&q=80", Description = "Chiếc váy quây dài duyên dáng, phù hợp cho những buổi tiệc tối sang trọng hoặc đi dạo biển." },
        new Product { Id = 2, Name = "Striped asymmetric t-shirt", Price = "820,000đ", OldPrice = "9,780,000đ", Sale = "-92%", Image = "https://images.unsplash.com/photo-1529139574466-a303027c1d8b?auto=format&fit=crop&w=800&q=80", Description = "Áo thun kẻ sọc với thiết kế bất đối xứng cá tính, chất liệu cotton thoáng mát." },
        new Product { Id = 3, Name = "Sơ Mi Nam Tay Dài", Price = "400,000đ", OldPrice = "", Sale = "", Image = "https://images.unsplash.com/photo-1602810318383-e386cc2a3ccf?auto=format&fit=crop&w=800&q=80", Description = "Sơ mi nam tay dài phom dáng slim-fit, tôn dáng và lịch lãm cho quý ông công sở." },
        new Product { Id = 4, Name = "Sơ mi caro tay ngắn", Price = "345,000đ", OldPrice = "", Sale = "", Image = "https://images.unsplash.com/photo-1617137968427-85924c800a22?auto=format&fit=crop&w=800&q=80", Description = "Sơ mi caro tay ngắn năng động, dễ dàng phối cùng quần short hoặc quần jeans." },
        new Product { Id = 5, Name = "Quần tây xanh navy", Price = "520,000đ", OldPrice = "650,000đ", Sale = "-20%", Image = "https://images.unsplash.com/photo-1624378439575-d8705ad7ae80?auto=format&fit=crop&w=800&q=80", Description = "Quần tây màu xanh navy thanh lịch, đường may tỉ mỉ, phù hợp cho môi trường chuyên nghiệp." },
        new Product { Id = 6, Name = "Áo thun lưng hở", Price = "310,000đ", OldPrice = "365,000đ", Sale = "-15%", Image = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=800&q=80", Description = "Áo thun lưng hở gợi cảm, phong cách trẻ trung và quyến rũ cho phái đẹp." },
        new Product { Id = 7, Name = "Sơ mi trắng công sở", Price = "390,000đ", OldPrice = "580,000đ", Sale = "-33%", Image = "https://images.unsplash.com/photo-1598033129183-c4f50c736f10?auto=format&fit=crop&w=800&q=80", Description = "Sơ mi trắng cơ bản không thể thiếu trong tủ đồ, chất vải mềm mịn, ít nhăn." },
        new Product { Id = 8, Name = "Quần short pastel", Price = "275,000đ", OldPrice = "", Sale = "", Image = "https://images.unsplash.com/photo-1603252109303-2751441dd157?auto=format&fit=crop&w=800&q=80", Description = "Quần short màu pastel nhẹ nhàng, mang lại cảm giác thoải mái và tươi mới trong ngày hè." }
    };

    public IActionResult Index()
    {
        return View(Products);
    }

    public IActionResult Details(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
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
