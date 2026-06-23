using GIUA_KY_WEB_3AE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GIUA_KY_WEB_3AE.Controllers
{
    /// <summary>
    /// Controller quản trị tài khoản người dùng: Đăng ký, Đăng nhập, Đăng xuất, và Phân quyền truy cập.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Hàm khởi tạo của AccountController, tiêm các dịch vụ Identity cần thiết.
        /// </summary>
        /// <param name="userManager">Đối tượng quản lý người dùng.</param>
        /// <param name="signInManager">Đối tượng quản lý phiên đăng nhập.</param>
        /// <param name="roleManager">Đối tượng quản lý vai trò.</param>
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Action hiển thị trang Đăng ký thành viên mới (GET).
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Action xử lý đăng ký tài khoản thành viên mới (POST).
        /// Tự động gán vai trò "Customer" mặc định cho tài khoản đăng ký thành công.
        /// </summary>
        /// <param name="model">Dữ liệu từ biểu mẫu đăng ký.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    FullName = $"{model.LastName} {model.FirstName}".Trim(),
                    Address = string.Empty,
                    Age = string.Empty
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Đảm bảo Role "Customer" đã tồn tại
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Customer))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                    }

                    // Gán vai trò mặc định là "Customer"
                    await _userManager.AddToRoleAsync(user, SD.Role_Customer);

                    // Ghi nhận Họ và Tên của người dùng qua Claim FullName (để tương thích ngược)
                    var fullName = $"{model.LastName} {model.FirstName}".Trim();
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", fullName));

                    // Đăng nhập tự động ngay sau khi đăng ký thành công
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Product");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Action hiển thị trang Đăng nhập (GET).
        /// </summary>
        /// <param name="returnUrl">Đường dẫn quay lại sau khi đăng nhập thành công.</param>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Action xử lý đăng nhập tài khoản người dùng (POST).
        /// </summary>
        /// <param name="model">Dữ liệu đăng nhập.</param>
        /// <param name="returnUrl">Đường dẫn quay lại.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Product");
                }
                
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        /// <summary>
        /// Chuyển hướng người dùng đến nhà cung cấp dịch vụ đăng nhập ngoài (Google hoặc Facebook).
        /// </summary>
        /// <param name="provider">Tên nhà cung cấp dịch vụ (Google, Facebook).</param>
        /// <param name="returnUrl">Đường dẫn chuyển hướng sau khi đăng nhập thành công.</param>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        /// <summary>
        /// Xử lý callback sau khi đăng nhập bằng dịch vụ ngoài thành công.
        /// Tự động đăng ký tài khoản khách hàng mới nếu email chưa tồn tại trong hệ thống.
        /// </summary>
        /// <param name="returnUrl">Đường dẫn chuyển hướng sau khi đăng nhập thành công.</param>
        /// <param name="remoteError">Lỗi trả về từ nhà cung cấp dịch vụ ngoài nếu có.</param>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi từ dịch vụ đăng nhập ngoài: {remoteError}");
                return View("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy thông tin đăng nhập từ dịch vụ ngoài.");
                return View("Login");
            }

            // Đăng nhập người dùng nếu tài khoản liên kết đã tồn tại trước đó
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction("AccessDenied");
            }

            // Lấy địa chỉ email từ Claim phản hồi của nhà cung cấp
            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");

            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Tạo tài khoản mới tự động nếu chưa tồn tại email này
                    var name = info.Principal.FindFirstValue(ClaimTypes.Name)
                               ?? info.Principal.FindFirstValue("name")
                               ?? email.Split('@')[0];

                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = name,
                        Address = string.Empty,
                        Age = string.Empty
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        // Đảm bảo vai trò khách hàng "Customer" tồn tại
                        if (!await _roleManager.RoleExistsAsync(SD.Role_Customer))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                        }
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                        await _userManager.AddClaimAsync(user, new Claim("FullName", name));
                    }
                    else
                    {
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View("Login");
                    }
                }

                // Liên kết thông tin đăng nhập ngoài với tài khoản user
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                    return LocalRedirect(returnUrl);
                }
            }

            ModelState.AddModelError(string.Empty, "Không thể xác định Email từ dịch vụ ngoài này.");
            return View("Login");
        }

        /// <summary>
        /// Action xử lý đăng xuất phiên làm việc của tài khoản (POST).
        /// Giỏ hàng của User được lưu riêng theo key "Mi3AECart_User_{UserId}"
        /// nên không cần xóa Session — dữ liệu giỏ hàng sẽ được giữ lại nếu User đăng nhập lại.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Product");
        }

        /// <summary>
        /// Action hiển thị trang báo lỗi từ chối truy cập do thiếu quyền (GET).
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Action hiển thị trang Cài đặt tài khoản để cập nhật thông tin cá nhân (GET).
        /// Yêu cầu người dùng phải đăng nhập trước.
        /// </summary>
        /// <returns>Giao diện trang Cài đặt tài khoản với thông tin hiện tại.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var model = new AccountSettingsViewModel
            {
                FullName = user.FullName,
                Phone = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty
            };

            return View(model);
        }

        /// <summary>
        /// Action xử lý việc lưu thông tin cá nhân mới được cài đặt (POST).
        /// Cập nhật họ tên, số điện thoại, địa chỉ và đồng bộ Claim FullName.
        /// </summary>
        /// <param name="model">Dữ liệu thông tin cá nhân mới.</param>
        /// <returns>Trang cài đặt cùng thông báo thành công hoặc lỗi.</returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(AccountSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.Phone;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Cập nhật hoặc thêm lại claim FullName để đảm bảo đồng bộ hóa thông tin hiển thị ở Layout
                var claims = await _userManager.GetClaimsAsync(user);
                var fullNameClaim = claims.FirstOrDefault(c => c.Type == "FullName");
                if (fullNameClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, fullNameClaim);
                }
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", model.FullName));

                // Nạp lại đăng nhập của người dùng để cập nhật các claims mới trong cookie
                await _signInManager.RefreshSignInAsync(user);

                TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
                return RedirectToAction(nameof(Settings));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
