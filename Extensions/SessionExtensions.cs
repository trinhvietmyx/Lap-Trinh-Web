using System.Text.Json;

namespace GIUA_KY_WEB_3AE.Extensions
{
    /// <summary>
    /// Các phương thức mở rộng cho đối tượng ISession.
    /// Giúp lưu trữ và truy xuất các đối tượng phức tạp (objects) dưới dạng chuỗi JSON.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Chuyển đổi một đối tượng thành chuỗi JSON và lưu vào Session.
        /// </summary>
        /// <param name="session">Đối tượng Session của HttpContext.</param>
        /// <param name="key">Khóa (key) định danh để lưu trữ trong Session.</param>
        /// <param name="value">Giá trị đối tượng cần được tuần tự hóa và lưu trữ.</param>
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Lấy một đối tượng từ Session bằng cách giải tuần tự hóa từ chuỗi JSON.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu của đối tượng cần phục hồi.</typeparam>
        /// <param name="session">Đối tượng Session của HttpContext.</param>
        /// <param name="key">Khóa (key) định danh của đối tượng trong Session.</param>
        /// <returns>Đối tượng kiểu T hoặc giá trị mặc định của T nếu không tìm thấy khóa.</returns>
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
