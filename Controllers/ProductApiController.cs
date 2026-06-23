using GIUA_KY_WEB_3AE.Models;
using GIUA_KY_WEB_3AE.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GIUA_KY_WEB_3AE.Controllers
{
    /// <summary>
    /// API Controller cung cấp các dịch vụ RESTful API cho đối tượng Sản phẩm (Product).
    /// Hỗ trợ cả hai endpoint định tuyến: /api/products và /api/ProductApi để tương thích tối đa.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/products")]
    [Route("api/ProductApi")]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// Hàm khởi tạo ProductApiController, tiêm Product Repository.
        /// </summary>
        /// <param name="productRepository">Repository chứa các phương thức truy vấn Product.</param>
        public ProductApiController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách sản phẩm từ cơ sở dữ liệu.
        /// Endpoint: GET /api/products hoặc GET /api/ProductApi
        /// </summary>
        /// <returns>HTTP OK kèm theo danh sách sản phẩm hoặc HTTP 500 nếu xảy ra lỗi.</returns>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _productRepository.GetProductsAsync();
                return Ok(products);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy chi tiết một sản phẩm theo mã ID.
        /// Endpoint: GET /api/products/{id} hoặc GET /api/ProductApi/{id}
        /// </summary>
        /// <param name="id">Mã sản phẩm cần tìm.</param>
        /// <returns>HTTP OK kèm sản phẩm, HTTP 404 nếu không thấy, hoặc HTTP 500 nếu lỗi.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Thêm mới một sản phẩm vào hệ thống.
        /// Endpoint: POST /api/products hoặc POST /api/ProductApi
        /// </summary>
        /// <param name="product">Đối tượng sản phẩm cần thêm (lấy từ Body).</param>
        /// <returns>HTTP 201 CreatedAtAction chứa đường dẫn lấy sản phẩm vừa tạo hoặc HTTP 500 nếu lỗi.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest("Product data is null");
                }
                
                // Đảm bảo CategoryId hợp lệ, nếu không có categoryId thì gán mặc định là 1 (Mì Quảng)
                if (product.CategoryId <= 0)
                {
                    product.CategoryId = 1;
                }

                await _productRepository.AddProductAsync(product);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cập nhật thông tin của một sản phẩm đã có dựa vào mã ID.
        /// Endpoint: PUT /api/products/{id} hoặc PUT /api/ProductApi/{id}
        /// </summary>
        /// <param name="id">Mã sản phẩm cần cập nhật.</param>
        /// <param name="product">Thông tin sản phẩm mới cập nhật (lấy từ Body).</param>
        /// <returns>HTTP 204 NoContent, HTTP 400 nếu ID không khớp, hoặc HTTP 500 nếu lỗi.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (product == null || id != product.Id)
                {
                    return BadRequest("Product mismatch or null");
                }

                var existing = await _productRepository.GetProductByIdAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                await _productRepository.UpdateProductAsync(product);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Xóa bỏ một sản phẩm ra khỏi hệ thống dựa vào mã ID.
        /// Endpoint: DELETE /api/products/{id} hoặc DELETE /api/ProductApi/{id}
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xóa.</param>
        /// <returns>HTTP 204 NoContent, HTTP 404 nếu không thấy sản phẩm, hoặc HTTP 500 nếu lỗi.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var existing = await _productRepository.GetProductByIdAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                await _productRepository.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
