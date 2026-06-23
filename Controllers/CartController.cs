using System.Net;
using System.Globalization;
using System.Text.Json;
using System.Text;
using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;
using GIUA_KY_WEB_3AE.Repositories;
using GIUA_KY_WEB_3AE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GIUA_KY_WEB_3AE.Controllers
{
    /// <summary>
    /// Controller quản lý các chức năng của Giỏ hàng (Session-based) và tiến hành đặt hàng (Checkout).
    /// </summary>
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private const string CART_SESSION_KEY = "Mi3AECart";
        private const string LEGACY_CART_SESSION_KEY = "TechStoreCart";

        /// <summary>
        /// Hàm khởi tạo CartController.
        /// </summary>
        public CartController(
            IProductRepository productRepository,
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _productRepository = productRepository;
            _dbContext = dbContext;
            _userManager = userManager;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Action thêm sản phẩm vào giỏ hàng rồi chuyển thẳng đến trang thanh toán.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuyNow(int id, int quantity = 1)
        {
            var (product, _) = AddProductToCart(id, quantity);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại hoặc đã bị xóa.";
                return RedirectToAction("Index", "Product");
            }

            return RedirectToAction(nameof(Checkout));
        }

        /// <summary>
        /// Xác định khóa lưu trữ giỏ hàng trong Session dựa theo trạng thái đăng nhập.
        /// - Chưa đăng nhập (Guest): "Mi3AECart_Guest"
        /// - Đã đăng nhập: "Mi3AECart_User_{UserId}"
        /// </summary>
        private string GetCartSessionKey()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    return $"{CART_SESSION_KEY}_User_{userId}";
                }
            }
            return $"{CART_SESSION_KEY}_Guest";
        }

        /// <summary>
        /// Phương thức helper lấy danh sách sản phẩm trong giỏ hàng từ Session.
        /// Nếu người dùng đã đăng nhập, tự động gộp (merge) giỏ hàng vãng lai (Guest) vào giỏ hàng tài khoản nếu có.
        /// </summary>
        private List<CartItemViewModel> GetCartFromSession()
        {
            var currentKey = GetCartSessionKey();

            // Dọn dẹp dữ liệu giỏ hàng cũ còn sót lại từ phiên bản trước (key "TechStoreCart" không có hậu tố)
            var legacyData = HttpContext.Session.GetString(LEGACY_CART_SESSION_KEY);
            if (!string.IsNullOrEmpty(legacyData))
            {
                try
                {
                    var legacyCart = JsonSerializer.Deserialize<List<CartItemViewModel>>(legacyData);
                    if (legacyCart != null && legacyCart.Any())
                    {
                        // Gộp dữ liệu cũ vào giỏ hàng hiện tại
                        var existingData = HttpContext.Session.GetString(currentKey);
                        var existingCart = new List<CartItemViewModel>();
                        if (!string.IsNullOrEmpty(existingData))
                        {
                            existingCart = JsonSerializer.Deserialize<List<CartItemViewModel>>(existingData) ?? new List<CartItemViewModel>();
                        }
                        foreach (var legacyItem in legacyCart)
                        {
                            var match = existingCart.FirstOrDefault(i => i.ProductId == legacyItem.ProductId);
                            if (match != null)
                            {
                                match.Quantity += legacyItem.Quantity;
                            }
                            else
                            {
                                existingCart.Add(legacyItem);
                            }
                        }
                        HttpContext.Session.SetString(currentKey, JsonSerializer.Serialize(existingCart));
                    }
                }
                catch {}
                // Xóa key cũ bất kể có parse thành công hay không
                HttpContext.Session.Remove(LEGACY_CART_SESSION_KEY);
            }

            // Nếu đã đăng nhập, kiểm tra và gộp giỏ hàng Guest vào giỏ hàng User
            if (User.Identity?.IsAuthenticated == true)
            {
                var guestKey = $"{CART_SESSION_KEY}_Guest";
                var guestSessionData = HttpContext.Session.GetString(guestKey);

                if (!string.IsNullOrEmpty(guestSessionData))
                 {
                    List<CartItemViewModel>? guestCart = null;
                    try
                    {
                        guestCart = JsonSerializer.Deserialize<List<CartItemViewModel>>(guestSessionData);
                    }
                    catch {}

                    if (guestCart != null && guestCart.Any())
                    {
                        // Lấy giỏ hàng hiện tại của User
                        var userSessionData = HttpContext.Session.GetString(currentKey);
                        var userCart = new List<CartItemViewModel>();
                        if (!string.IsNullOrEmpty(userSessionData))
                        {
                            try
                            {
                                userCart = JsonSerializer.Deserialize<List<CartItemViewModel>>(userSessionData) ?? new List<CartItemViewModel>();
                            }
                            catch {}
                        }

                        // Tiến hành gộp giỏ hàng (cộng dồn số lượng nếu trùng sản phẩm)
                        foreach (var guestItem in guestCart)
                        {
                            var existingItem = userCart.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
                            if (existingItem != null)
                            {
                                existingItem.Quantity += guestItem.Quantity;
                            }
                            else
                            {
                                userCart.Add(guestItem);
                            }
                        }

                        // Lưu lại giỏ hàng của User sau khi gộp
                        var mergedSessionData = JsonSerializer.Serialize(userCart);
                        HttpContext.Session.SetString(currentKey, mergedSessionData);

                        // Xóa sạch giỏ hàng Guest tạm thời
                        HttpContext.Session.Remove(guestKey);

                        return userCart;
                    }
                }
            }

            var sessionData = HttpContext.Session.GetString(currentKey);
            if (string.IsNullOrEmpty(sessionData))
            {
                return new List<CartItemViewModel>();
            }
            try
            {
                return JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionData) ?? new List<CartItemViewModel>();
            }
            catch
            {
                return new List<CartItemViewModel>();
            }
        }

        /// <summary>
        /// Phương thức helper lưu danh sách sản phẩm trong giỏ hàng vào Session.
        /// </summary>
        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            var currentKey = GetCartSessionKey();
            var sessionData = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(currentKey, sessionData);
        }

        private (Product? Product, int CartCount) AddProductToCart(int id, int quantity)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return (null, 0);
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            var cart = GetCartFromSession();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl ?? string.Empty,
                    Quantity = quantity
                });
            }

            SaveCartToSession(cart);
            return (product, cart.Sum(i => i.Quantity));
        }

        /// <summary>
        /// Action hiển thị trang xem giỏ hàng (GET).
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        /// <summary>
        /// Action thêm sản phẩm vào giỏ hàng qua form POST (an toàn).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Add")]
        public IActionResult AddPost(int id, int quantity = 1)
        {
            return AddGet(id, quantity);
        }

        /// <summary>
        /// Action thêm sản phẩm vào giỏ hàng qua link GET.
        /// </summary>
        [HttpGet]
        [ActionName("Add")]
        public IActionResult AddGet(int id, int quantity = 1)
        {
            var (product, cartCount) = AddProductToCart(id, quantity);
            if (product == null)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc đã bị xóa." });
                }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return Json(new { 
                    success = true, 
                    message = $"Đã thêm sản phẩm \"{product.Name}\" vào giỏ hàng thành công!",
                    cartCount = cartCount
                });
            }

            TempData["SuccessMessage"] = $"Đã thêm sản phẩm \"{product.Name}\" vào giỏ hàng thành công!";

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index", "Product");
        }

        /// <summary>
        /// Action cập nhật số lượng sản phẩm trong giỏ hàng (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCartFromSession();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }
                SaveCartToSession(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Action xóa một sản phẩm ra khỏi giỏ hàng (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            var cart = GetCartFromSession();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCartToSession(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Action làm trống toàn bộ giỏ hàng (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            var currentKey = GetCartSessionKey();
            HttpContext.Session.Remove(currentKey);
            return RedirectToAction(nameof(Index));
        }
        /// <summary>
        /// Action hiển thị màn hình thanh toán đơn hàng (GET).
        /// Hỗ trợ lọc danh sách sản phẩm được chọn để đặt hàng.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Checkout(List<int> selectedIds)
        {
            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sản phẩm để tiếp tục thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu không chọn cụ thể sản phẩm nào, mặc định thanh toán toàn bộ giỏ hàng
            if (selectedIds == null || !selectedIds.Any())
            {
                selectedIds = cart.Select(item => item.ProductId).ToList();
            }

            // Lọc ra các sản phẩm được tích chọn từ giỏ hàng hiện tại
            var selectedCartItems = cart.Where(item => selectedIds.Contains(item.ProductId)).ToList();

            if (!selectedCartItems.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để tiến hành thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy thông tin tài khoản hiện tại để điền sẵn vào trang thanh toán
            var user = await _userManager.GetUserAsync(User);

            var model = new CheckoutViewModel
            {
                CartItems = selectedCartItems,
                TotalAmount = selectedCartItems.Sum(item => item.TotalPrice),
                SelectedIds = selectedIds,
                CustomerName = user?.FullName ?? string.Empty,
                Phone = user?.PhoneNumber ?? string.Empty,
                Address = user?.Address ?? string.Empty
            };

            return View(model);
        }

        /// <summary>
        /// Action xử lý thanh toán và lưu đơn hàng vào database (POST).
        /// Chỉ đặt hàng các sản phẩm được chọn và giữ lại các sản phẩm khác trong giỏ hàng.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống.");
                return View(model);
            }

            // Lấy danh sách ID các sản phẩm được chọn
            var selectedIds = model.SelectedIds;
            if (selectedIds == null || !selectedIds.Any())
            {
                selectedIds = cart.Select(item => item.ProductId).ToList();
            }

            // Lọc sản phẩm được chọn thanh toán
            var selectedCartItems = cart.Where(item => selectedIds.Contains(item.ProductId)).ToList();
            if (!selectedCartItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất một sản phẩm để thanh toán.");
                model.CartItems = new List<CartItemViewModel>();
                model.TotalAmount = 0;
                return View(model);
            }

            var validatedItems = new List<OrderItem>();
            decimal computedTotal = 0;

            foreach (var cartItem in selectedCartItems)
            {
                var dbProduct = _productRepository.GetById(cartItem.ProductId);
                if (dbProduct == null)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm '{cartItem.Name}' không còn tồn tại hoặc đã bị xóa.";
                    // Xóa sản phẩm lỗi khỏi giỏ hàng chính
                    cart.RemoveAll(item => item.ProductId == cartItem.ProductId);
                    SaveCartToSession(cart);
                    return RedirectToAction(nameof(Index));
                }

                var orderItem = new OrderItem
                {
                    ProductId = dbProduct.Id,
                    ProductName = dbProduct.Name,
                    Price = dbProduct.Price,
                    Quantity = cartItem.Quantity,
                    ImageUrl = dbProduct.ImageUrl
                };

                validatedItems.Add(orderItem);
                computedTotal += orderItem.Price * orderItem.Quantity;
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }

                // Sử dụng phí ship đã được tính toán từ phía Client (qua GPS hoặc tỉnh thành)
                var shippingFee = model.ShippingFee;
                computedTotal += shippingFee;

                // Áp dụng giảm giá điểm thành viên Mì 3AE (22.190đ) nếu người dùng bật toggle sử dụng điểm
                if (model.UseMemberPoints)
                {
                    const decimal memberPointsDiscount = 22190m;
                    computedTotal -= memberPointsDiscount;
                    if (computedTotal < 0) computedTotal = 0; // Đảm bảo tổng tiền không âm
                }

                // Xác định trạng thái ban đầu: nếu thanh toán VietQR → "PendingPayment" (chờ Admin xác nhận đã nhận tiền)
                var initialStatus = "Pending";
                if (!string.IsNullOrEmpty(model.Notes) && model.Notes.Contains("[Trạng thái thanh toán: Đã chuyển khoản qua VietQR - Chờ xác nhận]"))
                {
                    initialStatus = "PendingPayment";
                }

                var order = new Order
                {
                    UserId = user.Id,
                    CustomerName = model.CustomerName,
                    Phone = model.Phone,
                    Address = model.Address,
                    Notes = model.Notes,
                    TotalAmount = computedTotal,
                    OrderDate = DateTime.Now,
                    Status = initialStatus,
                    OrderItems = validatedItems
                };

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();
                await _notificationService.CreateOrderStatusNotificationAsync(order, null, initialStatus, isInitial: true);

                // Gửi email xác nhận đơn hàng bất đồng bộ (sử dụng try-catch để tránh chặn luồng chính nếu SMTP lỗi)
                if (!string.IsNullOrEmpty(user.Email))
                {
                    try
                    {
                        System.IO.File.AppendAllText("email_log.txt", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Bắt đầu gửi email cho đơn hàng #{order.Id} tới email: {user.Email}\n");
                        await SendOrderConfirmationEmail(order, user.Email);
                        System.IO.File.AppendAllText("email_log.txt", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Gửi email hoàn tất cho đơn hàng #{order.Id}\n");
                    }
                    catch (Exception ex)
                    {
                        var logMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LỖI GỬI EMAIL đơn hàng #{order.Id}. Chi tiết: {ex.Message}\n";
                        if (ex.InnerException != null)
                        {
                            logMsg += $"Inner Exception: {ex.InnerException.Message}\n";
                        }
                        System.IO.File.AppendAllText("email_log.txt", logMsg);
                        Console.WriteLine($"LỖI GỬI EMAIL: Không thể gửi email xác nhận cho đơn hàng #{order.Id}. Chi tiết: {ex.Message}");
                    }
                }
                else
                {
                    System.IO.File.AppendAllText("email_log.txt", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] BỎ QUA GỬI EMAIL đơn hàng #{order.Id}: Email của user đang trống hoặc null.\n");
                }

                // Chỉ xóa những sản phẩm đã thanh toán ra khỏi giỏ hàng, giữ lại những sản phẩm khác
                var remainingCart = cart.Where(item => !selectedIds.Contains(item.ProductId)).ToList();
                SaveCartToSession(remainingCart);

                return RedirectToAction(nameof(CheckoutSuccess), new { id = order.Id });
            }

            model.CartItems = selectedCartItems;
            model.TotalAmount = selectedCartItems.Sum(item => item.TotalPrice);
            return View(model);
        }

        /// <summary>
        /// Tạo và gửi email xác nhận đặt hàng thành công tới khách hàng (HTML format).
        /// </summary>
        /// <param name="order">Thông tin đơn hàng vừa được tạo thành công.</param>
        /// <param name="userEmail">Địa chỉ email nhận của người dùng đăng nhập.</param>
        /// <returns>Tác vụ bất đồng bộ đại diện cho việc gửi email.</returns>
        private async Task SendOrderConfirmationEmail(Order order, string userEmail)
        {
            var subject = $"[Mì 3AE] Xác nhận đặt món thành công #{order.Id}";
            
            // Xây dựng bảng chi tiết món ăn dưới dạng HTML (encode mọi trường user-input để ngăn chặn HTML injection)
            var itemsTableHtml = "";
            foreach (var item in order.OrderItems)
            {
                // Encode tên sản phẩm và validate URL ảnh để tránh XSS injection qua email
                var safeProductName = WebUtility.HtmlEncode(item.ProductName);
                var safeImageUrl = SanitizeImageUrl(item.ImageUrl);

                itemsTableHtml += $@"
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #eeeeee;'>
                            <img src='{safeImageUrl}' alt='{safeProductName}' style='width: 50px; height: 50px; object-fit: cover; border-radius: 6px; margin-right: 10px; vertical-align: middle;' />
                            <span style='font-weight: bold; color: #333333;'>{safeProductName}</span>
                        </td>
                        <td style='padding: 10px; border-bottom: 1px solid #eeeeee; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #eeeeee; text-align: right; font-weight: bold; color: #333333;'>{item.Price.ToString("#,##0")}đ</td>
                        <td style='padding: 10px; border-bottom: 1px solid #eeeeee; text-align: right; font-weight: bold; color: #D2691E;'>{(item.Price * item.Quantity).ToString("#,##0")}đ</td>
                    </tr>";
            }

            // Giao diện email HTML Premium
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; background-color: #f5f5f7; padding: 30px; margin: 0;'>
                    <div style='max-width: 600px; background-color: #ffffff; margin: 0 auto; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);'>
                        <!-- Header -->
                        <div style='background-color: #D2691E; padding: 25px; text-align: center; color: #ffffff;'>
                            <h2 style='margin: 0; font-size: 22px; font-weight: bold;'>Mì 3AE</h2>
                            <p style='margin: 5px 0 0 0; opacity: 0.9; font-size: 14px;'>Cảm ơn quý khách đã đặt món tại hệ thống Mì Quảng 3AE</p>
                        </div>
                        
                        <!-- Body -->
                        <div style='padding: 30px; color: #444444; line-height: 1.6;'>
                            <h3 style='color: #333333; margin-top: 0;'>Xác Nhận Đặt Món Thành Công!</h3>
                            <p>Xin chào <strong>{WebUtility.HtmlEncode(order.CustomerName)}</strong>,</p>
                            <p>Chúng tôi xin xác nhận đơn hàng đặt món <strong>#{order.Id}</strong> của quý khách đã được tạo thành công và nhà bếp đang bắt đầu chuẩn bị thực phẩm.</p>
                            
                            <!-- Thông tin giao hàng -->
                            <div style='background-color: #f8f9fa; border-radius: 8px; padding: 15px; margin: 20px 0; border-left: 4px solid #D2691E;'>
                                <h4 style='margin-top: 0; color: #333333; border-bottom: 1px solid #e9ecef; padding-bottom: 5px;'>Thông tin nhận hàng</h4>
                                <table style='width: 100%; font-size: 14px;'>
                                    <tr>
                                        <td style='width: 35%; color: #777777;'>Người nhận:</td>
                                        <td style='font-weight: bold; color: #333333;'>{WebUtility.HtmlEncode(order.CustomerName)}</td>
                                    </tr>
                                    <tr>
                                        <td style='color: #777777;'>Số điện thoại:</td>
                                        <td style='font-weight: bold; color: #333333;'>{WebUtility.HtmlEncode(order.Phone)}</td>
                                    </tr>
                                    <tr>
                                        <td style='color: #777777;'>Địa chỉ nhận:</td>
                                        <td style='font-weight: bold; color: #333333;'>{WebUtility.HtmlEncode(order.Address)}</td>
                                    </tr>
                                    <tr>
                                        <td style='color: #777777;'>Ghi chú món ăn:</td>
                                        <td style='color: #333333;'>{(string.IsNullOrEmpty(order.Notes) ? "Không có" : WebUtility.HtmlEncode(order.Notes))}</td>
                                    </tr>
                                </table>
                            </div>

                            <!-- Chi tiết đơn hàng -->
                            <h4 style='color: #333333; border-bottom: 1px solid #eeeeee; padding-bottom: 10px; margin-top: 25px;'>Chi tiết món ăn đã đặt</h4>
                            <table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
                                <thead>
                                    <tr style='background-color: #f8f9fa;'>
                                        <th style='padding: 10px; text-align: left; color: #555555;'>Món ăn</th>
                                        <th style='padding: 10px; text-align: center; color: #555555;'>SL</th>
                                        <th style='padding: 10px; text-align: right; color: #555555;'>Đơn giá</th>
                                        <th style='padding: 10px; text-align: right; color: #555555;'>Thành tiền</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {itemsTableHtml}
                                </tbody>
                            </table>

                            <!-- Tổng cộng -->
                            <div style='text-align: right; margin-top: 20px; font-size: 16px;'>
                                <span style='color: #777777;'>Tổng cộng thanh toán:</span>
                                <strong style='font-size: 20px; color: #D2691E; margin-left: 10px;'>{order.TotalAmount.ToString("#,##0")}đ</strong>
                            </div>
                        </div>

                        <!-- Footer -->
                        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; color: #888888; font-size: 12px; border-top: 1px solid #eeeeee;'>
                            <p style='margin: 0;'>Đây là email tự động từ Hệ Thống Mì Quảng Gia Truyền Mì 3AE.</p>
                            <p style='margin: 5px 0 0 0;'>Mọi thắc mắc vui lòng liên hệ hotline +84 909 3AE 3AE hoặc phản hồi qua email này.</p>
                        </div>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(userEmail, subject, htmlBody);
            System.IO.File.AppendAllText("email_log.txt", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Dịch vụ Email đã xử lý thành công gửi tới {userEmail}\n");
            Console.WriteLine($"ĐÃ GỬI EMAIL: Email xác nhận đơn hàng #{order.Id} đã được gửi thành công đến {userEmail}.");
        }

        /// <summary>
        /// Action hiển thị màn hình báo đặt hàng thành công (GET).
        /// Yêu cầu đăng nhập và chỉ cho phép xem đơn hàng thuộc về chính người dùng hoặc Admin.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckoutSuccess(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu: chỉ chủ đơn hàng hoặc Admin mới được xem
            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole(SD.Role_Admin);
            if (order.UserId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            return View(order);
        }

        /// <summary>
        /// Tính toán phí vận chuyển và ngày giao hàng dự kiến dựa trên địa chỉ nhận hàng và tổng giá trị đơn hàng tạm tính.
        /// Áp dụng chính sách miễn phí vận chuyển cho đơn hàng có giá trị từ 300,000đ trở lên.
        /// </summary>
        /// <param name="address">Địa chỉ nhận hàng của khách.</param>
        /// <param name="subtotal">Tổng giá trị sản phẩm tạm tính.</param>
        /// <returns>Bộ giá trị gồm: Phí vận chuyển (Fee) và số ngày giao dự kiến (EstimatedDays).</returns>
        private (decimal Fee, int EstimatedDays) CalculateShippingFee(string address, decimal subtotal)
        {
            // Miễn phí vận chuyển cho các đơn hàng trị giá từ 300.000đ trở lên
            if (subtotal >= 300000)
            {
                return (0, 0);
            }

            if (string.IsNullOrEmpty(address))
            {
                return (35000, 3); // Phí mặc định
            }

            string normalizedAddress = NormalizeVietnameseText(address);

            if (normalizedAddress.Contains("ho chi minh") || normalizedAddress.Contains("hcm") || normalizedAddress.Contains("sai gon"))
            {
                return (20000, 1);
            }
            if (normalizedAddress.Contains("ha noi") || normalizedAddress.Contains("da nang"))
            {
                return (30000, 2);
            }
            if (normalizedAddress.Contains("hai phong") || normalizedAddress.Contains("can tho") || 
                normalizedAddress.Contains("binh duong") || normalizedAddress.Contains("dong nai") || 
                normalizedAddress.Contains("long an"))
            {
                return (35000, 2);
            }

            return (45000, 4); // Các tỉnh thành khác
        }

        /// <summary>
        /// Phương thức helper kiểm tra xem request hiện tại có phải là AJAX hay không.
        /// </summary>
        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private static string NormalizeVietnameseText(string value)
        {
            var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Replace('đ', 'd').Replace('Đ', 'd');
        }

        /// <summary>
        /// Kiểm tra và làm sạch URL ảnh sản phẩm trước khi chèn vào email HTML.
        /// Chỉ cho phép các URL có scheme http/https hoặc đường dẫn tương đối (bắt đầu bằng /).
        /// </summary>
        /// <param name="imageUrl">URL ảnh gốc cần kiểm tra.</param>
        /// <returns>URL đã được validate hoặc chuỗi rỗng nếu không hợp lệ.</returns>
        private static string SanitizeImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return string.Empty;

            // Cho phép đường dẫn tương đối (ví dụ: /images/product.jpg)
            if (imageUrl.StartsWith("/"))
                return WebUtility.HtmlEncode(imageUrl);

            // Chỉ cho phép scheme http hoặc https
            if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
                && (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                return WebUtility.HtmlEncode(imageUrl);
            }

            return string.Empty;
        }
    }
}
