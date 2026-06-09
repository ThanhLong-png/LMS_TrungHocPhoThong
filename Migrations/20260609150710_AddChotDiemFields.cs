using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddChotDiemFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsChotCuoiKy",
                table: "DiemHocKys",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsChotGiuaKy",
                table: "DiemHocKys",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsChotMieng",
                table: "DiemHocKys",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChotCuoiKy",
                table: "DiemHocKys");

            migrationBuilder.DropColumn(
                name: "IsChotGiuaKy",
                table: "DiemHocKys");

            migrationBuilder.DropColumn(
                name: "IsChotMieng",
                table: "DiemHocKys");
        }
    }
}
