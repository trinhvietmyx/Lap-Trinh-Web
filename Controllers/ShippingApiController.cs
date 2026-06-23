using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GIUA_KY_WEB_3AE.Controllers
{
    /// <summary>
    /// API Controller cung cấp dịch vụ tính phí vận chuyển (Shipping fee).
    /// </summary>
    [ApiController]
    [Route("api/shipping")]
    public class ShippingApiController : ControllerBase
    {
        // Tọa độ quán mặc định: Trường Đại học Sư phạm Kỹ thuật TP.HCM, Quận Thủ Đức (1 Võ Văn Ngân, Linh Chiểu, Thủ Đức)
        private const double RestaurantLatitude = 10.8499;
        private const double RestaurantLongitude = 106.7717;

        /// <summary>
        /// Tính toán phí vận chuyển và thời gian giao hàng dự kiến dựa trên tọa độ GPS của khách hàng.
        /// Sử dụng dịch vụ OSRM công cộng để lấy khoảng cách thực tế, có fallback bằng công thức Haversine.
        /// </summary>
        /// <param name="lat">Vĩ độ (Latitude) của khách hàng.</param>
        /// <param name="lng">Kinh độ (Longitude) của khách hàng.</param>
        /// <param name="orderTotal">Tổng số tiền của đơn hàng để xét miễn phí vận chuyển.</param>
        /// <returns>Thông tin quãng đường, thời gian giao dự kiến, phí ship và trạng thái xử lý.</returns>
        [HttpGet("calculate-gps")]
        public async Task<IActionResult> CalculateShippingFeeGps([FromQuery] double lat, [FromQuery] double lng, [FromQuery] decimal orderTotal = 0)
        {
            if (lat == 0 || lng == 0)
            {
                return BadRequest(new { success = false, message = "Tọa độ GPS không hợp lệ." });
            }

            double distanceKm = 0;
            int drivingMinutes = 0;
            bool isOSRMUsed = false;

            string url = $"https://router.project-osrm.org/route/v1/driving/{RestaurantLongitude.ToString(CultureInfo.InvariantCulture)},{RestaurantLatitude.ToString(CultureInfo.InvariantCulture)};{lng.ToString(CultureInfo.InvariantCulture)},{lat.ToString(CultureInfo.InvariantCulture)}?overview=false";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; Mi3AE/1.0)");
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using (var doc = JsonDocument.Parse(content))
                        {
                            var root = doc.RootElement;
                            if (root.GetProperty("code").GetString() == "Ok")
                            {
                                var route = root.GetProperty("routes")[0];
                                double distance = route.GetProperty("distance").GetDouble(); // mét
                                double duration = route.GetProperty("duration").GetDouble(); // giây

                                distanceKm = Math.Round(distance / 1000.0, 1);
                                drivingMinutes = (int)Math.Round(duration / 60.0);
                                isOSRMUsed = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi khi kết nối API OSRM: " + ex.Message);
                }
            }

            // Nếu không dùng được OSRM (lỗi mạng hoặc server sập), chuyển sang tính bằng Haversine (đường chim bay * 1.3)
            if (!isOSRMUsed)
            {
                distanceKm = Math.Round(CalculateHaversineDistance(RestaurantLatitude, RestaurantLongitude, lat, lng) * 1.3, 1);
                // Vận tốc xe máy trung bình ở TP.HCM khoảng 25km/h (tương đương 2.4 phút/km)
                drivingMinutes = (int)Math.Max(5, Math.Round(distanceKm * 2.4));
            }

            // Chỉ giao trong nội thành TP.HCM (phạm vi tối đa 30km từ quán tại Thủ Đức)
            if (distanceKm > 30)
            {
                return BadRequest(new { success = false, message = "Rất tiếc, Mì 3AE chỉ hỗ trợ giao hàng trong phạm vi 30km từ quán (nội thành TP. Hồ Chí Minh)." });
            }

            // Tính phí vận chuyển (Grab / ShopeeFood style)
            // - 2 km đầu tiên: 15,000đ
            // - Mỗi km tiếp theo: 5,000đ/km
            decimal shippingFee = 15000;
            if (distanceKm > 2)
            {
                shippingFee += (decimal)Math.Ceiling(distanceKm - 2) * 5000;
            }

            // Giới hạn phí ship tối đa là 100,000đ
            if (shippingFee > 100000)
            {
                shippingFee = 100000;
            }

            // Miễn phí vận chuyển cho đơn hàng từ 300,000đ
            if (orderTotal >= 300000)
            {
                shippingFee = 0;
            }

            int prepTime = 15; // 15 phút chuẩn bị mì Quảng
            int totalMinutes = drivingMinutes + prepTime;

            // Xác định khoảng thời gian hiển thị dự kiến
            int minDeliveryTime = Math.Max(15, totalMinutes - 5);
            int maxDeliveryTime = totalMinutes + 5;
            string deliveryTimeDisplay = $"{minDeliveryTime} - {maxDeliveryTime} phút";

            return Ok(new
            {
                success = true,
                distanceKm = distanceKm,
                drivingMinutes = drivingMinutes,
                prepTimeMinutes = prepTime,
                totalDeliveryMinutes = totalMinutes,
                deliveryTimeDisplay = deliveryTimeDisplay,
                shippingFee = shippingFee,
                formattedShippingFee = shippingFee.ToString("#,##0") + "đ",
                freeShipThreshold = 300000,
                isOSRM = isOSRMUsed
            });
        }

        /// <summary>
        /// Tính khoảng cách đường chim bay giữa 2 điểm tọa độ theo công thức Haversine.
        /// </summary>
        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double r = 6371; // Bán kính Trái Đất (km)
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }

        /// <summary>
        /// Chuyển đổi góc từ Độ sang Radian.
        /// </summary>
        private static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        /// <summary>
        /// Tính toán phí vận chuyển và thời gian giao hàng dự kiến dựa trên Tỉnh/Thành phố nhận hàng.
        /// </summary>
        /// <param name="province">Tên Tỉnh/Thành phố nhận hàng.</param>
        /// <returns>Thông tin phí ship, số ngày dự kiến và ngưỡng miễn phí ship.</returns>
        [HttpGet("calculate")]
        public IActionResult CalculateShippingFee([FromQuery] string province)
        {
            if (string.IsNullOrEmpty(province))
            {
                return BadRequest(new { success = false, message = "Vui lòng cung cấp Tỉnh/Thành phố." });
            }

            string normalizedProvince = NormalizeVietnameseText(province);

            // Chỉ hỗ trợ giao hàng trong nội thành TP. Hồ Chí Minh
            if (normalizedProvince.Contains("ho chi minh") || normalizedProvince.Contains("hcm") || normalizedProvince.Contains("sai gon"))
            {
                decimal shippingFee = 20000;
                int estimatedDays = 1;

                return Ok(new
                {
                    success = true,
                    province = province,
                    shippingFee = shippingFee,
                    estimatedDays = estimatedDays,
                    freeShipThreshold = 300000, // Miễn phí ship cho đơn món ăn từ 300.000đ
                    formattedShippingFee = shippingFee.ToString("#,##0") + "đ",
                    message = $"Giao hàng dự kiến trong {estimatedDays} ngày"
                });
            }
            else
            {
                return BadRequest(new { success = false, message = "Rất tiếc, Mì 3AE hiện tại chỉ hỗ trợ giao hàng trong nội thành TP. Hồ Chí Minh." });
            }
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
    }
}
