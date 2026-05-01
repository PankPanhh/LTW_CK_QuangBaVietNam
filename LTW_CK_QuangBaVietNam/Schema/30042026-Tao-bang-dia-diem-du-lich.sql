-- 1. Bảng Danh Mục
CREATE TABLE DanhMuc (
    MaDanhMuc INT PRIMARY KEY IDENTITY(1,1), -- Tự động tăng
    TenDanhMuc NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(MAX)
);

-- 2. Bảng Địa Điểm
CREATE TABLE DiaDiem (
    MaDiaDiem INT PRIMARY KEY IDENTITY(1,1), -- Tự động tăng
    TenDiaDiem NVARCHAR(255) NOT NULL,
    Slug VARCHAR(255) UNIQUE NOT NULL,
    MoTaNgan NVARCHAR(500),
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
    FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc)
);

-- 3. Bảng Ảnh Địa Điểm
CREATE TABLE AnhDiaDiem (
    MaAnh INT PRIMARY KEY IDENTITY(1,1), -- Tự động tăng
    MaDiaDiem INT NOT NULL,
    DuongDanAnh NVARCHAR(MAX) NOT NULL,
    LaAnhChinh BIT DEFAULT 0,
    FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem)
);

-- 4. Bảng Giá Vé Chi Tiết
CREATE TABLE GiaVeChiTiet (
    MaGiaVe INT PRIMARY KEY IDENTITY(1,1), -- Tự động tăng
    MaDiaDiem INT NOT NULL,
    LoaiKhach NVARCHAR(50) NOT NULL, 
    Gia DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem)
);

-- 5. Bảng Trải Nghiệm
CREATE TABLE TraiNghiem (
    MaTraiNghiem INT PRIMARY KEY IDENTITY(1,1), -- Tự động tăng
    MaDiaDiem INT NOT NULL,
    LoaiTraiNghiem NVARCHAR(50) NOT NULL, -- eat, play, checkin
    TieuDe NVARCHAR(255),
    MoTa NVARCHAR(MAX),
    Icon VARCHAR(100),
    FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem)
);

-- 6. Data test 
-- =============================================
-- SEED DATA CHO HỆ THỐNG QUẢNG BÁ DU LỊCH
-- =============================================

-- 1. CHÈN DANH MỤC
INSERT INTO DanhMuc (TenDanhMuc, MoTa) VALUES 
(N'Di sản thế giới', N'Các địa danh được UNESCO công nhận là di sản thiên nhiên hoặc văn hóa thế giới.'),
(N'Văn hóa - Lịch sử', N'Các điểm đến mang đậm giá trị truyền thống, lịch sử dân tộc.'),
(N'Thành phố nghỉ dưỡng', N'Các đô thị du lịch hiện đại, không khí trong lành, dịch vụ cao cấp.'),
(N'Biển đảo', N'Khám phá vẻ đẹp của đại dương và các hòn đảo hoang sơ.');

