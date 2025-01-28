using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalBudgetManager.Api.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class UserRolesTableCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleId = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(
                        type: "nvarchar(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleId);
                }
            );

            migrationBuilder.CreateIndex(name: "IX_Users_RoleId", table: "Users", column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRoles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "UserRoles",
                principalColumn: "UserRoleId",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Users_UserRoles_RoleId", table: "Users");

            migrationBuilder.DropTable(name: "UserRoles");

            migrationBuilder.DropIndex(name: "IX_Users_RoleId", table: "Users");

            migrationBuilder.DropColumn(name: "RoleId", table: "Users");
        }
    }
}
