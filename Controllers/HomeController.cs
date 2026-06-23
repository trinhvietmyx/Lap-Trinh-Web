using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GIUA_KY_WEB_3AE.Models;
using GIUA_KY_WEB_3AE.Repositories;
using System.Globalization;
using System.Text;

namespace GIUA_KY_WEB_3AE.Controllers;

/// <summary>
/// Controller trang chủ của hệ thống, xử lý hiển thị danh sách sản phẩm và các trang thông tin chung.
/// </summary>
public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// Hàm khởi tạo HomeController, sử dụng Dependency Injection để tiêm Product Repository.
    /// </summary>
    /// <param name="productRepository">Repository chứa các phương thức truy vấn Product.</param>
    public HomeController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// Action hiển thị trang chủ của website với danh sách sản phẩm công nghệ.
    /// </summary>
    /// <returns>View Index chứa danh sách sản phẩm.</returns>
    public IActionResult Index(string? searchTerm)
    {
        var products = _productRepository.GetAll();
        var categories = _productRepository.GetAllCategories().ToDictionary(c => c.Id, c => c.Name);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var keyword = NormalizeSearchText(searchTerm);
            products = products.Where(p =>
            {
                var categoryName = categories.TryGetValue(p.CategoryId, out var name) ? name : string.Empty;
                var searchableText = NormalizeSearchText($"{p.Name} {p.Description} {categoryName}");
                return searchableText.Contains(keyword);
            }).ToList();
        }

        ViewBag.SearchTerm = searchTerm?.Trim() ?? string.Empty;
        ViewBag.ResultCount = products.Count();
        ViewBag.Categories = categories;
        return View(products);
    }

    /// <summary>
    /// Action hiển thị trang chính sách bảo mật (Privacy Policy).
    /// </summary>
    /// <returns>View Privacy.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Action xử lý hiển thị trang thông báo lỗi khi có ngoại lệ xảy ra trong ứng dụng.
    /// </summary>
    /// <returns>View Error chứa thông tin RequestId.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static string NormalizeSearchText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Replace('đ', 'd').Replace('Đ', 'd');
    }
}
