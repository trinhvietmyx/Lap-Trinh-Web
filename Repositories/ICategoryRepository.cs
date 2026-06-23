using GIUA_KY_WEB_3AE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIUA_KY_WEB_3AE.Repositories
{
    /// <summary>
    /// Giao diện định nghĩa các thao tác CRUD đối với Danh mục sản phẩm (Category).
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Lấy toàn bộ danh sách danh mục bất đồng bộ.
        /// </summary>
        /// <returns>Tập hợp các danh mục.</returns>
        Task<IEnumerable<Category>> GetAllAsync();

        /// <summary>
        /// Lấy thông tin danh mục theo ID bất đồng bộ.
        /// </summary>
        /// <param name="id">Mã danh mục cần tìm.</param>
        /// <returns>Danh mục hoặc null nếu không tồn tại.</returns>
        Task<Category?> GetByIdAsync(int id);

        /// <summary>
        /// Thêm mới một danh mục bất đồng bộ.
        /// </summary>
        /// <param name="category">Thực thể danh mục cần thêm.</param>
        Task AddAsync(Category category);

        /// <summary>
        /// Cập nhật thông tin danh mục bất đồng bộ.
        /// </summary>
        /// <param name="category">Thực thể danh mục cần cập nhật.</param>
        Task UpdateAsync(Category category);

        /// <summary>
        /// Xóa danh mục dựa trên ID bất đồng bộ.
        /// </summary>
        /// <param name="id">Mã danh mục cần xóa.</param>
        Task DeleteAsync(int id);
    }
}
