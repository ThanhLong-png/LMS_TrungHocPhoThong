namespace LMS_THPT.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class YeuCauGiaoVien
    {
        public int Id { get; set; }

        [Required]
        public string MaGiaoVien { get; set; }

        [Required, MaxLength(100)]
        public string TieuDe { get; set; }

        [Required]
        public string NoiDung { get; set; }

        public LoaiYeuCau LoaiYeuCau { get; set; }

        public TrangThaiYeuCau TrangThai { get; set; } = TrangThaiYeuCau.ChoDuyet;

        public DateTime NgayGui { get; set; } = DateTime.Now;

        public DateTime? NgayXuLy { get; set; }

        public string? GhiChuAdmin { get; set; }

        public string? NguoiXuLyId { get; set; }

        public NguoiDung GiaoVien { get; set; }
        public NguoiDung? NguoiXuLy { get; set; }
    }

    public enum LoaiYeuCau
    {
        NghiPhep,
        DoiLich,
        YeuCauTaiNguyen,
        KhieuNai,
        Khac
    }

    public enum TrangThaiYeuCau
    {
        ChoDuyet,
        DaDuyet,
        TuChoi
    }
}