using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS_THPT.Models
{
    public class Khoi
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên khối")]
        public string TenKhoi { get; set; } = string.Empty;

        // Navigation: Khối có nhiều lớp
        public ICollection<Lop> Lops { get; set; } = new List<Lop>();
        public ICollection<MonHoc> MonHocs { get; set; } = new List<MonHoc>();
    }
}