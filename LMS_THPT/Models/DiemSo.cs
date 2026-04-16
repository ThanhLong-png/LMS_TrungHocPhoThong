﻿namespace LMS_THPT.Models
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
        public double Diem { get; set; }
        public LoaiDiem LoaiDiem { get; set; }
        public string? NhanXet { get; set; }
        public DateTime NgayNhap { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        // Khóa ngoại
        public int MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }

        public string HocSinhId { get; set; } = string.Empty;
        public NguoiDung? HocSinh { get; set; }

        public string GiangVienId { get; set; } = string.Empty;
        public NguoiDung? GiangVien { get; set; }
    }
}