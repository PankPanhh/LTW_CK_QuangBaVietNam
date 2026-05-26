INSERT INTO DiaDiem
(
    TenDiaDiem,
    Slug,
    MoTaNgan,
    MaDanhMuc,
    GiaVe,
    GioMoCua,
    VungMien,
    KinhDo,
    ViDo,
    DiaChiChiTiet,
    TinhThanh
)
VALUES
(
    N'Chợ Bến Thành',
    'cho-ben-thanh',
    N'Biểu tượng nổi tiếng của TP.HCM với khu ẩm thực và mua sắm nhộn nhịp.',
    2,
    0,
    N'06:00 - 18:00',
    N'Nam',
    106.6980,
    10.7720,
    N'Quận 1, TP.HCM',
    N'TP.HCM'
),
(
    N'Nhà thờ Đức Bà',
    'nha-tho-duc-ba',
    N'Công trình kiến trúc Pháp cổ nổi bật giữa trung tâm thành phố.',
    2,
    0,
    N'08:00 - 17:00',
    N'Nam',
    106.6990,
    10.7798,
    N'Quận 1, TP.HCM',
    N'TP.HCM'
),
(
    N'Bưu điện Thành phố',
    'buu-dien-thanh-pho',
    N'Công trình kiến trúc cổ kính nổi tiếng cạnh Nhà thờ Đức Bà.',
    2,
    0,
    N'07:00 - 19:00',
    N'Nam',
    106.6995,
    10.7801,
    N'Quận 1, TP.HCM',
    N'TP.HCM'
),
(
    N'Landmark 81',
    'landmark-81',
    N'Tòa nhà cao nhất Việt Nam với đài quan sát và trung tâm thương mại.',
    3,
    0,
    N'09:00 - 22:00',
    N'Nam',
    106.7218,
    10.7951,
    N'Bình Thạnh, TP.HCM',
    N'TP.HCM'
),
(
    N'Phố đi bộ Nguyễn Huệ',
    'pho-di-bo-nguyen-hue',
    N'Không gian vui chơi và check-in nổi tiếng.',
    3,
    0,
    N'Cả ngày',
    N'Nam',
    106.7032,
    10.7745,
    N'Quận 1, TP.HCM',
    N'TP.HCM'
),
(
    N'Dinh Độc Lập',
    'dinh-doc-lap',
    N'Di tích lịch sử nổi tiếng gắn với ngày thống nhất đất nước.',
    2,
    65000,
    N'08:00 - 16:30',
    N'Nam',
    106.6953,
    10.7770,
    N'Quận 1, TP.HCM',
    N'TP.HCM'
),
(
    N'Thảo Cầm Viên Sài Gòn',
    'thao-cam-vien',
    N'Sở thú và công viên xanh lâu đời giữa trung tâm thành phố.',
    3,
    60000,
    N'07:00 - 17:30',
    N'Nam',
    106.7052,
    10.7870,
    N'Quận 1, TP.HCM',
    N'TP.HCM'
),
(
    N'Địa đạo Củ Chi',
    'dia-dao-cu-chi',
    N'Hệ thống địa đạo lịch sử nổi tiếng thời chiến tranh.',
    2,
    125000,
    N'07:00 - 17:00',
    N'Nam',
    106.4620,
    11.1436,
    N'Củ Chi, TP.HCM',
    N'TP.HCM'
);
DELETE FROM ChiTietLichTrinh;
DELETE FROM NgayLichTrinh;
DELETE FROM LichTrinh;
DELETE FROM BinhLuan;
DELETE FROM LikeBaiViet;
DELETE FROM AnhBaiViet;
DELETE FROM BaiViet;
DELETE FROM BoSuuTapDiaDiem;
DELETE FROM BoSuuTap;
DELETE FROM YeuThich;
DBCC CHECKIDENT ('BaiViet', RESEED, 0);
DBCC CHECKIDENT ('AnhBaiViet', RESEED, 0);
DBCC CHECKIDENT ('LikeBaiViet', RESEED, 0);
DBCC CHECKIDENT ('BinhLuan', RESEED, 0);
DBCC CHECKIDENT ('BoSuuTap', RESEED, 0);
DBCC CHECKIDENT ('LichTrinh', RESEED, 0);
DBCC CHECKIDENT ('NgayLichTrinh', RESEED, 0);
DBCC CHECKIDENT ('ChiTietLichTrinh', RESEED, 0);
-- =========================================================================
-- SEED NGUOI DUNG
-- =========================================================================

DELETE FROM NguoiDung;

DBCC CHECKIDENT ('NguoiDung', RESEED, 0);

INSERT INTO NguoiDung
(
    HoTen,
    Email,
    MatKhauHash,
    SoDienThoai,
    ThanhPho,
    QuocGia,
    VaiTro
)
VALUES
(
    N'Nguyễn Phương Anh',
    'phuonganh@gmail.com',
    '123456',
    '0901234567',
    N'TP.HCM',
    N'Việt Nam',
    1
),
(
    N'Trần Minh Khang',
    'minhkhang@gmail.com',
    '123456',
    '0902222222',
    N'Đà Nẵng',
    N'Việt Nam',
    2
),
(
    N'Lê Thu Hà',
    'thuha@gmail.com',
    '123456',
    '0903333333',
    N'Hà Nội',
    N'Việt Nam',
    2
);
-- =========================================================================
-- YÊU THÍCH
-- =========================================================================

