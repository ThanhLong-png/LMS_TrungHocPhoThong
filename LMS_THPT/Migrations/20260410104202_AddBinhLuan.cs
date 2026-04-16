using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddBinhLuan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhSachBinhLuan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BaiGiangId = table.Column<int>(type: "int", nullable: true),
                    BaiTapId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhSachBinhLuan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhSachBinhLuan_AspNetUsers_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhSachBinhLuan_BaiGiang_BaiGiangId",
                        column: x => x.BaiGiangId,
                        principalTable: "BaiGiang",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DanhSachBinhLuan_BaiTap_BaiTapId",
                        column: x => x.BaiTapId,
                        principalTable: "BaiTap",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhSachBinhLuan_BaiGiangId",
                table: "DanhSachBinhLuan",
                column: "BaiGiangId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhSachBinhLuan_BaiTapId",
                table: "DanhSachBinhLuan",
                column: "BaiTapId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhSachBinhLuan_NguoiDungId",
                table: "DanhSachBinhLuan",
                column: "NguoiDungId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhSachBinhLuan");
        }
    }
}