-- 2. CHÈN ĐỊA ĐIỂM (Lấy 8 điểm tiêu biểu)
-- Lưu ý: Identity tự nhảy 1-8
INSERT INTO DiaDiem (TenDiaDiem, Slug, MoTaNgan, MaDanhMuc, GiaVe, GioMoCua, VungMien, KinhDo, ViDo, DiaChiChiTiet, SoDienThoai, Email, Website) VALUES 
(N'Vịnh Hạ Long', 'vinh-ha-long', N'Di sản thiên nhiên thế giới với hàng nghìn đảo đá vôi kỳ vĩ.', 1, 290000, N'07:30 - 16:30', N'Bắc', 107.0811, 20.9101, N'Thành phố Hạ Long, Quảng Ninh', '0203.384.6592', 'info@halongbay.com.vn', 'halongbay.com.vn'),
(N'Phố cổ Hội An', 'pho-co-hoi-an', N'Thành phố cổ kính bên dòng sông Hoài với những ánh đèn lồng rực rỡ.', 1, 120000, N'08:00 - 21:00', N'Trung', 108.3261, 15.8801, N'Hội An, Quảng Nam', '0235.386.1327', 'contact@hoiancenter.vn', 'hoian.vn'),
(N'Đà Lạt', 'da-lat', N'Thành phố ngàn hoa với khí hậu ôn hòa và những đồi thông lãng mạn.', 3, 0, N'Cả ngày', N'Nam', 108.4397, 11.9404, N'Thành phố Đà Lạt, Lâm Đồng', '0263.382.2144', 'dulich@lamdong.gov.vn', 'dalat.vn'),
(N'Đảo Phú Quốc', 'dao-phu-quoc', N'Đảo Ngọc với bãi cát trắng mịn và các khu nghỉ dưỡng đẳng cấp.', 4, 0, N'Cả ngày', N'Nam', 104.0016, 10.2289, N'Phú Quốc, Kiên Giang', '0297.384.6032', 'tourism@phuquoc.vn', 'phuquoc.vn'),
(N'Cố đô Huế', 'co-do-hue', N'Quần thể di tích triều Nguyễn với vẻ đẹp trầm mặc và uy nghiêm.', 1, 200000, N'07:00 - 17:30', N'Trung', 107.5781, 16.4678, N'Thừa Thiên Huế', '0234.352.3237', 'huemonuments@gmail.com', 'hueworldheritage.org.vn'),
(N'Sapa', 'sapa', N'Thị trấn trong sương với những thửa ruộng bậc thang kỳ vĩ.', 2, 0, N'Cả ngày', N'Bắc', 103.8438, 22.3364, N'Thị xã Sa Pa, Lào Cai', '0214.387.1975', 'info@sapatourism.com', 'sapa.laocai.gov.vn'),
(N'Đà Nẵng', 'da-nang', N'Thành phố của những cây cầu và bãi biển Mỹ Khê xinh đẹp.', 3, 0, N'Cả ngày', N'Trung', 108.2022, 16.0544, N'Thành phố Đà Nẵng', '0236.355.2700', 'tourism@danang.gov.vn', 'danangfantasticity.com'),
(N'Tràng An', 'trang-an', N'Quần thể danh thắng được ví như Vịnh Hạ Long trên cạn.', 1, 250000, N'07:00 - 16:00', N'Bắc', 105.8941, 20.2522, N'Ninh Bình', '0229.362.0335', 'info@trangan.vn', 'trangan.org');

-- 3. CHÈN GIÁ VÉ CHI TIẾT (Ví dụ cho Vịnh Hạ Long - MaDiaDiem = 1)
INSERT INTO GiaVeChiTiet (MaDiaDiem, LoaiKhach, Gia) VALUES 
(1, N'Người lớn', 290000), (1, N'Trẻ em (1m-1m4)', 145000), (1, N'Trẻ em (<1m)', 0),
(2, N'Khách quốc tế', 150000), (2, N'Khách Việt Nam', 80000),
(5, N'Người lớn', 200000), (5, N'Trẻ em', 40000),
(8, N'Người lớn', 250000), (8, N'Trẻ em', 120000);

-- 4. CHÈN TRẢI NGHIỆM (Ví dụ cho Vịnh Hạ Long - MaDiaDiem = 1)
INSERT INTO TraiNghiem (MaDiaDiem, LoaiTraiNghiem, TieuDe, MoTa, Icon) VALUES 
(1, 'eat', N'Hải sản trên tàu', N'Thưởng thức mực nhảy, bề bề nướng ngay giữa lòng di sản.', 'bi-cup-hot'),
(1, 'play', N'Chèo Kayak', N'Tự tay chèo thuyền len lỏi qua các hang động đá vôi.', 'bi-water'),
(1, 'checkin', N'Đỉnh Titop', N'Ngắm toàn cảnh vịnh từ trên cao, điểm sống ảo không thể bỏ qua.', 'bi-camera'),
(2, 'eat', N'Cao lầu Hội An', N'Món ăn đặc sản mang linh hồn của phố cổ.', 'bi-egg-fried'),
(3, 'checkin', N'Săn mây Đà Lạt', N'Trải nghiệm đón bình minh trên thảm gỗ cầu đất.', 'bi-cloud-sun');

-- 5. CHÈN ẢNH ĐỊA ĐIỂM (Mỗi nơi 1 ảnh chính)
INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
-- 1. Vịnh Hạ Long
(1, '/Content/images/Places/vinhhalong.jpg', 1),

-- 2. Phố cổ Hội An
(2, '/Content/images/Places/hoian.jpg', 1),

-- 3. Đà Lạt
(3, '/Content/images/Places/dalat.jpg', 1),

-- 4. Đảo Phú Quốc
(4, '/Content/images/Places/phuquoc.jpg', 1),

-- 5. Cố đô Huế
(5, '/Content/images/Places/hue.jpg', 1),

-- 6. Sapa
(6, '/Content/images/Places/sapa.jpg', 1),

