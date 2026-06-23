using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GIUA_KY_WEB_3AE.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu hội thoại của Chatbot AI.
    /// Kết nối đến Groq API và truy vấn thông tin thực đơn, đơn hàng thực tế từ CSDL.
    /// Yêu cầu đăng nhập để bảo vệ thông tin đơn hàng khỏi bị truy cập trái phép.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Hàm khởi tạo của ChatController.
        /// </summary>
        /// <param name="dbContext">Context kết nối cơ sở dữ liệu.</param>
        /// <param name="configuration">Đối tượng cấu hình hệ thống.</param>
        /// <param name="httpClientFactory">Factory để tạo HttpClient gửi request bên ngoài.</param>
        /// <param name="userManager">UserManager để xác định người dùng hiện tại.</param>
        public ChatController(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
        }

        /// <summary>
        /// Lớp đại diện cho yêu cầu gửi tin nhắn từ Client.
        /// </summary>
        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// API nhận tin nhắn từ người dùng, xây dựng ngữ cảnh và gửi yêu cầu tới Groq API.
        /// Chỉ cho phép truy vấn đơn hàng thuộc về chính người dùng đang đăng nhập (hoặc Admin).
        /// </summary>
        /// <param name="request">Tin nhắn của khách hàng.</param>
        /// <returns>Câu trả lời của Chatbot.</returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { reply = "Nội dung tin nhắn không được trống." });
            }

            string userMessage = request.Message.Trim();

            // Lấy thông tin người dùng hiện tại để kiểm tra quyền sở hữu đơn hàng
            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole(SD.Role_Admin);

            // 1. Quét tìm mã đơn hàng trong tin nhắn (Ví dụ: #3 hoặc số đơn 3)
            string orderContext = string.Empty;
            var match = Regex.Match(userMessage, @"#?(\d+)");
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int orderId))
                {
                    // Truy vấn đơn hàng từ CSDL
                    var order = await _dbContext.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.Id == orderId);

                    // Chỉ cho phép xem đơn hàng nếu thuộc về chính người dùng hoặc là Admin
                    if (order != null && (order.UserId == currentUserId || isAdmin))
                    {
                        string itemsStr = string.Join(", ", order.OrderItems.Select(oi => $"{oi.ProductName} (x{oi.Quantity})"));
                        string statusVietnamese = order.Status switch
                        {
                            "Pending" => "Chờ duyệt (Đang chuẩn bị nguyên liệu)",
                            "Confirmed" => "Đã xác nhận",
                            "Shipped" => "Đang giao hàng",
                            "Completed" => "Hoàn thành thành công",
                            "Cancelled" => "Đã hủy",
                            _ => order.Status
                        };

                        // Chỉ đưa thông tin trạng thái và món ăn vào ngữ cảnh, KHÔNG bao gồm SĐT/Địa chỉ
                        orderContext = $"Đơn hàng #{order.Id} của bạn ({order.CustomerName}). " +
                                       $"Tổng tiền: {order.TotalAmount:#,##0} đ. " +
                                       $"Trạng thái hiện tại: {statusVietnamese}. Các món đã đặt: {itemsStr}. " +
                                       $"Ghi chú món ăn: {order.Notes ?? "Không có"}.";
                    }
                    else if (order != null)
                    {
                        // Đơn hàng tồn tại nhưng không thuộc về người dùng hiện tại
                        orderContext = ""; // Không tiết lộ bất kỳ thông tin nào
                    }
                }
            }

            // 2. Lấy danh sách thực đơn hiện có để nạp làm ngữ cảnh
            var products = await _dbContext.Products.Where(p => p.Price > 0).ToListAsync();
            string menuContext = string.Join("\n", products.Select(p => $"- {p.Name} (Giá: {p.Price:#,##0} đ): {p.Description}"));

            // 3. Đọc Groq ApiKey từ cấu hình
            string groqApiKey = _configuration["Groq:ApiKey"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(groqApiKey) || groqApiKey.Contains("YOUR_"))
            {
                // CHẾ ĐỘ GIẢ LẬP CỤC BỘ (Fallback Rule-based Mock Chatbot)
                string fallbackReply = ProcessFallbackMessage(userMessage, menuContext, orderContext);
                return Ok(new { reply = fallbackReply });
            }

            // 4. Gọi Groq API thật
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", groqApiKey);

                var systemPrompt = "Bạn là Trợ lý ảo Mì 3AE, một AI thân thiện chuyên tư vấn món ăn của quán Mì 3AE. " +
                                   "Hãy trả lời khách hàng bằng tiếng Việt một cách lễ phép, ngắn gọn, ấm áp, có kèm biểu tượng cảm xúc phù hợp. " +
                                   "Giờ mở cửa: 6:00 sáng đến 10:00 tối hàng ngày. Địa chỉ: 123 Đường Ba Tháng Hai, Quận 10, TP. HCM. SĐT hotline: +84 909 3AE 3AE. " +
                                   "Chính sách giao hàng: Miễn phí vận chuyển cho đơn hàng từ 300,000đ trở lên. " +
                                   "TUYỆT ĐỐI KHÔNG tiết lộ số điện thoại, địa chỉ, hay thông tin cá nhân của khách hàng trong câu trả lời. " +
                                   "Dưới đây là thông tin thực đơn món ăn thực tế của quán:\n" + menuContext + "\n\n" +
                                   (string.IsNullOrEmpty(orderContext) ? "" : "Về đơn hàng khách đang hỏi: " + orderContext + "\n");

                var requestBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userMessage }
                    },
                    temperature = 0.7,
                    max_tokens = 500
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseJson);
                    string reply = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "Xin lỗi, tôi gặp sự cố trong quá trình xử lý câu trả lời.";

                    return Ok(new { reply = reply.Trim() });
                }
                else
                {
                    // Nếu lỗi API từ Groq (như sai Key), tự động fallback về giả lập cục bộ
                    string fallbackReply = ProcessFallbackMessage(userMessage, menuContext, orderContext);
                    return Ok(new { reply = fallbackReply });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chatbot: {ex.Message}");
                string fallbackReply = ProcessFallbackMessage(userMessage, menuContext, orderContext);
                return Ok(new { reply = fallbackReply });
            }
        }

        /// <summary>
        /// Xử lý sinh câu trả lời cục bộ bằng từ khóa thông minh (Rule-based Fallback) khi không kết nối được API.
        /// </summary>
        /// <param name="message">Tin nhắn từ khách.</param>
        /// <param name="menu">Ngữ cảnh thực đơn dạng text.</param>
        /// <param name="order">Ngữ cảnh đơn hàng dạng text.</param>
        /// <returns>Câu trả lời phản hồi khách.</returns>
        private string ProcessFallbackMessage(string message, string menu, string order)
        {
            string lowerMsg = message.ToLower();

            // 1. Hỏi về đơn hàng cụ thể
            if (!string.IsNullOrEmpty(order))
            {
                return $"🍜 [Trợ lý Mì 3AE] Dạ, tôi đã tìm thấy đơn hàng của bạn!\n\n📌 {order}\n\nChúc bạn ngon miệng và có trải nghiệm tuyệt vời với Mì 3AE ạ!";
            }

            // 2. Hỏi trạng thái đơn hàng chung chung
            if (lowerMsg.Contains("đơn hàng") || lowerMsg.Contains("trạng thái") || lowerMsg.Contains("kiểm tra đơn"))
            {
                return "🍜 [Trợ lý Mì 3AE] Để kiểm tra trạng thái đơn hàng, bạn vui lòng nhập kèm mã số đơn hàng dạng: **#mã** (Ví dụ: *#3* hoặc *đơn hàng 3*). Hoặc bạn có thể truy cập mục 'Lịch sử mua hàng' ở góc phải màn hình nhé!";
            }

            // 3. Hỏi thực đơn, món ăn
            if (lowerMsg.Contains("thực đơn") || lowerMsg.Contains("món ăn") || lowerMsg.Contains("mì quảng") || lowerMsg.Contains("có gì") || lowerMsg.Contains("ăn gì") || lowerMsg.Contains("menu"))
            {
                return $"🍜 [Trợ lý Mì 3AE] Dạ, đây là thực đơn các món ăn Quảng Nam nóng hổi của quán em:\n\n{menu}\n\n🛵 Đơn từ **300.000đ** sẽ được miễn phí vận chuyển trong nội thành đó ạ! Bạn thích món nào để em hướng dẫn đặt món nha.";
            }

            // 4. Hỏi địa chỉ, giờ mở cửa
            if (lowerMsg.Contains("địa chỉ") || lowerMsg.Contains("ở đâu") || lowerMsg.Contains("đến quán") || lowerMsg.Contains("giờ") || lowerMsg.Contains("mở cửa"))
            {
                return "🍜 [Trợ lý Mì 3AE] Dạ, quán Mì 3AE mở cửa đón khách từ **6:00 sáng đến 10:00 tối** hàng ngày.\n📍 **Địa chỉ:** 123 Đường Ba Tháng Hai, Quận 10, TP. HCM.\n📞 **Hotline hỗ trợ:** +84 909 3AE 3AE. Rất mong được đón tiếp bạn!";
            }

            // 5. Hỏi ship, giao hàng
            if (lowerMsg.Contains("ship") || lowerMsg.Contains("giao hàng") || lowerMsg.Contains("vận chuyển") || lowerMsg.Contains("phí"))
            {
                return "🍜 [Trợ lý Mì 3AE] Quán giao hàng nhanh trong vòng 30 phút. Đặc biệt **miễn phí vận chuyển** cho tất cả các hóa đơn từ **300.000đ** trở lên ạ!";
            }

            // 6. Câu chào hỏi chung
            return "🍜 Xin chào! Tôi là Trợ lý ảo Mì 3AE. Tôi có thể giúp bạn xem thực đơn, kiểm tra thông tin đơn hàng (vui lòng gõ kèm mã đơn ví dụ: *#3*) hoặc cung cấp thông tin liên hệ của quán. Bạn cần em hỗ trợ gì ạ?";
        }
    }
}
