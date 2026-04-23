using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class FixMultipleCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KhoiId",
                table: "MonHoc",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_KhoiId",
                table: "MonHoc",
                column: "KhoiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonHoc_Khoi_KhoiId",
                table: "MonHoc",
                column: "KhoiId",
                principalTable: "Khoi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonHoc_Khoi_KhoiId",
                table: "MonHoc");

            migrationBuilder.DropIndex(
                name: "IX_MonHoc_KhoiId",
                table: "MonHoc");

            migrationBuilder.DropColumn(
                name: "KhoiId",
                table: "MonHoc");
        }
    }
}
