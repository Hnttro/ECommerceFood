using System.ComponentModel.DataAnnotations;

namespace ECommerceFood.Entities
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        [MaxLength(50)]
        public string NameRole { get; set; }

        [MaxLength(200)]
        public string Describe { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
