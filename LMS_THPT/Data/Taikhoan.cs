using Microsoft.AspNetCore.Identity;

namespace LMS.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Tạo các Role
            string[] roles = { "Admin", "Teacher", "Student", "Principal" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Tạo tài khoản Admin
            await CreateUser(userManager,
                email: "admin@lms.com",
                password: "Admin@123",
                role: "Admin");

            // Tạo tài khoản Giảng viên
            await CreateUser(userManager,
                email: "teacher@lms.com",
                password: "Teacher@123",
                role: "Teacher");

            // Tạo tài khoản Học sinh
            await CreateUser(userManager,
                email: "student@lms.com",
                password: "Student@123",
                role: "Student");

            // Tạo tài khoản Hiệu trưởng
            await CreateUser(userManager,
                email: "principal@lms.com",
                password: "Principal@123",
                role: "Principal");
        }

        private static async Task CreateUser(UserManager<IdentityUser> userManager,
            string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true  // bỏ qua xác nhận email
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}