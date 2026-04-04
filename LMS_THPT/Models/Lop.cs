using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_THPT.Models
{
    public class Lop
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên lớp")]
        public string TenLop { get; set; } = string.Empty;

        // FK đến Khối
        public int MaKhoi { get; set; }
        public Khoi Khoi { get; set; }
        public string? GiaoVienChuNhiemId { get; set; }
        public NguoiDung? GiaoVienChuNhiem { get; set; }
        // Navigation: Lớp có nhiều học sinh (NguoiDung)
        public ICollection<NguoiDung> HocSinhs { get; set; } = new List<NguoiDung>();
        public ICollection<LopMonHoc> LopMonHocs { get; set; } = new List<LopMonHoc>();
    }
}