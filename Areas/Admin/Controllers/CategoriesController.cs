using GIUA_KY_WEB_3AE.Models;
using GIUA_KY_WEB_3AE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GIUA_KY_WEB_3AE.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản trị danh mục (Categories) thuộc Area Admin.
    /// Cho phép Thêm, Sửa, Xóa danh mục sản phẩm của hệ thống.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        /// <summary>
        /// Hàm khởi tạo CategoriesController trong Admin area.
        /// </summary>
        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Hiển thị danh sách danh mục để Admin quản lý.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        /// <summary>
        /// Hiển thị form thêm mới danh mục (GET).
        /// </summary>
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        /// <summary>
        /// Xử lý thêm mới danh mục (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddAsync(category);
                TempData["SuccessMessage"] = "Thêm danh mục mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        /// <summary>
        /// Hiển thị form cập nhật danh mục (GET).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin danh mục (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateAsync(category);
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        /// <summary>
        /// Hiển thị giao diện xác nhận xóa danh mục (GET).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        /// <summary>
        /// Thực thi xóa danh mục (POST).
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                await _categoryRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
