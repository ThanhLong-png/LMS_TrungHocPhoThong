USE LMS_THPT;
GO

-- ================================================================
-- FIX ENCODING FINAL: UPDATE theo Id chính xác
-- ================================================================

-- ============= MON HOC =============
-- Id=1: Toán 10
UPDATE MonHoc SET TenMonHoc=N'Toán 10', MoTa=N'Toán học lớp 10 THPT', MucTieu=N'Nắm vững đại số, hình học và giải tích cơ bản' WHERE Id=1;
-- Id=2: Vật Lý 10
UPDATE MonHoc SET TenMonHoc=N'Vật Lý 10', MoTa=N'Vật lý lớp 10 THPT', MucTieu=N'Hiểu các định luật cơ học và nhiệt học' WHERE Id=2;
-- Id=3: Hóa Học 10
UPDATE MonHoc SET TenMonHoc=N'Hóa Học 10', MoTa=N'Hóa học lớp 10 THPT', MucTieu=N'Nắm vững hóa học đại cương và vô cơ' WHERE Id=3;
-- Id=4: Ngữ Văn 10
UPDATE MonHoc SET TenMonHoc=N'Ngữ Văn 10', MoTa=N'Ngữ văn lớp 10 THPT', MucTieu=N'Phát triển kỹ năng đọc hiểu và tạo lập văn bản' WHERE Id=4;
-- Id=5: Sinh Học 10
UPDATE MonHoc SET TenMonHoc=N'Sinh Học 10', MoTa=N'Sinh học lớp 10 THPT', MucTieu=N'Hiểu cấu trúc tế bào và sinh học phân tử' WHERE Id=5;
-- Id=6: Tiếng Anh 10
UPDATE MonHoc SET TenMonHoc=N'Tiếng Anh 10', MoTa=N'Tiếng Anh lớp 10 THPT', MucTieu=N'Phát triển 4 kỹ năng nghe nói đọc viết' WHERE Id=6;
-- Id=7: Lịch Sử 10
UPDATE MonHoc SET TenMonHoc=N'Lịch Sử 10', MoTa=N'Lịch sử lớp 10 THPT', MucTieu=N'Nắm vững lịch sử Việt Nam và thế giới' WHERE Id=7;
-- Id=8: Địa Lý 10
UPDATE MonHoc SET TenMonHoc=N'Địa Lý 10', MoTa=N'Địa lý lớp 10 THPT', MucTieu=N'Hiểu địa lý tự nhiên và kinh tế xã hội' WHERE Id=8;
-- Id=9: Tin Học 10
UPDATE MonHoc SET TenMonHoc=N'Tin Học 10', MoTa=N'Tin học lớp 10 THPT', MucTieu=N'Nắm vững tin học căn bản và lập trình' WHERE Id=9;
-- Id=10: GDCD 10
UPDATE MonHoc SET TenMonHoc=N'GDCD 10', MoTa=N'Giáo dục công dân lớp 10', MucTieu=N'Hình thành nhận thức pháp luật và đạo đức' WHERE Id=10;
-- Id=11: Thể Dục 10
UPDATE MonHoc SET TenMonHoc=N'Thể Dục 10', MoTa=N'Thể dục lớp 10 THPT', MucTieu=N'Rèn luyện thể chất và kỹ năng thể thao' WHERE Id=11;

-- ============= BAI GIANG =============
-- (Xem lại Id BaiGiang - lấy từ DB)
DECLARE @bgIds TABLE (Id INT, TieuDe NVARCHAR(500));
INSERT INTO @bgIds SELECT Id, TieuDe FROM BaiGiang ORDER BY Id;

-- Update từng BaiGiang theo TieuDe patterns an toàn
-- BaiGiang của Sinh học (MonHocId=5)
UPDATE BaiGiang SET
    TieuDe = N'Tế bào - đơn vị cơ bản của sự sống',
    MoTa   = N'Cấu tạo tế bào nhân sơ và nhân thực'
WHERE MonHocId = 5 AND Id = (SELECT MIN(Id) FROM BaiGiang WHERE MonHocId = 5);

UPDATE BaiGiang SET
    TieuDe = N'Các phân tử sinh học',
    MoTa   = N'Protein, lipid, carbohydrate và axit nucleic'
WHERE MonHocId = 5 AND Id = (SELECT MAX(Id) FROM BaiGiang WHERE MonHocId = 5);