-- 7. Đà Nẵng
(7, '/Content/images/Places/danang.jpg', 1),

-- 8. Tràng An
(8, '/Content/images/Places/trangan.jpg', 1);

--Update lại mô tả 
-- 1. Vịnh Hạ Long
UPDATE DiaDiem 
SET MoTaNgan = N'Vịnh Hạ Long hiện lên như một kiệt tác thủy mặc khổng lồ được tạo hóa tạc bằng đá và nước giữa biển khơi Đông Bắc. Nơi đây mê hoặc lữ khách bởi hàng ngàn đảo đá vôi kỳ vĩ soi bóng xuống làn nước xanh màu ngọc bích, gợi nhắc huyền thoại rồng mẹ hạ phàm phun châu nhả ngọc bảo vệ bờ cõi. Đi sâu vào lòng di sản, những hang động như Sửng Sốt hay Thiên Cung mở ra thế giới siêu thực với thạch nhũ lấp lánh như lâu đài pha lê, nơi thời gian dường như ngưng đọng triệu năm. Trải nghiệm Hạ Long tuyệt vời nhất là khi thả mình trên du thuyền sang trọng, đón làn gió mặn mòi và chiêm ngưỡng hoàng hôn nhuộm đỏ rực rỡ cả vùng trời biển bao la. Đây chính là bản trường ca của đá và nước, một di sản thiên nhiên thế giới mà bất kỳ ai cũng khao khát được chạm tay vào một lần trong đời để cảm nhận trọn vẹn vẻ đẹp tráng lệ, kiêu hãnh của non sông Việt Nam.'
WHERE Slug = 'vinh-ha-long';

-- 2. Phố cổ Hội An
UPDATE DiaDiem 
SET MoTaNgan = N'Hội An hiện lên như một giấc mộng hoài cổ, nơi thời gian dường như lãng quên và ngừng trôi bên ngoài những khung cửa sổ vàng mù tạt đặc trưng. Là bảo tàng sống lưu giữ vẹn nguyên hình bóng thương cảng sầm uất thế kỷ 17, phố cổ quyến rũ bởi những nếp nhà ngói âm dương rêu phong, con hẻm nhỏ tĩnh lặng và giàn hoa giấy rực rỡ buông lơi trước hiên nhà. Khi màn đêm buông xuống, Hội An bừng tỉnh trong nhan sắc huyền ảo của hàng ngàn chiếc đèn lồng lụa lung linh. Dòng sông Hoài thơ mộng trở nên lộng lẫy với những vệt sáng hoa đăng chuyên chở ước nguyện bình an, trôi nhẹ qua Chùa Cầu – viên ngọc kiến trúc giao thoa Việt, Nhật, Hoa. Hội An không chỉ là di sản, mà còn là nơi xoa dịu những tâm hồn mỏi mệt bằng cái tình người đôn hậu và nền ẩm thực tinh tế xứ Quảng. Mảnh đất này đưa ta về với những giá trị bình yên, mộc mạc và nguyên bản nhất của cuộc đời.'
WHERE Slug = 'pho-co-hoi-an';

-- 3. Đà Lạt
UPDATE DiaDiem 
SET MoTaNgan = N'Nằm vắt mình trên cao nguyên Lâm Viên lộng gió, Đà Lạt tựa như nàng thơ đài các quanh năm khoác lớp sương mù bảng lảng và không khí se lạnh mơn man. Được mệnh danh là "Thành phố ngàn hoa", nơi đây là bản hòa ca tuyệt diệu giữa vẻ hoang sơ của đại ngàn Tây Nguyên với nét thanh lịch của kiến trúc Pháp cổ điển. Dọc theo những cung đường đèo uốn lượn chìm trong sương mây, xen giữa đồi thông bạt ngàn là những ngôi biệt thự cũ nằm im lìm như đang ôm ấp câu chuyện tình buồn vang bóng một thời. Đến với Đà Lạt là tìm về khoảng lặng bên mặt hồ Xuân Hương phẳng lặng, là lạc bước vào thung lũng rực rỡ sắc hoa hay chinh phục đỉnh Langbiang hùng vĩ để thu trọn đất trời vào lòng. Mảnh đất này đánh thức xúc cảm bằng tiếng thông reo vi vu và hương vị cà phê đậm đà trong buổi sớm mù sương. Đây là chốn dừng chân của sự lãng mạn, nơi mỗi hơi thở của thiên nhiên đều khiến ta muốn sống chậm lại để tận hưởng trọn vẹn yêu thương.'
WHERE Slug = 'da-lat';

