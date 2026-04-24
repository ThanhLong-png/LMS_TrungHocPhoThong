using System.ComponentModel.DataAnnotations.Schema;

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
        public double? Diem { get; set; } // DiemMieng1
        public double? DiemMieng2 { get; set; }
        public double? DiemMieng3 { get; set; }
        public double? DiemMieng4 { get; set; }
        public LoaiDiem LoaiDiem { get; set; }
        public string? NhanXet { get; set; }
        public DateTime NgayNhap { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        // Khóa ngoại
        public int MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }

        // Học sinh
        public string NguoiDungId { get; set; } = string.Empty;
        public NguoiDung? NguoiDung { get; set; }

        // Alias NguoiDungId -> HocSinhId (tương thích với controller)
        [NotMapped]
        public string HocSinhId { get => NguoiDungId; set => NguoiDungId = value; }
        [NotMapped]
        public NguoiDung? HocSinh { get => NguoiDung; set => NguoiDung = value; }

        // Giáo viên chấm điểm
        public string GiaoVienId { get; set; } = string.Empty;
        public NguoiDung? GiaoVien { get; set; }

        // Điểm theo kỳ
        public double? DiemGiuaKy { get; set; }
        public double? DiemCuoiKy { get; set; }
    }
}