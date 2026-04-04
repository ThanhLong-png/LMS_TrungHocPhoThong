using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_THPT.Models
{
    public class BinhLuan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NoiDung { get; set; } = "";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // FK đến NguoiDung
        public string NguoiDungId { get; set; } = "";

        [ForeignKey("NguoiDungId")]
        public NguoiDung? NguoiDung { get; set; }

        // FK đến BaiGiang (nullable)
        public int? BaiGiangId { get; set; }

        [ForeignKey("BaiGiangId")]
        public BaiGiang? BaiGiang { get; set; }

        // FK đến BaiTap (nullable)
        public int? BaiTapId { get; set; }

        [ForeignKey("BaiTapId")]
        public BaiTap? BaiTap { get; set; }

        // Reply
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public BinhLuan? Parent { get; set; }

        public ICollection<BinhLuan> Replies { get; set; } = new List<BinhLuan>();
    }
}