-- 4. Đảo Phú Quốc
UPDATE DiaDiem 
SET MoTaNgan = N'Đảo Ngọc Phú Quốc vươn mình giữa vịnh Thái Lan như một tuyệt tác của đại dương, nơi nắng vàng, cát trắng và biển xanh cùng hòa quyện tạo nên bản tình ca say đắm lòng người. Tạo hóa ưu ái ban tặng cho nơi đây những bãi biển hoàn mỹ với bờ cát mịn màng ôm trọn làn nước màu ngọc lam soi bóng những hàng dừa nghiêng mình trong gió. Phú Quốc không chỉ có thiên nhiên hoang sơ của rừng nguyên sinh hay hệ sinh thái san hô rực rỡ, mà còn là thủ phủ nghỉ dưỡng xa hoa với những quần thể resort đẳng cấp quốc tế. Đặc biệt, vẻ đẹp đảo ngọc đạt đến đỉnh cao vào chiều tà, khi ánh hoàng hôn nhuộm đỏ tía cả vùng trời biển rực rỡ, tạo nên cảnh sắc tráng lệ và vô cùng lãng mạn. Đứng trước không gian kỳ vĩ ấy, thưởng thức hải sản mặn mòi và tận hưởng dịch vụ thượng lưu, lữ khách sẽ cảm nhận được sự trọn vẹn của một thiên đường nghỉ dưỡng đích thực, nơi mỗi khoảnh khắc trôi qua đều là đặc ân tinh túy từ Mẹ thiên nhiên.'
WHERE Slug = 'dao-phu-quoc';

-- 5. Cố đô Huế
UPDATE DiaDiem 
SET MoTaNgan = N'Mang âm hưởng của triều đại phong kiến vàng son, Cố đô Huế tĩnh tại, uy nghiêm và sâu lắng, đọng lại trong lòng lữ khách những cảm xúc hoài niệm khó tả. Khác với nhịp sống hối hả, Huế chọn cho mình vẻ đẹp thâm trầm của chốn hoàng cung xưa cũ với những lăng tẩm oai nghiêm và cung điện phủ lớp rêu phong kể chuyện lịch sử trăm năm. Dòng sông Hương dùng dằng trôi tựa dải lụa mềm mại vắt ngang thành phố, ôm lấy núi Ngự Bình tạo nên bức tranh thủy mặc hữu tình đã đi vào thi ca. Huế đẹp không chỉ ở kiến trúc cung đình tinh xảo mà còn quyến rũ bởi chiều sâu văn hóa thấm đẫm trong tiếng chuông chùa Thiên Mụ vang vọng và điệu Nhã nhạc cung đình bác học. Hành trình về với Huế là hành trình tìm về cội nguồn dân tộc, nơi mỗi nhành cây, góc phố đều toát lên cốt cách thanh cao, đài các. Dẫu vật đổi sao dời, Huế vẫn vẹn nguyên nét mộng mơ, một khoảng lặng bình yên để con người soi rọi lại tâm hồn mình giữa dòng đời vạn biến.'
WHERE Slug = 'co-do-hue';

-- 6. Sapa
UPDATE DiaDiem 
SET MoTaNgan = N'Sapa hiện lên như miền cổ tích huyền bí ẩn hiện giữa biển mây trắng bồng bềnh của dãy Hoàng Liên Sơn hùng vĩ. Vùng đất này là bức bích họa khổng lồ được khắc tạc bởi thiên nhiên và bàn tay khéo léo của con người qua ngàn đời. Không bút mực nào tả xiết vẻ tráng lệ của thửa ruộng bậc thang mùa lúa chín, tựa như dải lụa vàng rực rỡ vắt ngang lưng chừng trời, vươn tận mây xanh. Sapa còn kiêu hãnh sở hữu đỉnh Fansipan sừng sững – "Nóc nhà Đông Dương", nơi vẫy gọi trái tim khao khát chinh phục mù sương để ôm trọn bầu trời biên cương vào lòng. Linh hồn Sapa nằm ở bản sắc văn hóa đa dạng của các dân tộc vùng cao, trong tiếng khèn gọi bạn và những nếp nhà trình tường tỏa khói lam chiều tĩnh lặng. Sự nguyên sơ, mộc mạc hòa quyện cùng mây gió đỉnh trời chính là chất xúc tác mạnh mẽ làm say lòng khách lãng du, khiến ai đã từng đặt chân đến đều đau đáu một nỗi niềm muốn quay trở lại để tìm về bản ngã tự nhiên.'
WHERE Slug = 'sapa';

