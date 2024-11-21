using ECommerceFood.Entities;
using ECommerceFood.Services.Account;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECommerceFood.ViewModels;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.EntityFrameworkCore;

namespace ECommerceFood.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountSV;

        public AccountController(IAccountService accountSV)
        {
            _accountSV = accountSV;
        }


        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        /// <summary>
        /// Post: Account/Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (user, roles, errorMessage) = await _accountSV.LoginAsync(model.UsernameOrEmail, model.Password, model.RememberMe);

                if (user != null)
                {
                    // Kiểm tra vai trò và chuyển hướng dựa trên vai trò
                    if (roles.Contains("User"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("HomePage", "HomeAdmin", new { area = "Admin" });
                    }
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    // Thông báo lỗi từ AccountService (tài khoản bị khóa hoặc không tồn tại)
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
                else
                {
                    // Đăng nhập thất bại - Thông báo "Tài khoản hoặc mật khẩu sai"
                    ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu sai.");
                }
            }

            return View(model);
        }


        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await _accountSV.Logout();
            return RedirectToAction("Login");
        }


        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }
        /// <summary>
        /// Post: Account/Register
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Kiểm tra xem định dạng email có hợp lệ không
            if (!IsValidEmail(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Định dạng email không hợp lệ.");
                return View(model);
            }

            // Kiểm tra xem tên người dùng đã tồn tại chưa
            if (await _accountSV.UserNameExistsAsync(model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "Tên người dùng đã tồn tại.");
                return View(model);
            }

            // Kiểm tra xem email đã tồn tại chưa
            if (await _accountSV.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
                return View(model);
            }

            // Kiểm tra xem mật khẩu và mật khẩu xác nhận có khớp không
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Mật khẩu không khớp.");
                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                UserName = model.Username,
                Password = model.Password,
                RegistrationDate = DateTime.Now
            };

            // Đăng ký người dùng mới
            var success = await _accountSV.RegisterAsync(user, "User");

            if (!success)
            {
                ModelState.AddModelError(nameof(model.Password), "Mật khẩu phải có ít nhất 8 chữ cái, bao gồm chữ hoa, chữ thường, số");
                return View(model);
            }

            return RedirectToAction("Login");
        }

        /// <summary>
        /// kiểm tra định dạng email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }


        // GET: Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }
        /// <summary>
        /// POST: Account/ForgotPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountSV.SendPasswordResetCodeAsync(model.Email);
            if (result)
            {
                HttpContext.Session.SetString("ResetEmail", model.Email);
                return RedirectToAction("VerifyResetCode");
            }

            ModelState.AddModelError("", "Email không hợp lệ hoặc không tồn tại.");
            return View(model);
        }

        // GET: Account/VerifyResetCode
        public IActionResult VerifyResetCode()
        {
            return View();
        }
        /// <summary>
        /// POST: Account/VerifyResetCode
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyResetCode(VerifyResetCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isValidCode = await _accountSV.VerifyResetCodeAsync(model.ResetCode);
            if (isValidCode)
            {
                HttpContext.Session.SetString("ValidResetCode", model.ResetCode); // Lưu mã vào Session
                return RedirectToAction("ResetPassword");
            }

            ModelState.AddModelError("", "Mã xác nhận không hợp lệ hoặc đã hết hạn.");
            return View(model);
        }




        // GET: Account/ResetPassword
        public IActionResult ResetPassword()
        {
            var resetCode = HttpContext.Session.GetString("ValidResetCode");
            if (string.IsNullOrEmpty(resetCode))
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }     
        /// <summary>
        /// POST: Account/ResetPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resetCode = HttpContext.Session.GetString("ValidResetCode");
            if (string.IsNullOrEmpty(resetCode))
            {
                ModelState.AddModelError("", "Bạn chưa nhập mã xác thực.");
                return View(model);
            }
            var result = await _accountSV.ResetPasswordAsync(resetCode, model.NewPassword);
            if (result)
            {
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Mã xác nhận không hợp lệ hoặc đã hết hạn.");
            return View(model);
        }


        // Phần đăng nhập bằng Facebook
        [HttpGet]   
        public IActionResult SiginFB(string returnUrl = null)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("FacebookLoginCallback", new { ReturnUrl = returnUrl })
            };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }
        /// <summary>
        /// Callback khi đăng nhập bằng Facebook
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> FacebookLoginCallback(string returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result.Succeeded)
            {
                var claims = result.Principal.Claims;
                var facebookId = claims.FirstOrDefault(c => c.Type == "urn:facebook:id")?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                try
                {
                    var user = await _accountSV.FacebookLoginAsync(facebookId, name, email);

                    var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return Redirect(returnUrl ?? "/");
                }
                catch (Exception ex)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return RedirectToAction("ErrorLogin");
                }
            }
            return RedirectToAction("Login");
        }


        // Phần đăng nhập bằng Google
        [HttpGet]
        public IActionResult SiginGG()
        {
            var redirectUrl = Url.Action("GoogleRp", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet]
        public async Task<IActionResult> GoogleRp()
        {

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result.Succeeded)
            {
                var claims = result.Principal.Claims;
                var googleId = claims.FirstOrDefault(c => c.Type == "urn:google:id")?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                try
                {
                    var user = await _accountSV.GoogleLoginAsync(googleId, name, email);

                    var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return RedirectToAction("ErrorLogin");
                }
            }
            return RedirectToAction("Login");
        }


        /// <summary>
        /// Trang thông báo lỗi đăng nhập
        /// </summary>
        /// <returns></returns>
        public IActionResult ErrorLogin()
        {
            return View();
        }

        /// <summary>
        /// Trang thông báo lỗi truy cập
        /// </summary>
        /// <returns></returns>
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
