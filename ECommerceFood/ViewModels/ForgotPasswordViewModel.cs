using System.ComponentModel.DataAnnotations;

namespace ECommerceFood.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập đầy đủ email.")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
