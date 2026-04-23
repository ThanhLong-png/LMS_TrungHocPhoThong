namespace LMS_THPT.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ThongBao
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string TieuDe { get; set; }

        [Required]
        public string NoiDung { get; set; }

        public DateTime NgayDang { get; set; } = DateTime.Now;

        public string? NguoiDangId { get; set; }

        // Navigation
        public NguoiDung? NguoiDang { get; set; }

        public bool HienThi { get; set; } = true; // Ẩn/hiện
    }
}