USE LMS_THPT;
GO

-- ================================================================
-- FIX ENCODING V3: Xóa sạch và insert lại đúng Unicode
-- Chiến lược: UPDATE theo Id thực tế trong DB
-- ================================================================

-- Bước 1: Lấy danh sách Id hiện tại theo thứ tự tên
-- (chúng ta biết tên gốc dù đang bị corrupt)
-- Cập nhật tất cả MonHoc theo số lượng rows và thứ tự alphabet

-- Xem Id thực tế
SELECT Id, TenMonHoc FROM MonHoc ORDER BY Id;
GO

-- Bước 2: UPDATE MoTa và MucTieu cho tất cả MonHoc theo từng TenMonHoc
-- Dùng CHARINDEX để tìm chuỗi không dấu ổn định

-- 'GDCD 10' - tên này không dấu nên khớp chính xác
UPDATE MonHoc SET
    MoTa    = N'Giáo dục công dân lớp 10',
    MucTieu = N'Hình thành nhận thức pháp luật và đạo đức'
WHERE TenMonHoc = 'GDCD 10';

-- Fix TenMonHoc + MoTa + MucTieu theo SUBSTRING patterns
-- 'Dia Ly' -> không có ký tự đặc biệt chuỗi ngắn
UPDATE MonHoc SET
    TenMonHoc = N'Địa Lý 10',
    MoTa      = N'Địa lý lớp 10 THPT',
    MucTieu   = N'Hiểu địa lý tự nhiên và kinh tế xã hội'
WHERE CHARINDEX('a Ly 10', TenMonHoc) > 0
   OR CHARINDEX('a L', TenMonHoc) > 0 AND CHARINDEX('10', TenMonHoc) > 0
      AND LEN(TenMonHoc) < 12;

UPDATE MonHoc SET
    TenMonHoc = N'Hóa Học 10',
    MoTa      = N'Hóa học lớp 10 THPT',
    MucTieu   = N'Nắm vững hóa học đại cương và vô cơ'
WHERE (CHARINDEX('a H', TenMonHoc) > 0 AND CHARINDEX('c 10', TenMonHoc) > 0)
  AND LEN(TenMonHoc) < 12
  AND TenMonHoc NOT LIKE '%Sinh%' AND TenMonHoc NOT LIKE '%Tin%';

UPDATE MonHoc SET
    TenMonHoc = N'Lịch Sử 10',
    MoTa      = N'Lịch sử lớp 10 THPT',
    MucTieu   = N'Nắm vững lịch sử Việt Nam và thế giới'
WHERE CHARINDEX('ch S', TenMonHoc) > 0 AND CHARINDEX('10', TenMonHoc) > 0;

UPDATE MonHoc SET
    TenMonHoc = N'Ngữ Văn 10',
    MoTa      = N'Ngữ văn lớp 10 THPT',
    MucTieu   = N'Phát triển kỹ năng đọc hiểu và tạo lập văn bản'
WHERE CHARINDEX('Van 10', TenMonHoc) > 0
   OR (CHARINDEX('g? Van', TenMonHoc) > 0);

UPDATE MonHoc SET
    TenMonHoc = N'Sinh Học 10',
    MoTa      = N'Sinh học lớp 10 THPT',
    MucTieu   = N'Hiểu cấu trúc tế bào và sinh học phân tử'
WHERE CHARINDEX('Sinh H', TenMonHoc) > 0 AND CHARINDEX('10', TenMonHoc) > 0;

UPDATE MonHoc SET
    TenMonHoc = N'Thể Dục 10',
    MoTa      = N'Thể dục lớp 10 THPT',
    MucTieu   = N'Rèn luyện thể chất và kỹ năng thể thao'
WHERE CHARINDEX('D', TenMonHoc) > 0 AND CHARINDEX('c 10', TenMonHoc) > 0
  AND LEN(TenMonHoc) < 12
  AND TenMonHoc NOT LIKE '%Sinh%' AND TenMonHoc NOT LIKE '%Tin%'
  AND TenMonHoc NOT LIKE '%H%a%';

