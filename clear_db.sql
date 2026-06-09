SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- 1. Xóa các bảng chứa khóa ngoại đến Học sinh, Lớp, Môn học, Bài tập, Bài giảng
DELETE FROM [DiemHocKys];
DELETE FROM [DanhSachBinhLuan];
DELETE FROM [BaiNop];
DELETE FROM [BaiTap];
DELETE FROM [TaiLieu];
DELETE FROM [BaiGiang];
DELETE FROM [DiemSo];
DELETE FROM [DangKyHoc];
DELETE FROM [MonHocGiaoViens];
DELETE FROM [LopMonHoc];
DELETE FROM [LichHoc];
DELETE FROM [ThongBaos];
DELETE FROM [YeuCauGiaoVien];

-- 2. Xóa bảng Môn học
DELETE FROM [MonHoc];

-- 3. Hủy bỏ liên kết vòng giữa NguoiDung và Lop
UPDATE [NguoiDung] SET [LopId] = NULL;
UPDATE [Lop] SET [GiaoVienChuNhiemId] = NULL;

-- 4. Xóa bảng Lớp
DELETE FROM [Lop];

-- 5. Xóa bảng Khối
DELETE FROM [Khoi];

-- 6. Xóa các bảng của ASP.NET Identity và Người dùng
DELETE FROM [AspNetUserClaims];
DELETE FROM [AspNetUserRoles];
DELETE FROM [AspNetUserLogins];
DELETE FROM [AspNetUserTokens];
DELETE FROM [NguoiDung];
DELETE FROM [AspNetRoleClaims];
DELETE FROM [AspNetRoles];

PRINT 'Database cleared successfully with correct relation order.';
