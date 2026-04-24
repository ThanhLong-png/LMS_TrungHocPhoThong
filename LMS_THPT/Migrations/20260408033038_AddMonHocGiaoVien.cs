using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddMonHocGiaoVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "FK_BaiNop_NguoiDung_HocSinhId",
                table: "BaiNop");

            migrationBuilder.DropForeignKey(
                name: "FK_DangKyHoc_NguoiDung_HocSinhId",
                table: "DangKyHoc");

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
                name: "FK_Lop_NguoiDung_GiaoVienChuNhiemId",
                table: "Lop");

            migrationBuilder.DropForeignKey(
                name: "FK_LopMonHoc_NguoiDung_GiaoVienId",
                table: "LopMonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_MonHoc_NguoiDung_GiangVienId",
                table: "MonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_Lop_LopId",
                table: "NguoiDung");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NguoiDung",
                table: "NguoiDung");

            migrationBuilder.RenameTable(
                name: "NguoiDung",
                newName: "AspNetUsers");

            migrationBuilder.RenameIndex(
                name: "IX_NguoiDung_LopId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_LopId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MonHocGiaoViens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    LopId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocGiaoViens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonHocGiaoViens_AspNetUsers_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonHocGiaoViens_Lop_LopId",
                        column: x => x.LopId,
                        principalTable: "Lop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MonHocGiaoViens_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonHocGiaoViens_LopId",
                table: "MonHocGiaoViens",
                column: "LopId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocGiaoViens_MonHocId",
                table: "MonHocGiaoViens",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocGiaoViens_NguoiDungId",
                table: "MonHocGiaoViens",
                column: "NguoiDungId");

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
                name: "FK_BaiNop_AspNetUsers_HocSinhId",
                table: "BaiNop",
                column: "HocSinhId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyHoc_AspNetUsers_HocSinhId",
                table: "DangKyHoc",
                column: "HocSinhId",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FK_BaiNop_AspNetUsers_HocSinhId",
                table: "BaiNop");

            migrationBuilder.DropForeignKey(
                name: "FK_DangKyHoc_AspNetUsers_HocSinhId",
                table: "DangKyHoc");

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
                name: "FK_Lop_AspNetUsers_GiaoVienChuNhiemId",
                table: "Lop");

            migrationBuilder.DropForeignKey(
                name: "FK_LopMonHoc_AspNetUsers_GiaoVienId",
                table: "LopMonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_MonHoc_AspNetUsers_GiangVienId",
                table: "MonHoc");

            migrationBuilder.DropTable(
                name: "MonHocGiaoViens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "NguoiDung");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_LopId",
                table: "NguoiDung",
                newName: "IX_NguoiDung_LopId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NguoiDung",
                table: "NguoiDung",
                column: "Id");

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
                name: "FK_BaiNop_NguoiDung_HocSinhId",
                table: "BaiNop",
                column: "HocSinhId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyHoc_NguoiDung_HocSinhId",
                table: "DangKyHoc",
                column: "HocSinhId",
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
                name: "FK_NguoiDung_Lop_LopId",
                table: "NguoiDung",
                column: "LopId",
                principalTable: "Lop",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
