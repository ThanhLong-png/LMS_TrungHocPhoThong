using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddNguoiDungToBaiTap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "BaiTap",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaiTap_NguoiDungId",
                table: "BaiTap",
                column: "NguoiDungId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTap_AspNetUsers_NguoiDungId",
                table: "BaiTap",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiTap_AspNetUsers_NguoiDungId",
                table: "BaiTap");

            migrationBuilder.DropIndex(
                name: "IX_BaiTap_NguoiDungId",
                table: "BaiTap");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "BaiTap");
        }
    }
}
