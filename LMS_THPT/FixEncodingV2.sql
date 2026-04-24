USE LMS_THPT;
GO

-- =====================================================
-- FIX ENCODING V2: Dùng LIKE pattern không dấu để tìm đúng dòng
-- Vì TenMonHoc cũng đang bị sai encoding
-- =====================================================

-- FIX TenMonHoc + MoTa + MucTieu của MonHoc
UPDATE MonHoc SET
    TenMonHoc = N'Địa Lý 10',
    MoTa      = N'Địa lý lớp 10 THPT',
    MucTieu   = N'Hiểu địa lý tự nhiên và kinh tế xã hội'
WHERE TenMonHoc LIKE '%a Ly 10%' OR TenMonHoc LIKE '%?a L%10%';

UPDATE MonHoc SET
    TenMonHoc = N'GDCD 10',
    MoTa      = N'Giáo dục công dân lớp 10',
    MucTieu   = N'Hình thành nhận thức pháp luật và đạo đức'
WHERE TenMonHoc = 'GDCD 10';

UPDATE MonHoc SET
    TenMonHoc = N'Hóa Học 10',
    MoTa      = N'Hóa học lớp 10 THPT',
    MucTieu   = N'Nắm vững hóa học đại cương và vô cơ'
WHERE TenMonHoc LIKE '%a H_c 10%' OR TenMonHoc LIKE 'H%a H%c 10';

UPDATE MonHoc SET
    TenMonHoc = N'Lịch Sử 10',
    MoTa      = N'Lịch sử lớp 10 THPT',
    MucTieu   = N'Nắm vững lịch sử Việt Nam và thế giới'
WHERE TenMonHoc LIKE 'L_ch S_ 10%' OR TenMonHoc LIKE 'L%ch S%10';

UPDATE MonHoc SET
    TenMonHoc = N'Ngữ Văn 10',
    MoTa      = N'Ngữ văn lớp 10 THPT',
    MucTieu   = N'Phát triển kỹ năng đọc hiểu và tạo lập văn bản'
WHERE TenMonHoc LIKE 'Ng_ Van 10%' OR TenMonHoc LIKE 'Ng%Van 10';

UPDATE MonHoc SET
    TenMonHoc = N'Sinh Học 10',
    MoTa      = N'Sinh học lớp 10 THPT',
    MucTieu   = N'Hiểu cấu trúc tế bào và sinh học phân tử'
WHERE TenMonHoc LIKE 'Sinh H_c 10%' OR TenMonHoc LIKE 'Sinh H%c 10';

UPDATE MonHoc SET
    TenMonHoc = N'Thể Dục 10',
    MoTa      = N'Thể dục lớp 10 THPT',
    MucTieu   = N'Rèn luyện thể chất và kỹ năng thể thao'
WHERE TenMonHoc LIKE 'Th_ D_c 10%' OR TenMonHoc LIKE 'Th%D%c 10';

UPDATE MonHoc SET
    TenMonHoc = N'Tiếng Anh 10',
    MoTa      = N'Tiếng Anh lớp 10 THPT',
    MucTieu   = N'Phát triển 4 kỹ năng nghe nói đọc viết'
WHERE TenMonHoc LIKE 'Ti_ng Anh 10%' OR TenMonHoc LIKE 'Ti%ng Anh 10';

UPDATE MonHoc SET
    TenMonHoc = N'Tin Học 10',
    MoTa      = N'Tin học lớp 10 THPT',
    MucTieu   = N'Nắm vững tin học căn bản và lập trình'
WHERE TenMonHoc LIKE 'Tin H_c 10%' OR TenMonHoc LIKE 'Tin H%c 10';

UPDATE MonHoc SET
    TenMonHoc = N'Toán 10',
    MoTa      = N'Toán học lớp 10 THPT',
    MucTieu   = N'Nắm vững đại số, hình học và giải tích cơ bản'
WHERE TenMonHoc LIKE 'To_n 10%' OR TenMonHoc LIKE 'To%n 10';

UPDATE MonHoc SET
    TenMonHoc = N'Vật Lý 10',
    MoTa      = N'Vật lý lớp 10 THPT',
    MucTieu   = N'Hiểu các định luật cơ học và nhiệt học'
WHERE TenMonHoc LIKE 'V_t L%10%' OR TenMonHoc LIKE 'V%t L%10';

-- =====================================================
-- FIX BaiGiang - TieuDe + MoTa (dùng pattern không dấu)
-- =====================================================
-- Sinh học
UPDATE BaiGiang SET
    TieuDe = N'Tế bào - đơn vị cơ bản của sự sống',
    MoTa   = N'Cấu tạo tế bào nhân sơ và nhân thực'
WHERE TieuDe LIKE 'T_ b_o%' OR TieuDe LIKE 'T%b%o%s_ng';