-- BaiGiang của Tiếng Anh (MonHocId=6)
UPDATE BaiGiang SET
    TieuDe = N'Unit 1: Family Life',
    MoTa   = N'Từ vựng và ngữ pháp về cuộc sống gia đình'
WHERE MonHocId = 6 AND Id = (SELECT MIN(Id) FROM BaiGiang WHERE MonHocId = 6);

UPDATE BaiGiang SET
    TieuDe = N'Unit 2: Your Body and You',
    MoTa   = N'Từ vựng về cơ thể và thì hiện tại hoàn thành'
WHERE MonHocId = 6 AND Id = (SELECT MAX(Id) FROM BaiGiang WHERE MonHocId = 6);

-- BaiGiang của Lịch Sử (MonHocId=7)
UPDATE BaiGiang SET
    TieuDe = N'Lịch sử thế giới cổ đại',
    MoTa   = N'Các nền văn minh cổ đại phương Đông và phương Tây'
WHERE MonHocId = 7 AND Id = (SELECT MIN(Id) FROM BaiGiang WHERE MonHocId = 7);

UPDATE BaiGiang SET
    TieuDe = N'Việt Nam thời tiền sử',
    MoTa   = N'Các giai đoạn tiền sử và sơ sử Việt Nam'
WHERE MonHocId = 7 AND Id = (SELECT MAX(Id) FROM BaiGiang WHERE MonHocId = 7);

-- BaiGiang của Địa Lý (MonHocId=8)
UPDATE BaiGiang SET
    TieuDe = N'Bản đồ và các phép chiếu bản đồ',
    MoTa   = N'Khái niệm bản đồ, tỷ lệ và cách đọc bản đồ'
WHERE MonHocId = 8 AND Id = (SELECT MIN(Id) FROM BaiGiang WHERE MonHocId = 8);

UPDATE BaiGiang SET
    TieuDe = N'Trái Đất trong hệ Mặt Trời',
    MoTa   = N'Vị trí, hình dạng và vận động của Trái Đất'
WHERE MonHocId = 8 AND Id = (SELECT MAX(Id) FROM BaiGiang WHERE MonHocId = 8);

-- BaiGiang của Tin Học (MonHocId=9)
UPDATE BaiGiang SET
    TieuDe = N'Giới thiệu về Tin học và máy tính',
    MoTa   = N'Lịch sử máy tính, cấu trúc phần cứng và phần mềm'
WHERE MonHocId = 9 AND Id = (SELECT MIN(Id) FROM BaiGiang WHERE MonHocId = 9);

UPDATE BaiGiang SET
    TieuDe = N'Lập trình cơ bản với Python',
    MoTa   = N'Biến, kiểu dữ liệu và cấu trúc điều khiển'
WHERE MonHocId = 9 AND Id = (SELECT MAX(Id) FROM BaiGiang WHERE MonHocId = 9);

-- BaiGiang của GDCD (MonHocId=10)
UPDATE BaiGiang SET
    TieuDe = N'Công dân với sự phát triển kinh tế',
    MoTa   = N'Vai trò công dân trong phát triển kinh tế đất nước'
WHERE MonHocId = 10;

-- BaiGiang của Thể Dục (MonHocId=11)
UPDATE BaiGiang SET
    TieuDe = N'Thể dục thể thao và sức khỏe',
    MoTa   = N'Lý thuyết thể dục và tầm quan trọng của vận động'
WHERE MonHocId = 11;

-- ============= BAI TAP =============
-- BaiTap của Sinh học (MonHocId=5)
UPDATE BaiTap SET
    TieuDe  = N'Bài tập Tế bào học',
    MoTa    = N'So sánh tế bào nhân sơ và nhân thực',
    NoiDung = N'Lập bảng so sánh tế bào nhân sơ và nhân thực theo: kích thước, nhân, màng nhân, bào quan.'
WHERE MonHocId = 5;

-- BaiTap của Tiếng Anh (MonHocId=6)
UPDATE BaiTap SET
    TieuDe  = N'Writing Task - My Family',
    MoTa    = N'Viết đoạn văn về gia đình bằng tiếng Anh',
    NoiDung = N'Write a paragraph (80-100 words) describing your family members and their daily routines.'
WHERE MonHocId = 6;

