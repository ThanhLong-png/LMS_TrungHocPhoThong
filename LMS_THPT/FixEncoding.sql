USE LMS_THPT;
GO

-- =====================================================
-- FIX ENCODING: MonHoc - MoTa & MucTieu
-- =====================================================
UPDATE MonHoc SET MoTa = N'Sinh học lớp 10 THPT',     MucTieu = N'Hiểu cấu trúc tế bào và sinh học phân tử'       WHERE TenMonHoc = N'Sinh Học 10';
UPDATE MonHoc SET MoTa = N'Tiếng Anh lớp 10 THPT',    MucTieu = N'Phát triển 4 kỹ năng nghe nói đọc viết'         WHERE TenMonHoc = N'Tiếng Anh 10';
UPDATE MonHoc SET MoTa = N'Lịch sử lớp 10 THPT',      MucTieu = N'Nắm vững lịch sử Việt Nam và thế giới'          WHERE TenMonHoc = N'Lịch Sử 10';
UPDATE MonHoc SET MoTa = N'Địa lý lớp 10 THPT',       MucTieu = N'Hiểu địa lý tự nhiên và kinh tế xã hội'         WHERE TenMonHoc = N'Địa Lý 10';
UPDATE MonHoc SET MoTa = N'Tin học lớp 10 THPT',      MucTieu = N'Nắm vững tin học căn bản và lập trình'           WHERE TenMonHoc = N'Tin Học 10';
UPDATE MonHoc SET MoTa = N'Giáo dục công dân lớp 10', MucTieu = N'Hình thành nhận thức pháp luật và đạo đức'       WHERE TenMonHoc = N'GDCD 10';
UPDATE MonHoc SET MoTa = N'Thể dục lớp 10 THPT',      MucTieu = N'Rèn luyện thể chất và kỹ năng thể thao'         WHERE TenMonHoc = N'Thể Dục 10';

-- Fix các môn học cũ (nếu có)
UPDATE MonHoc SET MoTa = N'Toán học lớp 10 THPT',     MucTieu = N'Nắm vững đại số, hình học và giải tích cơ bản'  WHERE TenMonHoc LIKE N'Toán%';
UPDATE MonHoc SET MoTa = N'Ngữ văn lớp 10 THPT',      MucTieu = N'Phát triển kỹ năng đọc hiểu và tạo lập văn bản' WHERE TenMonHoc LIKE N'Ngữ Văn%';
UPDATE MonHoc SET MoTa = N'Vật lý lớp 10 THPT',       MucTieu = N'Hiểu các định luật cơ học và nhiệt học'         WHERE TenMonHoc LIKE N'Vật Lý%';
UPDATE MonHoc SET MoTa = N'Hóa học lớp 10 THPT',      MucTieu = N'Nắm vững hóa học đại cương và vô cơ'            WHERE TenMonHoc LIKE N'Hóa Học%';

-- =====================================================
-- FIX ENCODING: BaiGiang - TieuDe & MoTa
-- =====================================================
UPDATE BaiGiang SET MoTa = N'Cấu tạo tế bào nhân sơ và nhân thực'
    WHERE TieuDe = N'Tế bào - đơn vị cơ bản của sự sống';
UPDATE BaiGiang SET MoTa = N'Protein, lipid, carbohydrate và axit nucleic'
    WHERE TieuDe = N'Các phân tử sinh học';
UPDATE BaiGiang SET MoTa = N'Từ vựng và ngữ pháp về cuộc sống gia đình'
    WHERE TieuDe = N'Unit 1: Family Life';
UPDATE BaiGiang SET MoTa = N'Từ vựng về cơ thể và thì hiện tại hoàn thành'
    WHERE TieuDe = N'Unit 2: Your Body and You';
UPDATE BaiGiang SET MoTa = N'Các nền văn minh cổ đại phương Đông và phương Tây'
    WHERE TieuDe = N'Lịch sử thế giới cổ đại';
UPDATE BaiGiang SET MoTa = N'Các giai đoạn tiền sử và sơ sử Việt Nam'
    WHERE TieuDe = N'Việt Nam thời tiền sử';
UPDATE BaiGiang SET MoTa = N'Khái niệm bản đồ, tỷ lệ và cách đọc bản đồ'
    WHERE TieuDe = N'Bản đồ và các phép chiếu bản đồ';
UPDATE BaiGiang SET MoTa = N'Vị trí, hình dạng và vận động của Trái Đất'
    WHERE TieuDe = N'Trái Đất trong hệ Mặt Trời';
UPDATE BaiGiang SET MoTa = N'Lịch sử máy tính, cấu trúc phần cứng và phần mềm'
    WHERE TieuDe = N'Giới thiệu về Tin học và máy tính';