UPDATE BaiGiang SET
    TieuDe = N'Các phân tử sinh học',
    MoTa   = N'Protein, lipid, carbohydrate và axit nucleic'
WHERE TieuDe LIKE 'C_c ph_n t_%' OR TieuDe LIKE 'C%ph%n t%sinh h%c';

-- Tiếng Anh (không dấu, chỉ fix MoTa)
UPDATE BaiGiang SET MoTa = N'Từ vựng và ngữ pháp về cuộc sống gia đình'
WHERE TieuDe = 'Unit 1: Family Life';

UPDATE BaiGiang SET MoTa = N'Từ vựng về cơ thể và thì hiện tại hoàn thành'
WHERE TieuDe = 'Unit 2: Your Body and You';

-- Lịch sử
UPDATE BaiGiang SET
    TieuDe = N'Lịch sử thế giới cổ đại',
    MoTa   = N'Các nền văn minh cổ đại phương Đông và phương Tây'
WHERE TieuDe LIKE 'L_ch s_ th_ gi_i c_ __i%' OR TieuDe LIKE 'L%ch s%gi%i c%';

UPDATE BaiGiang SET
    TieuDe = N'Việt Nam thời tiền sử',
    MoTa   = N'Các giai đoạn tiền sử và sơ sử Việt Nam'
WHERE TieuDe LIKE 'Vi_t Nam th_i ti_n s_%' OR TieuDe LIKE 'Vi%t Nam th%ti%n s%';

-- Địa lý
UPDATE BaiGiang SET
    TieuDe = N'Bản đồ và các phép chiếu bản đồ',
    MoTa   = N'Khái niệm bản đồ, tỷ lệ và cách đọc bản đồ'
WHERE TieuDe LIKE 'B_n %_ v_ c_c ph_p chi_u%' OR TieuDe LIKE 'B%n%v%ph%p chi%u';

UPDATE BaiGiang SET
    TieuDe = N'Trái Đất trong hệ Mặt Trời',
    MoTa   = N'Vị trí, hình dạng và vận động của Trái Đất'
WHERE TieuDe LIKE 'Tr_i __ trong h_ M_t Tr_i%' OR TieuDe LIKE 'Tr%i%trong h%M%t Tr%i';

-- Tin học
UPDATE BaiGiang SET
    TieuDe = N'Giới thiệu về Tin học và máy tính',
    MoTa   = N'Lịch sử máy tính, cấu trúc phần cứng và phần mềm'
WHERE TieuDe LIKE 'Gi_i thi_u v_ Tin h_c%' OR TieuDe LIKE 'Gi%i thi%u v%Tin h%c';

UPDATE BaiGiang SET
    TieuDe = N'Lập trình cơ bản với Python',
    MoTa   = N'Biến, kiểu dữ liệu và cấu trúc điều khiển'
WHERE TieuDe LIKE 'L_p trình c_ b_n v_i Python%' OR TieuDe LIKE 'L%p tr%nh c%b%n v%i Python';

-- GDCD
UPDATE BaiGiang SET
    TieuDe = N'Công dân với sự phát triển kinh tế',
    MoTa   = N'Vai trò công dân trong phát triển kinh tế đất nước'
WHERE TieuDe LIKE 'C_ng d_n v_i s_ ph_t tri_n kinh t_%' OR TieuDe LIKE 'C%ng d%n v%i s%ph%t tri%n kinh t%';

-- Thể dục
UPDATE BaiGiang SET
    TieuDe = N'Thể dục thể thao và sức khỏe',
    MoTa   = N'Lý thuyết thể dục và tầm quan trọng của vận động'
WHERE TieuDe LIKE 'Th_ d_c th_ thao v_ s_c kh_e%' OR TieuDe LIKE 'Th%d%c th%thao v%s%c kh%e';

-- =====================================================
-- FIX BaiTap - TieuDe, MoTa, NoiDung
-- =====================================================
UPDATE BaiTap SET
    TieuDe  = N'Bài tập Tế bào học',
    MoTa    = N'So sánh tế bào nhân sơ và nhân thực',
    NoiDung = N'Lập bảng so sánh tế bào nhân sơ và nhân thực theo: kích thước, nhân, màng nhân, bào quan.'
WHERE TieuDe LIKE 'B_i t_p T_ b_o h_c%' OR TieuDe LIKE 'B%i t%p T%b%o h%c';

UPDATE BaiTap SET
    TieuDe  = N'Writing Task - My Family',
    MoTa    = N'Viết đoạn văn về gia đình bằng tiếng Anh',
    NoiDung = N'Write a paragraph (80-100 words) describing your family members and their daily routines.'
WHERE TieuDe = 'Writing Task - My Family';

