-- =========================================================================
-- SYSTEM SCHEMA GENERATED FILE - DỰ ÁN QUẢNG BÁ DU LỊCH VIỆT NAM
-- Gộp toàn bộ các bảng, chỉ số (indexes) và dữ liệu mẫu (seed data)
-- =========================================================================

-- 1. BẢNG NGUOIDUNG (Thông tin tài khoản và phân quyền)
CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    MatKhauHash NVARCHAR(255) NOT NULL,
    SoDienThoai VARCHAR(20),
    
    -- Các trường bổ sung từ hồ sơ cá nhân
    NgaySinh DATE NULL,
    TieuSu NVARCHAR(500) NULL,
    ThanhPho NVARCHAR(100) NULL,
    QuocGia NVARCHAR(100) NULL,

    AnhDaiDien NVARCHAR(MAX),
    VaiTro INT NOT NULL DEFAULT 2, -- 1: Admin, 2: User
    TrangThai BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NULL
);

-- 2. BẢNG DANHMUC (Danh mục địa điểm du lịch)
CREATE TABLE DanhMuc (
    MaDanhMuc INT PRIMARY KEY IDENTITY(1,1),
    TenDanhMuc NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(MAX)
);

-- 3. BẢNG DIADIEM (Thông tin chi tiết các địa điểm du lịch)
CREATE TABLE DiaDiem (
    MaDiaDiem INT PRIMARY KEY IDENTITY(1,1),
    TenDiaDiem NVARCHAR(255) NOT NULL,
    Slug VARCHAR(255) UNIQUE NOT NULL,
    MoTaNgan NVARCHAR(MAX), -- Cập nhật trực tiếp kiểu dữ liệu tối đa
    MoTaChiTiet NVARCHAR(MAX),
    MaDanhMuc INT NOT NULL,
    GiaVe DECIMAL(18,2) DEFAULT 0,
    GioMoCua NVARCHAR(100),
    VungMien NVARCHAR(50), 
    KinhDo DECIMAL(18,10),
    ViDo DECIMAL(18,10),
    DiaChiChiTiet NVARCHAR(MAX),
    SoDienThoai NVARCHAR(20),
    Email NVARCHAR(255),
    Website NVARCHAR(255),
    TrangThai BIT DEFAULT 1,
    LuotXem INT DEFAULT 0,
    DiemDanhGiaTB FLOAT DEFAULT 0,
    NgayDang DATETIME DEFAULT GETDATE(),
    
    -- Các trường bổ sung từ các bản cập nhật schema
    TinhThanh NVARCHAR(100) NULL,
    LaDiemChinh BIT DEFAULT 0,
    
    FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc)
);

-- 4. BẢNG ANHDIADIEM (Hình ảnh chi tiết của địa điểm)
CREATE TABLE AnhDiaDiem (
    MaAnh INT PRIMARY KEY IDENTITY(1,1),
    MaDiaDiem INT NOT NULL,
    DuongDanAnh NVARCHAR(MAX) NOT NULL,
    LaAnhChinh BIT DEFAULT 0,
    FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem)
);

-- 5. BẢNG GIAVECHITIET (Bảng giá vé chi tiết theo phân loại đối tượng khách)
CREATE TABLE GiaVeChiTiet (
    MaGiaVe INT PRIMARY KEY IDENTITY(1,1),
    MaDiaDiem INT NOT NULL,
    LoaiKhach NVARCHAR(50) NOT NULL, 
    Gia DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem)
);

-- 6. BẢNG TRAINGHIEM (Các hoạt động ẩm thực, vui chơi, check-in tại địa điểm)
CREATE TABLE TraiNghiem (
    MaTraiNghiem INT PRIMARY KEY IDENTITY(1,1),
    MaDiaDiem INT NOT NULL,
    LoaiTraiNghiem NVARCHAR(50) NOT NULL, -- eat, play, checkin
    TieuDe NVARCHAR(255),
    MoTa NVARCHAR(MAX),
    Icon VARCHAR(100),
    FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem)
);

