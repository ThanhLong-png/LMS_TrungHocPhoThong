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

            // ===================== ROLES =====================
            string[] roles = { "Admin", "GiaoVien", "HocSinh", "HieuTruong" };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // ===================== USERS =====================
            var admin = await CreateUser(userManager, "admin@lms.com", "Admin@123", "Admin", "Quản Trị Viên", chucVu: "Quản trị");
            var hieuTruong = await CreateUser(userManager, "ht@lms.com", "Hieut@123", "HieuTruong", "Lê Minh Hiệu", chucVu: "Hiệu trưởng");

            // 5 giáo viên
            var gv1 = await CreateUser(userManager, "gv1@lms.com", "Giaovien@1", "GiaoVien", "Nguyễn Văn An", chuyenMon: "Toán học", chucVu: "Giáo viên");
            var gv2 = await CreateUser(userManager, "gv2@lms.com", "Giaovien@1", "GiaoVien", "Trần Thị Bình", chuyenMon: "Ngữ văn", chucVu: "Giáo viên");
            var gv3 = await CreateUser(userManager, "gv3@lms.com", "Giaovien@1", "GiaoVien", "Lê Văn Cường", chuyenMon: "Tiếng Anh", chucVu: "Giáo viên");
            var gv4 = await CreateUser(userManager, "gv4@lms.com", "Giaovien@1", "GiaoVien", "Phạm Thị Dung", chuyenMon: "Vật lý - Hóa học", chucVu: "Giáo viên");
            var gv5 = await CreateUser(userManager, "gv5@lms.com", "Giaovien@1", "GiaoVien", "Hoàng Văn Em", chuyenMon: "Sinh học - Tin học", chucVu: "Giáo viên");

            // 15 học sinh
            var hsList = new List<NguoiDung>();
            string[] hsNames = {
                "Trần Văn Học", "Nguyễn Thị Mai", "Lê Hoàng Nam",
                "Phạm Thị Lan", "Đỗ Văn Hùng", "Vũ Thị Thu",
                "Bùi Văn Tài", "Đinh Thị Hoa", "Trương Văn Bình",
                "Hồ Thị Ngọc", "Cao Văn Đức", "Lý Thị Kim",
                "Dương Văn Long", "Phan Thị Yến", "Tô Văn Minh"
            };
            for (int i = 0; i < 15; i++)
            {
                // CreateUser trả null nếu user đã tồn tại → fetch lại từ DB
                var hs = await CreateUser(userManager, $"hs{i+1:D2}@lms.com", "Hocsinh@1", "HocSinh",
                    hsNames[i], maHocSinh: $"HS2024{i+1:D3}", hanhKiem: "Tốt")
                    ?? await userManager.FindByEmailAsync($"hs{i+1:D2}@lms.com");
                if (hs != null) hsList.Add(hs);
            }

            // Guard dưới đây KHÔNG dùng admin==null vì CreateUser trả null khi user đã tồn tại
            // → việc kiểm tra từng bảng (Khois, Lops...) đã được xử lý bên dưới


            // ===================== KHỐI =====================
            if (!context.Khois.Any())
            {
                var k10 = new Khoi { TenKhoi = "Khối 10" };
                var k11 = new Khoi { TenKhoi = "Khối 11" };
                var k12 = new Khoi { TenKhoi = "Khối 12" };
                context.Khois.AddRange(k10, k11, k12);
                await context.SaveChangesAsync();
            }

            // Lấy lại users (đã tồn tại trong DB)
            var gv1u = await userManager.FindByEmailAsync("gv1@lms.com");
            var gv2u = await userManager.FindByEmailAsync("gv2@lms.com");
            var gv3u = await userManager.FindByEmailAsync("gv3@lms.com");
            var gv4u = await userManager.FindByEmailAsync("gv4@lms.com");
            var gv5u = await userManager.FindByEmailAsync("gv5@lms.com");
            var adminu = await userManager.FindByEmailAsync("admin@lms.com");
            var htu = await userManager.FindByEmailAsync("ht@lms.com");

            if (gv1u == null || gv2u == null || adminu == null) return;

            var khoi10 = context.Khois.First(k => k.TenKhoi == "Khối 10");
            var khoi11 = context.Khois.First(k => k.TenKhoi == "Khối 11");
            var khoi12 = context.Khois.First(k => k.TenKhoi == "Khối 12");

            // ===================== LỚP (fetch hoặc tạo mới) =====================
            Lop lop10A1, lop10A2, lop11A1, lop11A2, lop12A1;
            if (!context.Lops.Any())
            {
                lop10A1 = new Lop { TenLop = "10A1", MaKhoi = khoi10.Id, GiaoVienChuNhiemId = gv1u.Id };
                lop10A2 = new Lop { TenLop = "10A2", MaKhoi = khoi10.Id, GiaoVienChuNhiemId = gv2u.Id };
                lop11A1 = new Lop { TenLop = "11A1", MaKhoi = khoi11.Id, GiaoVienChuNhiemId = gv3u.Id };
                lop11A2 = new Lop { TenLop = "11A2", MaKhoi = khoi11.Id, GiaoVienChuNhiemId = gv4u.Id };
                lop12A1 = new Lop { TenLop = "12A1", MaKhoi = khoi12.Id, GiaoVienChuNhiemId = gv5u.Id };
                context.Lops.AddRange(lop10A1, lop10A2, lop11A1, lop11A2, lop12A1);
                await context.SaveChangesAsync();
            }
            else
            {
                // Lop đã có → fetch lại từ DB để lấy Id
                lop10A1 = context.Lops.First(l => l.TenLop == "10A1");
                lop10A2 = context.Lops.First(l => l.TenLop == "10A2");
                lop11A1 = context.Lops.First(l => l.TenLop == "11A1");
                lop11A2 = context.Lops.First(l => l.TenLop == "11A2");
                lop12A1 = context.Lops.First(l => l.TenLop == "12A1");
            }

            // Gán học sinh vào lớp (3 hs/lớp)
            var lopMap = new[] { lop10A1, lop10A1, lop10A1,
                                  lop10A2, lop10A2, lop10A2,
                                  lop11A1, lop11A1, lop11A1,
                                  lop11A2, lop11A2, lop11A2,
                                  lop12A1, lop12A1, lop12A1 };
            // hsList đã được build đủ 15 phần tử (fetch từ DB nếu user đã tồn tại)
            for (int i = 0; i < Math.Min(hsList.Count, lopMap.Length); i++)
            {
                var hs = await userManager.FindByIdAsync(hsList[i].Id);
                if (hs != null) { hs.LopId = lopMap[i].Id; await userManager.UpdateAsync(hs); }
            }

            // ===================== MÔN HỌC =====================
            var monData = new (string Ten, string MoTa, int KhoiId, string GvId)[]
            {
                ("Toán học 10",    "Đại số và Hình học lớp 10",         khoi10.Id, gv1u.Id),
                ("Ngữ văn 10",     "Văn học và Tiếng Việt lớp 10",      khoi10.Id, gv2u.Id),
                ("Tiếng Anh 10",   "Tiếng Anh giao tiếp và ngữ pháp",   khoi10.Id, gv3u.Id),
                ("Vật lý 10",      "Cơ học và Nhiệt học lớp 10",        khoi10.Id, gv4u.Id),
                ("Hóa học 10",     "Hóa học đại cương lớp 10",          khoi10.Id, gv4u.Id),
                ("Toán học 11",    "Đại số và Giải tích lớp 11",        khoi11.Id, gv1u.Id),
                ("Ngữ văn 11",     "Văn học Việt Nam và thế giới",      khoi11.Id, gv2u.Id),
                ("Tiếng Anh 11",   "Kỹ năng ngôn ngữ nâng cao",         khoi11.Id, gv3u.Id),
                ("Vật lý 11",      "Điện học và Quang học",             khoi11.Id, gv4u.Id),
                ("Sinh học 11",    "Sinh học cơ thể người và động vật", khoi11.Id, gv5u.Id),
                ("Toán học 12",    "Giải tích và Xác suất thống kê",    khoi12.Id, gv1u.Id),
                ("Ngữ văn 12",     "Ôn tập thi đại học môn Văn",        khoi12.Id, gv2u.Id),
                ("Tiếng Anh 12",   "Luyện thi THPT Quốc gia Tiếng Anh", khoi12.Id, gv3u.Id),
                ("Vật lý 12",      "Dao động, Sóng và Điện xoay chiều", khoi12.Id, gv4u.Id),
                ("Tin học 12",     "Lập trình Pascal và CSDL",           khoi12.Id, gv5u.Id),
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
                
                for (int i = 0; i < 5; i++)
                {
                    lopMonHocs.Add(new LopMonHoc { LopId = lop10A1.Id, MonHocId = monHocs[i].Id, GiaoVienId = monData[i].GvId });
                    lopMonHocs.Add(new LopMonHoc { LopId = lop10A2.Id, MonHocId = monHocs[i].Id, GiaoVienId = monData[i].GvId });
                    
                    monHocGiaoViens.Add(new MonHocGiaoVien { LopId = lop10A1.Id, MonHocId = monHocs[i].Id, NguoiDungId = monData[i].GvId });
                    monHocGiaoViens.Add(new MonHocGiaoVien { LopId = lop10A2.Id, MonHocId = monHocs[i].Id, NguoiDungId = monData[i].GvId });
                }
                for (int i = 5; i < 10; i++)
                {
                    lopMonHocs.Add(new LopMonHoc { LopId = lop11A1.Id, MonHocId = monHocs[i].Id, GiaoVienId = monData[i].GvId });
                    lopMonHocs.Add(new LopMonHoc { LopId = lop11A2.Id, MonHocId = monHocs[i].Id, GiaoVienId = monData[i].GvId });
                    
                    monHocGiaoViens.Add(new MonHocGiaoVien { LopId = lop11A1.Id, MonHocId = monHocs[i].Id, NguoiDungId = monData[i].GvId });
                    monHocGiaoViens.Add(new MonHocGiaoVien { LopId = lop11A2.Id, MonHocId = monHocs[i].Id, NguoiDungId = monData[i].GvId });
                }
                for (int i = 10; i < 15; i++)
                {
                    lopMonHocs.Add(new LopMonHoc { LopId = lop12A1.Id, MonHocId = monHocs[i].Id, GiaoVienId = monData[i].GvId });
                    monHocGiaoViens.Add(new MonHocGiaoVien { LopId = lop12A1.Id, MonHocId = monHocs[i].Id, NguoiDungId = monData[i].GvId });
                }
                    
                context.LopMonHocs.AddRange(lopMonHocs);
                context.MonHocGiaoViens.AddRange(monHocGiaoViens);
                await context.SaveChangesAsync();
            }


            // ===================== LỊCH HỌC =====================
            if (!context.LichHocs.Any())
            {
                var lichHocs = new List<LichHoc>();
                void AddLich(int lopId, int[] monIds, string[] gvIds)
                {
                    for (int thu = 2; thu <= 7; thu++) // Tới Thứ 7
                    {
                        int idx = (thu - 2) % monIds.Length;
                        lichHocs.Add(new LichHoc
                        {
                            LopId = lopId, MonHocId = monIds[idx], GiaoVienId = gvIds[idx],
                            Thu = thu, TietHoc = 1, PhongHoc = $"P.10{thu}",
                            NgayHoc = DateTime.Today.AddDays(thu - (int)DateTime.Today.DayOfWeek),
                            GioBatDau = new TimeSpan(7, 0, 0), GioKetThuc = new TimeSpan(9, 0, 0)
                        });
                    }
                }
                var ids10 = monHocs.Take(5).Select(m => m.Id).ToArray();
                var gvIds10 = monData.Take(5).Select(m => m.GvId).ToArray();
                AddLich(lop10A1.Id, ids10, gvIds10);
                AddLich(lop10A2.Id, ids10, gvIds10);
                var ids11 = monHocs.Skip(5).Take(5).Select(m => m.Id).ToArray();
                var gvIds11 = monData.Skip(5).Take(5).Select(m => m.GvId).ToArray();
                AddLich(lop11A1.Id, ids11, gvIds11);
                AddLich(lop11A2.Id, ids11, gvIds11);
                var ids12 = monHocs.Skip(10).Take(5).Select(m => m.Id).ToArray();
                var gvIds12 = monData.Skip(10).Take(5).Select(m => m.GvId).ToArray();
                AddLich(lop12A1.Id, ids12, gvIds12);
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
                var btData = new[]
                {
                    ("Bài tập chương 1", "Làm các bài tập từ 1 đến 10 trong SGK", TrangThaiBaiTap.DaDong),
                    ("Kiểm tra 15 phút", "Kiểm tra miệng và làm bài trắc nghiệm",  TrangThaiBaiTap.DaDong),
                    ("Bài tập chương 2", "Hoàn thành bài tập nâng cao",             TrangThaiBaiTap.DangMo),
                    ("Bài kiểm tra giữa kỳ", "Kiểm tra 45 phút nội dung đã học",   TrangThaiBaiTap.DangMo),
                };
                foreach (var mon in monHocs)
                    for (int i = 0; i < btData.Length; i++)
                    {
                        var (title, desc, tt) = btData[i];
                        baiTaps.Add(new BaiTap
                        {
                            TieuDe = $"{title} - {mon.TenMonHoc}", MoTa = desc, NoiDung = desc,
                            HanNop = tt == TrangThaiBaiTap.DaDong ? DateTime.Now.AddDays(-Random2(5, 20)) : DateTime.Now.AddDays(Random2(3, 14)),
                            DiemToiDa = 10, LoaiDiem = i < 2 ? LoaiDiem.BaiTap : LoaiDiem.GiuaKy,
                            TrangThai = tt, MonHocId = mon.Id, NguoiDungId = mon.GiaoVienId,
                            NgayTao = DateTime.Now.AddDays(-Random2(10, 40))
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
                var hsLop10 = hsList.Take(6).ToList();
                var hsLop11 = hsList.Skip(6).Take(6).ToList();
                var hsLop12 = hsList.Skip(12).Take(3).ToList();
                void AddNop(List<NguoiDung> hss, IEnumerable<BaiTap> bts)
                {
                    foreach (var hs in hss)
                        foreach (var bt in bts.Where(b => b.TrangThai == TrangThaiBaiTap.DaDong).Take(2))
                        {
                            var diem = Math.Round(5.0 + Random2(0, 50) / 10.0, 1);
                            baiNops.Add(new BaiNop
                            {
                                BaiTapId = bt.Id, HocSinhId = hs.Id,
                                NoiDung = "Em xin nộp bài làm của em ạ.",
                                NgayNop = bt.HanNop.AddDays(-Random2(1, 3)),
                                TrangThai = TrangThaiBaiNop.ChamXong, Diem = diem,
                                NhanXet = diem >= 8 ? "Bài làm tốt, trình bày rõ ràng." : "Cần xem lại phần lý thuyết.",
                                NgayCham = bt.HanNop.AddDays(1)
                            });
                        }
                }
                var monIds10 = monHocs.Take(5).Select(m => m.Id).ToHashSet();
                var monIds11 = monHocs.Skip(5).Take(5).Select(m => m.Id).ToHashSet();
                var monIds12 = monHocs.Skip(10).Take(5).Select(m => m.Id).ToHashSet();
                AddNop(hsLop10, baiTaps.Where(b => monIds10.Contains(b.MonHocId)));
                AddNop(hsLop11, baiTaps.Where(b => monIds11.Contains(b.MonHocId)));
                AddNop(hsLop12, baiTaps.Where(b => monIds12.Contains(b.MonHocId)));
                context.DanhSachBaiNop.AddRange(baiNops);
                await context.SaveChangesAsync();
            }

            // ===================== ĐIỂM SỐ =====================
            if (!context.DanhSachDiemSo.Any())
            {
                var diemSos = new List<DiemSo>();
                var hsLop10d = hsList.Take(6).ToList();
                var hsLop11d = hsList.Skip(6).Take(6).ToList();
                var hsLop12d = hsList.Skip(12).Take(3).ToList();
                void AddDiem(List<NguoiDung> hss, IEnumerable<MonHoc> mons, string gvId)
                {
                    foreach (var hs in hss)
                        foreach (var mon in mons.Take(3))
                            diemSos.Add(new DiemSo
                            {
                                NguoiDungId = hs.Id, MonHocId = mon.Id, GiaoVienId = gvId,
                                LoaiDiem = LoaiDiem.GiuaKy,
                                Diem = Math.Round(6.0 + Random2(0, 40) / 10.0, 1),
                                DiemGiuaKy = Math.Round(6.5 + Random2(0, 35) / 10.0, 1),
                                DiemCuoiKy = Math.Round(7.0 + Random2(0, 30) / 10.0, 1),
                                NhanXet = "Học sinh có tiến bộ, cần cố gắng hơn.",
                                NgayNhap = DateTime.Now.AddDays(-Random2(5, 15))
                            });
                }
                AddDiem(hsLop10d, monHocs.Take(5), gv1u.Id);
                AddDiem(hsLop11d, monHocs.Skip(5).Take(5), gv2u.Id);
                AddDiem(hsLop12d, monHocs.Skip(10).Take(5), gv3u.Id);
                context.DanhSachDiemSo.AddRange(diemSos);
                await context.SaveChangesAsync();
            }

            // ===================== THÔNG BÁO =====================
            if (!context.ThongBaos.Any())
            {
                context.ThongBaos.AddRange(
                    new ThongBao { TieuDe = "Chào mừng năm học mới 2024-2025", NoiDung = "Trường xin thông báo lịch khai giảng và các hoạt động đầu năm học.", NguoiDangId = adminu.Id, NgayDang = DateTime.Now.AddDays(-30) },
                    new ThongBao { TieuDe = "Lịch kiểm tra giữa kỳ I", NoiDung = "Lịch kiểm tra giữa kỳ 1 năm học 2024-2025 sẽ diễn ra từ ngày 15/11 đến 20/11.", NguoiDangId = htu!.Id, NgayDang = DateTime.Now.AddDays(-10) },
                    new ThongBao { TieuDe = "Thông báo nghỉ lễ", NoiDung = "Nhà trường thông báo lịch nghỉ lễ 30/4 và 1/5.", NguoiDangId = adminu.Id, NgayDang = DateTime.Now.AddDays(-5) }
                );
                await context.SaveChangesAsync();
            }

            // ===================== CLAIMS CHO AVATAR GV =====================
            foreach (var gv in new[] { gv1u, gv2u, gv3u, gv4u, gv5u })
            {
                if (gv == null) continue;
                var claims = await userManager.GetClaimsAsync(gv);
                if (!claims.Any(c => c.Type == "AnhDaiDien"))
                    await userManager.AddClaimAsync(gv, new Claim("AnhDaiDien", "~/images/default-avatar.svg"));
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
            string? maHocSinh = null, string? hanhKiem = null)
        {
            if (await userManager.FindByEmailAsync(email) != null) return null;

            var user = new NguoiDung
            {
                UserName = email, Email = email, HoTen = hoTen,
                EmailConfirmed = true, IsActive = true,
                ChuyenMon = chuyenMon, ChucVu = chucVu,
                MaHocSinh = maHocSinh, HanhKiem = hanhKiem,
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
            return null;
        }
    }
}