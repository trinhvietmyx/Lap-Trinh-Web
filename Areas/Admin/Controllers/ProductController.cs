using GIUA_KY_WEB_3AE.Models;
using GIUA_KY_WEB_3AE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GIUA_KY_WEB_3AE.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản trị Sản phẩm dành riêng cho khu vực Admin (Area Admin).
    /// Thực hiện các chức năng CRUD sản phẩm, kiểm soát quyền truy cập chặt chẽ.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Hàm khởi tạo ProductController của Admin.
        /// </summary>
        public ProductController(IProductRepository productRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Trang danh sách quản lý sản phẩm của Admin.
        /// </summary>
        public IActionResult Index()
        {
            var products = _productRepository.GetAll();
            ViewBag.Categories = _productRepository.GetAllCategories().ToDictionary(c => c.Id, c => c.Name);
            return View(products);
        }

        /// <summary>
        /// Action hiển thị giao diện thêm sản phẩm mới (GET).
        /// </summary>
        [HttpGet]
        public IActionResult Add()
        {
            var categories = _productRepository.GetAllCategories();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View();
        }

        /// <summary>
        /// Action xử lý thêm sản phẩm mới kèm hình ảnh (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product, IFormFile imageFile, List<IFormFile> subImageFiles)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    product.ImageUrl = await SaveImage(imageFile);
                }
                else
                {
                    product.ImageUrl = "/images/default_product.png";
                }

                if (subImageFiles != null && subImageFiles.Any())
                {
                    product.ImageUrls = new List<string>();
                    foreach (var file in subImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var subImageUrl = await SaveImage(file);
                            product.ImageUrls.Add(subImageUrl);
                        }
                    }
                }

                _productRepository.Add(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = _productRepository.GetAllCategories();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        /// <summary>
        /// Action hiển thị giao diện cập nhật thông tin sản phẩm (GET).
        /// </summary>
        [HttpGet]
        public IActionResult Update(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = _productRepository.GetAllCategories();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        /// <summary>
        /// Action xử lý cập nhật thông tin sản phẩm (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Product product, IFormFile imageFile, List<IFormFile> subImageFiles)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _productRepository.GetById(product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    product.ImageUrl = await SaveImage(imageFile);
                }
                else
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }

                if (subImageFiles != null && subImageFiles.Any())
                {
                    var newImageUrls = new List<string>();
                    foreach (var file in subImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var subImageUrl = await SaveImage(file);
                            newImageUrls.Add(subImageUrl);
                        }
                    }
                    product.ImageUrls = newImageUrls;
                }
                else
                {
                    product.ImageUrls = existingProduct.ImageUrls;
                }

                _productRepository.Update(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = _productRepository.GetAllCategories();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        /// <summary>
        /// Action hiển thị giao diện xác nhận xóa sản phẩm (GET).
        /// </summary>
        [HttpGet]
        public IActionResult Delete(int id)
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
        /// Action thực thi xóa sản phẩm (POST).
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _productRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper lưu hình ảnh tải lên.
        /// </summary>
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/" + uniqueFileName;
        }
    }
}
