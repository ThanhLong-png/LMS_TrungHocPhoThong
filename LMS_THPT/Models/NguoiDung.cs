using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_THPT.Models
{
    public class NguoiDung : IdentityUser
    {
        public string HoTen { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? DiaChi { get; set; }

        // Khóa ngoại lớp (dành cho học sinh)
        public int? LopId { get; set; }
        [ForeignKey("LopId")]
        public Lop? Lop { get; set; }

        // Các thông tin khác
        public string? MaHocSinh { get; set; }     // dành cho học sinh
        public string? HanhKiem { get; set; }      // điểm hạnh kiểm
        public string? ChuyenMon { get; set; }     // dành cho giáo viên
        public string? ChucVu { get; set; } // Ví dụ: Giáo viên, Tổ trưởng, Trưởng khoa...
        public bool IsActive { get; set; } = true;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        // Navigation
        public ICollection<DangKyHoc> DangKyHocs { get; set; } = new List<DangKyHoc>();
        public ICollection<BaiNop> BaiNops { get; set; } = new List<BaiNop>();
        public ICollection<DiemSo> DiemSos { get; set; } = new List<DiemSo>();
    }
}