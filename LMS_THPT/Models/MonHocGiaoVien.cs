namespace LMS_THPT.Models
{
    public class MonHocGiaoVien
    {
        public int Id { get; set; }
        public string NguoiDungId { get; set; }
        public int MonHocId { get; set; }
        public int? LopId { get; set; }  // Nullable để không lỗi nếu chưa phân lớp

        public NguoiDung GiaoVien { get; set; }
        public MonHoc MonHoc { get; set; }
        public Lop Lop { get; set; }
    }
}
