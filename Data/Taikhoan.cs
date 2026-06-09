using Microsoft.AspNetCore.Identity;
using LMS_THPT.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS_THPT.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<NguoiDung>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Nhanh chóng bỏ qua nếu đã có tài khoản (đặc biệt tránh vòng lặp kiểm tra mật khẩu nặng ở cuối)
            if (await context.Users.AnyAsync())
            {
                await AlignLichHocGiaoVien(context);
                return;
            }

            // ===================== ROLES =====================
            string[] roles = { "Admin", "GiaoVien", "HocSinh", "HieuTruong" };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // ===================== USERS (STAFF) =====================
            var admin = await CreateUser(userManager, "admin@lms.com", "Admin@123", "Admin", "Quản Trị Viên", chucVu: "Quản trị");
            var hieuTruong = await CreateUser(userManager, "ht@lms.com", "Hieut@123", "HieuTruong", "Lê Minh Hiệu", chucVu: "Hiệu trưởng");
            // ===================== 30 GIÁO VIÊN =====================
            // Toán
            var gv1 = await CreateUser(userManager, "gv1@lms.com", "GV001", "GiaoVien", "Nguyễn Văn An",
                chuyenMon: "Toán học", chucVu: "Giáo viên", userName: "GV001", gioiTinh: "Nam");
            // Ngữ văn
            var gv2 = await CreateUser(userManager, "gv2@lms.com", "GV002", "GiaoVien", "Trần Thị Bình",
                chuyenMon: "Ngữ văn", chucVu: "Tổ trưởng chuyên môn", userName: "GV002", gioiTinh: "Nữ");
            // Tiếng Anh
            var gv3 = await CreateUser(userManager, "gv3@lms.com", "GV003", "GiaoVien", "Lê Văn Cường",
                chuyenMon: "Tiếng Anh", chucVu: "Giáo viên", userName: "GV003", gioiTinh: "Nam");
            // Vật lý
            var gv4 = await CreateUser(userManager, "gv4@lms.com", "GV004", "GiaoVien", "Phạm Thị Dung",
                chuyenMon: "Vật lý", chucVu: "Giáo viên", userName: "GV004", gioiTinh: "Nữ");
            // Hóa học
            var gv5 = await CreateUser(userManager, "gv5@lms.com", "GV005", "GiaoVien", "Hoàng Văn Em",
                chuyenMon: "Hóa học", chucVu: "Giáo viên", userName: "GV005", gioiTinh: "Nam");
            // Sinh học
            var gv6 = await CreateUser(userManager, "gv6@lms.com", "GV006", "GiaoVien", "Vũ Thị Phương",
                chuyenMon: "Sinh học", chucVu: "Giáo viên", userName: "GV006", gioiTinh: "Nữ");
            // Lịch sử - Địa lý
            var gv7 = await CreateUser(userManager, "gv7@lms.com", "GV007", "GiaoVien", "Đỗ Minh Khoa",
                chuyenMon: "Lịch sử - Địa lý", chucVu: "Giáo viên", userName: "GV007", gioiTinh: "Nam");
            // Tin học
            var gv8 = await CreateUser(userManager, "gv8@lms.com", "GV008", "GiaoVien", "Bùi Thị Lan",
                chuyenMon: "Tin học", chucVu: "Giáo viên", userName: "GV008", gioiTinh: "Nữ");
            // GDCD - Thể dục
            var gv9 = await CreateUser(userManager, "gv9@lms.com", "GV009", "GiaoVien", "Ngô Quốc Hùng",
                chuyenMon: "GDCD - Thể dục", chucVu: "Giáo viên", userName: "GV009", gioiTinh: "Nam");
            // Toán (thứ 2)
            var gv10 = await CreateUser(userManager, "gv10@lms.com", "GV010", "GiaoVien", "Lý Thị Hoa",
                chuyenMon: "Toán học", chucVu: "Tổ phó chuyên môn", userName: "GV010", gioiTinh: "Nữ");

            // GIÁO VIÊN BỔ SUNG (11 - 30) để đủ mỗi môn 3 giáo viên
            var gv11 = await CreateUser(userManager, "gv11@lms.com", "GV011", "GiaoVien", "Phan Thanh Hải", chuyenMon: "Toán học", chucVu: "Giáo viên", userName: "GV011", gioiTinh: "Nam");
            
            var gv12 = await CreateUser(userManager, "gv12@lms.com", "GV012", "GiaoVien", "Trịnh Xuân Trí", chuyenMon: "Ngữ văn", chucVu: "Giáo viên", userName: "GV012", gioiTinh: "Nam");
            var gv13 = await CreateUser(userManager, "gv13@lms.com", "GV013", "GiaoVien", "Nguyễn Thu Hà", chuyenMon: "Ngữ văn", chucVu: "Giáo viên", userName: "GV013", gioiTinh: "Nữ");
            
            var gv14 = await CreateUser(userManager, "gv14@lms.com", "GV014", "GiaoVien", "Lê Bảo Ngọc", chuyenMon: "Tiếng Anh", chucVu: "Giáo viên", userName: "GV014", gioiTinh: "Nữ");
            var gv15 = await CreateUser(userManager, "gv15@lms.com", "GV015", "GiaoVien", "Hoàng Kim Chi", chuyenMon: "Tiếng Anh", chucVu: "Giáo viên", userName: "GV015", gioiTinh: "Nữ");
            
            var gv16 = await CreateUser(userManager, "gv16@lms.com", "GV016", "GiaoVien", "Phạm Văn Sơn", chuyenMon: "Vật lý", chucVu: "Giáo viên", userName: "GV016", gioiTinh: "Nam");
            var gv17 = await CreateUser(userManager, "gv17@lms.com", "GV017", "GiaoVien", "Đặng Văn Lâm", chuyenMon: "Vật lý", chucVu: "Giáo viên", userName: "GV017", gioiTinh: "Nam");
            
            var gv18 = await CreateUser(userManager, "gv18@lms.com", "GV018", "GiaoVien", "Nguyễn Tiến Đạt", chuyenMon: "Hóa học", chucVu: "Giáo viên", userName: "GV018", gioiTinh: "Nam");
            var gv19 = await CreateUser(userManager, "gv19@lms.com", "GV019", "GiaoVien", "Võ Hữu Trí", chuyenMon: "Hóa học", chucVu: "Giáo viên", userName: "GV019", gioiTinh: "Nam");
            
            var gv20 = await CreateUser(userManager, "gv20@lms.com", "GV020", "GiaoVien", "Mai Văn Hùng", chuyenMon: "Sinh học", chucVu: "Giáo viên", userName: "GV020", gioiTinh: "Nam");
            var gv21 = await CreateUser(userManager, "gv21@lms.com", "GV021", "GiaoVien", "Nguyễn Đức Phúc", chuyenMon: "Sinh học", chucVu: "Giáo viên", userName: "GV021", gioiTinh: "Nam");
            
            var gv22 = await CreateUser(userManager, "gv22@lms.com", "GV022", "GiaoVien", "Trần Mai Anh", chuyenMon: "Lịch sử", chucVu: "Giáo viên", userName: "GV022", gioiTinh: "Nữ");
            var gv23 = await CreateUser(userManager, "gv23@lms.com", "GV023", "GiaoVien", "Lê Đăng Khoa", chuyenMon: "Lịch sử", chucVu: "Giáo viên", userName: "GV023", gioiTinh: "Nam");
            
            var gv24 = await CreateUser(userManager, "gv24@lms.com", "GV024", "GiaoVien", "Bùi Việt Hoàng", chuyenMon: "Địa lý", chucVu: "Giáo viên", userName: "GV024", gioiTinh: "Nam");
            var gv25 = await CreateUser(userManager, "gv25@lms.com", "GV025", "GiaoVien", "Đỗ Quỳnh Như", chuyenMon: "Địa lý", chucVu: "Giáo viên", userName: "GV025", gioiTinh: "Nữ");
            var gv26 = await CreateUser(userManager, "gv26@lms.com", "GV026", "GiaoVien", "Phạm Hải Yến", chuyenMon: "Địa lý", chucVu: "Giáo viên", userName: "GV026", gioiTinh: "Nữ");
            
            var gv27 = await CreateUser(userManager, "gv27@lms.com", "GV027", "GiaoVien", "Hoàng Gia Bảo", chuyenMon: "Tin học", chucVu: "Giáo viên", userName: "GV027", gioiTinh: "Nam");
            var gv28 = await CreateUser(userManager, "gv28@lms.com", "GV028", "GiaoVien", "Nguyễn Tuấn Tài", chuyenMon: "Tin học", chucVu: "Giáo viên", userName: "GV028", gioiTinh: "Nam");
            
            var gv29 = await CreateUser(userManager, "gv29@lms.com", "GV029", "GiaoVien", "Trần Quốc Huy", chuyenMon: "GDCD - Thể dục", chucVu: "Giáo viên", userName: "GV029", gioiTinh: "Nam");
            var gv30 = await CreateUser(userManager, "gv30@lms.com", "GV030", "GiaoVien", "Lê Thanh Sơn", chuyenMon: "GDCD - Thể dục", chucVu: "Giáo viên", userName: "GV030", gioiTinh: "Nam");

            // ===================== KHỐI =====================
            var k10 = await context.Khois.FirstOrDefaultAsync(k => k.TenKhoi == "Khối 10") ?? new Khoi { TenKhoi = "Khối 10" };
            var k11 = await context.Khois.FirstOrDefaultAsync(k => k.TenKhoi == "Khối 11") ?? new Khoi { TenKhoi = "Khối 11" };
            var k12 = await context.Khois.FirstOrDefaultAsync(k => k.TenKhoi == "Khối 12") ?? new Khoi { TenKhoi = "Khối 12" };

            if (k10.Id == 0) context.Khois.Add(k10);
            if (k11.Id == 0) context.Khois.Add(k11);
            if (k12.Id == 0) context.Khois.Add(k12);

            if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync();

            // Lấy lại users (đã tồn tại trong DB) - Dùng FirstOrDefault để tránh lỗi nếu lỡ bị trùng email
            var gv1u  = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv1@lms.com");
            var gv2u  = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv2@lms.com");
            var gv3u  = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv3@lms.com");
            var gv4u  = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv4@lms.com");
            var gv5u  = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv5@lms.com");
            var gv6u  = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv6@lms.com");
            var gv7u  = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv7@lms.com");
            var gv8u  = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv8@lms.com");
            var gv9u  = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv9@lms.com");
            var gv10u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv10@lms.com");
            var gv11u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv11@lms.com");
            var gv12u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv12@lms.com");
            var gv13u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv13@lms.com");
            var gv14u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv14@lms.com");
            var gv15u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv15@lms.com");
            var gv16u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv16@lms.com");
            var gv17u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv17@lms.com");
            var gv18u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv18@lms.com");
            var gv19u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv19@lms.com");
            var gv20u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv20@lms.com");
            var gv21u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv21@lms.com");
            var gv22u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv22@lms.com");
            var gv23u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv23@lms.com");
            var gv24u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv24@lms.com");
            var gv25u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv25@lms.com");
            var gv26u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv26@lms.com");
            var gv27u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv27@lms.com");
            var gv28u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv28@lms.com");
            var gv29u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv29@lms.com");
            var gv30u = await context.Users.FirstOrDefaultAsync(u => u.Email == "gv30@lms.com");
            var adminu = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@lms.com");
            var htu    = await context.Users.FirstOrDefaultAsync(u => u.Email == "ht@lms.com");

            if (gv1u == null || gv30u == null || adminu == null) return;

            var khoi10 = await context.Khois.FirstOrDefaultAsync(k => k.TenKhoi == "Khối 10") ?? k10;
            var khoi11 = await context.Khois.FirstOrDefaultAsync(k => k.TenKhoi == "Khối 11") ?? k11;
            var khoi12 = await context.Khois.FirstOrDefaultAsync(k => k.TenKhoi == "Khối 12") ?? k12;

            // ===================== 15 LỚP — PHÂN LỚP CHỦ NHIỆM =====================
            var lop10A1 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "10A1") ?? new Lop { TenLop = "10A1", MaKhoi = khoi10.Id, GiaoVienChuNhiemId = gv1u.Id };
            var lop10A2 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "10A2") ?? new Lop { TenLop = "10A2", MaKhoi = khoi10.Id, GiaoVienChuNhiemId = gv2u.Id };
            var lop10A3 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "10A3") ?? new Lop { TenLop = "10A3", MaKhoi = khoi10.Id, GiaoVienChuNhiemId = gv3u.Id };
            var lop10A4 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "10A4") ?? new Lop { TenLop = "10A4", MaKhoi = khoi10.Id, GiaoVienChuNhiemId = gv4u.Id };
            var lop10A5 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "10A5") ?? new Lop { TenLop = "10A5", MaKhoi = khoi10.Id, GiaoVienChuNhiemId = gv5u.Id };

            var lop11A1 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "11A1") ?? new Lop { TenLop = "11A1", MaKhoi = khoi11.Id, GiaoVienChuNhiemId = gv6u.Id };
            var lop11A2 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "11A2") ?? new Lop { TenLop = "11A2", MaKhoi = khoi11.Id, GiaoVienChuNhiemId = gv7u.Id };
            var lop11A3 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "11A3") ?? new Lop { TenLop = "11A3", MaKhoi = khoi11.Id, GiaoVienChuNhiemId = gv8u.Id };
            var lop11A4 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "11A4") ?? new Lop { TenLop = "11A4", MaKhoi = khoi11.Id, GiaoVienChuNhiemId = gv9u.Id };
            var lop11A5 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "11A5") ?? new Lop { TenLop = "11A5", MaKhoi = khoi11.Id, GiaoVienChuNhiemId = gv10u.Id };

            var lop12A1 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "12A1") ?? new Lop { TenLop = "12A1", MaKhoi = khoi12.Id, GiaoVienChuNhiemId = gv11u.Id };
            var lop12A2 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "12A2") ?? new Lop { TenLop = "12A2", MaKhoi = khoi12.Id, GiaoVienChuNhiemId = gv12u.Id };
            var lop12A3 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "12A3") ?? new Lop { TenLop = "12A3", MaKhoi = khoi12.Id, GiaoVienChuNhiemId = gv13u.Id };
            var lop12A4 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "12A4") ?? new Lop { TenLop = "12A4", MaKhoi = khoi12.Id, GiaoVienChuNhiemId = gv14u.Id };
            var lop12A5 = await context.Lops.FirstOrDefaultAsync(l => l.TenLop == "12A5") ?? new Lop { TenLop = "12A5", MaKhoi = khoi12.Id, GiaoVienChuNhiemId = gv15u.Id };

            var allLops = new List<Lop> {
                lop10A1, lop10A2, lop10A3, lop10A4, lop10A5,
                lop11A1, lop11A2, lop11A3, lop11A4, lop11A5,
                lop12A1, lop12A2, lop12A3, lop12A4, lop12A5
            };

            // Cập nhật lại phân công giáo viên chủ nhiệm (do lớp có thể đã tồn tại)
            lop10A1.GiaoVienChuNhiemId = gv1u.Id;
            lop10A2.GiaoVienChuNhiemId = gv2u.Id;
            lop10A3.GiaoVienChuNhiemId = gv3u.Id;
            lop10A4.GiaoVienChuNhiemId = gv4u.Id;
            lop10A5.GiaoVienChuNhiemId = gv5u.Id;

            lop11A1.GiaoVienChuNhiemId = gv6u.Id;
            lop11A2.GiaoVienChuNhiemId = gv7u.Id;
            lop11A3.GiaoVienChuNhiemId = gv8u.Id;
            lop11A4.GiaoVienChuNhiemId = gv9u.Id;
            lop11A5.GiaoVienChuNhiemId = gv10u.Id;

            lop12A1.GiaoVienChuNhiemId = gv11u.Id;
            lop12A2.GiaoVienChuNhiemId = gv12u.Id;
            lop12A3.GiaoVienChuNhiemId = gv13u.Id;
            lop12A4.GiaoVienChuNhiemId = gv14u.Id;
            lop12A5.GiaoVienChuNhiemId = gv15u.Id;

            foreach (var lop in allLops)
            {
                if (lop.Id == 0) context.Lops.Add(lop);
            }
            await context.SaveChangesAsync();

            // ===================== DÂN SỐ HỌC SINH (450 Học sinh) =====================
            var hsList = new List<NguoiDung>();
            if (!context.Users.Any(u => u.MaHocSinh != null))
            {
                var hocSinhRole = await roleManager.FindByNameAsync("HocSinh");
                string hocSinhRoleId = hocSinhRole?.Id ?? Guid.NewGuid().ToString();

                var dummyUser = new NguoiDung();
                var passwordHasher = new PasswordHasher<NguoiDung>();
                // Bỏ sharedHash vì chúng ta sẽ hash mật khẩu theo mã sinh viên

                string[] ho = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý" };
                string[] demNam = { "Văn", "Hữu", "Đức", "Minh", "Quốc", "Gia", "Thành", "Hoàng", "Thế", "Anh", "Xuân", "Mạnh" };
                string[] demNu = { "Thị", "Ngọc", "Thu", "Phương", "Mỹ", "Diệu", "Quỳnh", "Thanh", "Bích", "Hồng", "Mai", "Trúc" };
                string[] tenNam = { "Anh", "Bình", "Cường", "Dũng", "Đạt", "Hùng", "Huy", "Khoa", "Lâm", "Minh", "Nam", "Phong", "Phúc", "Quân", "Sơn", "Tài", "Tân", "Tuấn", "Tùng", "Vinh" };
                string[] tenNu = { "Anh", "Bình", "Chi", "Diệp", "Dung", "Giang", "Hà", "Hoa", "Hương", "Lan", "Linh", "Mai", "Ngọc", "Oanh", "Phương", "Quỳnh", "Thảo", "Trang", "Vân", "Vy" };
                string[] tinhThanh = { "Hà Nội", "Hải Phòng", "Đà Nẵng", "TP. Hồ Chí Minh", "Quảng Nam", "Thừa Thiên Huế", "Cần Thơ", "Nghệ An", "Thanh Hóa" };

                var allStudents = new List<NguoiDung>();
                var allUserRoles = new List<IdentityUserRole<string>>();
                var allUserClaims = new List<IdentityUserClaim<string>>();
                var blocks = new[] {
                    new { LopList = new List<Lop> { lop10A1, lop10A2, lop10A3, lop10A4, lop10A5 }, BaseYear = 2009, PrefixYear = "26" },
                    new { LopList = new List<Lop> { lop11A1, lop11A2, lop11A3, lop11A4, lop11A5 }, BaseYear = 2008, PrefixYear = "25" },
                    new { LopList = new List<Lop> { lop12A1, lop12A2, lop12A3, lop12A4, lop12A5 }, BaseYear = 2007, PrefixYear = "24" }
                };

                foreach (var block in blocks)
                {
                    foreach (var lop in block.LopList)
                    {
                        string lopCode = System.Text.RegularExpressions.Regex.Replace(lop.TenLop ?? "", @"^\d+", "").ToUpper();
                        string prefix = block.PrefixYear + lopCode;

                        for (int s = 0; s < 30; s++)
                        {
                            var isNam = Random2(0, 1) == 0;
                            var hoTen = isNam
                                ? $"{ho[Random2(0, ho.Length - 1)]} {demNam[Random2(0, demNam.Length - 1)]} {tenNam[Random2(0, tenNam.Length - 1)]}"
                                : $"{ho[Random2(0, ho.Length - 1)]} {demNu[Random2(0, demNu.Length - 1)]} {tenNu[Random2(0, tenNu.Length - 1)]}";
                            
                            var id = Guid.NewGuid().ToString();
                            var maHs = $"{prefix}{(s + 1):D3}";
                            var email = $"{maHs}@truong.edu.vn".ToLower();

                            var studentUser = new NguoiDung
                            {
                                Id = id,
                                UserName = maHs,
                                NormalizedUserName = maHs.ToUpper(),
                                Email = email,
                                NormalizedEmail = email.ToUpper(),
                                EmailConfirmed = true,
                                PasswordHash = passwordHasher.HashPassword(dummyUser, maHs),
                                SecurityStamp = Guid.NewGuid().ToString(),
                                ConcurrencyStamp = Guid.NewGuid().ToString(),
                                HoTen = hoTen,
                                IsActive = true,
                                MaHocSinh = maHs,
                                HanhKiem = Random2(0, 10) > 1 ? "Tốt" : "Khá",
                                NgaySinh = new DateTime(block.BaseYear, 1, 1).AddDays(Random2(0, 364)),
                                NgayTao = DateTime.Now,
                                LopId = lop.Id,
                                NamHoc = "2024-2025",
                                DiaChi = tinhThanh[Random2(0, tinhThanh.Length - 1)],
                                GioiTinh = isNam ? "Nam" : "Nữ"
                            };

                            allStudents.Add(studentUser);
                            allUserRoles.Add(new IdentityUserRole<string> { UserId = id, RoleId = hocSinhRoleId });
                            allUserClaims.Add(new IdentityUserClaim<string> { UserId = id, ClaimType = "AnhDaiDien", ClaimValue = "~/images/default-avatar.svg" });
                        }
                    }
                }

                context.Users.AddRange(allStudents);
                context.Set<IdentityUserRole<string>>().AddRange(allUserRoles);
                context.Set<IdentityUserClaim<string>>().AddRange(allUserClaims);
                await context.SaveChangesAsync();

                hsList = allStudents;
            }
            else
            {
                var hsRole = await roleManager.FindByNameAsync("HocSinh");
                if (hsRole != null)
                {
                    var userRoles = await context.UserRoles.Where(ur => ur.RoleId == hsRole.Id).Select(ur => ur.UserId).ToListAsync();
                    hsList = await context.Users.Where(u => userRoles.Contains(u.Id)).ToListAsync();
                }
                else hsList = new List<NguoiDung>();
            }

            // ===================== MÔN HỌC (30 môn, 10 môn/khối) =====================
            var monData = new (string Ten, string MoTa, int KhoiId, string GvId)[]
            {
                // Khối 10 (Sử dụng các giáo viên: Toán(1), Văn(2), Anh(3), Lý(4), Hóa(5), Sinh(6), Sử(7), Địa(24), Tin(8), GDCD(9))
                ("Toán học 10",    "Đại số và Hình học lớp 10",         khoi10.Id, gv1u.Id),
                ("Ngữ văn 10",     "Văn học và Tiếng Việt lớp 10",      khoi10.Id, gv2u.Id),
                ("Tiếng Anh 10",   "Tiếng Anh giao tiếp và ngữ pháp",   khoi10.Id, gv3u.Id),
                ("Vật lý 10",      "Cơ học và Nhiệt học lớp 10",        khoi10.Id, gv4u.Id),
                ("Hóa học 10",     "Hóa học đại cương lớp 10",          khoi10.Id, gv5u.Id),
                ("Sinh học 10",    "Sinh học đại cương 10",             khoi10.Id, gv6u.Id),
                ("Lịch sử 10",     "Lịch sử Việt Nam và thế giới",      khoi10.Id, gv7u.Id),
                ("Địa lý 10",      "Địa lý tự nhiên và kinh tế",        khoi10.Id, gv24u.Id),
                ("Tin học 10",     "Tin học căn bản và lập trình",      khoi10.Id, gv8u.Id),
                ("GDCD 10",        "Giáo dục công dân 10",              khoi10.Id, gv9u.Id),

                // Khối 11 (Sử dụng các giáo viên: Toán(10), Văn(12), Anh(14), Lý(16), Hóa(18), Sinh(20), Sử(22), Địa(25), Tin(27), GDCD(29))
                ("Toán học 11",    "Đại số và Giải tích lớp 11",        khoi11.Id, gv10u.Id),
                ("Ngữ văn 11",     "Văn học Việt Nam và thế giới",      khoi11.Id, gv12u.Id),
                ("Tiếng Anh 11",   "Kỹ năng ngôn ngữ nâng cao",         khoi11.Id, gv14u.Id),
                ("Vật lý 11",      "Điện học và Quang học",             khoi11.Id, gv16u.Id),
                ("Hóa học 11",     "Hóa học vô cơ 11",                  khoi11.Id, gv18u.Id),
                ("Sinh học 11",    "Sinh học cơ thể người và động vật", khoi11.Id, gv20u.Id),
                ("Lịch sử 11",     "Lịch sử cận đại và hiện đại",       khoi11.Id, gv22u.Id),
                ("Địa lý 11",      "Địa lý các vùng kinh tế",           khoi11.Id, gv25u.Id),
                ("Tin học 11",     "Lập trình cơ sở dữ liệu",           khoi11.Id, gv27u.Id),
                ("GDCD 11",        "Giáo dục công dân 11",              khoi11.Id, gv29u.Id),

                // Khối 12 (Sử dụng các giáo viên: Toán(11), Văn(13), Anh(15), Lý(17), Hóa(19), Sinh(21), Sử(23), Địa(26), Tin(28), GDCD(30))
                ("Toán học 12",    "Giải tích và Xác suất thống kê",    khoi12.Id, gv11u.Id),
                ("Ngữ văn 12",     "Ôn tập thi đại học môn Văn",        khoi12.Id, gv13u.Id),
                ("Tiếng Anh 12",   "Luyện thi THPT Quốc gia Tiếng Anh", khoi12.Id, gv15u.Id),
                ("Vật lý 12",      "Dao động, Sóng và Điện xoay chiều", khoi12.Id, gv17u.Id),
                ("Hóa học 12",     "Hóa học hữu cơ và đại cương",       khoi12.Id, gv19u.Id),
                ("Sinh học 12",    "Di truyền và tiến hóa",             khoi12.Id, gv21u.Id),
                ("Lịch sử 12",     "Lịch sử ôn thi THPT Quốc gia",      khoi12.Id, gv23u.Id),
                ("Địa lý 12",      "Địa lý ôn thi THPT Quốc gia",       khoi12.Id, gv26u.Id),
                ("Tin học 12",     "Lập trình Pascal và CSDL",          khoi12.Id, gv28u.Id),
                ("GDCD 12",        "Giáo dục công dân 12",              khoi12.Id, gv30u.Id),
            };

            List<MonHoc> monHocs;
            if (!context.DanhSachMonHoc.Any())
            {
                monHocs = monData.Select(m => new MonHoc
                {
                    TenMonHoc = m.Ten, MoTa = m.MoTa, KhoiId = m.KhoiId,
                    GiaoVienId = m.GvId, IsActive = true, NgayTao = DateTime.Now
                }).ToList();
                context.DanhSachMonHoc.AddRange(monHocs);
                await context.SaveChangesAsync();
            }
            else
            {
                monHocs = await context.DanhSachMonHoc.OrderBy(m => m.Id).ToListAsync();
            }

            // ===================== LỚP - MÔN HỌC =====================
            if (!context.LopMonHocs.Any())
            {
                var lopMonHocs = new List<LopMonHoc>();
                var monHocGiaoViens = new List<MonHocGiaoVien>();
                
                foreach (var lop in allLops)
                {
                    int startMonIdx = 0;
                    if (lop.TenLop.StartsWith("10")) startMonIdx = 0;
                    else if (lop.TenLop.StartsWith("11")) startMonIdx = 10;
                    else if (lop.TenLop.StartsWith("12")) startMonIdx = 20;

                    for (int i = startMonIdx; i < startMonIdx + 10; i++) // 10 môn cho mỗi khối
                    {
                        lopMonHocs.Add(new LopMonHoc { LopId = lop.Id, MonHocId = monHocs[i].Id, GiaoVienId = monData[i].GvId });
                        monHocGiaoViens.Add(new MonHocGiaoVien { LopId = lop.Id, MonHocId = monHocs[i].Id, NguoiDungId = monData[i].GvId });
                    }
                }
                    
                context.LopMonHocs.AddRange(lopMonHocs);
                context.MonHocGiaoViens.AddRange(monHocGiaoViens);
                await context.SaveChangesAsync();
            }


            // ===================== LỊCH HỌC =====================
            // Mỗi lớp học 5 tiết/ngày × 6 ngày (Thứ 2 → Thứ 7) = 30 tiết/tuần
            // Phân bổ: Toán/Văn/Anh = 4 tiết/tuần; Lý/Hóa/Sinh/Sử/Địa/Tin/GDCD = 2 tiết/tuần
            // Tổng: 4×3 + 2×7 = 12 + 18 = 30 tiết ✓
            if (!context.LichHocs.Any())
            {
                var lichHocs = new List<LichHoc>();

                // Giờ bắt đầu/kết thúc theo tiết
                var tietTimes = new (TimeSpan Start, TimeSpan End)[]
                {
                    (new TimeSpan(7, 0, 0),  new TimeSpan(7, 45, 0)),   // Tiết 1
                    (new TimeSpan(7, 50, 0), new TimeSpan(8, 35, 0)),   // Tiết 2
                    (new TimeSpan(8, 45, 0), new TimeSpan(9, 30, 0)),   // Tiết 3
                    (new TimeSpan(9, 40, 0), new TimeSpan(10, 25, 0)),  // Tiết 4
                    (new TimeSpan(10, 35, 0), new TimeSpan(11, 20, 0)), // Tiết 5
                };

                // Thời khóa biểu tuần chuẩn: 6 ngày × 5 tiết
                // Mỗi phần tử là index trong mảng 10 môn của khối (0=Toán,1=Văn,2=Anh,3=Lý,4=Hóa,5=Sinh,6=Sử,7=Địa,8=Tin,9=GDCD)
                // 5 lớp/khối → mỗi lớp xoay vòng lịch khác nhau để tránh trùng GV cùng tiết
                // (mỗi khối dùng chung GV, nên các lớp cùng khối KHÔNG thể học cùng môn cùng tiết)
                var basePattern = new int[6, 5]
                {
                    // T2:  T1   T2   T3   T4   T5
                    {       0,   1,   2,   3,   4  },  // Toán Văn Anh Lý Hóa
                    // T3:  T1   T2   T3   T4   T5
                    {       5,   6,   7,   8,   9  },  // Sinh Sử Địa Tin GDCD
                    // T4:  T1   T2   T3   T4   T5
                    {       0,   1,   3,   4,   2  },  // Toán Văn Lý Hóa Anh
                    // T5:  T1   T2   T3   T4   T5
                    {       1,   0,   5,   6,   8  },  // Văn Toán Sinh Sử Tin
                    // T6:  T1   T2   T3   T4   T5
                    {       2,   3,   4,   7,   9  },  // Anh Lý Hóa Địa GDCD
                    // T7:  T1   T2   T3   T4   T5
                    {       0,   1,   2,   5,   7  },  // Toán Văn Anh Sinh Địa
                };

                // Các lớp cùng khối dùng GV chung → phải xoay vòng lịch để tránh xung đột
                // Dùng phép hoán vị hàng theo từng lớp (offset 0..4)
                void AddLich(int lopId, string tenLop, int[] monIds, string[] gvIds, int lopOffset)
                {
                    int phong = lopOffset + 1;
                    string tenPhong = $"P.{tenLop}";
                    int[] thuList = { 2, 3, 4, 5, 6, 7 };

                    for (int dayIdx = 0; dayIdx < 6; dayIdx++)
                    {
                        int thu = thuList[dayIdx];
                        // Xoay vòng hàng theo lopOffset để tránh GV trùng lịch
                        int rowIdx = (dayIdx + lopOffset) % 6;

                        for (int tiet = 1; tiet <= 5; tiet++)
                        {
                            int subjIdx = basePattern[rowIdx, tiet - 1];
                            var (gioBatDau, gioKetThuc) = tietTimes[tiet - 1];

                            // Tính ngày học trong tuần hiện tại (Monday = 2 → DayOfWeek.Monday = 1)
                            var today = DateTime.Today;
                            int daysToMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
                            DateTime monday = today.AddDays(daysToMonday);
                            DateTime ngayHoc = monday.AddDays(thu - 2); // thu=2 → +0 ngày, thu=7 → +5 ngày

                            lichHocs.Add(new LichHoc
                            {
                                LopId      = lopId,
                                MonHocId   = monIds[subjIdx],
                                GiaoVienId = gvIds[subjIdx],
                                Thu        = thu,
                                TietHoc    = tiet,
                                PhongHoc   = tenPhong,
                                NgayHoc    = ngayHoc,
                                GioBatDau  = gioBatDau,
                                GioKetThuc = gioKetThuc,
                                IsHocBu    = false
                            });
                        }
                    }
                }

                var ids10   = monHocs.Take(10).Select(m => m.Id).ToArray();
                var gvIds10 = monData.Take(10).Select(m => m.GvId).ToArray();

                var ids11   = monHocs.Skip(10).Take(10).Select(m => m.Id).ToArray();
                var gvIds11 = monData.Skip(10).Take(10).Select(m => m.GvId).ToArray();

                var ids12   = monHocs.Skip(20).Take(10).Select(m => m.Id).ToArray();
                var gvIds12 = monData.Skip(20).Take(10).Select(m => m.GvId).ToArray();

                // 15 lớp → 5 lớp/khối, offset 0..4
                var lops10 = allLops.Take(5).ToList();
                var lops11 = allLops.Skip(5).Take(5).ToList();
                var lops12 = allLops.Skip(10).Take(5).ToList();

                for (int i = 0; i < 5; i++)
                {
                    AddLich(lops10[i].Id, lops10[i].TenLop, ids10, gvIds10, i);
                    AddLich(lops11[i].Id, lops11[i].TenLop, ids11, gvIds11, i);
                    AddLich(lops12[i].Id, lops12[i].TenLop, ids12, gvIds12, i);
                }

                context.LichHocs.AddRange(lichHocs);
                await context.SaveChangesAsync();
            }

            // ===================== BÀI GIẢNG =====================
            List<BaiGiang> baiGiangs;
            if (!context.DanhSachBaiGiang.Any())
            {
                baiGiangs = new List<BaiGiang>();
                var bgTitles = new[]
                {
                    ("Chương 1: Giới thiệu tổng quan", "Giới thiệu nội dung chương trình học kỳ 1"),
                    ("Chương 2: Kiến thức nền tảng",   "Các khái niệm cơ bản và công thức trọng tâm"),
                    ("Ôn tập giữa kỳ",                 "Tổng hợp kiến thức và bài tập ôn luyện"),
                };
                foreach (var mon in monHocs)
                    foreach (var (title, desc) in bgTitles)
                        baiGiangs.Add(new BaiGiang
                        {
                            TieuDe = $"{title} - {mon.TenMonHoc}",
                            MoTa = desc, MonHocId = mon.Id, NguoiDungId = mon.GiaoVienId,
                            IsActive = true, ThuTu = baiGiangs.Count + 1,
                            NgayTao = DateTime.Now.AddDays(-Random2(1, 30))
                        });
                context.DanhSachBaiGiang.AddRange(baiGiangs);
                await context.SaveChangesAsync();
            }
            else
            {
                baiGiangs = await context.DanhSachBaiGiang.Take(20).ToListAsync();
            }

            // ===================== TÀI LIỆU =====================
            if (!context.DanhSachTaiLieu.Any())
            {
                var taiLieus = baiGiangs.Take(20).Select(bg => new TaiLieu
                {
                    TenTaiLieu = $"Slide bài giảng - {bg.TieuDe}",
                    DuongDanFile = "/uploads/sample/slide.pdf",
                    LoaiTaiLieu = LoaiTaiLieu.Slide, KichThuocFile = 1024000,
                    BaiGiangId = bg.Id, MonHocId = bg.MonHocId, NgayTao = bg.NgayTao
                }).ToList();
                context.DanhSachTaiLieu.AddRange(taiLieus);
                await context.SaveChangesAsync();
            }

            // ===================== BÀI TẬP =====================
            List<BaiTap> baiTaps;
            if (!context.DanhSachBaiTap.Any())
            {
                baiTaps = new List<BaiTap>();
                // 8 bài tập/môn: 4 đã đóng (lịch sử), 4 đang mở (hiện tại)
                // TiLe = tỉ lệ % học sinh sẽ nộp (dùng khi tạo BaiNop)
                var btData = new[]
                {
                    // ── Đã đóng (lịch sử) ──
                    ("Bài tập về nhà chương 1",   "Làm các bài từ 1-15 trong SGK, trình bày rõ từng bước.",          TrangThaiBaiTap.DaDong, LoaiDiem.BaiTap,  85),
                    ("Kiểm tra 15 phút lần 1",    "Trắc nghiệm 12 câu và 1 bài tự luận ngắn.",                       TrangThaiBaiTap.DaDong, LoaiDiem.BaiTap,  70),
                    ("Bài tập thực hành chương 2","Thực hành bài tập nhóm, nộp báo cáo theo mẫu.",                  TrangThaiBaiTap.DaDong, LoaiDiem.BaiTap,  60),
                    ("Kiểm tra 1 tiết lần 1",     "Kiểm tra 45 phút, hình thức tự luận kết hợp trắc nghiệm.",       TrangThaiBaiTap.DaDong, LoaiDiem.GiuaKy, 90),
                    // ── Đang mở (hiện tại) ──
                    ("Bài tập về nhà chương 3",   "Hoàn thành bài 1 đến 20, ghi rõ công thức sử dụng.",              TrangThaiBaiTap.DangMo, LoaiDiem.BaiTap,  75),
                    ("Kiểm tra 15 phút lần 2",    "Kiểm tra nội dung chương 2-3, 15 câu trắc nghiệm.",               TrangThaiBaiTap.DangMo, LoaiDiem.BaiTap,  50),
                    ("Bài luận tổng kết giữa kỳ", "Viết bài luận 1-2 trang tóm tắt kiến thức trọng tâm.",           TrangThaiBaiTap.DangMo, LoaiDiem.GiuaKy, 40),
                    ("Bài kiểm tra giữa kỳ",      "Kiểm tra 45 phút toàn bộ nội dung học kỳ I.",                     TrangThaiBaiTap.DangMo, LoaiDiem.GiuaKy, 65),
                };

                foreach (var mon in monHocs)
                    for (int i = 0; i < btData.Length; i++)
                    {
                        var (title, desc, tt, loai, _) = btData[i];
                        baiTaps.Add(new BaiTap
                        {
                            TieuDe   = $"{title} - {mon.TenMonHoc}",
                            MoTa     = desc,
                            NoiDung  = desc,
                            HanNop   = tt == TrangThaiBaiTap.DaDong
                                       ? DateTime.Now.AddDays(-Random2(5, 30))
                                       : DateTime.Now.AddDays(Random2(2, 21)),
                            DiemToiDa   = 10,
                            LoaiDiem    = loai,
                            TrangThai   = tt,
                            MonHocId    = mon.Id,
                            NguoiDungId = mon.GiaoVienId,
                            NgayTao     = DateTime.Now.AddDays(-Random2(15, 60))
                        });
                    }
                context.DanhSachBaiTap.AddRange(baiTaps);
                await context.SaveChangesAsync();
            }
            else
            {
                baiTaps = await context.DanhSachBaiTap.ToListAsync();
            }

            // ===================== BÀI NỘP =====================
            if (!context.DanhSachBaiNop.Any())
            {
                var baiNops = new List<BaiNop>();

                var lop10Ids = allLops.Take(5).Select(l => l.Id).ToHashSet();
                var lop11Ids = allLops.Skip(5).Take(5).Select(l => l.Id).ToHashSet();
                var lop12Ids = allLops.Skip(10).Take(5).Select(l => l.Id).ToHashSet();

                var hsLop10 = hsList.Where(u => lop10Ids.Contains(u.LopId ?? 0)).ToList();
                var hsLop11 = hsList.Where(u => lop11Ids.Contains(u.LopId ?? 0)).ToList();
                var hsLop12 = hsList.Where(u => lop12Ids.Contains(u.LopId ?? 0)).ToList();

                // Tỉ lệ nộp cố định theo chỉ số bài tập (index trong danh sách btData)
                // 0=85% 1=70% 2=60% 3=90% 4=75% 5=50% 6=40% 7=65%
                int[] tiLeNop = { 85, 70, 60, 90, 75, 50, 40, 65 };

                // Các mẫu nội dung nộp bài đa dạng
                string[] noiDungMau =
                {
                    "Em đã hoàn thành bài tập theo hướng dẫn của thầy/cô ạ.",
                    "Em nộp bài muộn một chút vì hôm qua bị ốm, mong thầy/cô thông cảm.",
                    "Em chưa chắc phần cuối, nhưng em đã cố gắng hết sức. Mong thầy/cô góp ý.",
                    "Bài làm của em đã được kiểm tra lại, em nghĩ câu 3 có thể còn sai sót.",
                    "Em hoàn thành bài đúng hạn, xin thầy/cô xem và cho điểm ạ.",
                    "Em nộp file bài làm ạ, có kèm theo phần giải thích thêm ở trang 2.",
                    "Em tự làm bài, không tham khảo bạn bè. Mong thầy/cô nhận xét kỹ giúp em.",
                    "Bài này em học khá chắc nên hoàn thành sớm. Kính nộp thầy/cô.",
                };

                void AddNopThucTe(List<NguoiDung> hss, IEnumerable<BaiTap> allBts)
                {
                    // Nhóm bài tập theo môn để gán đúng index
                    var btByMon = allBts.GroupBy(b => b.MonHocId).ToDictionary(g => g.Key, g => g.OrderBy(b => b.Id).ToList());

                    int hsIdx = 0;
                    foreach (var hs in hss)
                    {
                        hsIdx++;
                        // Mỗi học sinh có một "profile điểm" riêng để tạo sự đa dạng
                        // Profile: 0=Xuất sắc, 1=Giỏi, 2=Khá, 3=TrungBình, 4=Yếu
                        int profile = (hsIdx * 7 + 3) % 10;  // 0-9, phân tán

                        foreach (var (monId, baiTapList) in btByMon)
                        {
                            for (int btIdx = 0; btIdx < baiTapList.Count; btIdx++)
                            {
                                var bt = baiTapList[btIdx];
                                int tiLe = btIdx < tiLeNop.Length ? tiLeNop[btIdx] : 70;

                                // Quyết định HS này có nộp bài không dựa trên tỉ lệ + profile
                                // Dùng seed khác nhau theo (hsIdx, btIdx) để mỗi HS có hành vi khác
                                int hash = ((hsIdx * 31 + btIdx * 17 + monId) & 0x7fffffff) % 100;
                                bool coNop = hash < tiLe;
                                if (!coNop) continue;

                                // Tính điểm dựa trên profile học sinh + ngẫu nhiên
                                double diemBase = profile switch
                                {
                                    0 => 9.0 + (hash % 10) * 0.1,    // Xuất sắc: 9.0-9.9
                                    1 => 8.5 + (hash % 5) * 0.1,     // Giỏi: 8.5-8.9
                                    2 => 7.0 + (hash % 15) * 0.1,    // Khá: 7.0-8.4
                                    3 => 5.0 + (hash % 20) * 0.1,    // Trung bình: 5.0-6.9
                                    4 => 3.5 + (hash % 15) * 0.1,    // Yếu: 3.5-4.9
                                    _ => 6.0 + (hash % 25) * 0.1
                                };
                                double diem = Math.Round(Math.Min(10.0, diemBase), 1);

                                // Thời điểm nộp: sớm/đúng hạn/muộn
                                int ngayTruoc = profile <= 1 ? Random2(2, 7) : (profile == 2 ? Random2(0, 3) : 0);
                                DateTime ngayNop = bt.TrangThai == TrangThaiBaiTap.DaDong
                                    ? bt.HanNop.AddDays(-ngayTruoc)
                                    : DateTime.Now.AddDays(-Random2(0, 3));

                                string noiDung = noiDungMau[hash % noiDungMau.Length];
                                string nhanXet = diem >= 9.0 ? "Xuất sắc! Bài làm rất tốt, trình bày khoa học."
                                    : diem >= 8.0 ? "Bài làm tốt, nắm vững kiến thức."
                                    : diem >= 6.5 ? "Khá tốt, cần chú ý hơn phần tự luận."
                                    : diem >= 5.0 ? "Cần ôn lại lý thuyết, một số bài giải chưa đầy đủ."
                                    : "Cần cố gắng nhiều hơn, xem lại toàn bộ chương.";

                                baiNops.Add(new BaiNop
                                {
                                    BaiTapId = bt.Id,
                                    HocSinhId = hs.Id,
                                    NoiDung = noiDung,
                                    DuongDanFile = "/uploads/bainop/bai_tap_ve_nha.pdf",
                                    NgayNop = ngayNop,
                                    TrangThai = TrangThaiBaiNop.ChamXong,
                                    Diem = diem,
                                    NhanXet = nhanXet,
                                    NgayCham = ngayNop.AddDays(Random2(1, 3))
                                });
                            }
                        }
                    }
                }

                var monIds10 = monHocs.Take(10).Select(m => m.Id).ToHashSet();
                var monIds11 = monHocs.Skip(10).Take(10).Select(m => m.Id).ToHashSet();
                var monIds12 = monHocs.Skip(20).Take(10).Select(m => m.Id).ToHashSet();
                AddNopThucTe(hsLop10, baiTaps.Where(b => monIds10.Contains(b.MonHocId)));
                AddNopThucTe(hsLop11, baiTaps.Where(b => monIds11.Contains(b.MonHocId)));
                AddNopThucTe(hsLop12, baiTaps.Where(b => monIds12.Contains(b.MonHocId)));

                // Lưu theo từng batch để tránh timeout
                for (int i = 0; i < baiNops.Count; i += 3000)
                {
                    context.DanhSachBaiNop.AddRange(baiNops.Skip(i).Take(3000));
                    await context.SaveChangesAsync();
                }
            }

            // ===================== ĐIỂM SỐ =====================
            if (!context.DanhSachDiemSo.Any())
            {
                var diemSos = new List<DiemSo>();
                
                var lop10Ids = allLops.Take(5).Select(l => l.Id).ToHashSet();
                var lop11Ids = allLops.Skip(5).Take(5).Select(l => l.Id).ToHashSet();
                var lop12Ids = allLops.Skip(10).Take(5).Select(l => l.Id).ToHashSet();

                var hsLop10d = hsList.Where(u => lop10Ids.Contains(u.LopId ?? 0)).ToList();
                var hsLop11d = hsList.Where(u => lop11Ids.Contains(u.LopId ?? 0)).ToList();
                var hsLop12d = hsList.Where(u => lop12Ids.Contains(u.LopId ?? 0)).ToList();
                
                // Phân phối điểm theo profile học sinh: ~10% Xuất sắc, 20% Giỏi, 35% Khá, 25% TB, 10% Yếu
                void AddDiem(List<NguoiDung> hss, IEnumerable<MonHoc> mons, string gvId)
                {
                    int hsIdx = 0;
                    foreach (var hs in hss)
                    {
                        hsIdx++;
                        // Profile điểm dựa trên index để phân tán đều
                        // Cứ mỗi 10 học sinh: 1 XS, 2 Giỏi, 3 Khá, 3 TB, 1 Yếu
                        int profileBucket = hsIdx % 10;
                        int profile = profileBucket == 0 ? 0       // Xuất sắc
                            : profileBucket <= 2           ? 1       // Giỏi
                            : profileBucket <= 5           ? 2       // Khá
                            : profileBucket <= 8           ? 3       // Trung bình
                            :                                4;      // Yếu

                        foreach (var mon in mons)
                        {
                            // Offset theo (hsIdx + monId) để cùng profile nhưng khác nhau theo môn
                            int offset = (hsIdx * 13 + mon.Id * 7) & 0xff;

                            (double diemMin, double diemMax) = profile switch
                            {
                                0 => (8.8, 10.0),   // Xuất sắc
                                1 => (8.0, 9.0),    // Giỏi
                                2 => (6.5, 8.2),    // Khá
                                3 => (5.0, 6.8),    // Trung bình
                                4 => (3.0, 5.2),    // Yếu
                                _ => (5.5, 7.5)
                            };

                            double range = diemMax - diemMin;
                            double dm1 = Math.Round(diemMin + (offset % 100) / 100.0 * range, 1);
                            double dgk = Math.Round(diemMin + ((offset + 37) % 100) / 100.0 * range, 1);
                            double dck = Math.Round(diemMin + ((offset + 71) % 100) / 100.0 * range, 1);
                            dm1 = Math.Clamp(dm1, 0, 10);
                            dgk = Math.Clamp(dgk, 0, 10);
                            dck = Math.Clamp(dck, 0, 10);

                            string nhanXet = profile switch
                            {
                                0 => "Học sinh xuất sắc, duy trì phong độ tốt.",
                                1 => "Học sinh giỏi, cần phát huy thêm tư duy sáng tạo.",
                                2 => "Học sinh khá, cần chú ý hơn phần bài tập khó.",
                                3 => "Học sinh trung bình, cần ôn luyện thêm lý thuyết.",
                                4 => "Học sinh yếu, cần được hỗ trợ thêm và học phụ đạo.",
                                _ => "Học sinh có tiến bộ, cần cố gắng hơn."
                            };

                            diemSos.Add(new DiemSo
                            {
                                NguoiDungId = hs.Id,
                                MonHocId    = mon.Id,
                                GiaoVienId  = mon.GiaoVienId ?? gvId,
                                LoaiDiem    = LoaiDiem.GiuaKy,
                                Diem        = dm1,
                                DiemGiuaKy  = dgk,
                                DiemCuoiKy  = dck,
                                NhanXet     = nhanXet,
                                NgayNhap    = DateTime.Now.AddDays(-Random2(5, 20))
                            });
                        }
                    }
                }
                AddDiem(hsLop10d, monHocs.Take(10), gv1u.Id);
                AddDiem(hsLop11d, monHocs.Skip(10).Take(10), gv2u.Id);
                AddDiem(hsLop12d, monHocs.Skip(20).Take(10), gv3u.Id);
                context.DanhSachDiemSo.AddRange(diemSos);
                await context.SaveChangesAsync();
            }

            // ===================== THÔNG BÁO =====================
            if (!context.ThongBaos.Any())
            {
                context.ThongBaos.AddRange(
                    // --- Admin ---
                    new ThongBao { TieuDe = "Chào mừng năm học mới 2024-2025", NoiDung = "Trường THPT Quốc Học xin thông báo khai giảng năm học 2024-2025 vào ngày 05/09/2024. Toàn thể giáo viên và học sinh có mặt đúng giờ.", NguoiDangId = adminu.Id, NgayDang = DateTime.Now.AddDays(-90) },
                    new ThongBao { TieuDe = "Quy định nộp hồ sơ đầu năm", NoiDung = "Giáo viên hoàn thiện hồ sơ chuyên môn (giáo án, kế hoạch dạy học) nộp về văn phòng trước ngày 15/09/2024.", NguoiDangId = adminu.Id, NgayDang = DateTime.Now.AddDays(-85) },
                    new ThongBao { TieuDe = "Cập nhật phần mềm LMS", NoiDung = "Hệ thống LMS đã được nâng cấp phiên bản mới. Giáo viên và học sinh đăng nhập lại để trải nghiệm các tính năng mới.", NguoiDangId = adminu.Id, NgayDang = DateTime.Now.AddDays(-60) },
                    new ThongBao { TieuDe = "Thay đổi lịch trực ban tuần 45", NoiDung = "Do có sự kiện hội thao cấp quận, lịch trực ban tuần 45 có sự điều chỉnh. Xem chi tiết tại bảng tin văn phòng.", NguoiDangId = adminu.Id, NgayDang = DateTime.Now.AddDays(-14) },
                    new ThongBao { TieuDe = "Thông báo nghỉ lễ Quốc khánh 2/9", NoiDung = "Nhà trường thông báo học sinh được nghỉ từ ngày 02/09 đến 03/09/2024 nhân dịp lễ Quốc khánh. Học sinh đi học lại từ ngày 04/09.", NguoiDangId = adminu.Id, NgayDang = DateTime.Now.AddDays(-75) },

                    // --- Hiệu trưởng ---
                    new ThongBao { TieuDe = "Lịch kiểm tra giữa kỳ I năm học 2024-2025", NoiDung = "Kiểm tra giữa kỳ 1 diễn ra từ ngày 11/11 đến 16/11/2024. Giáo viên bộ môn ra đề và nộp đề về tổ trưởng chuyên môn trước ngày 05/11.", NguoiDangId = htu!.Id, NgayDang = DateTime.Now.AddDays(-25) },
                    new ThongBao { TieuDe = "Kế hoạch hội giảng cấp trường học kỳ I", NoiDung = "Hội giảng cấp trường sẽ tổ chức vào ngày 20/10/2024. Mỗi tổ bộ môn cử 2 giáo viên tham gia. Đăng ký qua email ban giám hiệu.", NguoiDangId = htu!.Id, NgayDang = DateTime.Now.AddDays(-45) },
                    new ThongBao { TieuDe = "Thông báo họp Hội đồng sư phạm tháng 11", NoiDung = "Họp Hội đồng sư phạm tháng 11 vào 14:00 ngày 04/11/2024 tại hội trường lớn. Toàn thể cán bộ giáo viên tham dự đầy đủ, đúng giờ.", NguoiDangId = htu!.Id, NgayDang = DateTime.Now.AddDays(-10) },
                    new ThongBao { TieuDe = "Kết quả xếp loại giáo viên học kỳ II (2023-2024)", NoiDung = "BGH thông báo kết quả xếp loại thi đua giáo viên HK2 năm học 2023-2024. Danh sách chi tiết đã gửi qua email cá nhân.", NguoiDangId = htu!.Id, NgayDang = DateTime.Now.AddDays(-55) },
                    new ThongBao { TieuDe = "Lịch thi học kỳ I 2024-2025", NoiDung = "Thi học kỳ 1 diễn ra từ ngày 16/12 đến 21/12/2024. Phòng thi và lịch thi cụ thể sẽ được thông báo sau khi hoàn tất ghép phòng.", NguoiDangId = htu!.Id, NgayDang = DateTime.Now.AddDays(-5) },

                    // --- Giáo viên bộ môn ---
                    new ThongBao { TieuDe = "Lịch ôn tập Toán học trước giữa kỳ (Khối 10)", NoiDung = "GV Toán thông báo lịch ôn tập buổi chiều cho học sinh khối 10 từ 13:30-16:00 các ngày thứ 3 và thứ 5 tuần 10/11-14/11. Học sinh đăng ký tại lớp.", NguoiDangId = gv1u.Id, NgayDang = DateTime.Now.AddDays(-20) },
                    new ThongBao { TieuDe = "Yêu cầu nộp bài Ngữ văn trước ngày 08/11", NoiDung = "GV Ngữ văn nhắc nhở học sinh các lớp 10A1, 11A1, 12A1 nộp bài luận 'Phân tích nhân vật' đã giao tuần trước. Hạn cuối 08/11/2024.", NguoiDangId = gv2u.Id, NgayDang = DateTime.Now.AddDays(-8) },
                    new ThongBao { TieuDe = "Kết quả thi Tiếng Anh cấp trường", NoiDung = "GV Tiếng Anh thông báo kết quả thi Olympic Tiếng Anh cấp trường. Danh sách học sinh vào vòng tỉnh đã được gửi về các lớp qua giáo viên chủ nhiệm.", NguoiDangId = gv3u.Id, NgayDang = DateTime.Now.AddDays(-18) },
                    new ThongBao { TieuDe = "Lịch thực hành Vật lý tuần 12", NoiDung = "Các lớp khối 10 sẽ thực hành đo điện trở tại phòng thí nghiệm số 2 theo lịch phân công. Học sinh mặc đồng phục và mang đầy đủ dụng cụ.", NguoiDangId = gv4u.Id, NgayDang = DateTime.Now.AddDays(-12) },
                    new ThongBao { TieuDe = "Thay đổi phòng học Hóa thực hành", NoiDung = "Do phòng thí nghiệm đang bảo trì thiết bị, tiết thực hành Hóa học tuần này chuyển sang phòng P.201. GV bộ môn xin lỗi vì sự bất tiện này.", NguoiDangId = gv5u.Id, NgayDang = DateTime.Now.AddDays(-3) }
                );
                await context.SaveChangesAsync();
            }

            // ===================== CLAIMS CHO AVATAR GV =====================
            foreach (var gv in new[] { 
                gv1u, gv2u, gv3u, gv4u, gv5u, gv6u, gv7u, gv8u, gv9u, gv10u,
                gv11u, gv12u, gv13u, gv14u, gv15u, gv16u, gv17u, gv18u, gv19u, gv20u,
                gv21u, gv22u, gv23u, gv24u, gv25u, gv26u, gv27u, gv28u, gv29u, gv30u 
            })
            {
                if (gv == null) continue;
                var claims = await userManager.GetClaimsAsync(gv);
                if (!claims.Any(c => c.Type == "AnhDaiDien"))
                    await userManager.AddClaimAsync(gv, new Claim("AnhDaiDien", "~/images/default-avatar.svg"));
            }

            // ===================== ĐỒNG BỘ ĐIỂM SỐ SANG ĐIỂM HỌC KỲ (CHO DỮ LIỆU SEED) =====================
            var dsDiemSo = await context.DiemSos.ToListAsync();
            var existingDiemHKKeys = await context.DiemHocKys
                .Select(d => new { d.HocSinhId, d.MonHocId, d.NamHoc, d.HocKy })
                .ToListAsync();
            var existingSet = existingDiemHKKeys
                .Select(x => $"{x.HocSinhId}_{x.MonHocId}_{x.NamHoc}_{x.HocKy}")
                .ToHashSet();

            var usersDict = await context.Users.ToDictionaryAsync(u => u.Id);

            var newDiemHocKys = new List<DiemHocKy>();
            foreach (var ds in dsDiemSo)
            {
                if (!usersDict.TryGetValue(ds.NguoiDungId, out var hs)) continue;

                string namHoc = hs.NamHoc ?? "2024-2025";
                int hocKy = 1; // Mặc định HK1 cho dữ liệu mẫu

                var key = $"{hs.Id}_{ds.MonHocId}_{namHoc}_{hocKy}";
                if (!existingSet.Contains(key))
                {
                    var dhk = new DiemHocKy
                    {
                        HocSinhId = hs.Id,
                        MonHocId = ds.MonHocId,
                        LopId = hs.LopId,
                        NamHoc = namHoc,
                        HocKy = hocKy,
                        DiemMieng1 = ds.Diem,
                        DiemMieng2 = ds.DiemMieng2,
                        DiemMieng3 = ds.DiemMieng3,
                        DiemMieng4 = ds.DiemMieng4,
                        DiemGiuaKy = ds.DiemGiuaKy,
                        DiemCuoiKy = ds.DiemCuoiKy,
                        GiaoVienId = ds.GiaoVienId,
                        NgayNhap = ds.NgayNhap,
                        NgayCapNhat = ds.NgayCapNhat
                    };

                    // Tính điểm tổng kết cho DiemHocKy
                    var listDiem = new List<double>();
                    if (dhk.DiemMieng1.HasValue) listDiem.Add(dhk.DiemMieng1.Value);
                    if (dhk.DiemMieng2.HasValue) listDiem.Add(dhk.DiemMieng2.Value);
                    if (dhk.DiemMieng3.HasValue) listDiem.Add(dhk.DiemMieng3.Value);
                    if (dhk.DiemMieng4.HasValue) listDiem.Add(dhk.DiemMieng4.Value);

                    if (dhk.DiemGiuaKy.HasValue && dhk.DiemCuoiKy.HasValue)
                    {
                        double avgMieng = listDiem.Any() ? listDiem.Average() : 0;
                        dhk.DiemTongKet = Math.Round((avgMieng + dhk.DiemGiuaKy.Value * 2 + dhk.DiemCuoiKy.Value * 3) / 6, 1);
                        dhk.XepLoai = dhk.DiemTongKet >= 8.0 ? "Giỏi" : dhk.DiemTongKet >= 6.5 ? "Khá" : dhk.DiemTongKet >= 5.0 ? "Trung bình" : "Yếu";
                    }

                    newDiemHocKys.Add(dhk);
                    existingSet.Add(key);
                }
            }
            if (newDiemHocKys.Any())
            {
                context.DiemHocKys.AddRange(newDiemHocKys);
                await context.SaveChangesAsync();
            }

            // ===================== ĐIỂM SỐ LỊCH SỬ: LỚP 11 (năm lớp 10) + LỚP 12 (năm lớp 10, 11) =====================
            {
                var lop11IdsH = allLops.Skip(5).Take(5).Select(l => l.Id).ToHashSet();
                var lop12IdsH = allLops.Skip(10).Take(5).Select(l => l.Id).ToHashSet();
                var hsLop11H = await context.Users.Where(u => u.LopId.HasValue && lop11IdsH.Contains(u.LopId.Value)).ToListAsync();
                var hsLop12H = await context.Users.Where(u => u.LopId.HasValue && lop12IdsH.Contains(u.LopId.Value)).ToListAsync();

                var monKhoi10H = monHocs.Take(10).ToList();
                var monKhoi11H = monHocs.Skip(10).Take(10).ToList();

                var diemHistList = new List<DiemHocKy>();

                void AddHistDiem(List<NguoiDung> hss, List<MonHoc> mons, string namHoc)
                {
                    foreach (var hs in hss)
                    {
                        foreach (var mon in mons)
                        {
                            for (int hk = 1; hk <= 2; hk++)
                            {
                                var key = $"{hs.Id}_{mon.Id}_{namHoc}_{hk}";
                                if (existingSet.Contains(key)) continue;

                                double dm1 = Math.Round(6.0 + Random2(0, 40) / 10.0, 1);
                                double dgk = Math.Round(6.5 + Random2(0, 35) / 10.0, 1);
                                double dck = Math.Round(7.0 + Random2(0, 30) / 10.0, 1);
                                double dtk = Math.Round((dm1 + dgk * 2 + dck * 3) / 6, 1);
                                string xl = dtk >= 8.0 ? "Giỏi" : dtk >= 6.5 ? "Khá" : dtk >= 5.0 ? "Trung bình" : "Yếu";

                                var dhk = new DiemHocKy
                                {
                                    HocSinhId = hs.Id,
                                    MonHocId = mon.Id,
                                    LopId = hs.LopId,
                                    NamHoc = namHoc,
                                    HocKy = hk,
                                    DiemMieng1 = dm1,
                                    DiemGiuaKy = dgk,
                                    DiemCuoiKy = dck,
                                    DiemTongKet = dtk,
                                    XepLoai = xl,
                                    NgayNhap = DateTime.Now.AddYears(namHoc == "2022-2023" ? -2 : -1),
                                    GiaoVienId = mon.GiaoVienId
                                };
                                diemHistList.Add(dhk);
                                existingSet.Add(key);
                            }
                        }
                    }
                }

                // Lớp 11: xem được điểm năm lớp 10 (2023-2024)
                AddHistDiem(hsLop11H, monKhoi10H, "2023-2024");
                // Lớp 12: xem được điểm năm lớp 11 (2023-2024)
                AddHistDiem(hsLop12H, monKhoi11H, "2023-2024");
                // Lớp 12: xem được điểm năm lớp 10 (2022-2023)
                AddHistDiem(hsLop12H, monKhoi10H, "2022-2023");

                if (diemHistList.Any())
                {
                    context.DiemHocKys.AddRange(diemHistList);
                    await context.SaveChangesAsync();
                }
            }

            // ===================== YÊU CẦU GIÁO VIÊN =====================
            if (!context.YeuCauGiaoVien.Any())
            {
                var listGiaoVienUsers = new[] { 
                    gv1u, gv2u, gv3u, gv4u, gv5u, gv6u, gv7u, gv8u, gv9u, gv10u,
                    gv11u, gv12u, gv13u, gv14u, gv15u, gv16u, gv17u, gv18u, gv19u, gv20u,
                    gv21u, gv22u, gv23u, gv24u, gv25u, gv26u, gv27u, gv28u, gv29u, gv30u 
                };

                var yeuCaus = new List<YeuCauGiaoVien>();
                var listLops = await context.Lops.ToListAsync();
                var listMons = await context.DanhSachMonHoc.ToListAsync();

                for (int i = 0; i < listGiaoVienUsers.Length; i++)
                {
                    var gv = listGiaoVienUsers[i];
                    if (gv == null) continue;

                    int checkType = i % 8;
                    if (checkType == 0)
                    {
                        yeuCaus.Add(new YeuCauGiaoVien
                        {
                            LoaiYeuCau = LoaiYeuCau.NghiPhep,
                            TieuDe = $"Đơn xin nghỉ phép học kỳ I - {gv.HoTen}",
                            MoTa = "Tôi có việc gia đình đột xuất ở quê cần giải quyết gấp, xin phép được nghỉ dạy các tiết trong ngày.",
                            TrangThai = i % 3 == 0 ? TrangThaiYeuCau.DaDuyet : TrangThaiYeuCau.ChoXuLy,
                            GiaoVienId = gv.Id,
                            NgayGui = DateTime.Now.AddDays(-i - 1),
                            NgayNghi = DateTime.Today.AddDays(i + 2),
                            NgayNghiKetThuc = DateTime.Today.AddDays(i + 2),
                            TuTiet = 1,
                            DenTiet = 3,
                            XuLyBoi = i % 3 == 0 ? htu?.Id : null,
                            NgayXuLy = i % 3 == 0 ? (DateTime?)DateTime.Now.AddDays(-i) : null,
                            GhiChu = i % 3 == 0 ? "Đã phê duyệt, yêu cầu sắp xếp dạy bù." : null
                        });
                    }
                    else if (checkType == 1)
                    {
                        var targetLop = listLops.ElementAtOrDefault(i % listLops.Count);
                        var targetMon = listMons.FirstOrDefault(m => m.GiaoVienId == gv.Id);
                        yeuCaus.Add(new YeuCauGiaoVien
                        {
                            LoaiYeuCau = LoaiYeuCau.HocBu,
                            TieuDe = $"Đăng ký dạy bù môn {targetMon?.TenMonHoc ?? "Chuyên môn"} - {gv.HoTen}",
                            MoTa = $"Đăng ký dạy bù cho lớp {targetLop?.TenLop ?? "10A1"} để hoàn thành tiến độ chương trình học tập của tuần.",
                            TrangThai = i % 3 == 0 ? TrangThaiYeuCau.DaDuyet : TrangThaiYeuCau.ChoXuLy,
                            GiaoVienId = gv.Id,
                            LopId = targetLop?.Id,
                            MonHocId = targetMon?.Id,
                            NgayGui = DateTime.Now.AddDays(-i - 1),
                            NgayNghi = DateTime.Today.AddDays(i + 3),
                            DanhSachTiet = "4,5",
                            XuLyBoi = i % 3 == 0 ? htu?.Id : null,
                            NgayXuLy = i % 3 == 0 ? (DateTime?)DateTime.Now.AddDays(-i) : null,
                            GhiChu = i % 3 == 0 ? "Đồng ý xếp lịch dạy bù." : null
                        });
                    }
                    else if (checkType == 2)
                    {
                        yeuCaus.Add(new YeuCauGiaoVien
                        {
                            LoaiYeuCau = LoaiYeuCau.YeuCauTaiNguyen,
                            TieuDe = $"Đề xuất trang bị máy chiếu phòng học - {gv.HoTen}",
                            MoTa = "Máy chiếu ở phòng học được phân công giảng dạy bị mờ và nhấp nháy liên tục, gây khó khăn cho việc truyền đạt bài học.",
                            TrangThai = TrangThaiYeuCau.ChoXuLy,
                            GiaoVienId = gv.Id,
                            NgayGui = DateTime.Now.AddDays(-i - 1)
                        });
                    }
                    else if (checkType == 3)
                    {
                        yeuCaus.Add(new YeuCauGiaoVien
                        {
                            LoaiYeuCau = LoaiYeuCau.DoiLich,
                            TieuDe = $"Xin đổi lịch giảng dạy tuần tới - {gv.HoTen}",
                            MoTa = "Do trùng lịch đi tập huấn chuyên môn của Sở Giáo dục và Đào tạo, tôi xin phép đổi buổi dạy sang ngày khác.",
                            TrangThai = TrangThaiYeuCau.ChoXuLy,
                            GiaoVienId = gv.Id,
                            NgayGui = DateTime.Now.AddDays(-i - 2)
                        });
                    }
                    else if (checkType == 4)
                    {
                        yeuCaus.Add(new YeuCauGiaoVien
                        {
                            LoaiYeuCau = LoaiYeuCau.ThayDoiLopChuNhiem,
                            TieuDe = $"Đề xuất chuyển giao công tác chủ nhiệm - {gv.HoTen}",
                            MoTa = "Vì lý do sức khỏe cá nhân không đảm bảo hoàn thành tốt nhiệm vụ, tôi xin phép được bàn giao lớp chủ nhiệm cho giáo viên khác vào học kỳ sau.",
                            TrangThai = TrangThaiYeuCau.ChoXuLy,
                            GiaoVienId = gv.Id,
                            NgayGui = DateTime.Now.AddDays(-i - 5)
                        });
                    }
                    else if (checkType == 5)
                    {
                        yeuCaus.Add(new YeuCauGiaoVien
                        {
                            LoaiYeuCau = LoaiYeuCau.TangCapCongChuc,
                            TieuDe = $"Nộp hồ sơ xét nâng hạng chức danh nghề nghiệp - {gv.HoTen}",
                            MoTa = "Nộp báo cáo thành tích giảng dạy, nghiên cứu khoa học cùng các chứng chỉ cần thiết để phục vụ xét nâng hạng chức danh nghề nghiệp.",
                            TrangThai = TrangThaiYeuCau.DaDuyet,
                            GiaoVienId = gv.Id,
                            XuLyBoi = htu?.Id,
                            NgayGui = DateTime.Now.AddDays(-15),
                            NgayXuLy = DateTime.Now.AddDays(-10),
                            GhiChu = "Hồ sơ hợp lệ, đã chuyển ban thi đua."
                        });
                    }
                    else if (checkType == 6)
                    {
                        yeuCaus.Add(new YeuCauGiaoVien
                        {
                            LoaiYeuCau = LoaiYeuCau.DanhGiaHangNam,
                            TieuDe = $"Báo cáo tự đánh giá kết quả công tác năm học - {gv.HoTen}",
                            MoTa = "Gửi bản tự đánh giá kết quả thực hiện nhiệm vụ năm học, tự nhận xếp loại hoàn thành xuất sắc nhiệm vụ.",
                            TrangThai = TrangThaiYeuCau.ChoXuLy,
                            GiaoVienId = gv.Id,
                            NgayGui = DateTime.Now.AddDays(-3)
                        });
                    }
                    else
                    {
                        yeuCaus.Add(new YeuCauGiaoVien
                        {
                            LoaiYeuCau = LoaiYeuCau.Khac,
                            TieuDe = $"Đề xuất tổ chức ngoại khóa cho học sinh - {gv.HoTen}",
                            MoTa = "Đề xuất tổ chức một buổi ngoại khóa trải nghiệm thực tế tìm hiểu lịch sử địa phương cho học sinh lớp chủ nhiệm.",
                            TrangThai = TrangThaiYeuCau.ChoXuLy,
                            GiaoVienId = gv.Id,
                            NgayGui = DateTime.Now.AddDays(-2)
                        });
                    }
                }

                context.YeuCauGiaoVien.AddRange(yeuCaus);
                await context.SaveChangesAsync();
            }

            // ===================== BÌNH LUẬN TƯƠNG TÁC SÂU =====================
            if (!context.DanhSachBinhLuan.Any())
            {
                var sampleBaiGiang = await context.DanhSachBaiGiang.FirstOrDefaultAsync();
                var sampleBaiTap = await context.DanhSachBaiTap.FirstOrDefaultAsync();
                var sampleHocSinh = hsList.FirstOrDefault();
                var hs2 = hsList.Skip(1).FirstOrDefault();

                if (sampleBaiGiang != null && sampleHocSinh != null && hs2 != null)
                {
                    var comment1 = new BinhLuan
                    {
                        NoiDung = "Bài giảng rất dễ hiểu ạ! Cảm ơn thầy cô.",
                        NgayTao = DateTime.Now.AddDays(-2),
                        NguoiDungId = sampleHocSinh.Id,
                        BaiGiangId = sampleBaiGiang.Id
                    };
                    context.DanhSachBinhLuan.Add(comment1);
                    await context.SaveChangesAsync();

                    var reply1 = new BinhLuan
                    {
                        NoiDung = "Cảm ơn em. Hãy làm thêm bài tập ôn tập ở chương 1 nhé.",
                        NgayTao = DateTime.Now.AddDays(-1),
                        NguoiDungId = gv1u.Id,
                        BaiGiangId = sampleBaiGiang.Id,
                        ParentId = comment1.Id
                    };
                    context.DanhSachBinhLuan.Add(reply1);
                    await context.SaveChangesAsync();

                    context.DanhSachBinhLuan.Add(new BinhLuan
                    {
                        NoiDung = "Dạ vâng ạ, phần trắc nghiệm chương này em đạt 9/10 luôn thầy ơi!",
                        NgayTao = DateTime.Now.AddMinutes(-30),
                        NguoiDungId = sampleHocSinh.Id,
                        BaiGiangId = sampleBaiGiang.Id,
                        ParentId = reply1.Id
                    });

                    var comment2 = new BinhLuan
                    {
                        NoiDung = "Thầy ơi cho em hỏi phần công thức ở trang 4 có lưu ý gì đặc biệt khi làm trắc nghiệm không ạ?",
                        NgayTao = DateTime.Now.AddDays(-1),
                        NguoiDungId = hs2.Id,
                        BaiGiangId = sampleBaiGiang.Id
                    };
                    context.DanhSachBinhLuan.Add(comment2);
                    await context.SaveChangesAsync();

                    context.DanhSachBinhLuan.Add(new BinhLuan
                    {
                        NoiDung = "Em cần đặc biệt lưu ý điều kiện của biến số để không bị lừa nhé.",
                        NgayTao = DateTime.Now.AddHours(-5),
                        NguoiDungId = gv1u.Id,
                        BaiGiangId = sampleBaiGiang.Id,
                        ParentId = comment2.Id
                    });
                }

                if (sampleBaiTap != null && sampleHocSinh != null)
                {
                    context.DanhSachBinhLuan.Add(new BinhLuan
                    {
                        NoiDung = "Thầy cô cho em hỏi hạn nộp bài này có được gia hạn thêm không ạ?",
                        NgayTao = DateTime.Now.AddDays(-3),
                        NguoiDungId = sampleHocSinh.Id,
                        BaiTapId = sampleBaiTap.Id
                    });
                }
            }

            // ===================== ĐĂNG KÝ HỌC (ELECTIVE ENROLLMENT) =====================
            if (!context.DanhSachDangKy.Any())
            {
                var sampleMon = monHocs.FirstOrDefault();
                if (sampleMon != null)
                {
                    context.DanhSachDangKy.AddRange(
                        new DangKyHoc
                        {
                            HocSinhId = hsList[0].Id,
                            MonHocId = sampleMon.Id,
                            TrangThai = TrangThaiDangKy.DaXetDuyet,
                            NgayDangKy = DateTime.Now.AddDays(-10),
                            GhiChu = "Đã duyệt vào lớp chọn"
                        },
                        new DangKyHoc
                        {
                            HocSinhId = hsList[1].Id,
                            MonHocId = sampleMon.Id,
                            TrangThai = TrangThaiDangKy.ChoXetDuyet,
                            NgayDangKy = DateTime.Now.AddDays(-2),
                            GhiChu = "Đăng ký bổ trợ nâng cao"
                        },
                        new DangKyHoc
                        {
                            HocSinhId = hsList[2].Id,
                            MonHocId = sampleMon.Id,
                            TrangThai = TrangThaiDangKy.TuChoi,
                            NgayDangKy = DateTime.Now.AddDays(-5),
                            GhiChu = "Không đủ điều kiện điểm đầu vào"
                        }
                    );
                    await context.SaveChangesAsync();
                }
            }

            // ===================== ĐỒNG BỘ ĐIỂM HỌC KỲ II =====================
            var dsDiemSoL2 = await context.DiemSos.ToListAsync();
            var newDiemHocKysL2 = new List<DiemHocKy>();
            foreach (var ds in dsDiemSoL2)
            {
                if (!usersDict.TryGetValue(ds.NguoiDungId, out var hs)) continue;

                string namHoc = hs.NamHoc ?? "2024-2025";
                int hocKy = 2; // Sinh thêm học kỳ 2

                var key = $"{hs.Id}_{ds.MonHocId}_{namHoc}_{hocKy}";
                if (!existingSet.Contains(key))
                {
                    var dhk = new DiemHocKy
                    {
                        HocSinhId = hs.Id,
                        MonHocId = ds.MonHocId,
                        LopId = hs.LopId,
                        NamHoc = namHoc,
                        HocKy = hocKy,
                        DiemMieng1 = Math.Round(Math.Min(10.0, (ds.Diem ?? 7.0) + 0.5), 1),
                        DiemMieng2 = Math.Round(Math.Min(10.0, (ds.DiemMieng2 ?? 7.0) + 0.8), 1),
                        DiemMieng3 = ds.DiemMieng3,
                        DiemMieng4 = ds.DiemMieng4,
                        DiemGiuaKy = Math.Round(Math.Min(10.0, (ds.DiemGiuaKy ?? 7.0) + 0.4), 1),
                        DiemCuoiKy = Math.Round(Math.Min(10.0, (ds.DiemCuoiKy ?? 7.0) + 0.6), 1),
                        GiaoVienId = ds.GiaoVienId,
                        NgayNhap = DateTime.Now.AddDays(-1),
                        NgayCapNhat = DateTime.Now
                    };

                    var listDiem = new List<double>();
                    if (dhk.DiemMieng1.HasValue) listDiem.Add(dhk.DiemMieng1.Value);
                    if (dhk.DiemMieng2.HasValue) listDiem.Add(dhk.DiemMieng2.Value);
                    if (dhk.DiemMieng3.HasValue) listDiem.Add(dhk.DiemMieng3.Value);
                    if (dhk.DiemMieng4.HasValue) listDiem.Add(dhk.DiemMieng4.Value);

                    if (dhk.DiemGiuaKy.HasValue && dhk.DiemCuoiKy.HasValue)
                    {
                        double avgMieng = listDiem.Any() ? listDiem.Average() : 0;
                        dhk.DiemTongKet = Math.Round((avgMieng + dhk.DiemGiuaKy.Value * 2 + dhk.DiemCuoiKy.Value * 3) / 6, 1);
                        dhk.XepLoai = dhk.DiemTongKet >= 8.0 ? "Giỏi" : dhk.DiemTongKet >= 6.5 ? "Khá" : dhk.DiemTongKet >= 5.0 ? "Trung bình" : "Yếu";
                    }

                    newDiemHocKysL2.Add(dhk);
                    existingSet.Add(key);
                }
            }
            if (newDiemHocKysL2.Any())
            {
                context.DiemHocKys.AddRange(newDiemHocKysL2);
                await context.SaveChangesAsync();
            }

            // ===================== LỊCH SỬ HỌC SINH (ARCHIVED) =====================
            if (!context.LichSuHocSinhs.Any())
            {
                var history1 = new LichSuHocSinh
                {
                    HoTen = "NGUYỄN VĂN THÀNH",
                    MaHocSinh = "HS2022099",
                    NgaySinh = new DateTime(2007, 5, 12),
                    GioiTinh = "Nam",
                    DiaChi = "Hà Nội",
                    TenLop = "12A1",
                    TenKhoi = "Khối 12",
                    NamHocCuoi = "2023-2024",
                    LyDoXoa = "Tốt nghiệp THPT",
                    TrangThai = "TốtNghiệp",
                    NgayXoa = DateTime.Now.AddMonths(-2),
                    NguoiXoaHoTen = "Lê Minh Hiệu",
                    NguoiDungIdGoc = Guid.NewGuid().ToString()
                };

                var history2 = new LichSuHocSinh
                {
                    HoTen = "PHẠM MINH TUẤN",
                    MaHocSinh = "HS2023150",
                    NgaySinh = new DateTime(2008, 9, 20),
                    GioiTinh = "Nam",
                    DiaChi = "Hải Phòng",
                    TenLop = "11A2",
                    TenKhoi = "Khối 11",
                    NamHocCuoi = "2023-2024",
                    LyDoXoa = "Chuyển trường về quê sinh sống cùng gia đình",
                    TrangThai = "ChuyểnTrường",
                    NgayXoa = DateTime.Now.AddMonths(-1),
                    NguoiXoaHoTen = "Lê Minh Hiệu",
                    NguoiDungIdGoc = Guid.NewGuid().ToString()
                };

                context.LichSuHocSinhs.AddRange(history1, history2);
                await context.SaveChangesAsync();

                context.LichSuDiemHocSinhs.AddRange(
                    new LichSuDiemHocSinh
                    {
                        LichSuHocSinhId = history1.Id,
                        TenMonHoc = "Toán học 12",
                        NamHoc = "2023-2024",
                        HocKy = 2,
                        TenLop = "12A1",
                        DiemGiuaKy = 8.5,
                        DiemCuoiKy = 9.0,
                        DiemTongKet = 8.8,
                        XepLoai = "Giỏi",
                        NhanXet = "Học sinh xuất sắc, học tập chăm chỉ."
                    },
                    new LichSuDiemHocSinh
                    {
                        LichSuHocSinhId = history1.Id,
                        TenMonHoc = "Ngữ văn 12",
                        NamHoc = "2023-2024",
                        HocKy = 2,
                        TenLop = "12A1",
                        DiemGiuaKy = 7.5,
                        DiemCuoiKy = 8.0,
                        DiemTongKet = 7.8,
                        XepLoai = "Khá",
                        NhanXet = "Có khả năng cảm thụ văn học rất tốt."
                    },
                    new LichSuDiemHocSinh
                    {
                        LichSuHocSinhId = history2.Id,
                        TenMonHoc = "Vật lý 11",
                        NamHoc = "2023-2024",
                        HocKy = 1,
                        TenLop = "11A2",
                        DiemGiuaKy = 8.0,
                        DiemCuoiKy = 7.5,
                        DiemTongKet = 7.7,
                        XepLoai = "Khá",
                        NhanXet = "Thông minh, có năng khiếu tự nhiên môn Vật lý."
                    }
                );
            }

            if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync();

            // ===================== DATA MIGRATION MÃ HỌC SINH CŨ =====================
            var hsCu = await context.Users.Where(u => u.MaHocSinh != null && u.MaHocSinh.StartsWith("HS")).ToListAsync();
            if (hsCu.Any())
            {
                var dictLopCounts = new Dictionary<int, int>();
                var passwordHasher = new PasswordHasher<NguoiDung>();
                
                foreach (var hs in hsCu)
                {
                    if (hs.LopId == null) continue;
                    var lop = await context.Lops.Include(l => l.Khoi).FirstOrDefaultAsync(l => l.Id == hs.LopId);
                    if (lop == null) continue;

                    string tenLop = lop.TenLop ?? "";
                    string tenKhoi = lop.Khoi?.TenKhoi ?? "";
                    int currentAcademicYear = DateTime.Now.Year;
                    if (tenLop.StartsWith("11") || tenKhoi == "Khối 11") currentAcademicYear -= 1;
                    else if (tenLop.StartsWith("12") || tenKhoi == "Khối 12") currentAcademicYear -= 2;

                    string prefixYear = (currentAcademicYear % 100).ToString("D2");
                    string lopCode = System.Text.RegularExpressions.Regex.Replace(tenLop, @"^\d+", "").ToUpper();
                    string prefix = prefixYear + lopCode;

                    if (!dictLopCounts.ContainsKey(lop.Id))
                    {
                        int currentCount = await context.Users.CountAsync(u => u.LopId == lop.Id && u.MaHocSinh != null && !u.MaHocSinh.StartsWith("HS") && u.MaHocSinh.StartsWith(prefix));
                        dictLopCounts[lop.Id] = currentCount;
                    }
                    
                    dictLopCounts[lop.Id]++;
                    string maHsMoi = $"{prefix}{dictLopCounts[lop.Id]:D3}";

                    hs.MaHocSinh = maHsMoi;
                    hs.UserName = maHsMoi;
                    hs.NormalizedUserName = maHsMoi.ToUpper();
                    hs.Email = $"{maHsMoi}@truong.edu.vn".ToLower();
                    hs.NormalizedEmail = hs.Email.ToUpper();
                    hs.PasswordHash = passwordHasher.HashPassword(hs, maHsMoi);
                }
                
                await context.SaveChangesAsync();
            }

            // ===================== ĐỒNG BỘ DIEMSО TỪ DIEMHOCKY (để tiến độ > 0%) =====================
            // Cập nhật DiemSo.DiemGiuaKy và DiemSo.DiemCuoiKy từ DiemHocKy HK1 cho từng HS
            // Điều này đảm bảo trang TienDo của GV hiển thị điểm thay vì 0.
            {
                var dsDiemHK1 = await context.DiemHocKys
                    .Where(d => d.HocKy == 1 && d.DiemGiuaKy.HasValue && d.DiemCuoiKy.HasValue)
                    .ToListAsync();

                var allDiemSo = await context.DiemSos.ToListAsync();
                var diemSoDict = allDiemSo.GroupBy(d => (d.NguoiDungId, d.MonHocId))
                    .ToDictionary(g => g.Key, g => g.First());

                bool diemSoUpdated = false;
                foreach (var dhk in dsDiemHK1)
                {
                    var key = (dhk.HocSinhId, dhk.MonHocId);
                    if (diemSoDict.TryGetValue(key, out var ds))
                    {
                        // Chỉ cập nhật nếu chưa có điểm
                        if (!ds.DiemGiuaKy.HasValue || !ds.DiemCuoiKy.HasValue)
                        {
                            ds.DiemGiuaKy = dhk.DiemGiuaKy;
                            ds.DiemCuoiKy = dhk.DiemCuoiKy;
                            ds.Diem = dhk.DiemMieng1;
                            ds.NgayCapNhat = DateTime.Now;
                            diemSoUpdated = true;
                        }
                    }
                }
                if (diemSoUpdated) await context.SaveChangesAsync();
            }

            // ===================== BỔ SUNG BAINOP CHO BÀI ĐANG MỞ (để tiến độ tăng lên) =====================
            // Hỗ trợ tăng tỉ lệ hoàn thành: thêm bài nộp cho bài DangMo với xác suất 75%
            {
                var baiTapDangMo = await context.DanhSachBaiTap
                    .Where(b => b.TrangThai == TrangThaiBaiTap.DangMo)
                    .ToListAsync();

                if (baiTapDangMo.Any())
                {
                    var allHocSinhIds = await context.UserRoles
                        .Join(context.Roles.Where(r => r.Name == "HocSinh"),
                              ur => ur.RoleId, r => r.Id, (ur, r) => ur.UserId)
                        .ToListAsync();

                    var existingNopSet = await context.DanhSachBaiNop
                        .Select(n => new { n.HocSinhId, n.BaiTapId })
                        .ToListAsync();
                    var existingNopHash = existingNopSet
                        .Select(x => $"{x.HocSinhId}_{x.BaiTapId}")
                        .ToHashSet();

                    var lopMonHocMap = await context.LopMonHocs
                        .ToListAsync();
                    var monBaiTapMap = baiTapDangMo.ToDictionary(b => b.Id, b => b.MonHocId);

                    var hocSinhLopMap = await context.Users
                        .Where(u => allHocSinhIds.Contains(u.Id) && u.LopId.HasValue)
                        .Select(u => new { u.Id, u.LopId })
                        .ToListAsync();

                    var newNops = new List<BaiNop>();
                    int counter = 0;
                    foreach (var hs in hocSinhLopMap)
                    {
                        // Lấy môn học của lớp HS
                        var monIdsOfLop = lopMonHocMap
                            .Where(lm => lm.LopId == hs.LopId)
                            .Select(lm => lm.MonHocId)
                            .ToHashSet();

                        foreach (var bt in baiTapDangMo)
                        {
                            if (!monIdsOfLop.Contains(bt.MonHocId)) continue;
                            var key = $"{hs.Id}_{bt.Id}";
                            if (existingNopHash.Contains(key)) continue;

                            // Xác suất 80% học sinh nộp bài đang mở
                            counter++;
                            if (counter % 5 == 0) continue; // bỏ qua 1/5 → 80% nộp

                            var diem = Math.Round(5.5 + Random2(0, 45) / 10.0, 1);
                            newNops.Add(new BaiNop
                            {
                                BaiTapId = bt.Id,
                                HocSinhId = hs.Id,
                                NoiDung = "Em đã hoàn thành bài tập theo yêu cầu của thầy/cô.",
                                DuongDanFile = "/uploads/bainop/bai_tap_ve_nha.pdf",
                                NgayNop = bt.HanNop.AddDays(-Random2(1, 5)),
                                TrangThai = TrangThaiBaiNop.ChamXong,
                                Diem = diem,
                                NhanXet = diem >= 8 ? "Bài làm tốt, trình bày rõ ràng." : "Cần cố gắng hơn ở phần lý thuyết.",
                                NgayCham = bt.HanNop
                            });

                            if (newNops.Count >= 5000)
                            {
                                context.DanhSachBaiNop.AddRange(newNops);
                                await context.SaveChangesAsync();
                                newNops.Clear();
                            }
                        }
                    }

                    if (newNops.Any())
                    {
                        context.DanhSachBaiNop.AddRange(newNops);
                        await context.SaveChangesAsync();
                    }
                }
            }

            // ===================== ĐỒNG BỘ GIÁO VIÊN BẰNG MÃ GIÁO VIÊN =====================
            var syncPasswordHasher = new PasswordHasher<NguoiDung>();
            var teachers = await context.Users
                .Where(u => u.Email != null && u.Email.StartsWith("gv") && u.Email.EndsWith("@lms.com"))
                .ToListAsync();

            bool teacherUpdated = false;
            foreach (var gv in teachers)
            {
                var emailPrefix = gv.Email.Split('@')[0];
                if (emailPrefix.StartsWith("gv") && int.TryParse(emailPrefix.Substring(2), out int gvNum))
                {
                    string maSo = $"GV{gvNum:D3}";
                    if (gv.UserName != maSo)
                    {
                        gv.UserName = maSo;
                        gv.NormalizedUserName = maSo.ToUpper();
                        teacherUpdated = true;
                    }

                    var verificationResult = syncPasswordHasher.VerifyHashedPassword(gv, gv.PasswordHash ?? "", maSo);
                    if (verificationResult != PasswordVerificationResult.Success)
                    {
                        gv.PasswordHash = syncPasswordHasher.HashPassword(gv, maSo);
                        gv.SecurityStamp = Guid.NewGuid().ToString();
                        teacherUpdated = true;
                    }

                    string expectedGioiTinh = "Nam";
                    string nameLower = gv.HoTen.ToLower();
                    if (nameLower.Contains("thị") || nameLower.Contains("nữ") || nameLower.Contains("như") || 
                        nameLower.Contains("hà") || nameLower.Contains("chi") || nameLower.Contains("yến") || 
                        nameLower.Contains("hoa") || nameLower.Contains("dung") || nameLower.Contains("phương") || 
                        nameLower.Contains("lan") || (nameLower.Contains("anh") && nameLower.Contains("mai")) || 
                        nameLower.Contains("ngọc"))
                    {
                        expectedGioiTinh = "Nữ";
                    }

                    if (gv.GioiTinh != expectedGioiTinh)
                    {
                        gv.GioiTinh = expectedGioiTinh;
                        teacherUpdated = true;
                    }
                }
            }

            if (teacherUpdated)
            {
                await context.SaveChangesAsync();
            }

            // ===================== ĐỒNG BỘ MẬT KHẨU HỌC SINH BẰNG MÃ HỌC SINH =====================
            var students = await context.Users
                .Where(u => u.MaHocSinh != null)
                .ToListAsync();

            bool updated = false;

            foreach (var hs in students)
            {
                string expectedPassword = hs.MaHocSinh.Trim();
                
                var verificationResult = syncPasswordHasher.VerifyHashedPassword(hs, hs.PasswordHash ?? "", expectedPassword);
                if (verificationResult != PasswordVerificationResult.Success)
                {
                    hs.PasswordHash = syncPasswordHasher.HashPassword(hs, expectedPassword);
                    hs.SecurityStamp = Guid.NewGuid().ToString();
                    updated = true;
                }
            }

            if (updated)
            {
                await context.SaveChangesAsync();
            }
        }

        private static int _seed = 42;
        private static int Random2(int min, int max)
        {
            _seed = (_seed * 1103515245 + 12345) & 0x7fffffff;
            return min + (_seed % (max - min + 1));
        }

        private static async Task<NguoiDung?> CreateUser(
            UserManager<NguoiDung> userManager,
            string email, string password, string role, string hoTen,
            string? chuyenMon = null, string? chucVu = null,
            string? maHocSinh = null, string? hanhKiem = null,
            string? userName = null, string? gioiTinh = null)
        {
            var un = userName ?? email;
            if (userManager.Users.Any(u => u.Email == email || u.UserName == un)) return null;

            var user = new NguoiDung
            {
                UserName = un, Email = email, HoTen = hoTen,
                EmailConfirmed = true, IsActive = true,
                ChuyenMon = chuyenMon, ChucVu = chucVu,
                MaHocSinh = maHocSinh, HanhKiem = hanhKiem,
                GioiTinh = gioiTinh,
                NgaySinh = new DateTime(2000, 1, 1).AddDays(Random2(0, 3650)),
                NgayTao = DateTime.Now
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                await userManager.AddClaimAsync(user, new Claim("AnhDaiDien", "~/images/default-avatar.svg"));
                return user;
            }
            
            Console.WriteLine($"[ERROR] Lỗi tạo user {email}: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        private static async Task AlignLichHocGiaoVien(ApplicationDbContext context)
        {
            var lopMonHocs = await context.LopMonHocs.ToListAsync();
            var dict = lopMonHocs
                .Where(x => x.GiaoVienId != null)
                .ToDictionary(x => (x.LopId, x.MonHocId), x => x.GiaoVienId);

            var lichHocs = await context.LichHocs.ToListAsync();
            bool changed = false;

            foreach (var lich in lichHocs)
            {
                if (dict.TryGetValue((lich.LopId, lich.MonHocId), out var expectedGvId))
                {
                    if (lich.GiaoVienId != expectedGvId)
                    {
                        lich.GiaoVienId = expectedGvId;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                await context.SaveChangesAsync();
            }
        }
    }
}