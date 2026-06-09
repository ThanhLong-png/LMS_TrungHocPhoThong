using System.ComponentModel.DataAnnotations;

namespace LMS_THPT.Models
{
    /// <summary>
    /// Lưu điểm tổng kết theo từng môn học, học kỳ và năm học.
    /// Giáo viên/Admin nhập điểm; khi học sinh ra trường/bị xóa thì dữ liệu này vẫn được giữ lại trong LichSuDiemHocSinh.
    /// </summary>
    public class DiemHocKy
    {
        public int Id { get; set; }

        // Học sinh
        [Required]
        public string HocSinhId { get; set; } = string.Empty;
        public NguoiDung? HocSinh { get; set; }

        // Môn học
        public int MonHocId { get; set; }
        public MonHoc? MonHoc { get; set; }

        // Lớp tại thời điểm nhập điểm
        public int? LopId { get; set; }
        public Lop? Lop { get; set; }

        // Thời điểm
        [Required]
        public string NamHoc { get; set; } = string.Empty;  // "2025-2026"
        public int HocKy { get; set; } = 1;                 // 1 hoặc 2

        // Các loại điểm
        public double? DiemMieng1 { get; set; }
        public double? DiemMieng2 { get; set; }
        public double? DiemMieng3 { get; set; }
        public double? DiemMieng4 { get; set; }
        public double? Diem15Phut1 { get; set; }
        public double? Diem15Phut2 { get; set; }
        public double? DiemMotTiet1 { get; set; }
        public double? DiemMotTiet2 { get; set; }
        public double? DiemGiuaKy { get; set; }
        public double? DiemCuoiKy { get; set; }

        // Điểm tổng kết học kỳ (tính tự động hoặc GV nhập trực tiếp)
        public double? DiemTongKet { get; set; }

        // Xếp loại
        public string? XepLoai { get; set; }  // Giỏi, Khá, TB, Yếu, Kém

        // Nhận xét
        public string? NhanXet { get; set; }

        // Trạng thái chốt điểm
        public bool IsChotMieng { get; set; } = false;
        public bool IsChotGiuaKy { get; set; } = false;
        public bool IsChotCuoiKy { get; set; } = false;

        // Người nhập & thời gian
        public string? GiaoVienId { get; set; }
        public NguoiDung? GiaoVien { get; set; }
        public DateTime NgayNhap { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }
    }
}
