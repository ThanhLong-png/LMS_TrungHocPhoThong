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
        public LoaiDiem LoaiDiem { get; set; } = LoaiDiem.BaiTap;
        public TrangThaiBaiTap TrangThai { get; set; } = TrangThaiBaiTap.ChuaMo;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        /// <summary>Học kỳ bài tập thuộc về (1 hoặc 2). 0 = chưa xác định.</summary>
        public int HocKy { get; set; } = 0;

        /// <summary>
        /// Cột điểm miệng cụ thể (1-4). Chỉ dùng khi LoaiDiem = MiengKiemTra.
        /// 1 = DiemMieng1, 2 = DiemMieng2, 3 = DiemMieng3, 4 = DiemMieng4.
        /// </summary>
        public int CotDiemMieng { get; set; } = 1;

        // Khóa ngoại
        public int MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }
        public int? LopId { get; set; }
        public Lop? Lop { get; set; }
        public string? NguoiDungId { get; set; }
        public NguoiDung? NguoiDung { get; set; }

        // Navigation
        public ICollection<BaiNop> BaiNops { get; set; } = new List<BaiNop>();
    }
}