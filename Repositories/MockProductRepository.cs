using GIUA_KY_WEB_3AE.Models;

namespace GIUA_KY_WEB_3AE.Repositories
{
    /// <summary>
    /// Triển khai IProductRepository sử dụng Mock Data trong bộ nhớ (In-Memory).
    /// </summary>
    public class MockProductRepository : IProductRepository
    {
        private readonly List<Product> _products;
        private readonly List<Category> _categories;

        /// <summary>
        /// Hàm khởi tạo của MockProductRepository, thiết lập các dữ liệu mẫu ban đầu.
        /// </summary>
        public MockProductRepository()
        {
            // Thiết lập danh mục món ăn mẫu
            _categories = new List<Category>
            {
                new Category { Id = 1, Name = "Mì Quảng" },
                new Category { Id = 2, Name = "Món Ăn Kèm" },
                new Category { Id = 3, Name = "Nước Uống" }
            };

            // Thiết lập món ăn mẫu với ảnh chính và ảnh phụ
            _products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Mì Quảng Tôm Thịt",
                    Price = 45000,
                    Description = "Tô mì Quảng truyền thống với tôm, thịt ba chỉ rim nghệ, nước lèo đậm vị, ăn kèm rau sống và bánh tráng mè.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Name = "Mì Quảng Gà",
                    Price = 40000,
                    Description = "Mì Quảng gà ta rim nghệ, thịt săn chắc, nước dùng thơm ngọt chuẩn vị miền Trung.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 3,
                    Name = "Mì Quảng Sườn",
                    Price = 50000,
                    Description = "Mì Quảng sườn non rim mềm, nước lèo đậm đà, thêm đậu phộng rang và rau sống tươi.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 4,
                    Name = "Mì Quảng Ếch",
                    Price = 55000,
                    Description = "Mì Quảng ếch đồng rim cay nhẹ, thịt chắc, dùng cùng nước lèo nghệ thơm nồng.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 5,
                    Name = "Mì Quảng Bò",
                    Price = 50000,
                    Description = "Mì Quảng bò mềm, nước dùng ngọt thanh, phù hợp cho bữa trưa no bụng.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 6,
                    Name = "Mì Quảng Cá Lóc",
                    Price = 55000,
                    Description = "Mì Quảng cá lóc đồng, thịt cá ngọt, ít xương, nước dùng nhẹ và thơm.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 7,
                    Name = "Mì Quảng Trứng",
                    Price = 35000,
                    Description = "Phần mì Quảng trứng giản dị, dễ ăn, phù hợp bữa sáng nhanh gọn.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 8,
                    Name = "Bánh Tráng Nướng",
                    Price = 15000,
                    Description = "Bánh tráng mè đen nướng giòn rụm, món ăn kèm đúng điệu với mì Quảng.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 2
                },
                new Product
                {
                    Id = 9,
                    Name = "Ram Cuốn",
                    Price = 20000,
                    Description = "Ram cuốn chiên giòn, nhân thịt thơm, ăn kèm rau sống và nước chấm.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 2
                },
                new Product
                {
                    Id = 10,
                    Name = "Chả Cá Chiên",
                    Price = 25000,
                    Description = "Chả cá chiên vàng, dai thơm, dùng thêm cùng tô mì hoặc ăn riêng.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 2
                },
                new Product
                {
                    Id = 11,
                    Name = "Gỏi Cuốn",
                    Price = 20000,
                    Description = "Gỏi cuốn tươi mát, rau xanh và nhân đầy đặn, phù hợp ăn nhẹ.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 2
                },
                new Product
                {
                    Id = 12,
                    Name = "Trà Đá",
                    Price = 5000,
                    Description = "Ly trà đá mát lạnh, dùng kèm các món mì Quảng.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 3
                },
                new Product
                {
                    Id = 13,
                    Name = "Nước Mía",
                    Price = 15000,
                    Description = "Nước mía ép tươi, ngọt thanh, giải khát sau bữa ăn.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 3
                },
                new Product
                {
                    Id = 14,
                    Name = "Sinh Tố Bơ",
                    Price = 25000,
                    Description = "Sinh tố bơ béo mịn, thơm dịu, dùng lạnh.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 3
                },
                new Product
                {
                    Id = 15,
                    Name = "Cà Phê Sữa Đá",
                    Price = 18000,
                    Description = "Cà phê sữa đá đậm vị, pha kiểu Việt Nam.",
                    ImageUrl = "/images/default_product.png",
                    ImageUrls = new List<string>(),
                    CategoryId = 3
                }
            };
        }

        /// <summary>
        /// Lấy tất cả danh sách sản phẩm mẫu.
        /// </summary>
        /// <returns>Danh sách sản phẩm.</returns>
        public IEnumerable<Product> GetAll()
        {
            return _products;
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo ID.
        /// </summary>
        /// <param name="id">Mã sản phẩm.</param>
        /// <returns>Đối tượng sản phẩm hoặc null nếu không thấy.</returns>
        public Product? GetById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Thêm một sản phẩm mới, tự động tăng mã ID.
        /// </summary>
        /// <param name="product">Sản phẩm cần thêm.</param>
        public void Add(Product product)
        {
            product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
        }

        /// <summary>
        /// Cập nhật thông tin của sản phẩm hiện tại.
        /// </summary>
        /// <param name="product">Sản phẩm có dữ liệu mới.</param>
        public void Update(Product product)
        {
            var existingProduct = GetById(product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                
                // Chỉ cập nhật ảnh nếu có đường dẫn ảnh mới được truyền vào
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    existingProduct.ImageUrl = product.ImageUrl;
                }
                if (product.ImageUrls != null && product.ImageUrls.Any())
                {
                    existingProduct.ImageUrls = product.ImageUrls;
                }
            }
        }

        /// <summary>
        /// Xóa sản phẩm khỏi danh sách theo ID.
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xóa.</param>
        public void Delete(int id)
        {
            var product = GetById(id);
            if (product != null)
            {
                _products.Remove(product);
            }
        }

        /// <summary>
        /// Lấy danh sách toàn bộ các danh mục sản phẩm mẫu.
        /// </summary>
        /// <returns>Danh sách danh mục.</returns>
        public IEnumerable<Category> GetAllCategories()
        {
            return _categories;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách sản phẩm mẫu (Bất đồng bộ).
        /// </summary>
        public Task<IEnumerable<Product>> GetProductsAsync()
        {
            return Task.FromResult<IEnumerable<Product>>(_products);
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo ID (Bất đồng bộ).
        /// </summary>
        public Task<Product?> GetProductByIdAsync(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(product);
        }

        /// <summary>
        /// Thêm một sản phẩm mới (Bất đồng bộ).
        /// </summary>
        public Task AddProductAsync(Product product)
        {
            Add(product);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Cập nhật thông tin của sản phẩm hiện tại (Bất đồng bộ).
        /// </summary>
        public Task UpdateProductAsync(Product product)
        {
            Update(product);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Xóa sản phẩm khỏi danh sách theo ID (Bất đồng bộ).
        /// </summary>
        public Task DeleteProductAsync(int id)
        {
            Delete(id);
            return Task.CompletedTask;
        }
    }
}
