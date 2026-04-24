using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class FixLichHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Lop_LopId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_BaiGiang_AspNetUsers_NguoiDungId",
                table: "BaiGiang");

            migrationBuilder.DropForeignKey(
                name: "FK_BaiNop_AspNetUsers_HocSinhId",
                table: "BaiNop");

            migrationBuilder.DropForeignKey(
                name: "FK_BaiTap_AspNetUsers_NguoiDungId",
                table: "BaiTap");

            migrationBuilder.DropForeignKey(
                name: "FK_DangKyHoc_AspNetUsers_HocSinhId",
                table: "DangKyHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhSachBinhLuan_AspNetUsers_NguoiDungId",
                table: "DanhSachBinhLuan");

            migrationBuilder.DropForeignKey(
                name: "FK_DiemSo_AspNetUsers_GiangVienId",
                table: "DiemSo");

            migrationBuilder.DropForeignKey(
                name: "FK_DiemSo_AspNetUsers_HocSinhId",
                table: "DiemSo");

            migrationBuilder.DropForeignKey(
                name: "FK_DiemSo_AspNetUsers_NguoiDungId",
                table: "DiemSo");

            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId",
                table: "LichHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_Lop_AspNetUsers_GiaoVienChuNhiemId",
                table: "Lop");

            migrationBuilder.DropForeignKey(
                name: "FK_LopMonHoc_AspNetUsers_GiaoVienId",
                table: "LopMonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_MonHoc_AspNetUsers_GiangVienId",
                table: "MonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_MonHocGiaoViens_AspNetUsers_NguoiDungId",
                table: "MonHocGiaoViens");

            migrationBuilder.DropForeignKey(
                name: "FK_ThongBaos_AspNetUsers_NguoiDangId",
                table: "ThongBaos");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauGiaoVien_AspNetUsers_MaGiaoVien",
                table: "YeuCauGiaoVien");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauGiaoVien_AspNetUsers_NguoiXuLyId",
                table: "YeuCauGiaoVien");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GiaoVien",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "Lop",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "MonHoc",
                table: "LichHoc");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "NguoiDung");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_LopId",
                table: "NguoiDung",
                newName: "IX_NguoiDung_LopId");

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
                name: "GiaoVienId",
                table: "LichHoc",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LopId",
                table: "LichHoc",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonHocId1",
                table: "LichHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_NguoiDung",
                table: "NguoiDung",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_GiaoVienId",
                table: "LichHoc",
                column: "GiaoVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_LopId",
                table: "LichHoc",
                column: "LopId");

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_MonHocId1",
                table: "LichHoc",
                column: "MonHocId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_NguoiDung_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_NguoiDung_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_NguoiDung_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_NguoiDung_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaiGiang_NguoiDung_NguoiDungId",
                table: "BaiGiang",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiNop_NguoiDung_HocSinhId",
                table: "BaiNop",
                column: "HocSinhId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTap_NguoiDung_NguoiDungId",
                table: "BaiTap",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyHoc_NguoiDung_HocSinhId",
                table: "DangKyHoc",
                column: "HocSinhId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhSachBinhLuan_NguoiDung_NguoiDungId",
                table: "DanhSachBinhLuan",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemSo_NguoiDung_GiangVienId",
                table: "DiemSo",
                column: "GiangVienId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemSo_NguoiDung_HocSinhId",
                table: "DiemSo",
                column: "HocSinhId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemSo_NguoiDung_NguoiDungId",
                table: "DiemSo",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_Lop_LopId",
                table: "LichHoc",
                column: "LopId",
                principalTable: "Lop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId",
                table: "LichHoc",
                column: "MonHocId",
                principalTable: "MonHoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId1",
                table: "LichHoc",
                column: "MonHocId1",
                principalTable: "MonHoc",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_NguoiDung_GiaoVienId",
                table: "LichHoc",
                column: "GiaoVienId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lop_NguoiDung_GiaoVienChuNhiemId",
                table: "Lop",
                column: "GiaoVienChuNhiemId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LopMonHoc_NguoiDung_GiaoVienId",
                table: "LopMonHoc",
                column: "GiaoVienId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MonHoc_NguoiDung_GiangVienId",
                table: "MonHoc",
                column: "GiangVienId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MonHocGiaoViens_NguoiDung_NguoiDungId",
                table: "MonHocGiaoViens",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_Lop_LopId",
                table: "NguoiDung",
                column: "LopId",
                principalTable: "Lop",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ThongBaos_NguoiDung_NguoiDangId",
                table: "ThongBaos",
                column: "NguoiDangId",
                principalTable: "NguoiDung",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauGiaoVien_NguoiDung_MaGiaoVien",
                table: "YeuCauGiaoVien",
                column: "MaGiaoVien",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauGiaoVien_NguoiDung_NguoiXuLyId",
                table: "YeuCauGiaoVien",
                column: "NguoiXuLyId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_NguoiDung_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_NguoiDung_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_NguoiDung_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_NguoiDung_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_BaiGiang_NguoiDung_NguoiDungId",
                table: "BaiGiang");

            migrationBuilder.DropForeignKey(
                name: "FK_BaiNop_NguoiDung_HocSinhId",
                table: "BaiNop");

            migrationBuilder.DropForeignKey(
                name: "FK_BaiTap_NguoiDung_NguoiDungId",
                table: "BaiTap");

            migrationBuilder.DropForeignKey(
                name: "FK_DangKyHoc_NguoiDung_HocSinhId",
                table: "DangKyHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhSachBinhLuan_NguoiDung_NguoiDungId",
                table: "DanhSachBinhLuan");

            migrationBuilder.DropForeignKey(
                name: "FK_DiemSo_NguoiDung_GiangVienId",
                table: "DiemSo");

            migrationBuilder.DropForeignKey(
                name: "FK_DiemSo_NguoiDung_HocSinhId",
                table: "DiemSo");

            migrationBuilder.DropForeignKey(
                name: "FK_DiemSo_NguoiDung_NguoiDungId",
                table: "DiemSo");

            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_Lop_LopId",
                table: "LichHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId",
                table: "LichHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId1",
                table: "LichHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_LichHoc_NguoiDung_GiaoVienId",
                table: "LichHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_Lop_NguoiDung_GiaoVienChuNhiemId",
                table: "Lop");

            migrationBuilder.DropForeignKey(
                name: "FK_LopMonHoc_NguoiDung_GiaoVienId",
                table: "LopMonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_MonHoc_NguoiDung_GiangVienId",
                table: "MonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_MonHocGiaoViens_NguoiDung_NguoiDungId",
                table: "MonHocGiaoViens");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_Lop_LopId",
                table: "NguoiDung");

            migrationBuilder.DropForeignKey(
                name: "FK_ThongBaos_NguoiDung_NguoiDangId",
                table: "ThongBaos");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauGiaoVien_NguoiDung_MaGiaoVien",
                table: "YeuCauGiaoVien");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauGiaoVien_NguoiDung_NguoiXuLyId",
                table: "YeuCauGiaoVien");

            migrationBuilder.DropIndex(
                name: "IX_LichHoc_GiaoVienId",
                table: "LichHoc");

            migrationBuilder.DropIndex(
                name: "IX_LichHoc_LopId",
                table: "LichHoc");

            migrationBuilder.DropIndex(
                name: "IX_LichHoc_MonHocId1",
                table: "LichHoc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NguoiDung",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "GiaoVienId",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "LopId",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "MonHocId1",
                table: "LichHoc");

            migrationBuilder.RenameTable(
                name: "NguoiDung",
                newName: "AspNetUsers");

            migrationBuilder.RenameIndex(
                name: "IX_NguoiDung_LopId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_LopId");

            migrationBuilder.AlterColumn<int>(
                name: "MonHocId",
                table: "LichHoc",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.AddColumn<string>(
                name: "MonHoc",
                table: "LichHoc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Lop_LopId",
                table: "AspNetUsers",
                column: "LopId",
                principalTable: "Lop",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaiGiang_AspNetUsers_NguoiDungId",
                table: "BaiGiang",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiNop_AspNetUsers_HocSinhId",
                table: "BaiNop",
                column: "HocSinhId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTap_AspNetUsers_NguoiDungId",
                table: "BaiTap",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyHoc_AspNetUsers_HocSinhId",
                table: "DangKyHoc",
                column: "HocSinhId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhSachBinhLuan_AspNetUsers_NguoiDungId",
                table: "DanhSachBinhLuan",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemSo_AspNetUsers_GiangVienId",
                table: "DiemSo",
                column: "GiangVienId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemSo_AspNetUsers_HocSinhId",
                table: "DiemSo",
                column: "HocSinhId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemSo_AspNetUsers_NguoiDungId",
                table: "DiemSo",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LichHoc_MonHoc_MonHocId",
                table: "LichHoc",
                column: "MonHocId",
                principalTable: "MonHoc",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lop_AspNetUsers_GiaoVienChuNhiemId",
                table: "Lop",
                column: "GiaoVienChuNhiemId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LopMonHoc_AspNetUsers_GiaoVienId",
                table: "LopMonHoc",
                column: "GiaoVienId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MonHoc_AspNetUsers_GiangVienId",
                table: "MonHoc",
                column: "GiangVienId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MonHocGiaoViens_AspNetUsers_NguoiDungId",
                table: "MonHocGiaoViens",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThongBaos_AspNetUsers_NguoiDangId",
                table: "ThongBaos",
                column: "NguoiDangId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauGiaoVien_AspNetUsers_MaGiaoVien",
                table: "YeuCauGiaoVien",
                column: "MaGiaoVien",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauGiaoVien_AspNetUsers_NguoiXuLyId",
                table: "YeuCauGiaoVien",
                column: "NguoiXuLyId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
