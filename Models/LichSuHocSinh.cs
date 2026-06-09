using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_THPT.Models
{
    /// <summary>
    /// Lưu lịch sử học sinh đã ra trường hoặc bị xóa.
    /// Dữ liệu này được tạo tự động khi xóa học sinh.
    /// </summary>
    public class LichSuHocSinh
    {
        public int Id { get; set; }

        // Thông tin cơ bản (snapshot tại thời điểm xóa)
        [Required]
        public string HoTen { get; set; } = string.Empty;
        public string? MaHocSinh { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? DiaChi { get; set; }
        public string? AnhDaiDien { get; set; }

        // Lớp cuối cùng
        public string? TenLop { get; set; }
        public string? TenKhoi { get; set; }

        // Năm học cuối
        public string? NamHocCuoi { get; set; }

        // Lý do / ghi chú xóa
        public string? LyDoXoa { get; set; }
        public string? GhiChu { get; set; }

        // Trạng thái (tốt nghiệp / chuyển trường / bị xóa...)
        public string TrangThai { get; set; } = "ĐãXóa";

        // Ngày xóa / tốt nghiệp
        public DateTime NgayXoa { get; set; } = DateTime.Now;

        // Người thực hiện xóa
        public string? NguoiXoaId { get; set; }
        public string? NguoiXoaHoTen { get; set; }

        // Điểm tổng kết tất cả các năm (lưu dưới dạng JSON string)
        public string? DanhSachDiemJson { get; set; }

        // ID gốc của NguoiDung để tra cứu nếu cần
        public string? NguoiDungIdGoc { get; set; }
    }

    /// <summary>
    /// Lưu snapshot điểm của từng môn, từng năm học theo học sinh đã nghỉ.
    /// </summary>
    public class LichSuDiemHocSinh
    {
        public int Id { get; set; }

        public int LichSuHocSinhId { get; set; }
        public LichSuHocSinh? LichSuHocSinh { get; set; }

        public string? TenMonHoc { get; set; }
        public string? NamHoc { get; set; }
        public int HocKy { get; set; }
        public string? TenLop { get; set; }

        public double? DiemMieng1 { get; set; }
        public double? DiemMieng2 { get; set; }
        public double? DiemMieng3 { get; set; }
        public double? Diem15Phut1 { get; set; }
        public double? Diem15Phut2 { get; set; }
        public double? DiemMotTiet1 { get; set; }
        public double? DiemMotTiet2 { get; set; }
        public double? DiemGiuaKy { get; set; }
        public double? DiemCuoiKy { get; set; }
        public double? DiemTongKet { get; set; }
        public string? XepLoai { get; set; }
        public string? NhanXet { get; set; }
    }
}