-- 7. BẢNG YEUTHICH (Lưu các địa điểm người dùng yêu thích)
CREATE TABLE YeuThich (
    MaNguoiDung INT NOT NULL,
    MaDiaDiem   INT NOT NULL,
    NgayLuu     DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_YeuThich PRIMARY KEY (MaNguoiDung, MaDiaDiem),
    CONSTRAINT FK_YeuThich_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT FK_YeuThich_DiaDiem FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem) ON DELETE CASCADE
);

-- 8. BẢNG BOSUUTAP (Lưu các bộ sưu tập địa điểm của cá nhân người dùng)
CREATE TABLE BoSuuTap (
    MaBoSuuTap INT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    TenBoSuuTap NVARCHAR(150) NOT NULL,
    MoTa NVARCHAR(500) NULL,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_BoSuuTap_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);

-- 9. BẢNG BOSUUTAPDIADIEM (Bảng trung gian liên kết địa điểm vào bộ sưu tập)
CREATE TABLE BoSuuTapDiaDiem (
    MaBoSuuTap INT NOT NULL,
    MaDiaDiem INT NOT NULL,
    NgayThem DATETIME NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY (MaBoSuuTap, MaDiaDiem),
    CONSTRAINT FK_BSTD_BoSuuTap FOREIGN KEY (MaBoSuuTap) REFERENCES BoSuuTap(MaBoSuuTap),
    CONSTRAINT FK_BSTD_DiaDiem FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem)
);

-- 10. BẢNG BAIVIET (Các bài viết chia sẻ/blog trải nghiệm của người dùng)
CREATE TABLE BaiViet (
    MaBaiViet INT IDENTITY(1,1) PRIMARY KEY,
    TieuDe NVARCHAR(255) NOT NULL,
    NoiDung NVARCHAR(MAX) NOT NULL,
    MaDiaDiem INT NULL,
    MaNguoiDung INT NOT NULL,
    
    -- Trạng thái duyệt bài
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'pending', -- pending | approved | rejected
    LyDoTuChoi NVARCHAR(500) NULL,
    
    LuotLike INT DEFAULT 0,
    NgayDang DATETIME DEFAULT GETDATE(),
    NgayDuyet DATETIME NULL,
    NguoiDuyet INT NULL,

    -- Các trường Soft Hide bổ sung phục vụ quản trị bài viết
    LyDoAn NVARCHAR(500) NULL,
    NgayAn DATETIME NULL,
    NguoiAn INT NULL,

    FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiDuyet) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiAn) REFERENCES NguoiDung(MaNguoiDung)
);

-- 11. BẢNG ANHBAIVIET (Các hình ảnh đính kèm bài viết chia sẻ)
CREATE TABLE AnhBaiViet (
    MaAnh INT IDENTITY(1,1) PRIMARY KEY,
    MaBaiViet INT NOT NULL,
    DuongDanAnh NVARCHAR(MAX) NOT NULL,
    ThuTu INT DEFAULT 0,
    FOREIGN KEY (MaBaiViet) REFERENCES BaiViet(MaBaiViet)
);

-- 12. BẢNG LIKEBAIVIET (Lưu vết lượt thích bài viết)
CREATE TABLE LikeBaiViet (
    MaLike INT IDENTITY(1,1) PRIMARY KEY,
    MaBaiViet INT NOT NULL,
    MaNguoiDung INT NOT NULL,
    NgayLike DATETIME DEFAULT GETDATE(),

    CONSTRAINT UQ_Like UNIQUE (MaBaiViet, MaNguoiDung),
    FOREIGN KEY (MaBaiViet) REFERENCES BaiViet(MaBaiViet),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);

-- 13. BẢNG BINHLUAN (Bình luận trao đổi trong bài viết chia sẻ)
CREATE TABLE BinhLuan (
    MaBinhLuan INT IDENTITY(1,1) PRIMARY KEY,
    MaBaiViet INT NOT NULL,
    MaNguoiDung INT NOT NULL,
    NoiDung NVARCHAR(MAX) NOT NULL,
    ParentId INT NULL,
    NgayDang DATETIME DEFAULT GETDATE(),

    -- Các trường bổ sung phục vụ Moderation (kiểm duyệt ẩn/xóa bình luận)
    TrangThai NVARCHAR(20) DEFAULT 'visible' NOT NULL,
    LyDoAn NVARCHAR(500) NULL,
    NgayXuLy DATETIME NULL,
    NguoiXuLy INT NULL,

    FOREIGN KEY (MaBaiViet) REFERENCES BaiViet(MaBaiViet),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (ParentId) REFERENCES BinhLuan(MaBinhLuan),
    FOREIGN KEY (NguoiXuLy) REFERENCES NguoiDung(MaNguoiDung)
);

