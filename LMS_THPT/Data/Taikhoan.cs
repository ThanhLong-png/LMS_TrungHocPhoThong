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

            // -----------------------------
            // 2. Tạo tài khoản mẫu
            await CreateUser(userManager, "admin@lms.com", "Admin@123", "Admin", "Quản trị viên");
            await CreateUser(userManager, "gv@lms.com", "Teacher@123", "GiaoVien", "Nguyễn Văn Giáo");
            await CreateUser(userManager, "hs@lms.com", "Student@123", "HocSinh", "Trần Văn Học");
            await CreateUser(userManager, "ht@lms.com", "Principal@123", "HieuTruong", "Lê Thầy Hiệu");

            // -----------------------------
            // 3. Tạo 3 Khối mặc định nếu chưa có
            if (!context.Khois.Any())
            {
                var khois = new List<Khoi>
                {
                    new Khoi { TenKhoi = "Khối 10" },
                    new Khoi { TenKhoi = "Khối 11" },
                    new Khoi { TenKhoi = "Khối 12" }
                };

                context.Khois.AddRange(khois);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateUser(
            UserManager<NguoiDung> userManager,
            string email,
            string password,
            string role,
            string hoTen)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new NguoiDung
                {
                    UserName = email,
                    Email = email,
                    HoTen = hoTen,
                    EmailConfirmed = true,
                    IsActive = true,
                    NgayTao = DateTime.Now
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}