using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddLopIdAndTracNghiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LopId",
                table: "BaiTap",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkTracNghiem",
                table: "BaiGiang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LopId",
                table: "BaiGiang",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TinhTienDo",
                table: "BaiGiang",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_BaiTap_LopId",
                table: "BaiTap",
                column: "LopId");

            migrationBuilder.CreateIndex(
                name: "IX_BaiGiang_LopId",
                table: "BaiGiang",
                column: "LopId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiGiang_Lop_LopId",
                table: "BaiGiang",
                column: "LopId",
                principalTable: "Lop",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTap_Lop_LopId",
                table: "BaiTap",
                column: "LopId",
                principalTable: "Lop",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiGiang_Lop_LopId",
                table: "BaiGiang");

            migrationBuilder.DropForeignKey(
                name: "FK_BaiTap_Lop_LopId",
                table: "BaiTap");

            migrationBuilder.DropIndex(
                name: "IX_BaiTap_LopId",
                table: "BaiTap");

            migrationBuilder.DropIndex(
                name: "IX_BaiGiang_LopId",
                table: "BaiGiang");

            migrationBuilder.DropColumn(
                name: "LopId",
                table: "BaiTap");

            migrationBuilder.DropColumn(
                name: "LinkTracNghiem",
                table: "BaiGiang");

            migrationBuilder.DropColumn(
                name: "LopId",
                table: "BaiGiang");

            migrationBuilder.DropColumn(
                name: "TinhTienDo",
                table: "BaiGiang");
        }
    }
}
