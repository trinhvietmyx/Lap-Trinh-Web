using GIUA_KY_WEB_3AE.Models;
using GIUA_KY_WEB_3AE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text;

namespace GIUA_KY_WEB_3AE.Controllers
{
    /// <summary>
    /// Controller điều hướng và xử lý các tác vụ CRUD đối với đối tượng Sản phẩm (Product).
    /// </summary>
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Hàm khởi tạo của ProductController, sử dụng Dependency Injection để tiêm Repository và Web Host Environment.
        /// </summary>
        /// <param name="productRepository">Repository chứa các phương thức xử lý Product.</param>
        /// <param name="webHostEnvironment">Môi trường chạy web để lấy đường dẫn vật lý cho việc lưu file.</param>
        public ProductController(IProductRepository productRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Action hiển thị danh sách toàn bộ sản phẩm.
        /// </summary>
        /// <returns>Trang Index chứa danh sách sản phẩm.</returns>
        public IActionResult Index(string? search)
        {
            var products = _productRepository.GetAll();
            var categories = _productRepository.GetAllCategories().ToDictionary(c => c.Id, c => c.Name);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = NormalizeSearchText(search);
                products = products.Where(p =>
                {
                    var categoryName = categories.TryGetValue(p.CategoryId, out var name) ? name : string.Empty;
                    var searchableText = NormalizeSearchText($"{p.Name} {p.Description} {categoryName}");
                    return searchableText.Contains(keyword);
                }).ToList();
            }

            ViewBag.Search = search?.Trim() ?? string.Empty;
            ViewBag.ResultCount = products.Count();
            ViewBag.Categories = categories;
            return View(products);
        }

        /// <summary>
        /// Action hiển thị chi tiết một sản phẩm theo mã ID.
        /// </summary>
        /// <param name="id">Mã định danh sản phẩm.</param>
        /// <returns>Trang Display chi tiết hoặc lỗi NotFound nếu không tồn tại.</returns>
        public IActionResult Display(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            var category = _productRepository.GetAllCategories().FirstOrDefault(c => c.Id == product.CategoryId);
            ViewBag.CategoryName = category != null ? category.Name : "Không xác định";
            return View(product);
        }

        /// <summary>
        /// Action alias hiển thị chi tiết sản phẩm theo mã ID thông qua đường dẫn /Product/Detail/{id}.
        /// Sử dụng chung view Display.cshtml để tránh trùng lặp mã nguồn.
        /// </summary>
        /// <param name="id">Mã định danh sản phẩm.</param>
        /// <returns>View Display chứa chi tiết sản phẩm hoặc NotFound nếu không tồn tại.</returns>
        [HttpGet]
        public IActionResult Detail(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            var category = _productRepository.GetAllCategories().FirstOrDefault(c => c.Id == product.CategoryId);
            ViewBag.CategoryName = category != null ? category.Name : "Không xác định";
            return View("Display", product);
        }

        /// <summary>
        /// Action hiển thị trang Liên hệ (Contact) của cửa hàng theo yêu cầu tích hợp từ Bài 1.
        /// </summary>
        /// <returns>Trang Contact chứa thông tin liên hệ và biểu mẫu phản hồi.</returns>
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        /// <summary>
        /// Action hiển thị giao diện khách (Frontend Client) gọi RESTful API trực tiếp bằng Ajax.
        /// Chỉ cho phép người dùng đăng nhập với vai trò Admin sử dụng tính năng này để đảm bảo an toàn.
        /// </summary>
        /// <returns>View ApiClient.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ApiClient()
        {
            return View();
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
}
