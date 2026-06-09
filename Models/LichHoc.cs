namespace LMS_THPT.Models
{
    public class LichHoc
    {
        public int Id { get; set; }

        public int LopId { get; set; }
        public int MonHocId { get; set; }
        public string? GiaoVienId { get; set; }

        public int Thu { get; set; }
        public int TietHoc { get; set; }
        public string PhongHoc { get; set; } = string.Empty;

        // Thông tin thời gian mở rộng (từ branch admin)
        public DateTime NgayHoc { get; set; } = DateTime.Now;
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public bool IsHocBu { get; set; } = false;

        // ✅ FIX QUAN TRỌNG
        public Lop? Lop { get; set; }
        public MonHoc? MonHoc { get; set; }
        public NguoiDung? GiaoVien { get; set; }
    }
}