UPDATE BaiTap SET
    TieuDe  = N'Bài tập Lịch sử cổ đại',
    MoTa    = N'Phân tích đặc điểm các nền văn minh cổ đại',
    NoiDung = N'So sánh 3 nền văn minh: Ai Cập, Lưỡng Hà, Hy Lạp về địa lý, kinh tế và văn hóa.'
WHERE TieuDe LIKE 'B_i t_p L_ch s_ c_ __i%' OR TieuDe LIKE 'B%i t%p L%ch s%c%';

UPDATE BaiTap SET
    TieuDe  = N'Bài tập đọc bản đồ',
    MoTa    = N'Thực hành đọc và phân tích bản đồ địa lý',
    NoiDung = N'Sử dụng Atlas, xác định: tọa độ địa lý 5 thành phố lớn VN, đọc ký hiệu bản đồ.'
WHERE TieuDe LIKE 'B_i t_p __c b_n ___' OR TieuDe LIKE 'B%i t%p%c b%n %';

UPDATE BaiTap SET
    TieuDe  = N'Bài tập Python - Bài 1',
    MoTa    = N'Viết chương trình Python cơ bản',
    NoiDung = N'Viết 3 chương trình: (1) In bảng cửu chương số 7, (2) Tính tổng 1..100, (3) Kiểm tra số nguyên tố.'
WHERE TieuDe LIKE 'B_i t_p Python%' OR TieuDe LIKE 'B%i t%p Python%';

UPDATE BaiTap SET
    TieuDe  = N'Bài luận GDCD - Vai trò công dân',
    MoTa    = N'Viết bài luận về nghĩa vụ công dân',
    NoiDung = N'Viết bài luận 300 từ: Là học sinh, em có thể đóng góp gì cho sự phát triển kinh tế đất nước?'
WHERE TieuDe LIKE 'B_i lu_n GDCD%' OR TieuDe LIKE 'B%i lu%n GDCD%';

UPDATE BaiTap SET
    TieuDe  = N'Bài tập lý thuyết Thể dục',
    MoTa    = N'Trả lời câu hỏi lý thuyết thể dục',
    NoiDung = N'Trả lời 5 câu hỏi về: lợi ích vận động, quy tắc an toàn khi tập, cách khởi động đúng cách.'
WHERE TieuDe LIKE 'B_i t_p l_ thuy_t Th_ d_c%' OR TieuDe LIKE 'B%i t%p l%thuy%t Th%d%c';

-- =====================================================
-- FIX TaiLieu
-- =====================================================
UPDATE TaiLieu SET TenTaiLieu = N'Sơ đồ cấu tạo tế bào'
WHERE TenTaiLieu LIKE 'S_ __ c_u t_o t_ b_o%' OR TenTaiLieu LIKE 'S%c%u t%o t%b%o';

UPDATE TaiLieu SET TenTaiLieu = N'Từ điển Anh-Việt Unit 1'
WHERE TenTaiLieu LIKE 'T_ _i_n Anh%' OR TenTaiLieu LIKE 'T%i%n Anh%';

UPDATE TaiLieu SET TenTaiLieu = N'Bản đồ thế giới cổ đại'
WHERE TenTaiLieu LIKE 'B_n __ th_ gi_i c_ __i%' OR TenTaiLieu LIKE 'B%n%th%gi%i c%';

UPDATE TaiLieu SET TenTaiLieu = N'Atlas Địa lý lớp 10'
WHERE TenTaiLieu LIKE 'Atlas%' AND TenTaiLieu LIKE '%10';

UPDATE TaiLieu SET TenTaiLieu = N'Hướng dẫn cài Python'
WHERE TenTaiLieu LIKE 'H__ng d_n c_i Python%' OR TenTaiLieu LIKE 'H%ng d%n c%i Python%';

UPDATE TaiLieu SET TenTaiLieu = N'Slide GDCD chương 1'
WHERE TenTaiLieu LIKE 'Slide GDCD%';

UPDATE TaiLieu SET TenTaiLieu = N'Quy tắc an toàn thể dục'
WHERE TenTaiLieu LIKE 'Quy t_c an to_n th_ d_c%' OR TenTaiLieu LIKE 'Quy t%c an to%n th%d%c';

-- =====================================================
-- VERIFY KẾT QUẢ
-- =====================================================
PRINT N'=== KẾT QUẢ SAU KHI FIX ===';
SELECT TenMonHoc, LEFT(MoTa, 50) AS MoTa FROM MonHoc ORDER BY TenMonHoc;
SELECT TieuDe, LEFT(MoTa, 50) AS MoTa FROM BaiGiang ORDER BY Id;
SELECT TieuDe, LEFT(MoTa, 50) AS MoTa FROM BaiTap ORDER BY Id;
GO
