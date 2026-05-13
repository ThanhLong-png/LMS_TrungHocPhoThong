using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LMS_THPT.Models
{
    public enum TrangThaiYeuCau
    {
        ChoXuLy,      // Chờ xử lý (phiên bản 1)
        ChoDuyet,     // Chờ duyệt (phiên bản 2 - alias)
        DaDuyet,      // Đã duyệt
        TuChoi,       // Từ chối
        HuyBo         // Hủy bỏ
    }

    public enum LoaiYeuCau
    {
        // Phiên bản 1
        DangKyLopChuNhiem,
        ThayDoiLopChuNhiem,
        HuyLopChuNhiem,
        TangCapCongChuc,
        DanhGiaHangNam,
        // Phiên bản 2
        NghiPhep,
        DoiLich,
        YeuCauTaiNguyen,
        KhieuNai,
        Khac,
        HocBu
    }

    /// <summary>
    /// Model cho yêu cầu từ giáo viên gửi lên admin (merged từ 2 branch)
    /// </summary>
    public class YeuCauGiaoVien
    {
        public int Id { get; set; }

        // Loại yêu cầu
        public LoaiYeuCau LoaiYeuCau { get; set; }

        [Required, MaxLength(100)]
        public string TieuDe { get; set; } = string.Empty;

        // MoTa (phiên bản 1) / NoiDung (phiên bản 2)
        public string MoTa { get; set; } = string.Empty;
        [NotMapped]
        public string NoiDung { get => MoTa; set => MoTa = value; }

        // Trạng thái xử lý
        public TrangThaiYeuCau TrangThai { get; set; } = TrangThaiYeuCau.ChoXuLy;

        // GhiChu (phiên bản 1) / GhiChuAdmin (phiên bản 2)
        public string? GhiChu { get; set; }
        [NotMapped]
        public string? GhiChuAdmin { get => GhiChu; set => GhiChu = value; }

        // Thông tin giáo viên gửi
        // GiaoVienId (phiên bản 1) / MaGiaoVien (phiên bản 2)
        public string GiaoVienId { get; set; } = string.Empty;
        [NotMapped]
        public string MaGiaoVien { get => GiaoVienId; set => GiaoVienId = value; }
        public NguoiDung? GiaoVien { get; set; }

        // Người xử lý (phiên bản 2)
        public string? XuLyBoi { get; set; }
        [NotMapped]
        public string? NguoiXuLyId { get => XuLyBoi; set => XuLyBoi = value; }
        public NguoiDung? NguoiXuLy { get; set; }

        // Thông tin lớp (nếu liên quan)
        public int? LopId { get; set; }
        public Lop? Lop { get; set; }

        // Thời gian
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public DateTime? NgayXuLy { get; set; }

        // Thông tin nghỉ phép (từ branch admin)
        public DateTime? NgayNghi { get; set; }
        public DateTime? NgayNghiKetThuc { get; set; }
        public int? TuTiet { get; set; }
        public int? DenTiet { get; set; }
        
        // Thông tin môn học (nếu liên quan đến học bù)
        public int? MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }
        
        // Danh sách các tiết học (cho phép chọn nhiều tiết)
        public string? DanhSachTiet { get; set; }

        // Tài liệu đính kèm
        public string? DuongDanTaiLieu { get; set; }
    }
}