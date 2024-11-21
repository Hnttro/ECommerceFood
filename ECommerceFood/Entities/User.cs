using System.ComponentModel.DataAnnotations;

namespace ECommerceFood.Entities
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(200)]

        public string? Address { get; set; }


        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(15)]

        public string? Phone { get; set; }


        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }
        public string? PasswordResetCode { get; set; }
        public DateTime? ResetCodeExpiration { get; set; }

        [MaxLength(50)]
        public string? IdFacebook { get; set; }
        [MaxLength(50)]
        public string? IdGoogle { get; set; }
        public bool IsDeleted { get; set; }
        public bool AccountStatus { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
