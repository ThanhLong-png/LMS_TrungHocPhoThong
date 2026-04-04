namespace LMS_THPT.Models
{
    public enum LoaiTaiLieu
    {
        PDF,
        Video,
        Slide,
        Khac
    }

    public class TaiLieu
    {
        public int Id { get; set; }
        public string TenTaiLieu { get; set; } = string.Empty;
        public string DuongDanFile { get; set; } = string.Empty;
        public LoaiTaiLieu LoaiTaiLieu { get; set; }
        public long KichThuocFile { get; set; }    // bytes
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        // Khóa ngoại
        public int? BaiGiangId { get; set; }
        public BaiGiang? BaiGiang { get; set; }

        public int? MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }
    }
}