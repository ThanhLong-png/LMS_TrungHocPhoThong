namespace LMS_THPT.Models
{
    public enum TrangThaiBaiNop
    {
        DaNop,
        ChamXong,
        NopTre
    }

    public class BaiNop
    {
        public int Id { get; set; }
        public string? NoiDung { get; set; }
        public string? DuongDanFile { get; set; }
        public DateTime NgayNop { get; set; } = DateTime.Now;
        public TrangThaiBaiNop TrangThai { get; set; } = TrangThaiBaiNop.DaNop;

        // Chấm điểm
        public double? Diem { get; set; }
        public string? NhanXet { get; set; }
        public DateTime? NgayCham { get; set; }

        // Khóa ngoại
        public int BaiTapId { get; set; }
        public BaiTap? BaiTap { get; set; }

        public string HocSinhId { get; set; } = string.Empty;
        public NguoiDung? HocSinh { get; set; }
    }
}