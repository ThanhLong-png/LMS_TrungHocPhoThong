namespace LMS_THPT.Models
{
    public class LichHoc
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public DateTime NgayHoc { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public string? PhongHoc { get; set; }
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Khóa ngoại
        public int MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }

        public int LopId { get; set; }
        public Lop? Lop { get; set; }
    }
}