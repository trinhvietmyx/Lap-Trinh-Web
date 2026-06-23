namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// ViewModel đại diện cho một mặt hàng nằm trong Giỏ hàng Session.
    /// </summary>
    public class CartItemViewModel
    {
        /// <summary>
        /// Mã định danh sản phẩm.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Tên sản phẩm.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Đơn giá sản phẩm.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Ảnh sản phẩm.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng sản phẩm được thêm vào giỏ.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Tổng số tiền cho mặt hàng này (Đơn giá x Số lượng).
        /// </summary>
        public decimal TotalPrice => Price * Quantity;
    }
}
