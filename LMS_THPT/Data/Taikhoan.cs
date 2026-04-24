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
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<NguoiDung>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ============================================================
            // 1. TẠO ROLES
            // ============================================================
            string[] roles = { "Admin", "GiaoVien", "HocSinh", "HieuTruong" };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // ============================================================
            // 2. TẠO ADMIN
            // ============================================================
            if (await userManager.FindByEmailAsync("admin@lms.com") == null)
            {
                var admin = new NguoiDung
                {
                    UserName = "admin@lms.com", Email = "admin@lms.com",
                    HoTen = "Nguyễn Quản Trị", EmailConfirmed = true,
                    IsActive = true, NgayTao = DateTime.Now, ChucVu = "Quản trị viên"
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ============================================================
            // 3. TẠO HIỆU TRƯỞNG
            // ============================================================
            if (await userManager.FindByEmailAsync("hieutruong@lms.com") == null)
            {
                var ht = new NguoiDung
                {
                    UserName = "hieutruong@lms.com", Email = "hieutruong@lms.com",
                    HoTen = "Trần Văn Hiệu Trưởng", EmailConfirmed = true,
                    IsActive = true, NgayTao = DateTime.Now, ChucVu = "Hiệu trưởng"
                };
                await userManager.CreateAsync(ht, "Ht@123456");
                await userManager.AddToRoleAsync(ht, "HieuTruong");
            }

            // ============================================================
            // 4. TẠO CÁC KHỐI (10, 11, 12) — chỉ seed nếu chưa có
            // ============================================================
            if (!context.Khois.Any())
            {
                context.Khois.AddRange(
                    new Khoi { TenKhoi = "Khối 10" },
                    new Khoi { TenKhoi = "Khối 11" },
                    new Khoi { TenKhoi = "Khối 12" }
                );
                await context.SaveChangesAsync();
            }

            var khoi10 = await context.Khois.FirstAsync(k => k.TenKhoi == "Khối 10");
            var khoi11 = await context.Khois.FirstAsync(k => k.TenKhoi == "Khối 11");
            var khoi12 = await context.Khois.FirstAsync(k => k.TenKhoi == "Khối 12");

            // ============================================================
            // 5. TẠO CÁC MÔN HỌC
            // ============================================================
            if (!context.MonHocs.Any())
            {
                context.MonHocs.AddRange(
                    // Khối 10
                    new MonHoc { TenMonHoc = "Toán 10", MoTa = "Đại số và Hình học lớp 10", MucTieu = "Nắm vững nền tảng Toán học THPT", KhoiId = khoi10.Id, IsActive = true, NgayTao = DateTime.Now },
                    new MonHoc { TenMonHoc = "Vật Lý 10", MoTa = "Cơ học, Nhiệt học, Điện học lớp 10", MucTieu = "Hiểu bản chất các hiện tượng vật lý", KhoiId = khoi10.Id, IsActive = true, NgayTao = DateTime.Now },
                    new MonHoc { TenMonHoc = "Hóa Học 10", MoTa = "Cấu tạo nguyên tử, Bảng tuần hoàn, Liên kết hóa học", MucTieu = "Nắm vững lý thuyết hóa học đại cương", KhoiId = khoi10.Id, IsActive = true, NgayTao = DateTime.Now },
                    new MonHoc { TenMonHoc = "Ngữ Văn 10", MoTa = "Văn học Việt Nam và thế giới lớp 10", MucTieu = "Rèn luyện kỹ năng đọc hiểu và viết", KhoiId = khoi10.Id, IsActive = true, NgayTao = DateTime.Now },
                    // Khối 11
                    new MonHoc { TenMonHoc = "Toán 11", MoTa = "Giải tích, Đại số Tổ hợp lớp 11", MucTieu = "Phát triển tư duy toán học", KhoiId = khoi11.Id, IsActive = true, NgayTao = DateTime.Now },
                    new MonHoc { TenMonHoc = "Vật Lý 11", MoTa = "Điện từ học lớp 11", MucTieu = "Hiểu sâu về điện và từ trường", KhoiId = khoi11.Id, IsActive = true, NgayTao = DateTime.Now },
                    new MonHoc { TenMonHoc = "Hóa Học 11", MoTa = "Hóa hữu cơ và vô cơ lớp 11", MucTieu = "Ứng dụng hóa học trong thực tiễn", KhoiId = khoi11.Id, IsActive = true, NgayTao = DateTime.Now },
                    // Khối 12
                    new MonHoc { TenMonHoc = "Toán 12", MoTa = "Tích phân, Số phức, Không gian lớp 12", MucTieu = "Chuẩn bị thi THPTQG", KhoiId = khoi12.Id, IsActive = true, NgayTao = DateTime.Now },
                    new MonHoc { TenMonHoc = "Vật Lý 12", MoTa = "Dao động, Sóng, Quang học lớp 12", MucTieu = "Ôn tập và nâng cao", KhoiId = khoi12.Id, IsActive = true, NgayTao = DateTime.Now },
                    new MonHoc { TenMonHoc = "Tiếng Anh", MoTa = "Tiếng Anh giao tiếp và học thuật", MucTieu = "Đạt trình độ B1-B2", KhoiId = khoi12.Id, IsActive = true, NgayTao = DateTime.Now }
                );
                await context.SaveChangesAsync();
            }

            // ============================================================
            // 6. TẠO CÁC LỚP
            // ============================================================
            if (!context.Lops.Any())
            {
                context.Lops.AddRange(
                    new Lop { TenLop = "10A1", MaKhoi = khoi10.Id },
                    new Lop { TenLop = "10A2", MaKhoi = khoi10.Id },
                    new Lop { TenLop = "11A1", MaKhoi = khoi11.Id },
                    new Lop { TenLop = "11A2", MaKhoi = khoi11.Id },
                    new Lop { TenLop = "12A1", MaKhoi = khoi12.Id },
                    new Lop { TenLop = "12A2", MaKhoi = khoi12.Id }
                );
                await context.SaveChangesAsync();
            }

            var lop10A1 = await context.Lops.FirstAsync(l => l.TenLop == "10A1");
            var lop10A2 = await context.Lops.FirstAsync(l => l.TenLop == "10A2");
            var lop11A1 = await context.Lops.FirstAsync(l => l.TenLop == "11A1");
            var lop11A2 = await context.Lops.FirstAsync(l => l.TenLop == "11A2");
            var lop12A1 = await context.Lops.FirstAsync(l => l.TenLop == "12A1");
            var lop12A2 = await context.Lops.FirstAsync(l => l.TenLop == "12A2");

            // ============================================================
            // 7. TẠO GIÁO VIÊN
            // ============================================================
            async Task<NguoiDung> EnsureTeacher(string email, string hoTen, string chuyenMon)
            {
                var gv = await userManager.FindByEmailAsync(email);
                if (gv == null)
                {
                    gv = new NguoiDung
                    {
                        UserName = email, Email = email, HoTen = hoTen,
                        EmailConfirmed = true, IsActive = true,
                        NgayTao = DateTime.Now, ChuyenMon = chuyenMon, ChucVu = "Giáo viên",
                        GioiTinh = hoTen.StartsWith("Nguyễn Thị") || hoTen.StartsWith("Trần Thị") ? "Nữ" : "Nam"
                    };
                    await userManager.CreateAsync(gv, "Gv@123456");
                    await userManager.AddToRoleAsync(gv, "GiaoVien");
                }
                return gv;
            }

            var gvToan  = await EnsureTeacher("gv.toan@lms.com",    "Nguyễn Văn Toán",    "Toán học");
            var gvLy    = await EnsureTeacher("gv.ly@lms.com",      "Trần Thị Lý",        "Vật Lý");
            var gvHoa   = await EnsureTeacher("gv.hoa@lms.com",     "Lê Minh Hóa",        "Hóa Học");
            var gvAnh   = await EnsureTeacher("gv.anh@lms.com",     "Phạm Thị Anh",       "Tiếng Anh");

            // Gán GVCN cho một số lớp
            if (lop10A1.GiaoVienChuNhiemId == null)
            {
                lop10A1.GiaoVienChuNhiemId = gvToan.Id;
                lop11A1.GiaoVienChuNhiemId = gvLy.Id;
                lop12A1.GiaoVienChuNhiemId = gvAnh.Id;
                await context.SaveChangesAsync();
            }

            // ============================================================    
            // 8. TẠO HỌC SINH
            // ============================================================
            var hocSinhData = new[]
            {
                // Lớp 10A1
                ("hs001@lms.com","Nguyễn Anh Tuấn","Nam","2009-03-15","HS001",lop10A1.Id),
                ("hs002@lms.com","Trần Thị Mai Anh","Nữ","2009-07-22","HS002",lop10A1.Id),
                ("hs003@lms.com","Lê Hoàng Nam","Nam","2009-11-08","HS003",lop10A1.Id),
                ("hs004@lms.com","Phạm Như Quỳnh","Nữ","2009-05-30","HS004",lop10A1.Id),
                ("hs005@lms.com","Hoàng Văn Đức","Nam","2009-01-14","HS005",lop10A1.Id),
                // Lớp 10A2
                ("hs006@lms.com","Vũ Thị Lan","Nữ","2009-09-03","HS006",lop10A2.Id),
                ("hs007@lms.com","Đặng Quốc Hùng","Nam","2009-04-19","HS007",lop10A2.Id),
                ("hs008@lms.com","Bùi Thị Hương","Nữ","2009-12-25","HS008",lop10A2.Id),
                // Lớp 11A1
                ("hs009@lms.com","Nguyễn Minh Khoa","Nam","2008-06-10","HS009",lop11A1.Id),
                ("hs010@lms.com","Trần Phương Thảo","Nữ","2008-02-28","HS010",lop11A1.Id),
                ("hs011@lms.com","Lê Thanh Tùng","Nam","2008-08-17","HS011",lop11A1.Id),
                ("hs012@lms.com","Phạm Khánh Linh","Nữ","2008-10-05","HS012",lop11A1.Id),
                // Lớp 11A2
                ("hs013@lms.com","Hoàng Trọng Nghĩa","Nam","2008-03-22","HS013",lop11A2.Id),
                ("hs014@lms.com","Vũ Ngọc Diệp","Nữ","2008-07-11","HS014",lop11A2.Id),
                ("hs015@lms.com","Đỗ Văn Lâm","Nam","2008-11-30","HS015",lop11A2.Id),
                // Lớp 12A1
                ("hs016@lms.com","Nguyễn Thị Hà","Nữ","2007-04-08","HS016",lop12A1.Id),
                ("hs017@lms.com","Trần Đình Khải","Nam","2007-09-14","HS017",lop12A1.Id),
                ("hs018@lms.com","Lê Thị Thu","Nữ","2007-01-27","HS018",lop12A1.Id),
                // Lớp 12A2
                ("hs019@lms.com","Phạm Văn Bình","Nam","2007-06-03","HS019",lop12A2.Id),
                ("hs020@lms.com","Hoàng Thị Ngân","Nữ","2007-12-19","HS020",lop12A2.Id),
            };

            foreach (var (email, hoTen, gioiTinh, ngaySinh, maHs, lopId) in hocSinhData)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var hs = new NguoiDung
                    {
                        UserName = email, Email = email, HoTen = hoTen,
                        EmailConfirmed = true, IsActive = true,
                        GioiTinh = gioiTinh, MaHocSinh = maHs,
                        NgaySinh = DateTime.Parse(ngaySinh),
                        LopId = lopId, NgayTao = DateTime.Now
                    };
                    await userManager.CreateAsync(hs, "Hs@123456");
                    await userManager.AddToRoleAsync(hs, "HocSinh");
                }
            }

            // ============================================================
            // 9. PHÂN CÔNG MÔN DẠY (MonHocGiaoVien + LopMonHoc)
            // ============================================================
            if (!context.MonHocGiaoViens.Any())
            {
                var toan10 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 10");
                var toan11 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 11");
                var toan12 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 12");
                var ly10   = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Vật Lý 10");
                var ly11   = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Vật Lý 11");
                var ly12   = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Vật Lý 12");
                var hoa10  = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Hóa Học 10");
                var hoa11  = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Hóa Học 11");
                var anh    = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Tiếng Anh");

                // GV Toán → dạy Toán ở nhiều lớp
                context.MonHocGiaoViens.AddRange(
                    new MonHocGiaoVien { NguoiDungId = gvToan.Id, MonHocId = toan10.Id, LopId = lop10A1.Id },
                    new MonHocGiaoVien { NguoiDungId = gvToan.Id, MonHocId = toan10.Id, LopId = lop10A2.Id },
                    new MonHocGiaoVien { NguoiDungId = gvToan.Id, MonHocId = toan11.Id, LopId = lop11A1.Id },
                    new MonHocGiaoVien { NguoiDungId = gvToan.Id, MonHocId = toan12.Id, LopId = lop12A1.Id }
                );

                // GV Lý → dạy Vật Lý
                context.MonHocGiaoViens.AddRange(
                    new MonHocGiaoVien { NguoiDungId = gvLy.Id, MonHocId = ly10.Id, LopId = lop10A1.Id },
                    new MonHocGiaoVien { NguoiDungId = gvLy.Id, MonHocId = ly11.Id, LopId = lop11A1.Id },
                    new MonHocGiaoVien { NguoiDungId = gvLy.Id, MonHocId = ly11.Id, LopId = lop11A2.Id },
                    new MonHocGiaoVien { NguoiDungId = gvLy.Id, MonHocId = ly12.Id, LopId = lop12A1.Id }
                );

                // GV Hóa → dạy Hóa Học
                context.MonHocGiaoViens.AddRange(
                    new MonHocGiaoVien { NguoiDungId = gvHoa.Id, MonHocId = hoa10.Id, LopId = lop10A1.Id },
                    new MonHocGiaoVien { NguoiDungId = gvHoa.Id, MonHocId = hoa10.Id, LopId = lop10A2.Id },
                    new MonHocGiaoVien { NguoiDungId = gvHoa.Id, MonHocId = hoa11.Id, LopId = lop11A2.Id }
                );

                // GV Anh → dạy Tiếng Anh
                context.MonHocGiaoViens.AddRange(
                    new MonHocGiaoVien { NguoiDungId = gvAnh.Id, MonHocId = anh.Id, LopId = lop12A1.Id },
                    new MonHocGiaoVien { NguoiDungId = gvAnh.Id, MonHocId = anh.Id, LopId = lop12A2.Id }
                );

                await context.SaveChangesAsync();
            }

            // ============================================================
            // 10. TẠO LopMonHoc (liên kết lớp ↔ môn ↔ giáo viên phụ trách)
            // ============================================================
            if (!context.LopMonHocs.Any())
            {
                var toan10 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 10");
                var toan11 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 11");
                var toan12 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 12");
                var ly10   = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Vật Lý 10");
                var ly11   = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Vật Lý 11");
                var ly12   = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Vật Lý 12");
                var hoa10  = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Hóa Học 10");
                var hoa11  = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Hóa Học 11");
                var anh    = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Tiếng Anh");

                context.LopMonHocs.AddRange(
                    // Lớp 10A1
                    new LopMonHoc { LopId = lop10A1.Id, MonHocId = toan10.Id, GiaoVienId = gvToan.Id },
                    new LopMonHoc { LopId = lop10A1.Id, MonHocId = ly10.Id,   GiaoVienId = gvLy.Id   },
                    new LopMonHoc { LopId = lop10A1.Id, MonHocId = hoa10.Id,  GiaoVienId = gvHoa.Id  },
                    // Lớp 10A2
                    new LopMonHoc { LopId = lop10A2.Id, MonHocId = toan10.Id, GiaoVienId = gvToan.Id },
                    new LopMonHoc { LopId = lop10A2.Id, MonHocId = hoa10.Id,  GiaoVienId = gvHoa.Id  },
                    // Lớp 11A1
                    new LopMonHoc { LopId = lop11A1.Id, MonHocId = toan11.Id, GiaoVienId = gvToan.Id },
                    new LopMonHoc { LopId = lop11A1.Id, MonHocId = ly11.Id,   GiaoVienId = gvLy.Id   },
                    // Lớp 11A2
                    new LopMonHoc { LopId = lop11A2.Id, MonHocId = ly11.Id,   GiaoVienId = gvLy.Id   },
                    new LopMonHoc { LopId = lop11A2.Id, MonHocId = hoa11.Id,  GiaoVienId = gvHoa.Id  },
                    // Lớp 12A1
                    new LopMonHoc { LopId = lop12A1.Id, MonHocId = toan12.Id, GiaoVienId = gvToan.Id },
                    new LopMonHoc { LopId = lop12A1.Id, MonHocId = ly12.Id,   GiaoVienId = gvLy.Id   },
                    new LopMonHoc { LopId = lop12A1.Id, MonHocId = anh.Id,    GiaoVienId = gvAnh.Id  },
                    // Lớp 12A2
                    new LopMonHoc { LopId = lop12A2.Id, MonHocId = anh.Id,    GiaoVienId = gvAnh.Id  }
                );
                await context.SaveChangesAsync();
            }

            // ============================================================
            // 11. TẠO LỊCH HỌC NẠP SẴN
            // ============================================================
            if (!context.LichHocs.Any())
            {
                var toan10 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 10");
                var toan11 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 11");
                var ly10   = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Vật Lý 10");
                var hoa10  = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Hóa Học 10");

                var today = DateTime.Today;
                var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
                if (today.DayOfWeek == DayOfWeek.Sunday) monday = monday.AddDays(-7);

                context.LichHocs.AddRange(
                    new LichHoc { 
                        TieuDe = "Đại số 10", LopId = lop10A1.Id, MonHocId = toan10.Id, GiaoVienId = gvToan.Id, 
                        Thu = 2, TietHoc = 1, PhongHoc = "P.101", 
                        NgayHoc = monday, GioBatDau = new TimeSpan(7,0,0), GioKetThuc = new TimeSpan(7,45,0) 
                    },
                    new LichHoc { 
                        TieuDe = "Hình học 10", LopId = lop10A1.Id, MonHocId = toan10.Id, GiaoVienId = gvToan.Id, 
                        Thu = 2, TietHoc = 2, PhongHoc = "P.101", 
                        NgayHoc = monday, GioBatDau = new TimeSpan(7,50,0), GioKetThuc = new TimeSpan(8,35,0) 
                    },
                    new LichHoc { 
                        TieuDe = "Động học chất điểm", LopId = lop10A1.Id, MonHocId = ly10.Id, GiaoVienId = gvLy.Id, 
                        Thu = 2, TietHoc = 3, PhongHoc = "P.101", 
                        NgayHoc = monday, GioBatDau = new TimeSpan(8,50,0), GioKetThuc = new TimeSpan(9,35,0) 
                    },
                    new LichHoc { 
                        TieuDe = "Đại số 11", LopId = lop11A1.Id, MonHocId = toan11.Id, GiaoVienId = gvToan.Id, 
                        Thu = 3, TietHoc = 1, PhongHoc = "P.201", 
                        NgayHoc = monday.AddDays(1), GioBatDau = new TimeSpan(7,0,0), GioKetThuc = new TimeSpan(7,45,0) 
                    },
                    new LichHoc { 
                        TieuDe = "Cấu tạo nguyên tử", LopId = lop10A2.Id, MonHocId = hoa10.Id, GiaoVienId = gvHoa.Id, 
                        Thu = 4, TietHoc = 4, PhongHoc = "P.102", 
                        NgayHoc = monday.AddDays(2), GioBatDau = new TimeSpan(9,40,0), GioKetThuc = new TimeSpan(10,25,0) 
                    }
                );
                await context.SaveChangesAsync();
            }

            // ============================================================
            // 12. TẠO BÀI GIẢNG VÀ TÀI LIỆU
            // ============================================================
            if (!context.BaiGiangs.Any())
            {
                var toan10 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 10");
                var ly10 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Vật Lý 10");

                var bg1 = new BaiGiang { TieuDe = "Chương 1: Mệnh đề - Tập hợp", MoTa = "Tìm hiểu về các khái niệm cơ bản của logic toán học", ThuTu = 1, MonHocId = toan10.Id, NguoiDungId = gvToan.Id };
                var bg2 = new BaiGiang { TieuDe = "Chương 2: Hàm số bậc nhất và bậc hai", MoTa = "Đại số 10", ThuTu = 2, MonHocId = toan10.Id, NguoiDungId = gvToan.Id };
                var bgLy = new BaiGiang { TieuDe = "Chương 1: Động học chất điểm", MoTa = "Cơ học 10", ThuTu = 1, MonHocId = ly10.Id, NguoiDungId = gvLy.Id };

                context.BaiGiangs.AddRange(bg1, bg2, bgLy);
                await context.SaveChangesAsync();

                context.TaiLieus.AddRange(
                    new TaiLieu { TenTaiLieu = "Slide Bài 1: Mệnh đề", DuongDanFile = "/uploads/menh-de.pdf", LoaiTaiLieu = LoaiTaiLieu.PDF, BaiGiangId = bg1.Id, MonHocId = toan10.Id },
                    new TaiLieu { TenTaiLieu = "Bài tập trắc nghiệm Tập hợp", DuongDanFile = "/uploads/tap-hop.docx", LoaiTaiLieu = LoaiTaiLieu.Khac, BaiGiangId = bg1.Id, MonHocId = toan10.Id },
                    new TaiLieu { TenTaiLieu = "Video Hướng dẫn vẽ đồ thị Parabol", DuongDanFile = "https://youtube.com/watch?v=123", LoaiTaiLieu = LoaiTaiLieu.Video, BaiGiangId = bg2.Id, MonHocId = toan10.Id }
                );
                await context.SaveChangesAsync();
            }

            // ============================================================
            // 13. TẠO BÀI TẬP VÀ BÀI NỘP
            // ============================================================
            if (!context.BaiTaps.Any())
            {
                var toan10 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 10");
                var ly10 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Vật Lý 10");

                var bt1 = new BaiTap { TieuDe = "Bài tập về nhà Chương 1 Toán 10", MoTa = "Giải 5 bài tập trong SGK", HanNop = DateTime.Now.AddDays(7), DiemToiDa = 10, TrangThai = TrangThaiBaiTap.DangMo, MonHocId = toan10.Id, NguoiDungId = gvToan.Id };
                var bt2 = new BaiTap { TieuDe = "Bài tập trắc nghiệm Động học", MoTa = "Làm trên form", HanNop = DateTime.Now.AddDays(3), DiemToiDa = 10, TrangThai = TrangThaiBaiTap.DangMo, MonHocId = ly10.Id, NguoiDungId = gvLy.Id };
                var bt3 = new BaiTap { TieuDe = "Đề cương ôn tập Toán 10", MoTa = "Làm vào vở", HanNop = DateTime.Now.AddDays(-2), DiemToiDa = 10, TrangThai = TrangThaiBaiTap.DaDong, MonHocId = toan10.Id, NguoiDungId = gvToan.Id };

                context.BaiTaps.AddRange(bt1, bt2, bt3);
                await context.SaveChangesAsync();

                // Lấy học sinh lớp 10A1
                var hs10A1_1 = await userManager.FindByEmailAsync("hs001@lms.com");
                var hs10A1_2 = await userManager.FindByEmailAsync("hs002@lms.com");

                if (hs10A1_1 != null && hs10A1_2 != null)
                {
                    context.BaiNops.AddRange(
                        new BaiNop { NoiDung = "Em nộp bài tập Toán ạ", TrangThai = TrangThaiBaiNop.DaNop, BaiTapId = bt1.Id, HocSinhId = hs10A1_1.Id, NgayNop = DateTime.Now.AddDays(-1) },
                        new BaiNop { NoiDung = "Em nộp bài tập ạ", TrangThai = TrangThaiBaiNop.ChamXong, BaiTapId = bt3.Id, HocSinhId = hs10A1_1.Id, Diem = 8.5, NhanXet = "Bài làm tốt, trình bày sạch đẹp", NgayCham = DateTime.Now.AddDays(-1) },
                        new BaiNop { NoiDung = "Tệp đính kèm", TrangThai = TrangThaiBaiNop.DaNop, BaiTapId = bt1.Id, HocSinhId = hs10A1_2.Id, DuongDanFile = "/uploads/bainop.pdf", NgayNop = DateTime.Now.AddDays(-2) }
                    );
                    await context.SaveChangesAsync();
                }
            }

            // ============================================================
            // 14. TẠO ĐIỂM SỐ
            // ============================================================
            if (!context.DiemSos.Any())
            {
                var toan10 = await context.MonHocs.FirstAsync(m => m.TenMonHoc == "Toán 10");
                
                var listHs10A1 = new[] { "hs001@lms.com", "hs002@lms.com", "hs003@lms.com", "hs004@lms.com", "hs005@lms.com" };

                foreach(var email in listHs10A1)
                {
                    var hs = await userManager.FindByEmailAsync(email);
                    if (hs != null)
                    {
                        // Điểm 15 phút (Miệng/Kiểm tra)
                        context.DiemSos.Add(new DiemSo { Diem = new Random().Next(6, 11), LoaiDiem = LoaiDiem.MiengKiemTra, MonHocId = toan10.Id, NguoiDungId = hs.Id, GiaoVienId = gvToan.Id });
                        // Điểm giữa kỳ
                        context.DiemSos.Add(new DiemSo { Diem = new Random().Next(7, 11), LoaiDiem = LoaiDiem.GiuaKy, MonHocId = toan10.Id, NguoiDungId = hs.Id, GiaoVienId = gvToan.Id });
                    }
                }
                await context.SaveChangesAsync();
            }

            // ============================================================
            // 15. TẠO THÔNG BÁO VÀ YÊU CẦU GIÁO VIÊN
            // ============================================================
            if (!context.ThongBaos.Any())
            {
                var adminUser = await userManager.FindByEmailAsync("admin@lms.com");
                if (adminUser != null)
                {
                    context.ThongBaos.AddRange(
                        new ThongBao { TieuDe = "Kế hoạch thi Giữa kỳ I", NoiDung = "Nhà trường thông báo lịch thi giữa kỳ I sẽ diễn ra vào tuần tới. Đề nghị các GVCN nhắc nhở học sinh ôn tập.", NguoiDangId = adminUser.Id, HienThi = true },
                        new ThongBao { TieuDe = "Họp hội đồng giáo viên tháng 10", NoiDung = "Trân trọng kính mời các đồng chí giáo viên tham gia họp hội đồng vào lúc 14h00 chiều thứ 6.", NguoiDangId = adminUser.Id, HienThi = true }
                    );
                    await context.SaveChangesAsync();
                }
            }

            if (!context.YeuCauGiaoViens.Any())
            {
                context.YeuCauGiaoViens.AddRange(
                    new YeuCauGiaoVien { LoaiYeuCau = LoaiYeuCau.NghiPhep, TieuDe = "Xin nghỉ phép việc gia đình", MoTa = "Tôi xin phép nghỉ ngày 25/10 do có việc gia đình đột xuất.", GiaoVienId = gvToan.Id, TrangThai = TrangThaiYeuCau.ChoDuyet },
                    new YeuCauGiaoVien { LoaiYeuCau = LoaiYeuCau.YeuCauTaiNguyen, TieuDe = "Đề nghị cấp thêm phấn và bảng", MoTa = "Lớp 10A1 hiện đã hết phấn viết bảng, đề nghị nhà trường cấp thêm.", GiaoVienId = gvLy.Id, TrangThai = TrangThaiYeuCau.DaDuyet, GhiChuAdmin = "Đã xuất kho 5 hộp phấn." }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