-- 7. Đà Nẵng
UPDATE DiaDiem 
SET MoTaNgan = N'Tựa lưng vào dãy Trường Sơn hùng vĩ và hướng ra Biển Đông bao la, Đà Nẵng là viên ngọc sáng rực rỡ nơi giao hòa tuyệt mỹ giữa nhịp sống hiện đại và thiên nhiên kỳ thú. Không trầm mặc như Huế hay Hội An, Đà Nẵng bừng sáng với những cây cầu độc đáo vắt ngang dòng sông Hàn thơ mộng và Cầu Vàng bồng bềnh trong mây ngàn tại Bà Nà Hills. Bãi biển Mỹ Khê quyến rũ với dải cát trắng mịn và làn nước xanh pha lê từng được vinh danh đẹp nhất hành tinh, cùng bán đảo Sơn Trà rợp bóng rừng nguyên sinh tạo nên "lá phổi xanh" tuyệt mỹ cho thành phố. Đà Nẵng giữ chân lữ khách bởi danh thắng Ngũ Hành Sơn linh thiêng và lòng hiếu khách đôn hậu của người dân miền Trung chất phác. Sự kết hợp hoàn hảo giữa hạ tầng hiện đại và cảnh quan thiên nhiên tráng lệ đã biến nơi đây thành "thành phố đáng sống nhất Việt Nam", điểm dừng chân lý tưởng cho kỳ nghỉ đầy năng lượng nhưng vẫn thư thái tuyệt đối giữa biển trời xanh thẳm.'
WHERE Slug = 'da-nang';

-- 8. Tràng An
UPDATE DiaDiem 
SET MoTaNgan = N'Ẩn mình giữa vùng đất cố đô Hoa Lư, Tràng An hiện lên như một tuyệt tác "Vịnh Hạ Long trên cạn", nơi thiên nhiên và các giá trị văn hóa tâm linh hòa quyện sâu sắc. Được UNESCO vinh danh là Di sản thế giới kép, Tràng An mê hoặc lòng người bằng hệ thống núi đá vôi muôn hình vạn trạng đứng sừng sững soi bóng xuống làn nước xanh trong vắt tận đáy. Ngồi trên thuyền nan mộc mạc lướt nhẹ trên dòng sông Sào Khê, du khách bắt đầu hành trình ngoạn thủy đầy mê hoặc, xuyên qua những hang động thạch nhũ kỳ bí để mở ra thung lũng bí ẩn, cô lập hoàn toàn với thế giới bên ngoài. Tràng An không chỉ là thắng cảnh mà còn là không gian linh thiêng với những mái đền cổ kính nép mình bên vách đá, kể lại trang sử oai hùng của dân tộc. Lắng nghe tiếng mái chèo khua nước giữa không gian tĩnh mịch, hít thở bầu không khí tinh khiết, lòng người bỗng trở nên nhẹ tênh, rũ bỏ mọi ưu phiền để đắm mình vào vẻ đẹp thoát tục của vùng đất địa linh nhân kiệt này.'
WHERE Slug = 'trang-an';


-- =============================================
-- 1. VỊNH HẠ LONG (MaDiaDiem = 1)
-- =============================================
DELETE FROM AnhDiaDiem WHERE MaDiaDiem = 1;
INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
(1, '/Content/images/Places/vinhhalong.jpg', 1), -- Ảnh chính
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
(1, '/Content/images/Places/vinhhalong11.jpg', 0);


-- =============================================
-- 2. PHỐ CỔ HỘI AN (MaDiaDiem = 2)
-- =============================================
DELETE FROM AnhDiaDiem WHERE MaDiaDiem = 2;

INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
(2, '/Content/images/Places/hoian.jpg', 1),  -- Ảnh chính (Hiển thị ở Card AllPlaces)
(2, '/Content/images/Places/hoian1.jpg', 0), -- Các ảnh phụ cho Gallery trang Detail
(2, '/Content/images/Places/hoian2.jpg', 0),
(2, '/Content/images/Places/hoian3.jpg', 0),
(2, '/Content/images/Places/hoian4.jpg', 0),
(2, '/Content/images/Places/hoian5.jpg', 0),
(2, '/Content/images/Places/hoian6.jpg', 0),
(2, '/Content/images/Places/hoian7.jpg', 0);

