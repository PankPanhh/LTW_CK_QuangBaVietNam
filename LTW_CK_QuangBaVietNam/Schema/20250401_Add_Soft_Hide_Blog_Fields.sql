-- Migration: Add Soft Hide Blog Fields
-- Description: Thêm các trường để triển khai tính năng ẩn mềm (soft hide) bài viết
-- Date: 2025-04-01

-- Kiểm tra xem các cột đã tồn tại chưa
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='BaiViet' AND COLUMN_NAME='LyDoAn')
BEGIN
    ALTER TABLE BaiViet ADD LyDoAn NVARCHAR(500) NULL;
    PRINT 'Added LyDoAn column to BaiViet';
END
ELSE
BEGIN
    PRINT 'LyDoAn column already exists';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='BaiViet' AND COLUMN_NAME='NgayAn')
BEGIN
    ALTER TABLE BaiViet ADD NgayAn DATETIME NULL;
    PRINT 'Added NgayAn column to BaiViet';
END
ELSE
BEGIN
    PRINT 'NgayAn column already exists';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='BaiViet' AND COLUMN_NAME='NguoiAn')
BEGIN
    ALTER TABLE BaiViet ADD NguoiAn INT NULL;
    PRINT 'Added NguoiAn column to BaiViet';
END
ELSE
BEGIN
    PRINT 'NguoiAn column already exists';
END

-- Tạo index để cải thiện hiệu năng truy vấn theo TrangThai
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_BaiViet_TrangThai')
BEGIN
    CREATE INDEX IX_BaiViet_TrangThai ON BaiViet(TrangThai) INCLUDE (MaBaiViet, TieuDe, NgayDang);
    PRINT 'Created index IX_BaiViet_TrangThai';
END

-- Tạo index để cải thiện truy vấn theo NguoiDung
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_BaiViet_MaNguoiDung_TrangThai')
BEGIN
    CREATE INDEX IX_BaiViet_MaNguoiDung_TrangThai ON BaiViet(MaNguoiDung, TrangThai);
    PRINT 'Created index IX_BaiViet_MaNguoiDung_TrangThai';
END

PRINT 'Migration completed successfully!';

-- Script bổ sung các cột phục vụ tính năng Moderation cho bảng BinhLuan

-- 1. Thêm cột trạng thái (mặc định là 'visible' cho các bình luận cũ)
ALTER TABLE BinhLuan ADD TrangThai NVARCHAR(20) DEFAULT 'visible' NOT NULL;

-- 2. Thêm cột lưu lý do khi Admin ẩn hoặc Xoá bình luận
ALTER TABLE BinhLuan ADD LyDoAn NVARCHAR(500);

-- 3. Thêm cột lưu thời gian xử lý vi phạm
ALTER TABLE BinhLuan ADD NgayXuLy DATETIME;

-- 4. Thêm cột lưu ID của quản trị viên đã xử lý (hoặc người tự xoá)
ALTER TABLE BinhLuan ADD NguoiXuLy INT;
