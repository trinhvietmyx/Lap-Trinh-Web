using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIUA_KY_WEB_3AE.Repositories
{
    /// <summary>
    /// Lớp thực thi ICategoryRepository sử dụng Entity Framework Core và SQL Server.
    /// </summary>
    public class EFCategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Khởi tạo repository với ApplicationDbContext được tiêm qua DI.
        /// </summary>
        /// <param name="context">DbContext kết nối cơ sở dữ liệu.</param>
        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ danh mục từ database dưới dạng bất đồng bộ.
        /// </summary>
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Lấy thông tin một danh mục dựa theo Id bất đồng bộ.
        /// </summary>
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Thêm mới một danh mục vào cơ sở dữ liệu và lưu thay đổi bất đồng bộ.
        /// </summary>
        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật thông tin danh mục trong cơ sở dữ liệu và lưu thay đổi bất đồng bộ.
        /// </summary>
        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa danh mục khỏi cơ sở dữ liệu dựa trên mã Id bất đồng bộ.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}
