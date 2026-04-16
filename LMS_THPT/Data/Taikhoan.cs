using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using LMS_THPT.Models;
using LMS_THPT.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS_THPT.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Lấy DbContext và Identity services
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<NguoiDung>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // -----------------------------
            // 1. Tạo các Role
            string[] roles = { "Admin", "GiaoVien", "HocSinh", "HieuTruong" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Tạo Giáo viên (gv@lms.com) - Lấy đối tượng trả về để dùng ID
            var gvUser = await userManager.FindByEmailAsync("gv@lms.com");
            if (gvUser == null)
            {
                gvUser = new NguoiDung { UserName = "gv@lms.com", Email = "gv@lms.com", HoTen = "Nguyễn Văn Giáo", EmailConfirmed = true, IsActive = true, NgayTao = DateTime.Now };
                await userManager.CreateAsync(gvUser, "Teacher@123");
                await userManager.AddToRoleAsync(gvUser, "GiaoVien");
            }

            // 3. Tạo Khối
            if (!context.Khois.Any())
            {
                context.Khois.AddRange(
                    new Khoi { TenKhoi = "Khối 10" },
                    new Khoi { TenKhoi = "Khối 11" },
                    new Khoi { TenKhoi = "Khối 12" }
                );
                await context.SaveChangesAsync();
            }
            var khoi12 = await context.Khois.FirstOrDefaultAsync(k => k.TenKhoi == "Khối 12");

            // 4. Tạo Môn Học
            if (!context.DanhSachMonHoc.Any())
            {
                context.DanhSachMonHoc.Add(new MonHoc { TenMonHoc = "Toán", MoTa = "Toán 12", KhoiId = khoi12.Id, IsActive = true });
                await context.SaveChangesAsync();
            }
            var monToan = await context.DanhSachMonHoc.FirstOrDefaultAsync(m => m.TenMonHoc == "Toán" && m.KhoiId == khoi12.Id);

            // 5. Tạo Lớp học & Gán Chủ nhiệm ngay lập tức
            if (!context.Lops.Any())
            {
                context.Lops.Add(new Lop { TenLop = "12A1", MaKhoi = khoi12.Id, GiaoVienChuNhiemId = gvUser.Id });
                await context.SaveChangesAsync();
            }
            var lop12A1 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "12A1");

            // 6. Gán Giáo viên dạy môn Toán cho lớp 12A1
            if (!context.MonHocGiaoViens.Any(x => x.NguoiDungId == gvUser.Id && x.MonHocId == monToan.Id))
            {
                context.MonHocGiaoViens.Add(new MonHocGiaoVien { NguoiDungId = gvUser.Id, MonHocId = monToan.Id });
                context.LopMonHocs.Add(new LopMonHoc { LopId = lop12A1.Id, MonHocId = monToan.Id });
                await context.SaveChangesAsync();
            }

            // 7. Tạo Học sinh (Gán thẳng vào lớp 12A1)
            var studentCount = await userManager.GetUsersInRoleAsync("HocSinh");
            if (studentCount.Count < 10)
            {
                for (int i = 1; i <= 40; i++)
                {
                    var email = $"student{i:D3}@lms.com";
                    if (await userManager.FindByEmailAsync(email) == null)
                    {
                        var hs = new NguoiDung { UserName = email, Email = email, HoTen = $"Học Sinh {i}", LopId = lop12A1.Id, EmailConfirmed = true, IsActive = true, NgayTao = DateTime.Now };
                        await userManager.CreateAsync(hs, "Student@123");
                        await userManager.AddToRoleAsync(hs, "HocSinh");
                    }
                }
            }

            // 8. Tạo Bài tập (Lấy ID sau khi Save)
            if (!context.BaiTaps.Any(x => x.NguoiDungId == gvUser.Id))
            {
                var baiTap = new BaiTap
                {
                    TieuDe = "Bài tập Giải tích 12",
                    NoiDung = "Làm bài 1, 2 trang 50",
                    NgayTao = DateTime.Now.AddDays(-1),
                    HanNop = DateTime.Now.AddDays(7),
                    MonHocId = monToan.Id,
                    NguoiDungId = gvUser.Id
                };
                context.BaiTaps.Add(baiTap);
                await context.SaveChangesAsync();

                // 9. Tạo Bài nộp (Lấy list học sinh thực tế từ DB)
                var listHS = await context.NguoiDungs.Where(u => u.LopId == lop12A1.Id).Take(15).ToListAsync();
                foreach (var hs in listHS)
                {
                    context.BaiNops.Add(new BaiNop
                    {
                        BaiTapId = baiTap.Id,
                        HocSinhId = hs.Id, // ID thật từ Identity
                        NgayNop = DateTime.Now.AddHours(-2),
                        DuongDanFile = "bt.pdf",
                        TrangThai = TrangThaiBaiNop.DaNop,
                        Diem = null
                    });
                }
                await context.SaveChangesAsync();
            }

            // 10. Tạo Lịch học (Quan trọng: Phải có LopId)
            if (!context.LichHocs.Any(x => x.NgayHoc.Date == DateTime.Today))
            {
                context.LichHocs.Add(new LichHoc
                {
                    TieuDe = "Tiết 1: Toán học",
                    MonHocId = monToan.Id,
                    LopId = lop12A1.Id,
                    NgayHoc = DateTime.Today,
                    GioBatDau = new TimeSpan(7, 30, 0),
                    GioKetThuc = new TimeSpan(9, 0, 0),
                    PhongHoc = "P.301"
                });
                await context.SaveChangesAsync();
            }
        }
    }
}