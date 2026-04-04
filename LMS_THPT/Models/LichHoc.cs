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
        public string PhongHoc { get; set; }

        // ✅ FIX QUAN TRỌNG
        public Lop? Lop { get; set; }
        public MonHoc? MonHoc { get; set; }
        public NguoiDung? GiaoVien { get; set; }
    }
}