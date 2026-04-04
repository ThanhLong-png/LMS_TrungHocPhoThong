using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class addthu1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaHoc",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "NgayHoc",
                table: "LichHoc");

            migrationBuilder.AddColumn<int>(
                name: "Thu",
                table: "LichHoc",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Thu",
                table: "LichHoc");

            migrationBuilder.AddColumn<string>(
                name: "CaHoc",
                table: "LichHoc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHoc",
                table: "LichHoc",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