INSERT INTO YeuThich
(MaNguoiDung, MaDiaDiem)
VALUES
(1, 1),
(1, 3),
(1, 10),

(2, 2),
(2, 13),

(3, 4),
(3, 15);
-- =========================================================================
-- BỘ SƯU TẬP
-- =========================================================================

INSERT INTO BoSuuTap
(MaNguoiDung, TenBoSuuTap, MoTa)
VALUES
(1, N'Địa điểm chill', N'Các nơi thư giãn đẹp'),
(2, N'Du lịch miền Trung', N'Checklist đi chơi'),
(3, N'Sài Gòn cuối tuần', N'Ăn chơi tại TP.HCM');
-- =========================================================================
-- BỘ SƯU TẬP ĐỊA ĐIỂM
-- =========================================================================

INSERT INTO BoSuuTapDiaDiem
(MaBoSuuTap, MaDiaDiem)
VALUES
(1, 3),
(1, 9),

(2, 2),
(2, 5),
(2, 7),

(3, 10),
(3, 14),
(3, 15);
-- =========================================================================
-- BÀI VIẾT
-- =========================================================================

INSERT INTO BaiViet
(
    TieuDe,
    NoiDung,
    MaDiaDiem,
    MaNguoiDung,
    TrangThai,
    LuotLike
)
VALUES
(
    N'Kinh nghiệm đi Đà Lạt 3N2Đ',
    N'Thời tiết rất đẹp, nên đi săn mây sáng sớm.',
    3,
    1,
    'approved',
    12
),
(
    N'Hội An về đêm cực đẹp',
    N'Đèn lồng và đồ ăn rất đáng thử.',
    2,
    2,
    'approved',
    20
),
(
    N'Sài Gòn cuối tuần đi đâu?',
    N'Nguyễn Huệ và Landmark rất đông vui.',
    14,
    3,
    'approved',
    8
);
-- =========================================================================
-- ẢNH BÀI VIẾT
-- =========================================================================

INSERT INTO AnhBaiViet
(MaBaiViet, DuongDanAnh, ThuTu)
VALUES
(1, '/Content/images/blogs/dalat-blog.jpg', 1),
(2, '/Content/images/blogs/hoian-blog.jpg', 1),
(3, '/Content/images/blogs/saigon-blog.jpg', 1);
-- =========================================================================
-- LIKE BÀI VIẾT
-- =========================================================================

INSERT INTO LikeBaiViet
(MaBaiViet, MaNguoiDung)
VALUES
(1, 2),
(1, 3),

(2, 1),
(2, 3),

(3, 1);
-- =========================================================================
-- BÌNH LUẬN
-- =========================================================================

INSERT INTO BinhLuan
(
    MaBaiViet,
    MaNguoiDung,
    NoiDung,
    TrangThai
)
VALUES
(1, 2, N'Đà Lạt mùa này đẹp lắm', 'visible'),
(1, 3, N'Cho xin lịch trình với', 'visible'),

(2, 1, N'Hội An rất chill', 'visible'),

(3, 2, N'Landmark buổi tối siêu đẹp', 'visible');
-- =========================================================================
-- LỊCH TRÌNH
-- =========================================================================

INSERT INTO LichTrinh
(
    TenLichTrinh,
    MoTa,
    MaNguoiDung,
    SoNgay,
    TrangThai
)
VALUES
(
    N'Đà Lạt thư giãn',
    N'Đi chơi và săn mây',
    1,
    3,
    'public'
),
(
    N'Sài Gòn cuối tuần',
    N'Food tour và check-in',
    2,
    2,
    'public'
);
-- =========================================================================
-- NGÀY LỊCH TRÌNH
-- =========================================================================

INSERT INTO NgayLichTrinh
(MaLichTrinh, ThuTuNgay, TieuDe)
VALUES
(1, 1, N'Ngày đầu khám phá'),
(1, 2, N'Săn mây'),

(2, 1, N'Quận 1'),
(2, 2, N'Check-in hiện đại');
-- =========================================================================
-- CHI TIẾT LỊCH TRÌNH
-- =========================================================================

INSERT INTO ChiTietLichTrinh
(
    MaNgay,
    MaDiaDiem,
    GioBatDau,
    GioKetThuc,
    GhiChu,
    ThuTu
)
VALUES
(1, 3, '08:00', '11:00', N'Uống cà phê', 1),
(1, 9, '13:00', '16:00', N'Chụp ảnh', 2),

(2, 3, '05:00', '08:00', N'Săn mây sáng sớm', 1),

(3, 10, '09:00', '11:00', N'Ăn sáng', 1),
(3, 14, '18:00', '21:00', N'Dạo phố đi bộ', 2),

(4, 13, '15:00', '18:00', N'Ngắm thành phố', 1);
