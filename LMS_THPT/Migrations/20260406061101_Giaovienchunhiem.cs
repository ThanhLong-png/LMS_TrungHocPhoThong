using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class Giaovienchunhiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GiaoVienChuNhiemId",
                table: "Lop",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lop_GiaoVienChuNhiemId",
                table: "Lop",
                column: "GiaoVienChuNhiemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lop_NguoiDung_GiaoVienChuNhiemId",
                table: "Lop",
                column: "GiaoVienChuNhiemId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lop_NguoiDung_GiaoVienChuNhiemId",
                table: "Lop");

            migrationBuilder.DropIndex(
                name: "IX_Lop_GiaoVienChuNhiemId",
                table: "Lop");

            migrationBuilder.DropColumn(
                name: "GiaoVienChuNhiemId",
                table: "Lop");
        }
    }
}