-- 14. BẢNG LICHTRINH (Lịch trình chuyến đi của người dùng)
CREATE TABLE LichTrinh (
    MaLichTrinh INT IDENTITY(1,1) PRIMARY KEY,
    TenLichTrinh NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(MAX),
    MaNguoiDung INT NOT NULL,
    SoNgay INT DEFAULT 1,
    TrangThai NVARCHAR(20) DEFAULT 'private', -- private | public
    AnhBia NVARCHAR(MAX),
    NgayBatDau DATE,
    NgayKetThuc DATE,
    LuotXem INT DEFAULT 0,
    LuotLike INT DEFAULT 0,
    NgayTao DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);

-- 15. BẢNG NGAYLICHTRINH (Chi tiết phân chia các ngày trong một lịch trình)
CREATE TABLE NgayLichTrinh (
    MaNgay INT IDENTITY(1,1) PRIMARY KEY,
    MaLichTrinh INT NOT NULL,
    ThuTuNgay INT NOT NULL,
    TieuDe NVARCHAR(255),
    
    FOREIGN KEY (MaLichTrinh) REFERENCES LichTrinh(MaLichTrinh)
);

-- 16. BẢNG CHITIETLICHTRINH (Các địa điểm cụ thể cần ghé thăm trong ngày)
CREATE TABLE ChiTietLichTrinh (
    MaChiTiet INT IDENTITY(1,1) PRIMARY KEY,
    MaNgay INT NOT NULL,
    MaDiaDiem INT NOT NULL,
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NULL,
    GhiChu NVARCHAR(500),
    ThuTu INT NOT NULL,
    
    FOREIGN KEY (MaNgay) REFERENCES NgayLichTrinh(MaNgay),
    FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem)
);

-- =========================================================================
-- TẠO CHỈ SỐ (INDEXES) ĐỂ TỐI ƯU HOÁ HIỆU NĂNG TRUY VẤN
-- =========================================================================
CREATE INDEX IDX_Post_Status ON BaiViet(TrangThai);
CREATE INDEX IDX_Post_Place ON BaiViet(MaDiaDiem);
CREATE INDEX IX_BaiViet_TrangThai ON BaiViet(TrangThai) INCLUDE (MaBaiViet, TieuDe, NgayDang);
CREATE INDEX IX_BaiViet_MaNguoiDung_TrangThai ON BaiViet(MaNguoiDung, TrangThai);


-- =========================================================================
-- SEED DATA (DỮ LIỆU BAN ĐẦU) CHO HỆ THỐNG
-- =========================================================================

-- 1. CHÈN DANH MỤC DU LỊCH
INSERT INTO DanhMuc (TenDanhMuc, MoTa) VALUES 
(N'Di sản thế giới', N'Các địa danh được UNESCO công nhận là di sản thiên nhiên hoặc văn hóa thế giới.'),
(N'Văn hóa - Lịch sử', N'Các điểm đến mang đậm giá trị truyền thống, lịch sử dân tộc.'),
(N'Thành phố nghỉ dưỡng', N'Các đô thị du lịch hiện đại, không khí trong lành, dịch vụ cao cấp.'),
(N'Biển đảo', N'Khám phá vẻ đẹp của đại dương và các hòn đảo hoang sơ.');

