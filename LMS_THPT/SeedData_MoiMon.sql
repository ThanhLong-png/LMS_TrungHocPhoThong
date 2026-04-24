USE LMS_THPT;
GO

DECLARE @gvId NVARCHAR(450) = (SELECT Id FROM NguoiDung WHERE UserName = 'gv@lms.com');
DECLARE @hsId NVARCHAR(450) = (SELECT Id FROM NguoiDung WHERE UserName = 'hs@lms.com');
DECLARE @lopId INT = (SELECT TOP 1 Id FROM Lop WHERE TenLop = N'10A1');

-- =====================================================
-- MON HOC MOI (7 mon)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM MonHoc WHERE TenMonHoc = N'Sinh Học 10')
    INSERT INTO MonHoc (TenMonHoc,MoTa,MucTieu,IsActive,NgayTao,KhoiId,GiangVienId) VALUES
    (N'Sinh Học 10',N'Sinh học lớp 10 THPT',N'Hiểu cấu trúc tế bào và sinh học phân tử',1,GETDATE(),1,@gvId);

IF NOT EXISTS (SELECT 1 FROM MonHoc WHERE TenMonHoc = N'Tiếng Anh 10')
    INSERT INTO MonHoc (TenMonHoc,MoTa,MucTieu,IsActive,NgayTao,KhoiId,GiangVienId) VALUES
    (N'Tiếng Anh 10',N'Tiếng Anh lớp 10 THPT',N'Phát triển 4 kỹ năng nghe nói đọc viết',1,GETDATE(),1,@gvId);

IF NOT EXISTS (SELECT 1 FROM MonHoc WHERE TenMonHoc = N'Lịch Sử 10')
    INSERT INTO MonHoc (TenMonHoc,MoTa,MucTieu,IsActive,NgayTao,KhoiId,GiangVienId) VALUES
    (N'Lịch Sử 10',N'Lịch sử lớp 10 THPT',N'Nắm vững lịch sử Việt Nam và thế giới',1,GETDATE(),1,NULL);

IF NOT EXISTS (SELECT 1 FROM MonHoc WHERE TenMonHoc = N'Địa Lý 10')
    INSERT INTO MonHoc (TenMonHoc,MoTa,MucTieu,IsActive,NgayTao,KhoiId,GiangVienId) VALUES
    (N'Địa Lý 10',N'Địa lý lớp 10 THPT',N'Hiểu địa lý tự nhiên và kinh tế xã hội',1,GETDATE(),1,NULL);

IF NOT EXISTS (SELECT 1 FROM MonHoc WHERE TenMonHoc = N'Tin Học 10')
    INSERT INTO MonHoc (TenMonHoc,MoTa,MucTieu,IsActive,NgayTao,KhoiId,GiangVienId) VALUES
    (N'Tin Học 10',N'Tin học lớp 10 THPT',N'Nắm vững tin học căn bản và lập trình',1,GETDATE(),1,@gvId);

IF NOT EXISTS (SELECT 1 FROM MonHoc WHERE TenMonHoc = N'GDCD 10')
    INSERT INTO MonHoc (TenMonHoc,MoTa,MucTieu,IsActive,NgayTao,KhoiId,GiangVienId) VALUES
    (N'GDCD 10',N'Giáo dục công dân lớp 10',N'Hình thành nhận thức pháp luật và đạo đức',1,GETDATE(),1,NULL);

IF NOT EXISTS (SELECT 1 FROM MonHoc WHERE TenMonHoc = N'Thể Dục 10')
    INSERT INTO MonHoc (TenMonHoc,MoTa,MucTieu,IsActive,NgayTao,KhoiId,GiangVienId) VALUES
    (N'Thể Dục 10',N'Thể dục lớp 10 THPT',N'Rèn luyện thể chất và kỹ năng thể thao',1,GETDATE(),1,NULL);

