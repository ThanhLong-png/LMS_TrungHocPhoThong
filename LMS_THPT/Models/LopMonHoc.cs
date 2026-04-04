using System.ComponentModel.DataAnnotations;

namespace LMS_THPT.Models
{
    public class LopMonHoc
    {
        public int Id { get; set; }

        // FK tới lớp
        public int LopId { get; set; }
        public Lop Lop { get; set; }

        // FK tới môn học
        public int MonHocId { get; set; }
        public MonHoc MonHoc { get; set; }

        // GV phụ trách môn
        public string? GiaoVienId { get; set; }
        public NguoiDung? GiaoVien { get; set; }
    }
}