-- =============================================
-- 3. ĐÀ LẠT (MaDiaDiem = 3)
-- =============================================
DELETE FROM AnhDiaDiem WHERE MaDiaDiem = 3;

INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
(3, '/Content/images/Places/dalat.jpg', 1),  -- Ảnh chính (tấm ảnh dọc đẹp nhất)
(3, '/Content/images/Places/dalat1.jpg', 0),
(3, '/Content/images/Places/dalat2.jpg', 0),
(3, '/Content/images/Places/dalat3.jpg', 0),
(3, '/Content/images/Places/dalat4.jpg', 0),
(3, '/Content/images/Places/dalat5.jpg', 0),
(3, '/Content/images/Places/dalat6.jpg', 0),
(3, '/Content/images/Places/dalat7.jpg', 0);


-- =============================================
-- 4. ĐẢO PHÚ QUỐC (MaDiaDiem = 4)
-- =============================================
DELETE FROM AnhDiaDiem WHERE MaDiaDiem = 4;
INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
(4, '/Content/images/Places/phuquoc1.jpg', 0),
(4, '/Content/images/Places/phuquoc2.jpg', 1),
(4, '/Content/images/Places/phuquoc3.jpg', 0),
(4, '/Content/images/Places/phuquoc4.jpg', 0),
(4, '/Content/images/Places/phuquoc5.jpg', 0),
(4, '/Content/images/Places/phuquoc6.jpg', 0),
(4, '/Content/images/Places/phuquoc7.jpg', 0),
(4, '/Content/images/Places/phuquoc8.jpg', 0),
(4, '/Content/images/Places/phuquoc9.jpg', 0),
(4, '/Content/images/Places/phuquoc10.jpg', 0),
(4, '/Content/images/Places/phuquoc11.jpg', 0),
(4, '/Content/images/Places/phuquoc12.jpg', 0);

-- =============================================
-- 5. CỐ ĐÔ HUẾ (MaDiaDiem = 5)
-- =============================================
DELETE FROM AnhDiaDiem WHERE MaDiaDiem = 5;
INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
(5, '/Content/images/Places/hue.jpg', 1), -- Ảnh chính
(5, '/Content/images/Places/hue1.jpg', 0),
(5, '/Content/images/Places/hue2.jpg', 0),
(5, '/Content/images/Places/hue3.jpg', 0),
(5, '/Content/images/Places/hue4.jpg', 0),
(5, '/Content/images/Places/hue5.jpg', 0),
(5, '/Content/images/Places/hue6.jpg', 0),
(5, '/Content/images/Places/hue7.jpg', 0);

-- =============================================
-- 6. SAPA (MaDiaDiem = 6)
-- =============================================
DELETE FROM AnhDiaDiem WHERE MaDiaDiem = 6;
INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
(6, '/Content/images/Places/sapa.jpg', 1), -- Ảnh chính
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
(6, '/Content/images/Places/sapa11.jpg', 0);

-- =============================================
-- 7. ĐÀ NẴNG (MaDiaDiem = 7)
-- =============================================
DELETE FROM AnhDiaDiem WHERE MaDiaDiem = 7;
INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
(7, '/Content/images/Places/danang.jpg', 1), -- Ảnh chính
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
(7, '/Content/images/Places/danang11.jpg', 0);

-- =============================================
-- 8. TRÀNG AN (MaDiaDiem = 8)
-- =============================================
DELETE FROM AnhDiaDiem WHERE MaDiaDiem = 8;
INSERT INTO AnhDiaDiem (MaDiaDiem, DuongDanAnh, LaAnhChinh) VALUES 
(8, '/Content/images/Places/trangan.jpg', 1), -- Ảnh chính
(8, '/Content/images/Places/trangan1.jpg', 0),
(8, '/Content/images/Places/trangan2.jpg', 0),
(8, '/Content/images/Places/trangan3.jpg', 0),
(8, '/Content/images/Places/trangan4.jpg', 0),
(8, '/Content/images/Places/trangan5.jpg', 0),
(8, '/Content/images/Places/trangan6.jpg', 0),
(8, '/Content/images/Places/trangan7.jpg', 0);

-- =============================================
-- KIỂM TRA LẠI KẾT QUẢ
-- =============================================
SELECT * FROM AnhDiaDiem ORDER BY MaDiaDiem, LaAnhChinh DESC;