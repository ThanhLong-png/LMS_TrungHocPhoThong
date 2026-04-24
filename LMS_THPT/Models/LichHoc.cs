namespace LMS_THPT.Models
{
    public class LichHoc
    {
        public int Id { get; set; }

        public string? TieuDe { get; set; }

        public int LopId { get; set; }
        public int MonHocId { get; set; }
        public string? GiaoVienId { get; set; }

        public int Thu { get; set; }
        public int TietHoc { get; set; }
        public string PhongHoc { get; set; }

        // Fields cho lịch cụ thể theo ngày
        public DateTime NgayHoc { get; set; } = DateTime.Today;
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }

        // Navigation properties
        public Lop? Lop { get; set; }
        public MonHoc? MonHoc { get; set; }
        public NguoiDung? GiaoVien { get; set; }
    }
}