UPDATE MonHoc SET
    TenMonHoc = N'Tiếng Anh 10',
    MoTa      = N'Tiếng Anh lớp 10 THPT',
    MucTieu   = N'Phát triển 4 kỹ năng nghe nói đọc viết'
WHERE CHARINDEX('Anh 10', TenMonHoc) > 0;

UPDATE MonHoc SET
    TenMonHoc = N'Tin Học 10',
    MoTa      = N'Tin học lớp 10 THPT',
    MucTieu   = N'Nắm vững tin học căn bản và lập trình'
WHERE CHARINDEX('Tin H', TenMonHoc) > 0 AND CHARINDEX('10', TenMonHoc) > 0;

UPDATE MonHoc SET
    TenMonHoc = N'Toán 10',
    MoTa      = N'Toán học lớp 10 THPT',
    MucTieu   = N'Nắm vững đại số, hình học và giải tích cơ bản'
WHERE CHARINDEX('n 10', TenMonHoc) > 0 AND LEN(TenMonHoc) < 9;

UPDATE MonHoc SET
    TenMonHoc = N'Vật Lý 10',
    MoTa      = N'Vật lý lớp 10 THPT',
    MucTieu   = N'Hiểu các định luật cơ học và nhiệt học'
WHERE CHARINDEX('t L', TenMonHoc) > 0 AND CHARINDEX('10', TenMonHoc) > 0;

-- ================================================================
-- Fix BaiGiang MoTa (TieuDe để nguyên nếu không thể fix)
-- ================================================================
-- Tiếng Anh - TieuDe không có dấu
UPDATE BaiGiang SET MoTa = N'Từ vựng và ngữ pháp về cuộc sống gia đình'
WHERE TieuDe = 'Unit 1: Family Life';

UPDATE BaiGiang SET MoTa = N'Từ vựng về cơ thể và thì hiện tại hoàn thành'
WHERE TieuDe = 'Unit 2: Your Body and You';

-- Sinh học
UPDATE BaiGiang SET
    TieuDe = N'Tế bào - đơn vị cơ bản của sự sống',
    MoTa   = N'Cấu tạo tế bào nhân sơ và nhân thực'
WHERE CHARINDEX('b', TieuDe) > 0 AND CHARINDEX('o', TieuDe) > 0 AND CHARINDEX('s', TieuDe) > 0
  AND CHARINDEX('ng', TieuDe) > 0 AND LEN(TieuDe) BETWEEN 30 AND 50
  AND TieuDe NOT LIKE '%phân%' AND TieuDe NOT LIKE '%Python%';

UPDATE BaiGiang SET
    TieuDe = N'Các phân tử sinh học',
    MoTa   = N'Protein, lipid, carbohydrate và axit nucleic'
WHERE (CHARINDEX('ph', TieuDe) > 0 AND CHARINDEX('sinh h', TieuDe) > 0)
   OR (CHARINDEX('n t', TieuDe) > 0 AND CHARINDEX('sinh', TieuDe) > 0 AND LEN(TieuDe) < 30);

-- Lịch sử
UPDATE BaiGiang SET
    TieuDe = N'Lịch sử thế giới cổ đại',
    MoTa   = N'Các nền văn minh cổ đại phương Đông và phương Tây'
WHERE CHARINDEX('ch s', TieuDe) > 0 AND CHARINDEX('gi', TieuDe) > 0 AND LEN(TieuDe) < 35;

UPDATE BaiGiang SET
    TieuDe = N'Việt Nam thời tiền sử',
    MoTa   = N'Các giai đoạn tiền sử và sơ sử Việt Nam'
WHERE CHARINDEX('Vi', TieuDe) > 0 AND CHARINDEX('Nam', TieuDe) > 0 AND CHARINDEX('ti', TieuDe) > 0;

-- Địa lý
UPDATE BaiGiang SET
    TieuDe = N'Bản đồ và các phép chiếu bản đồ',
    MoTa   = N'Khái niệm bản đồ, tỷ lệ và cách đọc bản đồ'
