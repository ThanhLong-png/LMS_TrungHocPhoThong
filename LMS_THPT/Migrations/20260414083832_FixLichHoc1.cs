using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class FixLichHoc1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId1",
                table: "LichHoc");

            migrationBuilder.DropIndex(
                name: "IX_LichHoc_MonHocId1",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "MonHocId1",
                table: "LichHoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonHocId1",
                table: "LichHoc",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_MonHocId1",
                table: "LichHoc",
                column: "MonHocId1");

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId1",
                table: "LichHoc",
                column: "MonHocId1",
                principalTable: "MonHoc",
                principalColumn: "Id");
        }
    }
}