UPDATE BaiGiang SET MoTa = N'Biến, kiểu dữ liệu và cấu trúc điều khiển'
    WHERE TieuDe = N'Lập trình cơ bản với Python';
UPDATE BaiGiang SET MoTa = N'Vai trò công dân trong phát triển kinh tế đất nước'
    WHERE TieuDe = N'Công dân với sự phát triển kinh tế';
UPDATE BaiGiang SET MoTa = N'Lý thuyết thể dục và tầm quan trọng của vận động'
    WHERE TieuDe = N'Thể dục thể thao và sức khỏe';

-- =====================================================
-- FIX ENCODING: BaiTap - TieuDe, MoTa, NoiDung
-- =====================================================
UPDATE BaiTap SET
    MoTa    = N'So sánh tế bào nhân sơ và nhân thực',
    NoiDung = N'Lập bảng so sánh tế bào nhân sơ và nhân thực theo: kích thước, nhân, màng nhân, bào quan.'
    WHERE TieuDe = N'Bài tập Tế bào học';

UPDATE BaiTap SET
    MoTa    = N'Viết đoạn văn về gia đình bằng tiếng Anh',
    NoiDung = N'Write a paragraph (80-100 words) describing your family members and their daily routines.'
    WHERE TieuDe = N'Writing Task - My Family';

UPDATE BaiTap SET
    MoTa    = N'Phân tích đặc điểm các nền văn minh cổ đại',
    NoiDung = N'So sánh 3 nền văn minh: Ai Cập, Lưỡng Hà, Hy Lạp về địa lý, kinh tế và văn hóa.'
    WHERE TieuDe = N'Bài tập Lịch sử cổ đại';

UPDATE BaiTap SET
    MoTa    = N'Thực hành đọc và phân tích bản đồ địa lý',
    NoiDung = N'Sử dụng Atlas, xác định: tọa độ địa lý 5 thành phố lớn VN, đọc ký hiệu bản đồ.'
    WHERE TieuDe = N'Bài tập đọc bản đồ';

UPDATE BaiTap SET
    MoTa    = N'Viết chương trình Python cơ bản',
    NoiDung = N'Viết 3 chương trình: (1) In bảng cửu chương số 7, (2) Tính tổng 1..100, (3) Kiểm tra số nguyên tố.'
    WHERE TieuDe = N'Bài tập Python - Bài 1';

UPDATE BaiTap SET
    MoTa    = N'Viết bài luận về nghĩa vụ công dân',
    NoiDung = N'Viết bài luận 300 từ: Là học sinh, em có thể đóng góp gì cho sự phát triển kinh tế đất nước?'
    WHERE TieuDe = N'Bài luận GDCD - Vai trò công dân';

UPDATE BaiTap SET
    MoTa    = N'Trả lời câu hỏi lý thuyết thể dục',
    NoiDung = N'Trả lời 5 câu hỏi về: lợi ích vận động, quy tắc an toàn khi tập, cách khởi động đúng cách.'
    WHERE TieuDe = N'Bài tập lý thuyết Thể dục';

-- =====================================================
-- FIX ENCODING: TaiLieu
-- =====================================================
UPDATE TaiLieu SET TenTaiLieu = N'Sơ đồ cấu tạo tế bào'       WHERE TenTaiLieu LIKE N'%t%b%o%';
UPDATE TaiLieu SET TenTaiLieu = N'Từ điển Anh-Việt Unit 1'     WHERE TenTaiLieu LIKE N'T% i%n Anh%';
UPDATE TaiLieu SET TenTaiLieu = N'Bản đồ thế giới cổ đại'      WHERE TenTaiLieu LIKE N'B%n %% th%';
UPDATE TaiLieu SET TenTaiLieu = N'Atlas Địa lý lớp 10'         WHERE TenTaiLieu LIKE N'Atlas%';
UPDATE TaiLieu SET TenTaiLieu = N'Hướng dẫn cài Python'        WHERE TenTaiLieu LIKE N'H%ng d%n%Python%';
UPDATE TaiLieu SET TenTaiLieu = N'Slide GDCD chương 1'         WHERE TenTaiLieu LIKE N'Slide GDCD%';
UPDATE TaiLieu SET TenTaiLieu = N'Quy tắc an toàn thể dục'     WHERE TenTaiLieu LIKE N'Quy t%c%';

-- =====================================================
-- VERIFY
-- =====================================================
PRINT N'=== ĐÃ SỬA XONG ENCODING ===';
SELECT TenMonHoc, MoTa FROM MonHoc ORDER BY TenMonHoc;
GO