-- 2. CHÈN ĐỊA ĐIỂM TIÊU BIỂU (8 Địa điểm cốt lõi với Mô tả ngắn chi tiết & Tỉnh thành / Điểm chính)
INSERT INTO DiaDiem (TenDiaDiem, Slug, MoTaNgan, MaDanhMuc, GiaVe, GioMoCua, VungMien, KinhDo, ViDo, DiaChiChiTiet, SoDienThoai, Email, Website, TinhThanh, LaDiemChinh) VALUES 
(
    N'Vịnh Hạ Long', 'vinh-ha-long', 
    N'Vịnh Hạ Long hiện lên như một kiệt tác thủy mặc khổng lồ được tạo hóa tạc bằng đá và nước giữa biển khơi Đông Bắc. Nơi đây mê hoặc lữ khách bởi hàng ngàn đảo đá vôi kỳ vĩ soi bóng xuống làn nước xanh màu ngọc bích, gợi nhắc huyền thoại rồng mẹ hạ phàm phun châu nhả ngọc bảo vệ bờ cõi. Đi sâu vào lòng di sản, những hang động như Sửng Sốt hay Thiên Cung mở ra thế giới siêu thực với thạch nhũ lấp lánh như lâu đài pha lê, nơi thời gian dường như ngưng đọng triệu năm. Trải nghiệm Hạ Long tuyệt vời nhất là khi thả mình trên du thuyền sang trọng, đón làn gió mặn mòi và chiêm ngưỡng hoàng hôn nhuộm đỏ rực rỡ cả vùng trời biển bao la. Đây chính là bản trường ca của đá và nước, một di sản thiên nhiên thế giới mà bất kỳ ai cũng khao khát được chạm tay vào một lần trong đời để cảm nhận trọn vẹn vẻ đẹp tráng lệ, kiêu hãnh của non sông Việt Nam.', 
    1, 290000, N'07:30 - 16:30', N'Bắc', 107.0811, 20.9101, N'Thành phố Hạ Long, Quảng Ninh', '0203.384.6592', 'info@halongbay.com.vn', 'halongbay.com.vn', N'Quảng Ninh', 1
),
(
    N'Phố cổ Hội An', 'pho-co-hoi-an', 
    N'Hội An hiện lên như một giấc mộng hoài cổ, nơi thời gian dường như lãng quên và ngừng trôi bên ngoài những khung cửa sổ vàng mù tạt đặc trưng. Là bảo tàng sống lưu giữ vẹn nguyên hình bóng thương cảng sầm uất thế kỷ 17, phố cổ quyến rũ bởi những nếp nhà ngói âm dương rêu phong, con hẻm nhỏ tĩnh lặng và giàn hoa giấy rực rỡ buông lơi trước hiên nhà. Khi màn đêm buông xuống, Hội An bừng tỉnh trong nhan sắc huyền ảo của hàng ngàn chiếc đèn lồng lụa lung linh. Dòng sông Hoài thơ mộng trở nên lộng lẫy với những vệt sáng hoa đăng chuyên chở ước nguyện bình an, trôi nhẹ qua Chùa Cầu – viên ngọc kiến trúc giao thoa Việt, Nhật, Hoa. Hội An không chỉ là di sản, mà còn là nơi xoa dịu những tâm hồn mỏi mệt bằng cái tình người đôn hậu và nền ẩm thực tinh tế xứ Quảng. Mảnh đất này đưa ta về với những giá trị bình yên, mộc mạc và nguyên bản nhất của cuộc đời.', 
    1, 120000, N'08:00 - 21:00', N'Trung', 108.3261, 15.8801, N'Hội An, Quảng Nam', '0235.386.1327', 'contact@hoiancenter.vn', 'hoian.vn', N'Quảng Nam', 1
),
(
    N'Đà Lạt', 'da-lat', 
    N'Nằm vắt mình trên cao nguyên Lâm Viên lộng gió, Đà Lạt tựa như nàng thơ đài các quanh năm khoác lớp sương mù bảng lảng và không khí se lạnh mơn man. Được mệnh danh là "Thành phố ngàn hoa", nơi đây là bản hòa ca tuyệt diệu giữa vẻ hoang sơ của đại ngàn Tây Nguyên với nét thanh lịch của kiến trúc Pháp cổ điển. Dọc theo những cung đường đèo uốn lượn chìm trong sương mây, xen giữa đồi thông bạt ngàn là những ngôi biệt thự cũ nằm im lìm như đang ôm ấp câu chuyện tình buồn vang bóng một thời. Đến với Đà Lạt là tìm về khoảng lặng bên mặt hồ Xuân Hương phẳng lặng, là lạc bước vào thung lũng rực rỡ sắc hoa hay chinh phục đỉnh Langbiang hùng vĩ để thu trọn đất trời vào lòng. Mảnh đất này đánh thức xúc cảm bằng tiếng thông reo vi vu và hương vị cà phê đậm đà trong buổi sớm mù sương. Đây là chốn dừng chân của sự lãng mạn, nơi mỗi hơi thở của thiên nhiên đều khiến ta muốn sống chậm lại để tận hưởng trọn vẹn yêu thương.', 
    3, 0, N'Cả ngày', N'Nam', 108.4397, 11.9404, N'Thành phố Đà Lạt, Lâm Đồng', '0263.382.2144', 'dulich@lamdong.gov.vn', 'dalat.vn', N'Lâm Đồng', 1
),
(
    N'Đảo Phú Quốc', 'dao-phu-quoc', 
    N'Đảo Ngọc Phú Quốc vươn mình giữa vịnh Thái Lan như một tuyệt tác của đại dương, nơi nắng vàng, cát trắng và biển xanh cùng hòa quyện tạo nên bản tình ca say đắm lòng người. Tạo hóa ưu ái ban tặng cho nơi đây những bãi biển hoàn mỹ với bờ cát mịn màng ôm trọn làn nước màu ngọc lam soi bóng những hàng dừa nghiêng mình trong gió. Phú Quốc không chỉ có thiên nhiên hoang sơ của rừng nguyên sinh hay hệ sinh thái san hô rực rỡ, mà còn là thủ phủ nghỉ dưỡng xa hoa với những quần thể resort đẳng cấp quốc tế. Đặc biệt, vẻ đẹp đảo ngọc đạt đến đỉnh cao vào chiều tà, khi ánh hoàng hôn nhuộm đỏ tía cả vùng trời biển rực rỡ, tạo nên cảnh sắc tráng lệ và vô cùng lãng mạn. Đứng trước không gian kỳ vĩ ấy, thưởng thức hải sản mặn mòi và tận hưởng dịch vụ thượng lưu, lữ khách sẽ cảm nhận được sự trọn vẹn của một thiên đường nghỉ dưỡng đích thực, nơi mỗi khoảnh khắc trôi qua đều là đặc ân tinh túy từ Mẹ thiên nhiên.', 
    4, 0, N'Cả ngày', N'Nam', 104.0016, 10.2289, N'Phú Quốc, Kiên Giang', '0297.384.6032', 'tourism@phuquoc.vn', 'phuquoc.vn', N'Kiên Giang', 1
),
(
    N'Cố đô Huế', 'co-do-hue', 
    N'Mang âm hưởng của triều đại phong kiến vàng son, Cố đô Huế tĩnh tại, uy nghiêm và sâu lắng, đọng lại trong lòng lữ khách những cảm xúc hoài niệm khó tả. Khác với nhịp sống hối hả, Huế chọn cho mình vẻ đẹp thâm trầm của chốn hoàng cung xưa cũ với những lăng tẩm oai nghiêm và cung điện phủ lớp rêu phong kể chuyện lịch sử trăm năm. Dòng sông Hương dùng dằng trôi tựa dải lụa mềm mại vắt ngang thành phố, ôm lấy núi Ngự Bình tạo nên bức tranh thủy mặc hữu tình đã đi vào thi ca. Huế đẹp không chỉ ở kiến trúc cung đình tinh xảo mà còn quyến rũ bởi chiều sâu văn hóa thấm đẫm trong tiếng chuông chùa Thiên Mụ vang vọng và điệu Nhã nhạc cung đình bác học. Hành trình về với Huế là hành trình tìm về cội nguồn dân tộc, nơi mỗi nhành cây, góc phố đều toát lên cốt cách thanh cao, đài các. Dẫu vật đổi sao dời, Huế vẫn vẹn nguyên nét mộng mơ, một khoảng lặng bình yên để con người soi rọi lại tâm hồn mình giữa dòng đời vạn biến.', 
    1, 200000, N'07:00 - 17:30', N'Trung', 107.5781, 16.4678, N'Thừa Thiên Huế', '0234.352.3237', 'huemonuments@gmail.com', 'hueworldheritage.org.vn', N'Thừa Thiên Huế', 1
),
(
    N'Sapa', 'sapa', 
    N'Sapa hiện lên như miền cổ tích huyền bí ẩn hiện giữa biển mây trắng bồng bềnh của dãy Hoàng Liên Sơn hùng vĩ. Vùng đất này là bức bích họa khổng lồ được khắc tạc bởi thiên nhiên và bàn tay khéo léo của con người qua ngàn đời. Không bút mực nào tả xiết vẻ tráng lệ của thửa ruộng bậc thang mùa lúa chín, tựa như dải lụa vàng rực rỡ vắt ngang lưng chừng trời, vươn tận mây xanh. Sapa còn kiêu hãnh sở hữu đỉnh Fansipan sừng sững – "Nóc nhà Đông Dương", nơi vẫy gọi trái tim khao khát chinh phục mù sương để ôm trọn bầu trời biên cương vào lòng. Linh hồn Sapa nằm ở bản sắc văn hóa đa dạng của các dân tộc vùng cao, trong tiếng khèn gọi bạn và những nếp nhà trình tường tỏa khói lam chiều tĩnh lặng. Sự nguyên sơ, mộc mạc hòa quyện cùng mây gió đỉnh trời chính là chất xúc tác mạnh mẽ làm say lòng khách lãng du, khiến ai đã từng đặt chân đến đều đau đáu một nỗi niềm muốn quay trở lại để tìm về bản ngã tự nhiên.', 
    2, 0, N'Cả ngày', N'Bắc', 103.8438, 22.3364, N'Thị xã Sa Pa, Lào Cai', '0214.387.1975', 'info@sapatourism.com', 'sapa.laocai.gov.vn', N'Lào Cai', 1
),
(
    N'Đà Nẵng', 'da-nang', 
    N'Tựa lưng vào dãy Trường Sơn hùng vĩ và hướng ra Biển Đông bao la, Đà Nẵng là viên ngọc sáng rực rỡ nơi giao hòa tuyệt mỹ giữa nhịp sống hiện đại và thiên nhiên kỳ thú. Không trầm mặc như Huế hay Hội An, Đà Nẵng bừng sáng với những cây cầu độc đáo vắt ngang dòng sông Hàn thơ mộng và Cầu Vàng bồng bềnh trong mây ngàn tại Bà Nà Hills. Bãi biển Mỹ Khê quyến rũ với dải cát trắng mịn và làn nước xanh pha lê từng được vinh danh đẹp nhất hành tinh, cùng bán đảo Sơn Trà rợp bóng rừng nguyên sinh tạo nên "lá phổi xanh" tuyệt mỹ cho thành phố. Đà Nẵng giữ chân lữ khách bởi danh thắng Ngũ Hành Sơn linh thiêng và lòng hiếu khách đôn hậu của người dân miền Trung chất phác. Sự kết hợp hoàn hảo giữa hạ tầng hiện đại và cảnh quan thiên nhiên tráng lệ đã biến nơi đây thành "thành phố đáng sống nhất Việt Nam", điểm dừng chân lý tưởng cho kỳ nghỉ đầy năng lượng nhưng vẫn thư thái tuyệt đối giữa biển trời xanh thẳm.', 
    3, 0, N'Cả ngày', N'Trung', 108.2022, 16.0544, N'Thành phố Đà Nẵng', '0236.355.2700', 'tourism@danang.gov.vn', 'danangfantasticity.com', N'Đà Nẵng', 1
),
(
    N'Tràng An', 'trang-an', 
    N'Ẩn mình giữa vùng đất cố đô Hoa Lư, Tràng An hiện lên như một tuyệt tác "Vịnh Hạ Long trên cạn", nơi thiên nhiên và các giá trị văn hóa tâm linh hòa quyện sâu sắc. Được UNESCO vinh danh là Di sản thế giới kép, Tràng An mê hoặc lòng người bằng hệ thống núi đá vôi muôn hình vạn trạng đứng sừng sững soi bóng xuống làn nước xanh trong vắt tận đáy. Ngồi trên thuyền nan mộc mạc lướt nhẹ trên dòng sông Sào Khê, du khách bắt đầu hành trình ngoạn thủy đầy mê hoặc, xuyên qua những hang động thạch nhũ kỳ bí để mở ra thung lũng bí ẩn, cô lập hoàn toàn với thế giới bên ngoài. Tràng An không chỉ là thắng cảnh mà còn là không gian linh thiêng với những mái đền cổ kính nép mình bên vách đá, kể lại trang sử oai hùng của dân tộc. Lắng nghe tiếng mái chèo khua nước giữa không gian tĩnh mịch, hít thở bầu không khí tinh khiết, lòng người bỗng trở nên nhẹ tênh, rũ bỏ mọi ưu phiền để đắm mình vào vẻ đẹp thoát tục của vùng đất địa linh nhân kiệt này.', 
    1, 250000, N'07:00 - 16:00', N'Bắc', 105.8941, 20.2522, N'Ninh Bình', '0229.362.0335', 'info@trangan.vn', 'trangan.org', N'Ninh Bình', 1
);

