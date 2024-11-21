using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ECommerceFood.Migrations
{
    /// <inheritdoc />
    public partial class Up_Role_User_UR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Describe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PasswordResetCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetCodeExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdFacebook = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdGoogle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AccountStatus = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleID", "Describe", "NameRole" },
                values: new object[,]
                {
                    { 1, "Administrator Role", "Admin" },
                    { 2, "Vai trò người dùng trải nghiệm", "User" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AccountStatus", "Address", "Email", "IdFacebook", "IdGoogle", "IsDeleted", "Name", "Password", "PasswordResetCode", "Phone", "RegistrationDate", "ResetCodeExpiration", "UserName" },
                values: new object[,]
                {
                    { 1, false, null, "admin@gmail.com", null, null, false, "Administrator", "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", null, null, new DateTime(2024, 11, 21, 10, 21, 48, 930, DateTimeKind.Local).AddTicks(1550), null, "admin" },
                    { 2, false, null, "hngoctro@gmail.com", null, null, false, "Huỳnh Ngọc Trợ", "71EwYhTZpjYe4dW0UubSu3DcfruFv54Cw9R0f7V9a+w=", null, null, new DateTime(2024, 11, 21, 10, 21, 48, 930, DateTimeKind.Local).AddTicks(1579), null, "NgocTro" },
                    { 3, false, null, "phucbin366@gmail.com", null, null, false, "Trần Văn Phúc", "71EwYhTZpjYe4dW0UubSu3DcfruFv54Cw9R0f7V9a+w=", null, null, new DateTime(2024, 11, 21, 10, 21, 48, 930, DateTimeKind.Local).AddTicks(1594), null, "VanPhuc" },
                    { 4, false, null, "caothiphuongvy27@gmail.com", null, null, false, "Cao Thị Phương Vy", "71EwYhTZpjYe4dW0UubSu3DcfruFv54Cw9R0f7V9a+w=", null, null, new DateTime(2024, 11, 21, 10, 21, 48, 930, DateTimeKind.Local).AddTicks(1642), null, "PhuongVy" },
                    { 5, false, null, "nguyenngocquy182752@gmail.com", null, null, false, "Nguyễn Thị Ngọc Quý", "71EwYhTZpjYe4dW0UubSu3DcfruFv54Cw9R0f7V9a+w=", null, null, new DateTime(2024, 11, 21, 10, 21, 48, 930, DateTimeKind.Local).AddTicks(1657), null, "NgocQuy" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 3 },
                    { 2, 4 },
                    { 2, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
