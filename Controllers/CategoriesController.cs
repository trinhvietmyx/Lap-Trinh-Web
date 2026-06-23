using GIUA_KY_WEB_3AE.Models;
using GIUA_KY_WEB_3AE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GIUA_KY_WEB_3AE.Controllers
{
    /// <summary>
    /// Controller điều hướng và quản lý các thao tác CRUD đối với danh mục sản phẩm (Category).
    /// Hầu hết các thao tác thay đổi dữ liệu yêu cầu quyền quản trị viên (Admin).
    /// </summary>
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        /// <summary>
        /// Khởi tạo CategoriesController, sử dụng Dependency Injection để tiêm ICategoryRepository.
        /// </summary>
        /// <param name="categoryRepository">Repository quản lý truy xuất Category.</param>
        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Action hiển thị danh sách toàn bộ danh mục sản phẩm.
        /// </summary>
        /// <returns>View Index hiển thị danh sách các danh mục.</returns>
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        /// <summary>
        /// Action hiển thị chi tiết thông tin của một danh mục sản phẩm.
        /// </summary>
        /// <param name="id">Mã định danh danh mục.</param>
        /// <returns>View Display chi tiết danh mục hoặc NotFound nếu không tồn tại.</returns>
        public async Task<IActionResult> Display(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
    }
}
