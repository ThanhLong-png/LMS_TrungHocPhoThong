// ViewModels/TeacherDashboardViewModel.cs
// Thay thế / bổ sung file hiện có trong ViewModels/

using LMS_THPT.Models;

namespace LMS_THPT.ViewModels
{
    // ── Dashboard tổng quan ──────────────────────────────────────────────────
    public class TeacherDashboardViewModel
    {
        public NguoiDung GiaoVien { get; set; } = new();

        // Stats cards
        public int TongHocSinh { get; set; }
        public int TongMonHoc { get; set; }
        public int BaiNopChamPending { get; set; }
        public int TongTaiLieu { get; set; }

        // Bài nộp gần đây
        public List<BaiNopGanDayItem> BaiNopGanDay { get; set; } = new();

        // Tiến độ lớp học
        public List<TienDoLopItem> TienDoLop { get; set; } = new();

        // Lịch hôm nay
        public List<LichHocItem> LichHomNay { get; set; } = new();

        // Lịch các ngày tiếp theo trong tuần
        public List<LichHocNgayItem> LichCacNgayTiepTheo { get; set; } = new();
    }

    public class LichHocNgayItem
    {
        public string Ngay { get; set; } = "";
        public List<LichHocItem> CacTietHoc { get; set; } = new();
    }

    public class BaiNopGanDayItem
    {
        public string TenHocSinh { get; set; } = "";
        public string TenVietTat { get; set; } = "";
        public string TenBaiTap { get; set; } = "";
        public string ThoiGianNop { get; set; } = "";
        /// <summary>"pending" | "graded"</summary>
        public string TrangThai { get; set; } = "pending";
        public int? BaiNopId { get; set; }
    }

    public class TienDoLopItem
    {
        public string TenLop { get; set; } = "";
        public string TenMonHoc { get; set; } = "";
        public int PhanTram { get; set; }
    }

    public class LichHocItem
    {
        public string ThoiGian { get; set; } = "";
        public string TenMon { get; set; } = "";
        public string PhongLop { get; set; } = "";
        public bool NhanManh { get; set; }
    }

    // ── Quản lý môn học (3.1) ────────────────────────────────────────────────
    public class MonHocManageViewModel
    {
        public MonHoc MonHoc { get; set; } = new();
        public List<HocSinhSelectItem> DanhSachHocSinh { get; set; } = new();
        public List<string> TuyChonTrangThai { get; set; } = new()
        {
            "Chuyển sang học lại","Miễn học phần","Bảo lưu","Hoàn thành"
        };
        public List<NhapDiemItem> DanhSachDiem { get; set; } = new();
    }

    public class HocSinhSelectItem
    {
        public string Id { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string TenLop { get; set; } = "";
        public string Display => $"{HoTen} – {TenLop}";
    }

    public class NhapDiemItem
    {
        public string HocSinhId { get; set; } = "";
        public string TenHocSinh { get; set; } = "";
        public double? DiemMieng { get; set; }
        public double? DiemMieng2 { get; set; }
        public double? DiemMieng3 { get; set; }
        public double? DiemMieng4 { get; set; }
        public double? DiemGiuaKy { get; set; }
        public double? DiemCuoiKy { get; set; }
        public int DiemSoId { get; set; }

        public double? DiemTongKet { get; set; }
        public string XepLoai { get; set; } = "";
    }

    // ── Quản lý bài giảng (3.2) ──────────────────────────────────────────────
    public class BaiGiangManageViewModel
    {
        public List<BaiGiangItem> DanhSachBaiGiang { get; set; } = new();
        public int MonHocId { get; set; }
    }

    public class BaiGiangItem
    {
        public int Id { get; set; }
        public int ThuTu { get; set; }
        public string TieuDe { get; set; } = "";
        /// <summary>"PDF" | "Video" | "Slide"</summary>
        public string LoaiTaiLieu { get; set; } = "";
        public string ThongTinFile { get; set; } = "";
    }

    // ── Bài tập & Đánh giá (3.3) ─────────────────────────────────────────────
    public class BaiTapManageViewModel
    {
        public List<BaiNopChopItem> BaiNopChoChoam { get; set; } = new();
        public List<BaiNopDiemItem> DanhSachDiem { get; set; } = new();
    }

    public class BaiNopChopItem
    {
        public int BaiNopId { get; set; }
        public string TenHocSinh { get; set; } = "";
        public string TenLop { get; set; } = "";
        public string TenVietTat { get; set; } = "";
        public string TenBaiTap { get; set; } = "";
        public string ThoiGianNop { get; set; } = "";
        public string TrangThai { get; set; } = "pending";
        public double? Diem { get; set; }
    }

    public class BaiNopDiemItem
    {
        public int BaiNopId { get; set; }
        public string TenHocSinh { get; set; } = "";
        public string TenLop { get; set; } = "";
        public string TenBaiTap { get; set; } = "";
        public double? Diem { get; set; }
        public string NhanXet { get; set; } = "";
        public string TrangThai { get; set; } = "pending";
    }

    // ── Tiến độ học tập (3.4) ────────────────────────────────────────────────
    public class TienDoViewModel
    {
        public double DiemTrungBinhLop { get; set; }
        public int TiLeHoanThanh { get; set; }
        public int SoHocSinhXuatSac { get; set; }
        public int SoHocSinhCanHoTro { get; set; }
        public List<TienDoHocSinhItem> TienDoHocSinh { get; set; } = new();
        public List<ThongKeXepLoaiItem> ThongKeXepLoai { get; set; } = new();
        public List<string> DanhSachLop { get; set; } = new();
    }

    public class TienDoHocSinhItem
    {
        public string TenHocSinh { get; set; } = "";
        public string TenVietTat { get; set; } = "";
        public string TenLop { get; set; } = "";
        public double Diem { get; set; }
        public int PhanTram { get; set; }

        public string MauBar => PhanTram switch
        {
            >= 80 => "#1D4ED8",
            >= 60 => "#166534",
            >= 40 => "#92400E",
            _ => "#991B1B"
        };
    }

    public class ThongKeXepLoaiItem
    {
        public string NhanXepLoai { get; set; } = "";
        public int SoLuong { get; set; }
        public int PhanTram { get; set; }
        public string Mau { get; set; } = "#6B7280";
    }

    // ── Request models cho AJAX ───────────────────────────────────────────────
    public class YeuCauThayDoiTrangThaiModel
    {
        public string HocSinhId { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public string LyDo { get; set; } = "";
    }

    public class LuuDiemRequest
    {
        public string HocSinhId { get; set; } = "";
        public int MonHocId { get; set; }
        public double? DiemMieng { get; set; }
        public double? DiemMieng2 { get; set; }
        public double? DiemMieng3 { get; set; }
        public double? DiemMieng4 { get; set; }
        public double? DiemGiuaKy { get; set; }
        public double? DiemCuoiKy { get; set; }
    }

    public class LuuDiemBaiTapRequest
    {
        public int BaiNopId { get; set; }
        public double? Diem { get; set; }
        public string NhanXet { get; set; } = "";
    }

    public class SapXepBaiGiangRequest
    {
        public List<int> ThuTuIds { get; set; } = new();
    }

    public class LuuHanhKiemRequest
    {
        public string HocSinhId { get; set; } = "";
        public string HanhKiem { get; set; } = "";
    }
}
