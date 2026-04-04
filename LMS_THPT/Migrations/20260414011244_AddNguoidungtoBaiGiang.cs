using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddNguoidungtoBaiGiang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "BaiGiang",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaiGiang_NguoiDungId",
                table: "BaiGiang",
                column: "NguoiDungId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiGiang_AspNetUsers_NguoiDungId",
                table: "BaiGiang",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiGiang_AspNetUsers_NguoiDungId",
                table: "BaiGiang");

            migrationBuilder.DropIndex(
                name: "IX_BaiGiang_NguoiDungId",
                table: "BaiGiang");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "BaiGiang");
        }
    }
}
