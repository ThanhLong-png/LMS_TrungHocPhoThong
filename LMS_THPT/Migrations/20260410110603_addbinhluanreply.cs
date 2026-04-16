using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class addbinhluanreply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "DanhSachBinhLuan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhSachBinhLuan_ParentId",
                table: "DanhSachBinhLuan",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhSachBinhLuan_DanhSachBinhLuan_ParentId",
                table: "DanhSachBinhLuan",
                column: "ParentId",
                principalTable: "DanhSachBinhLuan",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhSachBinhLuan_DanhSachBinhLuan_ParentId",
                table: "DanhSachBinhLuan");

            migrationBuilder.DropIndex(
                name: "IX_DanhSachBinhLuan_ParentId",
                table: "DanhSachBinhLuan");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "DanhSachBinhLuan");
        }
    }
}
