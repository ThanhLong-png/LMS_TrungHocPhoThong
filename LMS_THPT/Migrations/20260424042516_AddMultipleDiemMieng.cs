using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleDiemMieng : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DiemMieng2",
                table: "DiemSo",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DiemMieng3",
                table: "DiemSo",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DiemMieng4",
                table: "DiemSo",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiemMieng2",
                table: "DiemSo");

            migrationBuilder.DropColumn(
                name: "DiemMieng3",
                table: "DiemSo");

            migrationBuilder.DropColumn(
                name: "DiemMieng4",
                table: "DiemSo");
        }
    }
}