DECLARE @m5 INT = (SELECT TOP 1 Id FROM MonHoc WHERE TenMonHoc = N'Sinh Học 10');
DECLARE @m6 INT = (SELECT TOP 1 Id FROM MonHoc WHERE TenMonHoc = N'Tiếng Anh 10');
DECLARE @m7 INT = (SELECT TOP 1 Id FROM MonHoc WHERE TenMonHoc = N'Lịch Sử 10');
DECLARE @m8 INT = (SELECT TOP 1 Id FROM MonHoc WHERE TenMonHoc = N'Địa Lý 10');
DECLARE @m9 INT = (SELECT TOP 1 Id FROM MonHoc WHERE TenMonHoc = N'Tin Học 10');
DECLARE @m10 INT = (SELECT TOP 1 Id FROM MonHoc WHERE TenMonHoc = N'GDCD 10');
DECLARE @m11 INT = (SELECT TOP 1 Id FROM MonHoc WHERE TenMonHoc = N'Thể Dục 10');

-- =====================================================
-- BAI GIANG MOI
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Tế bào - đơn vị cơ bản của sự sống')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Tế bào - đơn vị cơ bản của sự sống',N'Cấu tạo tế bào nhân sơ và nhân thực',1,1,GETDATE(),@m5,@gvId);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Các phân tử sinh học')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Các phân tử sinh học',N'Protein, lipid, carbohydrate và axit nucleic',2,1,GETDATE(),@m5,@gvId);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Unit 1: Family Life')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Unit 1: Family Life',N'Từ vựng và ngữ pháp về cuộc sống gia đình',1,1,GETDATE(),@m6,@gvId);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Unit 2: Your Body and You')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Unit 2: Your Body and You',N'Từ vựng về cơ thể và thì hiện tại hoàn thành',2,1,GETDATE(),@m6,@gvId);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Lịch sử thế giới cổ đại')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Lịch sử thế giới cổ đại',N'Các nền văn minh cổ đại phương Đông và phương Tây',1,1,GETDATE(),@m7,NULL);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Việt Nam thời tiền sử')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Việt Nam thời tiền sử',N'Các giai đoạn tiền sử và sơ sử Việt Nam',2,1,GETDATE(),@m7,NULL);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Bản đồ và các phép chiếu bản đồ')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Bản đồ và các phép chiếu bản đồ',N'Khái niệm bản đồ, tỷ lệ và cách đọc bản đồ',1,1,GETDATE(),@m8,NULL);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Trái Đất trong hệ Mặt Trời')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Trái Đất trong hệ Mặt Trời',N'Vị trí, hình dạng và vận động của Trái Đất',2,1,GETDATE(),@m8,NULL);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Giới thiệu về Tin học và máy tính')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Giới thiệu về Tin học và máy tính',N'Lịch sử máy tính, cấu trúc phần cứng và phần mềm',1,1,GETDATE(),@m9,@gvId);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Lập trình cơ bản với Python')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Lập trình cơ bản với Python',N'Biến, kiểu dữ liệu và cấu trúc điều khiển',2,1,GETDATE(),@m9,@gvId);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Công dân với sự phát triển kinh tế')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Công dân với sự phát triển kinh tế',N'Vai trò công dân trong phát triển kinh tế đất nước',1,1,GETDATE(),@m10,NULL);

