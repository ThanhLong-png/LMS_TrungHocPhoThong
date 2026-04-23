using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_THPT.Migrations
{
    /// <inheritdoc />
    public partial class AddLopMonHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LopMonHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LopId = table.Column<int>(type: "int", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    GiaoVienId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopMonHoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LopMonHoc_Lop_LopId",
                        column: x => x.LopId,
                        principalTable: "Lop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LopMonHoc_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LopMonHoc_NguoiDung_GiaoVienId",
                        column: x => x.GiaoVienId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LopMonHoc_GiaoVienId",
                table: "LopMonHoc",
                column: "GiaoVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LopMonHoc_LopId",
                table: "LopMonHoc",
                column: "LopId");

            migrationBuilder.CreateIndex(
                name: "IX_LopMonHoc_MonHocId",
                table: "LopMonHoc",
                column: "MonHocId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LopMonHoc");
        }
    }
}