-- 3. CHÈN BẢNG GIÁ VÉ CHI TIẾT
INSERT INTO GiaVeChiTiet (MaDiaDiem, LoaiKhach, Gia) VALUES 
(1, N'Người lớn', 290000), (1, N'Trẻ em (1m-1m4)', 145000), (1, N'Trẻ em (<1m)', 0),
(2, N'Khách quốc tế', 150000), (2, N'Khách Việt Nam', 80000),
(5, N'Người lớn', 200000), (5, N'Trẻ em', 40000),
(8, N'Người lớn', 250000), (8, N'Trẻ em', 120000);

-- 4. CHÈN TRẢI NGHIỆM ĐỊA PHƯƠNG
INSERT INTO TraiNghiem (MaDiaDiem, LoaiTraiNghiem, TieuDe, MoTa, Icon) VALUES 
(1, 'eat', N'Hải sản trên tàu', N'Thưởng thức mực nhảy, bề bề nướng ngay giữa lòng di sản.', 'bi-cup-hot'),
(1, 'play', N'Chèo Kayak', N'Tự tay chèo thuyền len lỏi qua các hang động đá vôi.', 'bi-water'),
(1, 'checkin', N'Đỉnh Titop', N'Ngắm toàn cảnh vịnh từ trên cao, điểm sống ảo không thể bỏ qua.', 'bi-camera'),
(2, 'eat', N'Cao lầu Hội An', N'Món ăn đặc sản mang linh hồn của phố cổ.', 'bi-egg-fried'),
(3, 'checkin', N'Săn mây Đà Lạt', N'Trải nghiệm đón bình minh trên thảm gỗ cầu đất.', 'bi-cloud-sun');

