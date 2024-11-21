using ECommerceFood.Services.Email;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System;
using ECommerceFood.Models;
using Microsoft.EntityFrameworkCore;
using ECommerceFood.Entities;
using ECommerceFood.Services.PaswordH;

namespace ECommerceFood.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        public AccountService(AppDbContext context, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }


        // Kiểm tra Tên đăng nhập có tồn tại chưa
        public async Task<bool> UserNameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username);
        }

        // Kiểm tra Email đã tồn tại chưa
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        // Phần Kiểm tra mật khẩu
        private bool IsValidPassword(string password)
        {

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$");
            return regex.IsMatch(password);
        }


        // Phần đăng Nhập
        public async Task<(User? user, List<string> roles, string? errorMessage)> LoginAsync(string usernameOrEmail, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                return (null, new List<string>(), "Thông tin đăng nhập không hợp lệ.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => (u.UserName == usernameOrEmail || u.Email == usernameOrEmail));

            if (user != null)
            {
                // Kiểm tra trạng thái tài khoản
                if (user.IsDeleted)
                {
                    return (null, new List<string>(), "Tài khoản không tồn tại.");
                }

                if (user.AccountStatus) // Nếu AccountStatus là true thì tài khoản đã bị khóa
                {
                    return (null, new List<string>(), "Tài khoản đã bị khóa.");
                }

                // Kiểm tra mật khẩu
                if (PasswordHasher.VerifyPassword(user.Password, password))
                {
                    var roles = await _context.UserRoles
                        .Where(ur => ur.UserId == user.UserID)
                        .Select(ur => _context.Roles.FirstOrDefault(r => r.RoleID == ur.RoleId).NameRole)
                        .ToListAsync();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Email, user.Email)
                    };
                    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = rememberMe,
                        ExpiresUtc = rememberMe ? DateTime.UtcNow.AddDays(14) : DateTime.UtcNow.AddMinutes(20)
                    };

                    // Đăng nhập vào hệ thống
                    await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return (user, roles, string.Empty);
                }
            }

            return (null, new List<string>(), "Tài khoản hoặc mật khẩu sai.");
        }



        // Phần đăng Ký
        public async Task<bool> RegisterAsync(User user, string role)
        {
            // Kiểm tra xem tên người dùng đã tồn tại chưa
            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                return false;
            }

            // Kiểm tra xem email đã tồn tại chưa
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return false;
            }

            // Kiểm tra độ mạnh của mật khẩu
            if (!IsValidPassword(user.Password))
            {

                return false;
            }

            // Mã hóa mật khẩu
            user.Password = PasswordHasher.HashPassword(user.Password);

            // Thêm người dùng mới
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Gán vai trò cho người dùng mới
            var userRole = new UserRole
            {
                UserId = user.UserID,
                RoleId = _context.Roles.First(r => r.NameRole == role).RoleID
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return true;
        }


        // Đăng xuất , xóa đi phần Session Cookie
        public async Task Logout()
        {
            if (_httpContextAccessor != null)
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        }

        // Phương thức gửi mã xác nhận
        public async Task<bool> SendPasswordResetCodeAsync(string email)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }
            // Kiểm tra mã đã tồn tại và chưa hết hạn
            //if (!string.IsNullOrEmpty(user.PasswordResetCode) && user.ResetCodeExpiration > DateTime.UtcNow)
            //{
            //    return false;
            //}
            var resetCode = GenerateResetCode();
            user.PasswordResetCode = resetCode;
            user.ResetCodeExpiration = DateTime.UtcNow.AddMinutes(1); // 1 phút
            await _context.SaveChangesAsync();

            var subject = "Yêu cầu đặt lại mật khẩu";
            var message = $@"
            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; padding: 20px;'>
                <h2 style='color: #4CAF50; text-align: center;'>Yêu cầu đặt lại mật khẩu</h2>
                <p>Xin chào <strong>{user.Name}</strong>,</p>
                <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình. Dưới đây là mã xác nhận của bạn:</p>
                <div style='text-align: center; margin: 20px 0;'>
                    <div style='border: 2px solid #4CAF50; display: inline-block; padding: 15px 20px; font-size: 24px; font-weight: bold; background: #f9f9f9; border-radius: 8px;' id='resetCode'>
                        {resetCode}
                    </div>
                    <br>
              
                </div>
                <p style='font-size: 14px; color: #666;'>Mã sẽ hết hạn sau <strong>1 phút</strong>. Nếu bạn không yêu cầu điều này, hãy bỏ qua email này và mật khẩu của bạn sẽ không thay đổi.</p>
                <p style='text-align: center; margin-top: 20px; font-size: 14px; color: #aaa;'>Trân trọng,<br/>Đội ngũ hỗ trợ</p>
               
            </div>";


            try
            {
                await _emailService.SendEmailAsync(email, subject, message);
                return true;
            }
            catch (SmtpFailedRecipientException)
            {
                return false; 
            }
            catch (Exception)
            {
                return false; 
            }
        }
       


        // Kiểm tra mã xác nhận
        public async Task<bool> VerifyResetCodeAsync(string resetCode)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.PasswordResetCode == resetCode);
            return user != null && user.ResetCodeExpiration >= DateTime.UtcNow;
        }


        // Phương thức tạo mã xác nhận
        private string GenerateResetCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6);
        }


        // Phương thức xác thực mã và đổi mật khẩu
        public async Task<bool> ResetPasswordAsync(string resetCode, string newPassword)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.PasswordResetCode == resetCode);
            if (user == null )
            {
                return false;
            }
            if (!IsValidPassword(newPassword))
            {
                return false;
            }

            user.Password = PasswordHasher.HashPassword(newPassword);
            user.PasswordResetCode = null;
            user.ResetCodeExpiration = null;
            await _context.SaveChangesAsync();

            return true;
        }


        // Phương thức đăng nhập bằng Facebook
        public async Task<User> FacebookLoginAsync(string facebookId, string name, string email)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Name = name,
                    Email = email,
                    UserName = email,
                    Password = "",
                    RegistrationDate = DateTime.Now,
                    IdFacebook = facebookId

                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var userRole = new UserRole { UserId = user.UserID, RoleId = 2 };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(user.Password) || !string.IsNullOrEmpty(user.IdGoogle))
                {
                    throw new Exception("Email này đã được đăng ký. Vui lòng sử dụng phương thức đăng nhập thông thường.");
                }
                user.Name = name;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            return user;
        }


        // Phương thức đăng nhập bằng Google
        public async Task<User> GoogleLoginAsync(string googleId, string name, string email)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Name = name,
                    Email = email,
                    UserName = email,
                    Password = "",
                    RegistrationDate = DateTime.Now,
                    IdGoogle = googleId

                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var userRole = new UserRole { UserId = user.UserID, RoleId = 2 };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(user.Password) || !string.IsNullOrEmpty(user.IdFacebook))
                {
                    throw new Exception("Email này đã được đăng ký. Vui lòng sử dụng phương thức đăng nhập khác.");
                }
                user.Name = name;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            return user;
        }
    }
}
