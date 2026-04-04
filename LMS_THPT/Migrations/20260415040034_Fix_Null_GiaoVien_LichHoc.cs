using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Null_GiaoVien_LichHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_NguoiDung_GiaoVienId",
                table: "LichHoc");

            migrationBuilder.AlterColumn<string>(
                name: "GiaoVienId",
                table: "LichHoc",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_NguoiDung_GiaoVienId",
                table: "LichHoc",
                column: "GiaoVienId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_NguoiDung_GiaoVienId",
                table: "LichHoc");

            migrationBuilder.AlterColumn<string>(
                name: "GiaoVienId",
                table: "LichHoc",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_NguoiDung_GiaoVienId",
                table: "LichHoc",
                column: "GiaoVienId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