-- 5. CHÈN ẢNH ĐỊA ĐIỂM (Ảnh chính và Thư viện ảnh phụ của các địa điểm du lịch)
INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
-- 1. Vịnh Hạ Long
(1, '/Content/images/Places/vinhhalong.jpg', 1),
(1, '/Content/images/Places/vinhhalong1.jpg', 0),
(1, '/Content/images/Places/vinhhalong2.jpg', 0),
(1, '/Content/images/Places/vinhhalong3.jpg', 0),
(1, '/Content/images/Places/vinhhalong4.jpg', 0),
(1, '/Content/images/Places/vinhhalong5.jpg', 0),
(1, '/Content/images/Places/vinhhalong6.jpg', 0),
(1, '/Content/images/Places/vinhhalong7.jpg', 0),
(1, '/Content/images/Places/vinhhalong8.jpg', 0),
(1, '/Content/images/Places/vinhhalong9.jpg', 0),
(1, '/Content/images/Places/vinhhalong10.jpg', 0),
(1, '/Content/images/Places/vinhhalong11.jpg', 0),

-- 2. Phố cổ Hội An
(2, '/Content/images/Places/hoian.jpg', 1),
(2, '/Content/images/Places/hoian1.jpg', 0),
(2, '/Content/images/Places/hoian2.jpg', 0),
(2, '/Content/images/Places/hoian3.jpg', 0),
(2, '/Content/images/Places/hoian4.jpg', 0),
(2, '/Content/images/Places/hoian5.jpg', 0),
(2, '/Content/images/Places/hoian6.jpg', 0),
(2, '/Content/images/Places/hoian7.jpg', 0),

