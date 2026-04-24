using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveScheduleSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DenTiet",
                table: "YeuCauGiaoVien",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayNghi",
                table: "YeuCauGiaoVien",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TuTiet",
                table: "YeuCauGiaoVien",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DenTiet",
                table: "YeuCauGiaoVien");

            migrationBuilder.DropColumn(
                name: "NgayNghi",
                table: "YeuCauGiaoVien");

            migrationBuilder.DropColumn(
                name: "TuTiet",
                table: "YeuCauGiaoVien");
        }
    }
}
