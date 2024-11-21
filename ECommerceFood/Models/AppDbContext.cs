using ECommerceFood.Entities;
using ECommerceFood.Services.PaswordH;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerceFood.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //  Thiết Lập khóa cho Bảng            ROLE - USER - USERROLE
            modelBuilder.Entity<UserRole>()
           .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
            // Seed dữ liệu cho bảng Role
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, NameRole = "Admin", Describe = "Administrator Role" },
                new Role { RoleID = 2, NameRole = "User", Describe = "Vai trò người dùng trải nghiệm" }


            );
            // Seed dữ liệu cho bảng User 
            modelBuilder.Entity<User>().HasData(

                new User { UserID = 1, Name = "Administrator", Email = "admin@gmail.com", UserName = "admin", Password = PasswordHasher.HashPassword("admin"), RegistrationDate = DateTime.Now },
                new User { UserID = 2, Name = "Huỳnh Ngọc Trợ", Email = "hngoctro@gmail.com", UserName = "NgocTro", Password = PasswordHasher.HashPassword("1234qwer"), RegistrationDate = DateTime.Now },
                new User { UserID = 3, Name = "Trần Văn Phúc", Email = "phucbin366@gmail.com", UserName = "VanPhuc", Password = PasswordHasher.HashPassword("1234qwer"), RegistrationDate = DateTime.Now },
                new User { UserID = 4, Name = "Cao Thị Phương Vy", Email = "caothiphuongvy27@gmail.com", UserName = "PhuongVy", Password = PasswordHasher.HashPassword("1234qwer"), RegistrationDate = DateTime.Now },
                new User { UserID = 5, Name = "Nguyễn Thị Ngọc Quý", Email = "nguyenngocquy182752@gmail.com", UserName = "NgocQuy", Password = PasswordHasher.HashPassword("1234qwer"), RegistrationDate = DateTime.Now }

            );

            // Seed dữ liệu cho bảng UserRole
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 },
                new UserRole { UserId = 2, RoleId = 1 },
                new UserRole { UserId = 3, RoleId = 2 },
                new UserRole { UserId = 4, RoleId = 2 },
                new UserRole { UserId = 5, RoleId = 2 }

            );

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
       
    }
}
