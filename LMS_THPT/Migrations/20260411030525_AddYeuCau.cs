using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddYeuCau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YeuCauGiaoVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGiaoVien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiYeuCau = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayXuLy = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChuAdmin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiXuLyId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauGiaoVien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YeuCauGiaoVien_AspNetUsers_MaGiaoVien",
                        column: x => x.MaGiaoVien,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YeuCauGiaoVien_AspNetUsers_NguoiXuLyId",
                        column: x => x.NguoiXuLyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauGiaoVien_MaGiaoVien",
                table: "YeuCauGiaoVien",
                column: "MaGiaoVien");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauGiaoVien_NguoiXuLyId",
                table: "YeuCauGiaoVien",
                column: "NguoiXuLyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YeuCauGiaoVien");
        }
    }
}
