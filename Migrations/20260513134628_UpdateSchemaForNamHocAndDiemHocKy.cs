using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaForNamHocAndDiemHocKy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NamHoc",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: true);


            migrationBuilder.CreateTable(
                name: "DiemHocKys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocSinhId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    LopId = table.Column<int>(type: "int", nullable: true),
                    NamHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HocKy = table.Column<int>(type: "int", nullable: false),
                    DiemMieng1 = table.Column<double>(type: "float", nullable: true),
                    DiemMieng2 = table.Column<double>(type: "float", nullable: true),
                    DiemMieng3 = table.Column<double>(type: "float", nullable: true),
                    Diem15Phut1 = table.Column<double>(type: "float", nullable: true),
                    Diem15Phut2 = table.Column<double>(type: "float", nullable: true),
                    DiemMotTiet1 = table.Column<double>(type: "float", nullable: true),
                    DiemMotTiet2 = table.Column<double>(type: "float", nullable: true),
                    DiemGiuaKy = table.Column<double>(type: "float", nullable: true),
                    DiemCuoiKy = table.Column<double>(type: "float", nullable: true),
                    DiemTongKet = table.Column<double>(type: "float", nullable: true),
                    XepLoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NhanXet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaoVienId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NgayNhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemHocKys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemHocKys_Lop_LopId",
                        column: x => x.LopId,
                        principalTable: "Lop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DiemHocKys_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiemHocKys_NguoiDung_GiaoVienId",
                        column: x => x.GiaoVienId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DiemHocKys_NguoiDung_HocSinhId",
                        column: x => x.HocSinhId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuHocSinhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaHocSinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenLop = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenKhoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamHocCuoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LyDoXoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayXoa = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiXoaId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiXoaHoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DanhSachDiemJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiDungIdGoc = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuHocSinhs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LichSuDiemHocSinhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LichSuHocSinhId = table.Column<int>(type: "int", nullable: false),
                    TenMonHoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamHoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HocKy = table.Column<int>(type: "int", nullable: false),
                    TenLop = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiemMieng1 = table.Column<double>(type: "float", nullable: true),
                    DiemMieng2 = table.Column<double>(type: "float", nullable: true),
                    DiemMieng3 = table.Column<double>(type: "float", nullable: true),
                    Diem15Phut1 = table.Column<double>(type: "float", nullable: true),
                    Diem15Phut2 = table.Column<double>(type: "float", nullable: true),
                    DiemMotTiet1 = table.Column<double>(type: "float", nullable: true),
                    DiemMotTiet2 = table.Column<double>(type: "float", nullable: true),
                    DiemGiuaKy = table.Column<double>(type: "float", nullable: true),
                    DiemCuoiKy = table.Column<double>(type: "float", nullable: true),
                    DiemTongKet = table.Column<double>(type: "float", nullable: true),
                    XepLoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NhanXet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDiemHocSinhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuDiemHocSinhs_LichSuHocSinhs_LichSuHocSinhId",
                        column: x => x.LichSuHocSinhId,
                        principalTable: "LichSuHocSinhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiemHocKys_GiaoVienId",
                table: "DiemHocKys",
                column: "GiaoVienId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemHocKys_HocSinhId",
                table: "DiemHocKys",
                column: "HocSinhId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemHocKys_LopId",
                table: "DiemHocKys",
                column: "LopId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemHocKys_MonHocId",
                table: "DiemHocKys",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDiemHocSinhs_LichSuHocSinhId",
                table: "LichSuDiemHocSinhs",
                column: "LichSuHocSinhId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiemHocKys");

            migrationBuilder.DropTable(
                name: "LichSuDiemHocSinhs");

            migrationBuilder.DropTable(
                name: "LichSuHocSinhs");

            migrationBuilder.DropColumn(
                name: "NamHoc",
                table: "NguoiDung");
        }
    }
}