IF NOT EXISTS (SELECT 1 FROM BaiGiang WHERE TieuDe = N'Thể dục thể thao và sức khỏe')
    INSERT INTO BaiGiang (TieuDe,MoTa,ThuTu,IsActive,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Thể dục thể thao và sức khỏe',N'Lý thuyết thể dục và tầm quan trọng của vận động',1,1,GETDATE(),@m11,NULL);

-- =====================================================
-- BAI GIANG IDs
-- =====================================================
DECLARE @bgS1 INT = (SELECT TOP 1 Id FROM BaiGiang WHERE TieuDe = N'Tế bào - đơn vị cơ bản của sự sống');
DECLARE @bgE1 INT = (SELECT TOP 1 Id FROM BaiGiang WHERE TieuDe = N'Unit 1: Family Life');
DECLARE @bgT1 INT = (SELECT TOP 1 Id FROM BaiGiang WHERE TieuDe = N'Giới thiệu về Tin học và máy tính');

-- =====================================================
-- TAI LIEU MOI
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM TaiLieu WHERE TenTaiLieu = N'Sơ đồ cấu tạo tế bào')
    INSERT INTO TaiLieu (TenTaiLieu,DuongDanFile,LoaiTaiLieu,KichThuocFile,NgayTao,BaiGiangId,MonHocId) VALUES
    (N'Sơ đồ cấu tạo tế bào',N'/uploads/te-bao.pdf',0,1048576,GETDATE(),@bgS1,@m5);

IF NOT EXISTS (SELECT 1 FROM TaiLieu WHERE TenTaiLieu = N'Từ điển Anh-Việt Unit 1')
    INSERT INTO TaiLieu (TenTaiLieu,DuongDanFile,LoaiTaiLieu,KichThuocFile,NgayTao,BaiGiangId,MonHocId) VALUES
    (N'Từ điển Anh-Việt Unit 1',N'/uploads/vocab-u1.pdf',0,512000,GETDATE(),@bgE1,@m6);

IF NOT EXISTS (SELECT 1 FROM TaiLieu WHERE TenTaiLieu = N'Bản đồ thế giới cổ đại')
    INSERT INTO TaiLieu (TenTaiLieu,DuongDanFile,LoaiTaiLieu,KichThuocFile,NgayTao,BaiGiangId,MonHocId) VALUES
    (N'Bản đồ thế giới cổ đại',N'/uploads/ban-do-co-dai.pdf',0,2097152,GETDATE(),NULL,@m7);

IF NOT EXISTS (SELECT 1 FROM TaiLieu WHERE TenTaiLieu = N'Atlas Địa lý lớp 10')
    INSERT INTO TaiLieu (TenTaiLieu,DuongDanFile,LoaiTaiLieu,KichThuocFile,NgayTao,BaiGiangId,MonHocId) VALUES
    (N'Atlas Địa lý lớp 10',N'/uploads/atlas-dia-ly.pdf',0,5242880,GETDATE(),NULL,@m8);

IF NOT EXISTS (SELECT 1 FROM TaiLieu WHERE TenTaiLieu = N'Hướng dẫn cài Python')
    INSERT INTO TaiLieu (TenTaiLieu,DuongDanFile,LoaiTaiLieu,KichThuocFile,NgayTao,BaiGiangId,MonHocId) VALUES
    (N'Hướng dẫn cài Python',N'/uploads/huong-dan-python.pdf',0,307200,GETDATE(),@bgT1,@m9);

IF NOT EXISTS (SELECT 1 FROM TaiLieu WHERE TenTaiLieu = N'Slide GDCD chương 1')
    INSERT INTO TaiLieu (TenTaiLieu,DuongDanFile,LoaiTaiLieu,KichThuocFile,NgayTao,BaiGiangId,MonHocId) VALUES
    (N'Slide GDCD chương 1',N'/uploads/slide-gdcd.pdf',2,1048576,GETDATE(),NULL,@m10);

IF NOT EXISTS (SELECT 1 FROM TaiLieu WHERE TenTaiLieu = N'Quy tắc an toàn thể dục')
    INSERT INTO TaiLieu (TenTaiLieu,DuongDanFile,LoaiTaiLieu,KichThuocFile,NgayTao,BaiGiangId,MonHocId) VALUES
    (N'Quy tắc an toàn thể dục',N'/uploads/antoan-theduc.pdf',0,204800,GETDATE(),NULL,@m11);

