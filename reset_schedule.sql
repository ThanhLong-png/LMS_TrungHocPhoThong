-- Script xóa lịch học, thông báo và yêu cầu cũ để seeding lại

-- 1. Xóa toàn bộ lịch học cũ
DELETE FROM [dbo].[LichHoc];
DBCC CHECKIDENT ('[LichHoc]', RESEED, 0);

-- 2. Xóa thông báo cũ
DELETE FROM [dbo].[ThongBaos];
DBCC CHECKIDENT ('[ThongBaos]', RESEED, 0);

-- 3. Xóa yêu cầu cũ
DELETE FROM [dbo].[YeuCauGiaoVien];
DBCC CHECKIDENT ('[YeuCauGiaoVien]', RESEED, 0);

PRINT 'Đã xóa toàn bộ dữ liệu cũ của Lịch Học, Thông Báo và Yêu Cầu. Hãy khởi động lại ứng dụng để seeding lại dữ liệu mới.';
