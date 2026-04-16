using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddLichhoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "GioBatDau",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "GioKetThuc",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "LichHoc");

            migrationBuilder.RenameColumn(
                name: "TieuDe",
                table: "LichHoc",
                newName: "MonHoc");

            migrationBuilder.AlterColumn<string>(
                name: "PhongHoc",
                table: "LichHoc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MonHocId",
                table: "LichHoc",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CaHoc",
                table: "LichHoc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GiaoVien",
                table: "LichHoc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Lop",
                table: "LichHoc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TietHoc",
                table: "LichHoc",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId",
                table: "LichHoc",
                column: "MonHocId",
                principalTable: "MonHoc",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "CaHoc",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "GiaoVien",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "Lop",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "TietHoc",
                table: "LichHoc");

            migrationBuilder.RenameColumn(
                name: "MonHoc",
                table: "LichHoc",
                newName: "TieuDe");

            migrationBuilder.AlterColumn<string>(
                name: "PhongHoc",
                table: "LichHoc",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "MonHocId",
                table: "LichHoc",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "LichHoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "GioBatDau",
                table: "LichHoc",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "GioKetThuc",
                table: "LichHoc",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "LichHoc",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId",
                table: "LichHoc",
                column: "MonHocId",
                principalTable: "MonHoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
