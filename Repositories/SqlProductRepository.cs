using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;
using Microsoft.EntityFrameworkCore;

namespace GIUA_KY_WEB_3AE.Repositories
{
    /// <summary>
    /// Triển khai IProductRepository sử dụng cơ sở dữ liệu Microsoft SQL Server thông qua Entity Framework Core.
    /// </summary>
    public class SqlProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Hàm khởi tạo của SqlProductRepository, sử dụng Dependency Injection để tiêm ApplicationDbContext.
        /// </summary>
        /// <param name="context">Context kết nối SQL Server.</param>
        public SqlProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách sản phẩm từ SQL Server.
        /// </summary>
        /// <returns>Danh sách sản phẩm.</returns>
        public IEnumerable<Product> GetAll()
        {
            return _context.Products.Include(p => p.Images).AsNoTracking().ToList();
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo ID từ SQL Server.
        /// </summary>
        /// <param name="id">Mã sản phẩm.</param>
        /// <returns>Đối tượng sản phẩm hoặc null nếu không thấy.</returns>
        public Product? GetById(int id)
        {
            return _context.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Thêm mới một sản phẩm vào SQL Server và lưu thay đổi.
        /// </summary>
        /// <param name="product">Sản phẩm cần thêm mới.</param>
        public void Add(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        /// <summary>
        /// Cập nhật thông tin sản phẩm trong SQL Server.
        /// </summary>
        /// <param name="product">Sản phẩm chứa thông tin cập nhật.</param>
        public void Update(Product product)
        {
            // Tải sản phẩm kèm theo danh sách ảnh phụ cũ từ database
            var existingProduct = _context.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                // Chỉ cập nhật ảnh chính nếu người dùng upload ảnh mới
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    existingProduct.ImageUrl = product.ImageUrl;
                }
                
                // Xử lý cập nhật ảnh phụ trong bảng ProductImages
                if (product.Images != null)
                {
                    // Xóa bỏ tất cả ảnh phụ cũ của sản phẩm này ra khỏi cơ sở dữ liệu
                    if (existingProduct.Images != null)
                    {
                        _context.ProductImages.RemoveRange(existingProduct.Images);
                    }

                    // Gán danh sách ảnh phụ mới
                    existingProduct.Images = product.Images;
                }

                _context.Products.Update(existingProduct);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Xóa sản phẩm khỏi SQL Server dựa trên mã ID.
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xóa.</param>
        public void Delete(int id)
        {
            var product = GetById(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Lấy toàn bộ danh mục sản phẩm từ SQL Server để phục vụ binding.
        /// </summary>
        /// <returns>Danh sách danh mục.</returns>
        public IEnumerable<Category> GetAllCategories()
        {
            return _context.Categories.AsNoTracking().ToList();
        }

        /// <summary>
        /// Lấy toàn bộ danh sách sản phẩm bất đồng bộ từ SQL Server.
        /// </summary>
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Products.Include(p => p.Images).AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo ID bất đồng bộ từ SQL Server.
        /// </summary>
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Thêm mới sản phẩm bất đồng bộ vào SQL Server.
        /// </summary>
        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật sản phẩm bất đồng bộ trong SQL Server.
        /// </summary>
        public async Task UpdateProductAsync(Product product)
        {
            var existingProduct = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    existingProduct.ImageUrl = product.ImageUrl;
                }

                if (product.Images != null)
                {
                    if (existingProduct.Images != null)
                    {
                        _context.ProductImages.RemoveRange(existingProduct.Images);
                    }
                    existingProduct.Images = product.Images;
                }

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Xóa sản phẩm bất đồng bộ khỏi SQL Server.
        /// </summary>
        public async Task DeleteProductAsync(int id)
        {
            var product = await GetProductByIdAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