-- 3. Đà Lạt
(3, '/Content/images/Places/dalat.jpg', 1),
(3, '/Content/images/Places/dalat1.jpg', 0),
(3, '/Content/images/Places/dalat2.jpg', 0),
(3, '/Content/images/Places/dalat3.jpg', 0),
(3, '/Content/images/Places/dalat4.jpg', 0),
(3, '/Content/images/Places/dalat5.jpg', 0),
(3, '/Content/images/Places/dalat6.jpg', 0),
(3, '/Content/images/Places/dalat7.jpg', 0),

-- 4. Đảo Phú Quốc
(4, '/Content/images/Places/phuquoc2.jpg', 1),
(4, '/Content/images/Places/phuquoc1.jpg', 0),
(4, '/Content/images/Places/phuquoc3.jpg', 0),
(4, '/Content/images/Places/phuquoc4.jpg', 0),
(4, '/Content/images/Places/phuquoc5.jpg', 0),
(4, '/Content/images/Places/phuquoc6.jpg', 0),
(4, '/Content/images/Places/phuquoc7.jpg', 0),
(4, '/Content/images/Places/phuquoc8.jpg', 0),
(4, '/Content/images/Places/phuquoc9.jpg', 0),
(4, '/Content/images/Places/phuquoc10.jpg', 0),
(4, '/Content/images/Places/phuquoc11.jpg', 0),
(4, '/Content/images/Places/phuquoc12.jpg', 0),

