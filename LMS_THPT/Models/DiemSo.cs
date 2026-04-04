namespace LMS_THPT.Models
{
    public enum LoaiDiem
    {
        MiengKiemTra,
        GiuaKy,
        CuoiKy,
        BaiTap
    }

    public class DiemSo
    {
        public int Id { get; set; }
        // Điểm giữa kỳ / cuối kỳ (hỗ trợ cả 2 loại)
        public double? DiemGiuaKy { get; set; }
        public double? DiemCuoiKy { get; set; }
        public double? Diem { get; set; }
        public LoaiDiem LoaiDiem { get; set; }
        public string? NhanXet { get; set; }
        public DateTime NgayNhap { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        // Khóa ngoại
        public int MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }

        // Học sinh / người được chấm
        public string NguoiDungId { get; set; } = string.Empty;
        public NguoiDung? NguoiDung { get; set; }

        // Giáo viên nhập điểm
        public string GiaoVienId { get; set; } = string.Empty;
        public NguoiDung? GiaoVien { get; set; }
    }
}