using System.ComponentModel.DataAnnotations;

namespace ECommerceFood.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập đầy đủ họ và tên")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tài khoản đã được sử dụng. Vui lòng sử dụng tên khác.")]
        [MaxLength(50, ErrorMessage = "Tài khoản  không được vượt quá 50 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu phải có ít nhất 8 kí tự, gồm 1 chữ cái in hoa, 1 chữ thường và 1 số. ")]
        [MaxLength(100, ErrorMessage = "Mật khẩu  không được vượt quá 50 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
