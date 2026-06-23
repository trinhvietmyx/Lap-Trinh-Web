using GIUA_KY_WEB_3AE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GIUA_KY_WEB_3AE.Services;
using System.IO;

namespace GIUA_KY_WEB_3AE.Data
{
    /// <summary>
    /// Lớp khởi tạo dữ liệu (Seed Data) cho cơ sở dữ liệu Microsoft SQL Server.
    /// Tự động nạp danh mục và sản phẩm mẫu ban đầu, đồng thời seed các vai trò và tài khoản quản trị mặc định cho quán Mì 3AE.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Phương thức nạp dữ liệu mẫu ban đầu vào cơ sở dữ liệu nếu bảng rỗng.
        /// </summary>
        /// <param name="app">Đối tượng IApplicationBuilder của ứng dụng.</param>
        public static void Seed(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (context == null) return;

                // Tự động kiểm tra và tạo cơ sở dữ liệu nếu chưa có bằng Migrate()
                context.Database.Migrate();

                // Lấy dịch vụ quản trị vai trò và người dùng từ DI Container
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // 1. Tạo các Role mặc định nếu chưa tồn tại
                string[] roleNames = { SD.Role_Admin, SD.Role_Customer, SD.Role_Employee, SD.Role_Company };
                foreach (var roleName in roleNames)
                {
                    if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                    {
                        roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                    }
                }

                // 2. Tạo tài khoản Admin mặc định cho Mì 3AE nếu chưa tồn tại
                string adminEmail = "admin@mi3ae.local";
                string adminPassword = "Admin@12345";
                var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
                if (adminUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FullName = "Admin Mì 3AE",
                        Address = "Mì 3AE",
                        Age = "30"
                    };
                    var createAdminResult = userManager.CreateAsync(user, adminPassword).GetAwaiter().GetResult();
                    if (createAdminResult.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                    }
                }

                // 3. Tự động import dữ liệu Bếp Nhà Xíu từ các file HTML của Excel nếu trống (Đã vô hiệu hóa cho dự án Mì 3AE)
                /*
                string? excelFolder = FindExcelFolder();
                if (excelFolder != null && Directory.Exists(excelFolder) && !context.Customers.Any() && !context.SellableItems.Any())
                {
                    try
                    {
                        Console.WriteLine($"[INFO] Tìm thấy thư mục dữ liệu Excel tại: {excelFolder}");
                        var importer = new HtmlDataImporterService(context);
                        importer.ImportAllAsync(excelFolder).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi tự động import dữ liệu Bếp Nhà Xíu: {ex.Message}");
                    }
                }
                */

