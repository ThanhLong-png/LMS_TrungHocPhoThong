using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddThongBao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThongBaos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiDangId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HienThi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongBaos_AspNetUsers_NguoiDangId",
                        column: x => x.NguoiDangId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_NguoiDangId",
                table: "ThongBaos",
                column: "NguoiDangId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThongBaos");
        }
    }
}