WHERE CHARINDEX('n', TieuDe) > 0 AND CHARINDEX('ph', TieuDe) > 0 AND CHARINDEX('chi', TieuDe) > 0;

UPDATE BaiGiang SET
    TieuDe = N'Trái Đất trong hệ Mặt Trời',
    MoTa   = N'Vị trí, hình dạng và vận động của Trái Đất'
WHERE CHARINDEX('Tr', TieuDe) > 0 AND CHARINDEX('trong', TieuDe) > 0 AND CHARINDEX('Tr', RIGHT(TieuDe,10)) > 0;

-- Tin học
UPDATE BaiGiang SET
    TieuDe = N'Giới thiệu về Tin học và máy tính',
    MoTa   = N'Lịch sử máy tính, cấu trúc phần cứng và phần mềm'
WHERE CHARINDEX('Tin h', TieuDe) > 0 AND CHARINDEX('thi', TieuDe) > 0;

UPDATE BaiGiang SET
    TieuDe = N'Lập trình cơ bản với Python',
    MoTa   = N'Biến, kiểu dữ liệu và cấu trúc điều khiển'
WHERE CHARINDEX('Python', TieuDe) > 0;

-- GDCD
UPDATE BaiGiang SET
    TieuDe = N'Công dân với sự phát triển kinh tế',
    MoTa   = N'Vai trò công dân trong phát triển kinh tế đất nước'
WHERE CHARINDEX('ng d', TieuDe) > 0 AND CHARINDEX('kinh t', TieuDe) > 0;

-- Thể dục
UPDATE BaiGiang SET
    TieuDe = N'Thể dục thể thao và sức khỏe',
    MoTa   = N'Lý thuyết thể dục và tầm quan trọng của vận động'
WHERE CHARINDEX('d', TieuDe) > 0 AND CHARINDEX('thao', TieuDe) > 0 AND CHARINDEX('kh', TieuDe) > 0;

-- ================================================================
-- Fix BaiTap
-- ================================================================
UPDATE BaiTap SET
    TieuDe  = N'Bài tập Tế bào học',
    MoTa    = N'So sánh tế bào nhân sơ và nhân thực',
    NoiDung = N'Lập bảng so sánh tế bào nhân sơ và nhân thực theo: kích thước, nhân, màng nhân, bào quan.'
WHERE CHARINDEX('T', TieuDe) > 0 AND CHARINDEX('b', TieuDe) > 0 AND CHARINDEX('o h', TieuDe) > 0
  AND LEN(TieuDe) < 30;

UPDATE BaiTap SET
    TieuDe  = N'Writing Task - My Family',
    MoTa    = N'Viết đoạn văn về gia đình bằng tiếng Anh',
    NoiDung = N'Write a paragraph (80-100 words) describing your family members and their daily routines.'
WHERE TieuDe LIKE 'Writing Task%';

UPDATE BaiTap SET
    TieuDe  = N'Bài tập Lịch sử cổ đại',
    MoTa    = N'Phân tích đặc điểm các nền văn minh cổ đại',
    NoiDung = N'So sánh 3 nền văn minh: Ai Cập, Lưỡng Hà, Hy Lạp về địa lý, kinh tế và văn hóa.'
WHERE CHARINDEX('ch s', TieuDe) > 0 AND CHARINDEX('i', TieuDe) > 0 AND LEN(TieuDe) < 35;

UPDATE BaiTap SET
    TieuDe  = N'Bài tập đọc bản đồ',
    MoTa    = N'Thực hành đọc và phân tích bản đồ địa lý',
    NoiDung = N'Sử dụng Atlas, xác định: tọa độ địa lý 5 thành phố lớn VN, đọc ký hiệu bản đồ.'
WHERE CHARINDEX('c b', TieuDe) > 0 AND LEN(TieuDe) < 25
  AND TieuDe NOT LIKE '%Tin%' AND TieuDe NOT LIKE '%Python%' AND TieuDe NOT LIKE '%Sinh%';

