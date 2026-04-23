namespace LMS_THPT.Models
{
    public enum TrangThaiDangKy
    {
        ChoXetDuyet,
        DaXetDuyet,
        TuChoi,
        HoanThanh
    }

    public class DangKyHoc
    {
        public int Id { get; set; }
        public DateTime NgayDangKy { get; set; } = DateTime.Now;
        public TrangThaiDangKy TrangThai { get; set; } = TrangThaiDangKy.ChoXetDuyet;
        public string? GhiChu { get; set; }

        // Khóa ngoại
        public int MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }

        public string HocSinhId { get; set; } = string.Empty;
        public NguoiDung? HocSinh { get; set; }
    }
}