using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Collistable.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGithubAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_GoogleSub",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "GoogleSub",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "GithubId",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GithubId",
                table: "Users",
                column: "GithubId",
                unique: true,
                filter: "[GithubId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleSub",
                table: "Users",
                column: "GoogleSub",
                unique: true,
                filter: "[GoogleSub] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_GithubId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_GoogleSub",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GithubId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "GoogleSub",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleSub",
                table: "Users",
                column: "GoogleSub",
                unique: true);
        }
    }
}
