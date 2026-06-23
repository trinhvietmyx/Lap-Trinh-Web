using GIUA_KY_WEB_3AE.Models;

namespace GIUA_KY_WEB_3AE.Repositories
{
    /// <summary>
    /// Giao diện định nghĩa các thao tác lưu trữ và truy vấn dữ liệu sản phẩm (CRUD).
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Lấy toàn bộ danh sách sản phẩm.
        /// </summary>
        /// <returns>Danh sách sản phẩm.</returns>
        IEnumerable<Product> GetAll();

        /// <summary>
        /// Lấy chi tiết một sản phẩm theo mã ID.
        /// </summary>
        /// <param name="id">Mã sản phẩm.</param>
        /// <returns>Đối tượng sản phẩm tìm được hoặc null nếu không tồn tại.</returns>
        Product? GetById(int id);

        /// <summary>
        /// Thêm một sản phẩm mới vào danh sách.
        /// </summary>
        /// <param name="product">Đối tượng sản phẩm cần thêm.</param>
        void Add(Product product);

        /// <summary>
        /// Cập nhật thông tin của một sản phẩm đã có.
        /// </summary>
        /// <param name="product">Đối tượng sản phẩm chứa thông tin mới.</param>
        void Update(Product product);

        /// <summary>
        /// Xóa một sản phẩm khỏi danh sách theo mã ID.
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xóa.</param>
        void Delete(int id);

        /// <summary>
        /// Lấy toàn bộ danh sách các danh mục sản phẩm (hỗ trợ cho việc hiển thị dropdown list).
        /// </summary>
        /// <returns>Danh sách các danh mục sản phẩm.</returns>
        IEnumerable<Category> GetAllCategories();

        /// <summary>
        /// Lấy toàn bộ danh sách sản phẩm (Bất đồng bộ).
        /// </summary>
        /// <returns>Task chứa danh sách sản phẩm.</returns>
        Task<IEnumerable<Product>> GetProductsAsync();

        /// <summary>
        /// Lấy chi tiết một sản phẩm theo mã ID (Bất đồng bộ).
        /// </summary>
        /// <param name="id">Mã sản phẩm.</param>
        /// <returns>Task chứa đối tượng sản phẩm hoặc null nếu không tồn tại.</returns>
        Task<Product?> GetProductByIdAsync(int id);

        /// <summary>
        /// Thêm mới một sản phẩm (Bất đồng bộ).
        /// </summary>
        /// <param name="product">Đối tượng sản phẩm cần thêm.</param>
        /// <returns>Task hoàn thành tác vụ.</returns>
        Task AddProductAsync(Product product);

        /// <summary>
        /// Cập nhật thông tin một sản phẩm (Bất đồng bộ).
        /// </summary>
        /// <param name="product">Đối tượng sản phẩm chứa thông tin cập nhật.</param>
        /// <returns>Task hoàn thành tác vụ.</returns>
        Task UpdateProductAsync(Product product);

        /// <summary>
        /// Xóa một sản phẩm khỏi danh sách theo mã ID (Bất đồng bộ).
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xóa.</param>
        /// <returns>Task hoàn thành tác vụ.</returns>
        Task DeleteProductAsync(int id);
    }
}
