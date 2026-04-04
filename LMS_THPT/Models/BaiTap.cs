namespace LMS_THPT.Models
{
    public enum TrangThaiBaiTap
    {
        DangMo,
        DaDong,
        ChuaMo
    }

    public class BaiTap
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? NoiDung { get; set; }
        public DateTime HanNop { get; set; }
        public int DiemToiDa { get; set; } = 10;
        public TrangThaiBaiTap TrangThai { get; set; } = TrangThaiBaiTap.ChuaMo;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        // Khóa ngoại
        public int MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }
        public string? NguoiDungId { get; set; }
        public NguoiDung? NguoiDung { get; set; }

        // Navigation
        public ICollection<BaiNop> BaiNops { get; set; } = new List<BaiNop>();
    }
}