-- BaiTap của Lịch Sử (MonHocId=7)
UPDATE BaiTap SET
    TieuDe  = N'Bài tập Lịch sử cổ đại',
    MoTa    = N'Phân tích đặc điểm các nền văn minh cổ đại',
    NoiDung = N'So sánh 3 nền văn minh: Ai Cập, Lưỡng Hà, Hy Lạp về địa lý, kinh tế và văn hóa.'
WHERE MonHocId = 7;

-- BaiTap của Địa Lý (MonHocId=8)
UPDATE BaiTap SET
    TieuDe  = N'Bài tập đọc bản đồ',
    MoTa    = N'Thực hành đọc và phân tích bản đồ địa lý',
    NoiDung = N'Sử dụng Atlas, xác định: tọa độ địa lý 5 thành phố lớn VN, đọc ký hiệu bản đồ.'
WHERE MonHocId = 8;

-- BaiTap của Tin Học (MonHocId=9)
UPDATE BaiTap SET
    TieuDe  = N'Bài tập Python - Bài 1',
    MoTa    = N'Viết chương trình Python cơ bản',
    NoiDung = N'Viết 3 chương trình: (1) In bảng cửu chương số 7, (2) Tính tổng 1..100, (3) Kiểm tra số nguyên tố.'
WHERE MonHocId = 9;

-- BaiTap của GDCD (MonHocId=10)
UPDATE BaiTap SET
    TieuDe  = N'Bài luận GDCD - Vai trò công dân',
    MoTa    = N'Viết bài luận về nghĩa vụ công dân',
    NoiDung = N'Viết bài luận 300 từ: Là học sinh, em có thể đóng góp gì cho sự phát triển kinh tế đất nước?'
WHERE MonHocId = 10;

-- BaiTap của Thể Dục (MonHocId=11)
UPDATE BaiTap SET
    TieuDe  = N'Bài tập lý thuyết Thể dục',
    MoTa    = N'Trả lời câu hỏi lý thuyết thể dục',
    NoiDung = N'Trả lời 5 câu hỏi về: lợi ích vận động, quy tắc an toàn khi tập, cách khởi động đúng cách.'
WHERE MonHocId = 11;

-- ============= TAI LIEU =============
-- Id=8: Sơ đồ cấu tạo tế bào
UPDATE TaiLieu SET TenTaiLieu = N'Sơ đồ cấu tạo tế bào' WHERE Id = 8;
-- Id=9: Từ điển Anh-Việt Unit 1
UPDATE TaiLieu SET TenTaiLieu = N'Từ điển Anh-Việt Unit 1' WHERE Id = 9;
-- Id=10: Bản đồ thế giới cổ đại
UPDATE TaiLieu SET TenTaiLieu = N'Bản đồ thế giới cổ đại' WHERE Id = 10;
-- Id=11: Atlas Địa lý lớp 10
UPDATE TaiLieu SET TenTaiLieu = N'Atlas Địa lý lớp 10' WHERE Id = 11;
-- Id=12: Hướng dẫn cài Python
UPDATE TaiLieu SET TenTaiLieu = N'Hướng dẫn cài Python' WHERE Id = 12;
-- Id=13: Slide GDCD chương 1
UPDATE TaiLieu SET TenTaiLieu = N'Slide GDCD chương 1' WHERE Id = 13;
-- Id=14: Quy tắc an toàn thể dục
UPDATE TaiLieu SET TenTaiLieu = N'Quy tắc an toàn thể dục' WHERE Id = 14;

-- ================================================================
-- VERIFY FINAL
-- ================================================================
PRINT N'=== MonHoc SAU KHI FIX ===';
SELECT Id, TenMonHoc, LEFT(MoTa,40) MoTa FROM MonHoc ORDER BY Id;

PRINT N'=== BaiGiang SAU KHI FIX ===';
SELECT Id, MonHocId, LEFT(TieuDe,50) TieuDe FROM BaiGiang ORDER BY MonHocId, Id;

PRINT N'=== BaiTap SAU KHI FIX ===';
SELECT Id, MonHocId, LEFT(TieuDe,50) TieuDe FROM BaiTap ORDER BY MonHocId, Id;

PRINT N'=== TaiLieu SAU KHI FIX ===';
SELECT Id, TenTaiLieu FROM TaiLieu ORDER BY Id;
GO