                // Nạp danh mục mẫu nếu bảng Categories trống
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Mì Quảng" },
                        new Category { Name = "Món Ăn Kèm" },
                        new Category { Name = "Nước Uống" }
                    );
                    context.SaveChanges();
                }

                // Lấy danh sách ID thực tế của danh mục sau khi lưu vào database
                var categories = context.Categories.AsNoTracking().ToList();
                var miQuangCat = categories.FirstOrDefault(c => c.Name == "Mì Quảng");
                var monAnKemCat = categories.FirstOrDefault(c => c.Name == "Món Ăn Kèm");
                var nuocUongCat = categories.FirstOrDefault(c => c.Name == "Nước Uống");

                int miQuangId = miQuangCat != null ? miQuangCat.Id : 1;
                int monAnKemId = monAnKemCat != null ? monAnKemCat.Id : 2;
                int nuocUongId = nuocUongCat != null ? nuocUongCat.Id : 3;

                // Nạp 15 sản phẩm món ăn/thức uống mẫu cho quán Mì 3AE nếu bảng Products trống
                if (!context.Products.Any())
                {
                    context.Products.AddRange(
                        new Product
                        {
                            Name = "Mì Quảng Tôm Thịt Đặc Biệt",
                            Price = 45000,
                            Description = "Mì Quảng Tôm Thịt truyền thống với sợi mì dai ngon, tôm đất rim đậm vị, thịt ba chỉ béo ngậy, chan nước lèo ngọt thanh từ xương ống, dùng kèm bánh tráng nướng giòn rụm và rau sống tươi xanh.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = miQuangId
                        },
                        new Product
                        {
                            Name = "Mì Quảng Gà Ta Quê",
                            Price = 40000,
                            Description = "Mì Quảng Gà với thịt gà ta thả vườn săn chắc, thơm ngọt được rim nghệ vàng ươm, nước lèo sánh quyện đậm đà hương vị miền Trung.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = miQuangId
                        },
                        new Product
                        {
                            Name = "Mì Quảng Sườn Non Rim",
                            Price = 50000,
                            Description = "Mì Quảng Sườn Non với những cọng sườn heo tuyển chọn, rim mềm thấm vị kết hợp với trứng cút và đậu phộng rang thơm lừng.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = miQuangId
                        },
                        new Product
                        {
                            Name = "Mì Quảng Ếch Đồng Nghệ",
                            Price = 55000,
                            Description = "Sự kết hợp độc đáo giữa thịt ếch đồng thơm ngon, xào lăn nghệ vàng đậm đà hương vị đồng quê cùng củ nén khử dầu phộng thơm nức mũi.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = miQuangId
                        },
                        new Product
                        {
                            Name = "Mì Quảng Bò Tơ Củ Chi",
                            Price = 50000,
                            Description = "Mì Quảng Bò mềm tan, nước lèo ngọt bùi hầm từ xương bò và các gia vị truyền thống, rắc thêm chút hành lá và ớt xiêm xanh đúng điệu.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = miQuangId
                        },
                        new Product
                        {
                            Name = "Mì Quảng Cá Lóc Đồng",
                            Price = 55000,
                            Description = "Mì Quảng Cá Lóc với phần thịt cá lóc phi lê rim thấm vị, hoàn toàn không xương, nước dùng ngọt đậm đà, rất tốt cho sức khỏe.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = miQuangId
                        },
                        new Product
                        {
                            Name = "Mì Quảng Trứng Thịt Xá Xíu",
                            Price = 35000,
                            Description = "Mì Quảng trứng bình dân với thịt xá xíu đậm đà, kết hợp trứng cút luộc lòng đào béo bùi, thích hợp cho bữa sáng nhanh gọn.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = miQuangId
                        },
                        new Product
                        {
                            Name = "Bánh Tráng Nướng Mè Đen",
                            Price = 15000,
                            Description = "Bánh tráng mè đen nướng giòn rụm đúng chuẩn Đại Lộc - Quảng Nam, món ăn kèm không thể thiếu khi thưởng thức Mì Quảng.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = monAnKemId
                        },
                        new Product
                        {
                            Name = "Ram Cuốn Cải Quê",
                            Price = 20000,
                            Description = "Phần ram cuốn giòn tan bọc thịt băm, mộc nhĩ, cuốn kèm rau cải cay và chấm nước mắm tỏi ớt chua ngọt siêu kích thích vị giác.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = monAnKemId
                        },
                        new Product
                        {
                            Name = "Chả Cá Thác Lác Chiên Giòn",
                            Price = 25000,
                            Description = "Chả cá thác lác nguyên chất dai ngon nướng hoặc chiên nóng hổi, ăn kèm tỏi cô đơn và nước mắm ớt cay xè.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = monAnKemId
                        },
                        new Product
                        {
                            Name = "Gỏi Cuốn Tôm Thịt (2 chiếc)",
                            Price = 20000,
                            Description = "Gỏi cuốn thanh mát với tôm luộc, thịt ba chỉ, bún tươi và các loại rau thơm miền Trung, ăn cùng tương đậu phộng béo ngậy.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = monAnKemId
                        },
                        new Product
                        {
                            Name = "Trà Đá Lá Dứa",
                            Price = 5000,
                            Description = "Trà đá mát lạnh, thơm mùi lá dứa tự nhiên giúp thanh nhiệt giải độc trong lúc thưởng thức tô mì cay nồng.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = nuocUongId
                        },
                        new Product
                        {
                            Name = "Nước Mía Ép Tươi Nguyên Chất",
                            Price = 15000,
                            Description = "Nước mía ép từ mía lau ngọt lịm kết hợp cùng hương tắc (quất) thơm mát dễ chịu.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = nuocUongId
                        },
                        new Product
                        {
                            Name = "Sinh Tố Bơ Sáp Đắk Lắk",
                            Price = 25000,
                            Description = "Sinh tố bơ xay mịn thơm béo từ bơ sáp Đắk Lắk tự nhiên, kết hợp sữa đặc và đá xay mát lạnh.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = nuocUongId
                        },
                        new Product
                        {
                            Name = "Cà Phê Sữa Đá Truyền Thống",
                            Price = 18000,
                            Description = "Cà phê phin đậm đà chuẩn vị pha cùng sữa đặc ngôi sao, đánh bọt thơm nồng nàn đậm chất Việt Nam.",
                            ImageUrl = "/images/default_product.png",
                            ImageUrls = new List<string>(),
                            CategoryId = nuocUongId
                        }
                    );
                    context.SaveChanges();
                }
            }
        }

        /*
        /// <summary>
        /// Tìm kiếm thư mục Bếp Nhà Xíu.xlsx~ bằng cách duyệt ngược từ thư mục hiện tại lên các thư mục cha.
        /// </summary>
        /// <returns>Đường dẫn đến thư mục dữ liệu, hoặc null nếu không tìm thấy.</returns>
        private static string? FindExcelFolder()
        {
            string currentDir = Directory.GetCurrentDirectory();
            
            // Duyệt tối đa 3 cấp thư mục cha
            for (int i = 0; i < 4; i++)
            {
                if (string.IsNullOrEmpty(currentDir)) break;

                try
                {
                    var dirs = Directory.GetDirectories(currentDir);
                    foreach (var dir in dirs)
                    {
                        string dirName = Path.GetFileName(dir);
                        string cleanName = HtmlDataImporterService.RemoveDiacritics(dirName).ToLower();
                        if (cleanName.Contains("bep nha xiu.xlsx~"))
                        {
                            return dir;
                        }
                    }
                }
                catch
                {
                    // Bỏ qua lỗi truy cập
                }

                string? parent = Path.GetDirectoryName(currentDir);
                if (parent == currentDir) break;
                currentDir = parent ?? "";
            }

            return null;
        }
        */
    }
}
