using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class Unique_MonHoc_Lop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonHocGiaoViens_MonHocId",
                table: "MonHocGiaoViens");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocGiaoViens_MonHocId_LopId",
                table: "MonHocGiaoViens",
                columns: new[] { "MonHocId", "LopId" },
                unique: true,
                filter: "[LopId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonHocGiaoViens_MonHocId_LopId",
                table: "MonHocGiaoViens");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocGiaoViens_MonHocId",
                table: "MonHocGiaoViens",
                column: "MonHocId");
        }
    }
}
