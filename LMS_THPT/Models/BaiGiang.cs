namespace LMS_THPT.Models
{
    public class BaiGiang
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int ThuTu { get; set; } = 0;        // sắp xếp thứ tự
        public bool IsActive { get; set; } = true;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        // Khóa ngoại
        public int MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }
        public int? LopId { get; set; }
        public Lop? Lop { get; set; }
        public string? NguoiDungId { get; set; }
        public string? LinkTracNghiem { get; set; }
        public bool TinhTienDo { get; set; } = false;

        public NguoiDung? NguoiDung { get; set; }

        // Navigation
        public ICollection<TaiLieu> TaiLieus { get; set; } = new List<TaiLieu>();
    }
}