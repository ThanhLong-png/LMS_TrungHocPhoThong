namespace LMS_THPT.Models
{
    public enum TrangThaiYeuCau
    {
        ChoXuLy,      // Chờ xử lý
        DaDuyet,      // Đã duyệt
        TuChoi,       // Từ chối
        HuyBo         // Hủy bỏ
    }

    public enum LoaiYeuCau
    {
        DangKyLopChuNhiem,    // Đăng ký lớp chủ nhiệm
        ThayDoiLopChuNhiem,   // Thay đổi lớp chủ nhiệm
        HuyLopChuNhiem,       // Hủy lớp chủ nhiệm
        TangCapCongChuc,      // Tăng cấp công chức
        DanhGiaHangNam        // Đánh giá hàng năm
    }

    /// <summary>
    /// Model cho yêu cầu từ giáo viên gửi lên admin
    /// </summary>
    public class YeuCauGiaoVien
    {
        public int Id { get; set; }

        // Loại yêu cầu
        public LoaiYeuCau LoaiYeuCau { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;

        // Trạng thái xử lý
        public TrangThaiYeuCau TrangThai { get; set; } = TrangThaiYeuCau.ChoXuLy;
        public string? GhiChu { get; set; }

        // Thông tin giáo viên gửi
        public string GiaoVienId { get; set; } = string.Empty;
        public NguoiDung? GiaoVien { get; set; }

        // Thông tin lớp (nếu liên quan)
        public int? LopId { get; set; }
        public Lop? Lop { get; set; }

        // Thời gian
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public DateTime? NgayXuLy { get; set; }
        public string? XuLyBoi { get; set; }  // User ID của admin xử lý

        // Tài liệu đính kèm
        public string? DuongDanTaiLieu { get; set; }
    }
}
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