-- 5. Cố đô Huế
(5, '/Content/images/Places/hue.jpg', 1),
(5, '/Content/images/Places/hue1.jpg', 0),
(5, '/Content/images/Places/hue2.jpg', 0),
(5, '/Content/images/Places/hue3.jpg', 0),
(5, '/Content/images/Places/hue4.jpg', 0),
(5, '/Content/images/Places/hue5.jpg', 0),
(5, '/Content/images/Places/hue6.jpg', 0),
(5, '/Content/images/Places/hue7.jpg', 0),

-- 6. Sapa
(6, '/Content/images/Places/sapa.jpg', 1),
(6, '/Content/images/Places/sapa1.jpg', 0),
(6, '/Content/images/Places/sapa2.jpg', 0),
(6, '/Content/images/Places/sapa3.jpg', 0),
(6, '/Content/images/Places/sapa4.jpg', 0),
(6, '/Content/images/Places/sapa5.jpg', 0),
(6, '/Content/images/Places/sapa6.jpg', 0),
(6, '/Content/images/Places/sapa7.jpg', 0),
(6, '/Content/images/Places/sapa8.jpg', 0),
(6, '/Content/images/Places/sapa9.jpg', 0),
(6, '/Content/images/Places/sapa10.jpg', 0),
(6, '/Content/images/Places/sapa11.jpg', 0),

-- 7. Đà Nẵng
(7, '/Content/images/Places/danang.jpg', 1),
(7, '/Content/images/Places/danang1.jpg', 0),
(7, '/Content/images/Places/danang2.jpg', 0),
(7, '/Content/images/Places/danang3.jpg', 0),
(7, '/Content/images/Places/danang4.jpg', 0),
(7, '/Content/images/Places/danang5.jpg', 0),
(7, '/Content/images/Places/danang6.jpg', 0),
(7, '/Content/images/Places/danang7.jpg', 0),
(7, '/Content/images/Places/danang8.jpg', 0),
(7, '/Content/images/Places/danang9.jpg', 0),
(7, '/Content/images/Places/danang10.jpg', 0),
(7, '/Content/images/Places/danang11.jpg', 0),

-- 8. Tràng An
(8, '/Content/images/Places/trangan.jpg', 1),
(8, '/Content/images/Places/trangan1.jpg', 0),
(8, '/Content/images/Places/trangan2.jpg', 0),
(8, '/Content/images/Places/trangan3.jpg', 0),
(8, '/Content/images/Places/trangan4.jpg', 0),
(8, '/Content/images/Places/trangan5.jpg', 0),
(8, '/Content/images/Places/trangan6.jpg', 0),
(8, '/Content/images/Places/trangan7.jpg', 0);