UPDATE BaiTap SET
    TieuDe  = N'Bài tập Python - Bài 1',
    MoTa    = N'Viết chương trình Python cơ bản',
    NoiDung = N'Viết 3 chương trình: (1) In bảng cửu chương số 7, (2) Tính tổng 1..100, (3) Kiểm tra số nguyên tố.'
WHERE CHARINDEX('Python', TieuDe) > 0;

UPDATE BaiTap SET
    TieuDe  = N'Bài luận GDCD - Vai trò công dân',
    MoTa    = N'Viết bài luận về nghĩa vụ công dân',
    NoiDung = N'Viết bài luận 300 từ: Là học sinh, em có thể đóng góp gì cho sự phát triển kinh tế đất nước?'
WHERE CHARINDEX('GDCD', TieuDe) > 0 AND CHARINDEX('lu', TieuDe) > 0;

UPDATE BaiTap SET
    TieuDe  = N'Bài tập lý thuyết Thể dục',
    MoTa    = N'Trả lời câu hỏi lý thuyết thể dục',
    NoiDung = N'Trả lời 5 câu hỏi về: lợi ích vận động, quy tắc an toàn khi tập, cách khởi động đúng cách.'
WHERE CHARINDEX('thuy', TieuDe) > 0 AND (CHARINDEX('D', TieuDe) > 0 OR CHARINDEX('d', TieuDe) > 0)
  AND LEN(TieuDe) < 35;

-- ================================================================
-- Fix TaiLieu
-- ================================================================
UPDATE TaiLieu SET TenTaiLieu = N'Sơ đồ cấu tạo tế bào'
WHERE CHARINDEX('u t', TenTaiLieu) > 0 AND CHARINDEX('b', TenTaiLieu) > 0 AND LEN(TenTaiLieu) < 30;

UPDATE TaiLieu SET TenTaiLieu = N'Từ điển Anh-Việt Unit 1'
WHERE CHARINDEX('Anh', TenTaiLieu) > 0 AND CHARINDEX('Unit 1', TenTaiLieu) > 0;

UPDATE TaiLieu SET TenTaiLieu = N'Bản đồ thế giới cổ đại'
WHERE CHARINDEX('n', TenTaiLieu) > 0 AND CHARINDEX('gi', TenTaiLieu) > 0 AND CHARINDEX('i', TenTaiLieu) > 0
  AND LEN(TenTaiLieu) < 30 AND TenTaiLieu NOT LIKE 'Atlas%';

UPDATE TaiLieu SET TenTaiLieu = N'Atlas Địa lý lớp 10'
WHERE TenTaiLieu LIKE 'Atlas%';

UPDATE TaiLieu SET TenTaiLieu = N'Hướng dẫn cài Python'
WHERE CHARINDEX('Python', TenTaiLieu) > 0;

UPDATE TaiLieu SET TenTaiLieu = N'Slide GDCD chương 1'
WHERE TenTaiLieu LIKE 'Slide GDCD%';

UPDATE TaiLieu SET TenTaiLieu = N'Quy tắc an toàn thể dục'
WHERE CHARINDEX('an to', TenTaiLieu) > 0 AND CHARINDEX('d', TenTaiLieu) > 0;

-- ================================================================
-- VERIFY
-- ================================================================
PRINT N'=== MonHoc ===';
SELECT Id, TenMonHoc, LEFT(MoTa,40) MoTa FROM MonHoc ORDER BY TenMonHoc;

PRINT N'=== BaiGiang ===';
SELECT Id, LEFT(TieuDe,40) TieuDe, LEFT(MoTa,40) MoTa FROM BaiGiang ORDER BY Id;

PRINT N'=== BaiTap ===';
SELECT Id, LEFT(TieuDe,40) TieuDe, LEFT(MoTa,40) MoTa FROM BaiTap ORDER BY Id;

PRINT N'=== TaiLieu ===';
SELECT Id, TenTaiLieu FROM TaiLieu ORDER BY Id;
GO
