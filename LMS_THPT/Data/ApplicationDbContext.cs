using LMS_THPT.Models;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS_THPT.Data
{
    public class ApplicationDbContext : IdentityDbContext<NguoiDung>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Các bảng
        public DbSet<MonHoc> DanhSachMonHoc { get; set; }
        public DbSet<BaiGiang> DanhSachBaiGiang { get; set; }
        public DbSet<TaiLieu> DanhSachTaiLieu { get; set; }
        public DbSet<BaiTap> DanhSachBaiTap { get; set; }
        public DbSet<BaiNop> DanhSachBaiNop { get; set; }
        public DbSet<DangKyHoc> DanhSachDangKy { get; set; }
        public DbSet<DiemSo> DanhSachDiemSo { get; set; }
        public DbSet<LichHoc> LichHocs { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<BinhLuan> DanhSachBinhLuan { get; set; }
        
        // Bảng quản lý học sinh
        public DbSet<Khoi> Khois { get; set; }
        public DbSet<Lop> Lops { get; set; }
        // Trong class AppDbContext
        public DbSet<YeuCauGiaoVien> YeuCauGiaoVien { get; set; }
        public DbSet<LopMonHoc> LopMonHocs { get; set; }
        public DbSet<MonHocGiaoVien> MonHocGiaoViens { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- Đặt tên bảng ---
            builder.Entity<NguoiDung>().ToTable("NguoiDung");
            builder.Entity<MonHoc>().ToTable("MonHoc");
            builder.Entity<BaiGiang>().ToTable("BaiGiang");
            builder.Entity<TaiLieu>().ToTable("TaiLieu");
            builder.Entity<BaiTap>().ToTable("BaiTap");
            builder.Entity<BaiNop>().ToTable("BaiNop");
            builder.Entity<DangKyHoc>().ToTable("DangKyHoc");
            builder.Entity<DiemSo>().ToTable("DiemSo");
            builder.Entity<LichHoc>().ToTable("LichHoc");
            builder.Entity<Khoi>().ToTable("Khoi");
            builder.Entity<Lop>().ToTable("Lop");
            builder.Entity<LopMonHoc>().ToTable("LopMonHoc");

            // --- Quan hệ DiemSo ---
            builder.Entity<DiemSo>()
                .HasOne(d => d.HocSinh)
                .WithMany()
                .HasForeignKey(d => d.HocSinhId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DiemSo>()
                .HasOne(d => d.GiangVien)
                .WithMany()
                .HasForeignKey(d => d.GiangVienId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Quan hệ MonHoc - GiangVien ---
            builder.Entity<MonHoc>()
                .HasOne(m => m.GiangVien)
                .WithMany()
                .HasForeignKey(m => m.GiangVienId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- Quan hệ Khối - Lớp ---
            builder.Entity<Lop>()
                .HasOne(l => l.Khoi)
                .WithMany(k => k.Lops)
                .HasForeignKey(l => l.MaKhoi)
                .OnDelete(DeleteBehavior.Cascade); // giữ cascade

            // --- Quan hệ Lớp - Học sinh (NguoiDung) ---
            builder.Entity<NguoiDung>()
                .HasOne(u => u.Lop)
                .WithMany(l => l.HocSinhs)
                .HasForeignKey(u => u.LopId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- Quan hệ Lớp - GVCN ---
            builder.Entity<Lop>()
                .HasOne(l => l.GiaoVienChuNhiem)
                .WithMany()
                .HasForeignKey(l => l.GiaoVienChuNhiemId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- Quan hệ Lớp - Môn học (LopMonHoc) ---
            builder.Entity<LopMonHoc>()
                .HasOne(lm => lm.Lop)
                .WithMany(l => l.LopMonHocs)
                .HasForeignKey(lm => lm.LopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LopMonHoc>()
                .HasOne(lm => lm.MonHoc)
                .WithMany(m => m.LopMonHocs)
                .HasForeignKey(lm => lm.MonHocId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LopMonHoc>()
                .HasOne(lm => lm.GiaoVien)
                .WithMany()
                .HasForeignKey(lm => lm.GiaoVienId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- Quan hệ MonHoc - Khoi ---
            builder.Entity<MonHoc>()
                .HasOne(m => m.Khoi)
                .WithMany(k => k.MonHocs)
                .HasForeignKey(m => m.KhoiId)
                .OnDelete(DeleteBehavior.Restrict);
           

            builder.Entity<MonHocGiaoVien>()
                .HasOne(mg => mg.GiaoVien)
                .WithMany()
                .HasForeignKey(mg => mg.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<LichHoc>(entity =>
            {
                entity.HasOne(x => x.Lop)
                    .WithMany()
                    .HasForeignKey(x => x.LopId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.MonHoc)
                    .WithMany(m => m.LichHocs)
                    .HasForeignKey(x => x.MonHocId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.GiaoVien)
    .WithMany()
    .HasForeignKey(x => x.GiaoVienId)
    .HasPrincipalKey(x => x.Id)
    .OnDelete(DeleteBehavior.SetNull); // ✅ sửa ở đây
            });
            builder.Entity<MonHocGiaoVien>()
                .HasOne(mg => mg.MonHoc)
                .WithMany(m => m.MonHocGiaoViens)
                .HasForeignKey(mg => mg.MonHocId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MonHocGiaoVien>()
                .HasOne(mg => mg.Lop)
                .WithMany()
                .HasForeignKey(mg => mg.LopId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<YeuCauGiaoVien>(entity =>
            {
                // Quan hệ với GiaoVien (người gửi)
                entity.HasOne(y => y.GiaoVien)
                      .WithMany()
                      .HasForeignKey(y => y.MaGiaoVien)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ với NguoiXuLy (Admin/HieuTruong duyệt)
                entity.HasOne(y => y.NguoiXuLy)
                      .WithMany()
                      .HasForeignKey(y => y.NguoiXuLyId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
                // ❗ THÊM Ở ĐÂY
                builder.Entity<MonHocGiaoVien>()
                    .HasIndex(m => new { m.MonHocId, m.LopId })
                    .IsUnique();
            });// KHÔNG cascade, fix lỗi multiple cascade paths
        }
    }
}