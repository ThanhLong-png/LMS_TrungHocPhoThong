namespace LMS_THPT.Models
{
    public class MonHoc
    {
        public int Id { get; set; }
        public string TenMonHoc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? MucTieu { get; set; }
        public string? NoiDung { get; set; }
        public string? HinhAnh { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        // FK Khối
        public int KhoiId { get; set; }              // ← thêm
        public Khoi Khoi { get; set; }               // ← navigation

        // Giáo viên phụ trách
        public string? GiaoVienId { get; set; }
        public NguoiDung? GiaoVien { get; set; }

        // Navigation
        public ICollection<BaiGiang> BaiGiangs { get; set; } = new List<BaiGiang>();
        public ICollection<TaiLieu> TaiLieus { get; set; } = new List<TaiLieu>();
        public ICollection<BaiTap> BaiTaps { get; set; } = new List<BaiTap>();
        public ICollection<DangKyHoc> DangKyHocs { get; set; } = new List<DangKyHoc>();
        public ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
        public ICollection<DiemSo> DiemSos { get; set; } = new List<DiemSo>();
        public ICollection<LopMonHoc> LopMonHocs { get; set; } = new List<LopMonHoc>();
        public ICollection<MonHocGiaoVien> MonHocGiaoViens { get; set; } = new List<MonHocGiaoVien>();
    }
}