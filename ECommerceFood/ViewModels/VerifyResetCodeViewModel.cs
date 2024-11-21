using System.ComponentModel.DataAnnotations;

namespace ECommerceFood.ViewModels
{
    public class VerifyResetCodeViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã xác nhận")]
        [Display(Name = "Mã xác nhận")]
        public string ResetCode { get; set; }
    }
}
