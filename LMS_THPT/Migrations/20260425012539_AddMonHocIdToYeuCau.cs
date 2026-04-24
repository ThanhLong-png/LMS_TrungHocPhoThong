using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddMonHocIdToYeuCau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonHocId",
                table: "YeuCauGiaoVien",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauGiaoVien_MonHocId",
                table: "YeuCauGiaoVien",
                column: "MonHocId");

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauGiaoVien_MonHoc_MonHocId",
                table: "YeuCauGiaoVien",
                column: "MonHocId",
                principalTable: "MonHoc",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauGiaoVien_MonHoc_MonHocId",
                table: "YeuCauGiaoVien");

            migrationBuilder.DropIndex(
                name: "IX_YeuCauGiaoVien_MonHocId",
                table: "YeuCauGiaoVien");

            migrationBuilder.DropColumn(
                name: "MonHocId",
                table: "YeuCauGiaoVien");
        }
    }
}
