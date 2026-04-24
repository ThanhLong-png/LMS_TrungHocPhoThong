// ViewModels/LopChuNhiemViewModel.cs
namespace LMS_THPT.ViewModels
{
    using LMS_THPT.Models;

    public class LopChuNhiemViewModel
    {
        public Lop? LopChuNhiem { get; set; }
        public List<HocSinhLopItem> DanhSachHocSinh { get; set; } = new();
        public int SiSo => DanhSachHocSinh.Count;
        public int SoNamSinh => DanhSachHocSinh.Count(x => x.GioiTinh == "Nam");
        public int SoNuSinh => DanhSachHocSinh.Count(x => x.GioiTinh == "Nữ");
        public List<LichHocItem> LichHomNay { get; set; } = new();
        public List<YeuCauGiaoVien> YeuCauGanDay { get; set; } = new();
        public List<string> DanhSachMonHoc { get; set; } = new();
    }

    public class HocSinhLopItem
    {
        public string Id { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string TenVietTat { get; set; } = "";
        public string GioiTinh { get; set; } = "";
        public string? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
        public string? AnhDaiDien { get; set; }
        public string Email { get; set; } = "";
        public double? DiemTrungBinh { get; set; }
        public Dictionary<string, double?> DiemTungMon { get; set; } = new();
        public string? HanhKiem { get; set; }
    }
}