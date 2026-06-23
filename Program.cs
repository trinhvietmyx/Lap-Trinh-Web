using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;
using GIUA_KY_WEB_3AE.Repositories;
using GIUA_KY_WEB_3AE.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
var databaseProvider = builder.Configuration["DatabaseProvider"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase) ||
        defaultConnection.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
        defaultConnection.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
        defaultConnection.Contains("Database=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(defaultConnection);
        return;
    }

    options.UseSqlite(defaultConnection);
});

// Đăng ký dịch vụ ASP.NET Core Identity để quản trị người dùng và vai trò
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Đăng ký dịch vụ xác thực ngoài (Google & Facebook)
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "dummy-google-id";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "dummy-google-secret";
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? "dummy-facebook-id";
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? "dummy-facebook-secret";
        // Khai báo quyền truy cập email và thông tin công khai từ Facebook
        options.Scope.Add("email");
        options.Scope.Add("public_profile");
        // Khai báo các trường dữ liệu cần lấy từ Graph API
        options.Fields.Add("id");
        options.Fields.Add("name");
        options.Fields.Add("email");
        options.Fields.Add("picture");
    });

// Cấu hình chuyển hướng trang đăng nhập và truy cập bị từ chối đối với Cookie của Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Đăng ký dịch vụ Product Repository sử dụng SQL Server thực tế
builder.Services.AddScoped<IProductRepository, SqlProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
// builder.Services.AddScoped<HtmlDataImporterService>();

// Đăng ký dịch vụ gửi email
builder.Services.AddTransient<IEmailService, GmailEmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Đăng ký dịch vụ HttpClient phục vụ Chatbot AI
builder.Services.AddHttpClient();

// Đăng ký dịch vụ HttpContextAccessor và Session phục vụ giỏ hàng
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Lưu giỏ hàng trong 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowOrigins", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500", "http://localhost:5162", "https://localhost:7162")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Khởi tạo dữ liệu mẫu cho SQL Server (nếu bảng trống)
DbInitializer.Seed(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Kích hoạt CORS (đặt trước Authorization)
app.UseCors("MyAllowOrigins");

// Thêm Middleware xác thực và phân quyền TRƯỚC Session
// để đảm bảo User.Identity đã được thiết lập khi CartController đọc Session key
app.UseAuthentication();
app.UseAuthorization();

// Sử dụng Session cho giỏ hàng (phải nằm SAU Authentication để GetCartSessionKey() lấy đúng UserId)
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Đăng ký định tuyến cho Web API Controllers
app.MapControllers();

app.Run();