-- =====================================================
-- BAI TAP MOI
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM BaiTap WHERE TieuDe = N'Bài tập Tế bào học')
    INSERT INTO BaiTap (TieuDe,MoTa,NoiDung,HanNop,DiemToiDa,TrangThai,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Bài tập Tế bào học',N'So sánh tế bào nhân sơ và nhân thực',
     N'Lập bảng so sánh tế bào nhân sơ và nhân thực theo: kích thước, nhân, màng nhân, bào quan.',
     DATEADD(DAY,7,GETDATE()),10,0,GETDATE(),@m5,@gvId);

IF NOT EXISTS (SELECT 1 FROM BaiTap WHERE TieuDe = N'Writing Task - My Family')
    INSERT INTO BaiTap (TieuDe,MoTa,NoiDung,HanNop,DiemToiDa,TrangThai,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Writing Task - My Family',N'Viết đoạn văn về gia đình bằng tiếng Anh',
     N'Write a paragraph (80-100 words) describing your family members and their daily routines.',
     DATEADD(DAY,5,GETDATE()),10,0,GETDATE(),@m6,@gvId);

IF NOT EXISTS (SELECT 1 FROM BaiTap WHERE TieuDe = N'Bài tập Lịch sử cổ đại')
    INSERT INTO BaiTap (TieuDe,MoTa,NoiDung,HanNop,DiemToiDa,TrangThai,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Bài tập Lịch sử cổ đại',N'Phân tích đặc điểm các nền văn minh cổ đại',
     N'So sánh 3 nền văn minh: Ai Cập, Lưỡng Hà, Hy Lạp về địa lý, kinh tế và văn hóa.',
     DATEADD(DAY,10,GETDATE()),10,0,GETDATE(),@m7,NULL);

IF NOT EXISTS (SELECT 1 FROM BaiTap WHERE TieuDe = N'Bài tập đọc bản đồ')
    INSERT INTO BaiTap (TieuDe,MoTa,NoiDung,HanNop,DiemToiDa,TrangThai,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Bài tập đọc bản đồ',N'Thực hành đọc và phân tích bản đồ địa lý',
     N'Sử dụng Atlas, xác định: tọa độ địa lý 5 thành phố lớn VN, đọc ký hiệu bản đồ.',
     DATEADD(DAY,6,GETDATE()),10,0,GETDATE(),@m8,NULL);

IF NOT EXISTS (SELECT 1 FROM BaiTap WHERE TieuDe = N'Bài tập Python - Bài 1')
    INSERT INTO BaiTap (TieuDe,MoTa,NoiDung,HanNop,DiemToiDa,TrangThai,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Bài tập Python - Bài 1',N'Viết chương trình Python cơ bản',
     N'Viết 3 chương trình: (1) In bảng cửu chương số 7, (2) Tính tổng 1..100, (3) Kiểm tra số nguyên tố.',
     DATEADD(DAY,4,GETDATE()),10,0,GETDATE(),@m9,@gvId);

IF NOT EXISTS (SELECT 1 FROM BaiTap WHERE TieuDe = N'Bài luận GDCD - Vai trò công dân')
    INSERT INTO BaiTap (TieuDe,MoTa,NoiDung,HanNop,DiemToiDa,TrangThai,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Bài luận GDCD - Vai trò công dân',N'Viết bài luận về nghĩa vụ công dân',
     N'Viết bài luận 300 từ: Là học sinh, em có thể đóng góp gì cho sự phát triển kinh tế đất nước?',
     DATEADD(DAY,14,GETDATE()),10,0,GETDATE(),@m10,NULL);

IF NOT EXISTS (SELECT 1 FROM BaiTap WHERE TieuDe = N'Bài tập lý thuyết Thể dục')
    INSERT INTO BaiTap (TieuDe,MoTa,NoiDung,HanNop,DiemToiDa,TrangThai,NgayTao,MonHocId,NguoiDungId) VALUES
    (N'Bài tập lý thuyết Thể dục',N'Trả lời câu hỏi lý thuyết thể dục',
     N'Trả lời 5 câu hỏi về: lợi ích vận động, quy tắc an toàn khi tập, cách khởi động đúng cách.',
     DATEADD(DAY,8,GETDATE()),10,0,GETDATE(),@m11,NULL);

-- =====================================================
-- LOP_MON_HOC MOI
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM LopMonHoc WHERE LopId=@lopId AND MonHocId=@m5)
    INSERT INTO LopMonHoc (LopId,MonHocId,GiaoVienId) VALUES (@lopId,@m5,@gvId);

IF NOT EXISTS (SELECT 1 FROM LopMonHoc WHERE LopId=@lopId AND MonHocId=@m6)
    INSERT INTO LopMonHoc (LopId,MonHocId,GiaoVienId) VALUES (@lopId,@m6,@gvId);

IF NOT EXISTS (SELECT 1 FROM LopMonHoc WHERE LopId=@lopId AND MonHocId=@m7)
    INSERT INTO LopMonHoc (LopId,MonHocId,GiaoVienId) VALUES (@lopId,@m7,NULL);

IF NOT EXISTS (SELECT 1 FROM LopMonHoc WHERE LopId=@lopId AND MonHocId=@m8)
    INSERT INTO LopMonHoc (LopId,MonHocId,GiaoVienId) VALUES (@lopId,@m8,NULL);

IF NOT EXISTS (SELECT 1 FROM LopMonHoc WHERE LopId=@lopId AND MonHocId=@m9)
    INSERT INTO LopMonHoc (LopId,MonHocId,GiaoVienId) VALUES (@lopId,@m9,@gvId);

IF NOT EXISTS (SELECT 1 FROM LopMonHoc WHERE LopId=@lopId AND MonHocId=@m10)
    INSERT INTO LopMonHoc (LopId,MonHocId,GiaoVienId) VALUES (@lopId,@m10,NULL);

IF NOT EXISTS (SELECT 1 FROM LopMonHoc WHERE LopId=@lopId AND MonHocId=@m11)
    INSERT INTO LopMonHoc (LopId,MonHocId,GiaoVienId) VALUES (@lopId,@m11,NULL);

-- =====================================================
-- LICH HOC MOI
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m5 AND Thu=2)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m5,@gvId,2,5,N'P.Sinh học');

IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m6 AND Thu=3)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m6,@gvId,3,1,N'P.Anh văn');

IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m6 AND Thu=5)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m6,@gvId,5,1,N'P.Anh văn');

IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m7 AND Thu=4)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m7,NULL,4,5,N'P.103');

IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m8 AND Thu=6)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m8,NULL,6,5,N'P.104');

IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m9 AND Thu=3)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m9,@gvId,3,5,N'P.Máy tính');

IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m9 AND Thu=6)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m9,@gvId,6,4,N'P.Máy tính');

IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m10 AND Thu=2)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m10,NULL,2,6,N'P.105');

IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m11 AND Thu=4)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m11,NULL,4,6,N'Sân thể dục');

IF NOT EXISTS (SELECT 1 FROM LichHoc WHERE LopId=@lopId AND MonHocId=@m5 AND Thu=5)
    INSERT INTO LichHoc (LopId,MonHocId,GiaoVienId,Thu,TietHoc,PhongHoc) VALUES
    (@lopId,@m5,@gvId,5,5,N'P.Sinh học');

-- =====================================================
-- VERIFY
-- =====================================================
PRINT '=== KET QUA ===';
SELECT 'MonHoc'    AS Bang, COUNT(*) AS SoLuong FROM MonHoc   UNION ALL
SELECT 'LopMonHoc',          COUNT(*)             FROM LopMonHoc UNION ALL
SELECT 'BaiGiang',           COUNT(*)             FROM BaiGiang  UNION ALL
SELECT 'TaiLieu',            COUNT(*)             FROM TaiLieu   UNION ALL
SELECT 'BaiTap',             COUNT(*)             FROM BaiTap    UNION ALL
SELECT 'LichHoc',            COUNT(*)             FROM LichHoc;
GO
