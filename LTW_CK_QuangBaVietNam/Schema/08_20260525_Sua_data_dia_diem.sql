select * from DiaDiem


-- Chuyển dữ liệu từ MoTaNgan sang MoTaChiTiet
UPDATE DiaDiem
SET MoTaChiTiet = MoTaNgan
WHERE (MoTaChiTiet IS NULL OR LTRIM(RTRIM(MoTaChiTiet)) = '');

-- Xóa dữ liệu trong MoTaNgan
UPDATE DiaDiem
SET MoTaNgan = NULL;


-- =============================================
-- THÊM MÔ TẢ NGẮN CHO CÁC ĐỊA ĐIỂM
-- =============================================

UPDATE DiaDiem
SET MoTaNgan = N'Di sản thiên nhiên thế giới nổi tiếng với hàng nghìn đảo đá vôi kỳ vĩ giữa làn nước xanh ngọc bích.'
WHERE Slug = 'vinh-ha-long';

UPDATE DiaDiem
SET MoTaNgan = N'Phố cổ lung linh đèn lồng, lưu giữ vẻ đẹp hoài cổ và văn hóa giao thoa hàng trăm năm.'
WHERE Slug = 'pho-co-hoi-an';

UPDATE DiaDiem
SET MoTaNgan = N'Thành phố ngàn hoa với khí hậu se lạnh, đồi thông lãng mạn và cảnh sắc thơ mộng quanh năm.'
WHERE Slug = 'da-lat';

UPDATE DiaDiem
SET MoTaNgan = N'Đảo ngọc thiên đường nghỉ dưỡng với biển xanh, cát trắng và những khu resort đẳng cấp.'
WHERE Slug = 'dao-phu-quoc';

UPDATE DiaDiem
SET MoTaNgan = N'Cố đô mang vẻ đẹp trầm mặc với quần thể di tích triều Nguyễn và dòng sông Hương thơ mộng.'
WHERE Slug = 'co-do-hue';

UPDATE DiaDiem
SET MoTaNgan = N'Thị trấn vùng cao nổi tiếng với ruộng bậc thang tuyệt đẹp và đỉnh Fansipan hùng vĩ.'
WHERE Slug = 'sapa';

UPDATE DiaDiem
SET MoTaNgan = N'Thành phố biển hiện đại với những cây cầu biểu tượng và bãi biển Mỹ Khê nổi tiếng.'
WHERE Slug = 'da-nang';

UPDATE DiaDiem
SET MoTaNgan = N'Danh thắng non nước hữu tình được ví như Vịnh Hạ Long trên cạn giữa lòng Ninh Bình.'
WHERE Slug = 'trang-an';

-- =============================================
-- THÊM MÔ TẢ NGẮN CHI TIẾT CHO CÁC ĐỊA ĐIỂM MỚI
-- =============================================

UPDATE DiaDiem
SET MoTaNgan = N'Thung lũng lãng mạn nổi tiếng của Đà Lạt với hồ nước thơ mộng, đồi hoa rực rỡ, cầu kính hiện đại và không gian mang đậm sắc màu tình yêu giữa núi rừng cao nguyên.'
WHERE Slug = 'thung-lung-tinh-yeu';

UPDATE DiaDiem
SET MoTaNgan = N'Đỉnh núi huyền thoại của Tây Nguyên với khung cảnh hùng vĩ, khí hậu mát lạnh quanh năm và trải nghiệm trekking ngắm toàn cảnh Đà Lạt từ trên cao.'
WHERE Slug = 'nui-langbiang';

UPDATE DiaDiem
SET MoTaNgan = N'Thác nước nổi tiếng giữa rừng thông Đà Lạt, hấp dẫn du khách bởi vẻ đẹp hoang sơ kết hợp hệ thống máng trượt xuyên rừng đầy cảm giác mạnh.'
WHERE Slug = 'thac-datanla';

UPDATE DiaDiem
SET MoTaNgan = N'Hồ nước lớn thơ mộng được bao quanh bởi rừng thông xanh mát cùng Thiền viện Trúc Lâm thanh tịnh, tạo nên không gian nghỉ dưỡng yên bình giữa thiên nhiên.'
WHERE Slug = 'ho-tuyen-lam';

UPDATE DiaDiem
SET MoTaNgan = N'Công trình kiến trúc kỳ dị nổi tiếng thế giới với thiết kế như mê cung cổ tích, thu hút du khách bởi phong cách siêu thực độc đáo hiếm có.'
WHERE Slug = 'crazy-house';

UPDATE DiaDiem
SET MoTaNgan = N'Khu vườn hoa lớn nhất Đà Lạt với hàng trăm loài hoa khoe sắc quanh năm, là điểm check-in nổi bật của thành phố ngàn hoa.'
WHERE Slug = 'vuon-hoa-da-lat';

UPDATE DiaDiem
SET MoTaNgan = N'Tổ hợp vui chơi giải trí hiện đại bậc nhất Hạ Long với cáp treo Nữ Hoàng, công viên nước và nhiều hoạt động hấp dẫn cho mọi lứa tuổi.'
WHERE Slug = 'sun-world-ha-long';

UPDATE DiaDiem
SET MoTaNgan = N'Đảo du lịch nổi tiếng của Hạ Long với bãi biển nhân tạo, khu nghỉ dưỡng cao cấp và các chương trình biểu diễn nghệ thuật đặc sắc.'
WHERE Slug = 'dao-tuan-chau';

UPDATE DiaDiem
SET MoTaNgan = N'Quần thể danh thắng tâm linh nổi tiếng với chùa Đồng linh thiêng trên đỉnh núi, được xem là cái nôi của Thiền phái Trúc Lâm Yên Tử.'
WHERE Slug = 'yen-tu';

UPDATE DiaDiem
SET MoTaNgan = N'Bãi biển nhân tạo sôi động nằm giữa trung tâm Hạ Long với nhiều dịch vụ du lịch, vui chơi và giải trí ven biển hiện đại.'
WHERE Slug = 'bai-chay';

UPDATE DiaDiem
SET MoTaNgan = N'Vịnh biển hoang sơ mang vẻ đẹp nguyên bản với hàng trăm đảo đá vôi kỳ vĩ và không gian yên bình ít chịu tác động du lịch.'
WHERE Slug = 'vinh-bai-tu-long';

UPDATE DiaDiem
SET MoTaNgan = N'Cụm đảo hoang sơ ngoài khơi Hội An nổi tiếng với nước biển trong xanh, rạn san hô rực rỡ và nét đẹp bình yên của làng chài miền biển.'
WHERE Slug = 'cu-lao-cham';

UPDATE DiaDiem
SET MoTaNgan = N'Khu sinh thái đặc trưng miền sông nước với rừng dừa xanh mát, trải nghiệm chèo thuyền thúng và khám phá văn hóa làng quê Hội An.'
WHERE Slug = 'rung-dua-bay-mau';

UPDATE DiaDiem
SET MoTaNgan = N'Tổ hợp vui chơi giải trí hiện đại kết hợp văn hóa truyền thống, công viên nước và safari trên bờ biển miền Trung.'
WHERE Slug = 'vinwonders-nam-hoi-an';

UPDATE DiaDiem
SET MoTaNgan = N'Quần thể đền tháp Chăm Pa cổ kính được UNESCO công nhận là Di sản văn hóa thế giới, mang đậm dấu ấn kiến trúc và tín ngưỡng Champa.'
WHERE Slug = 'my-son';

UPDATE DiaDiem
SET MoTaNgan = N'Làng quê thanh bình nổi tiếng với nghề trồng rau hữu cơ truyền thống, nơi du khách có thể trải nghiệm làm nông dân thực thụ.'
WHERE Slug = 'lang-rau-tra-que';

UPDATE DiaDiem
SET MoTaNgan = N'Công viên chủ đề lớn nhất Phú Quốc với hàng trăm trò chơi hiện đại, thủy cung và các show diễn hoành tráng.'
WHERE Slug = 'vinwonders-phu-quoc';

UPDATE DiaDiem
SET MoTaNgan = N'Tuyến cáp treo vượt biển dài hàng đầu thế giới, mang đến trải nghiệm ngắm toàn cảnh biển đảo Phú Quốc từ trên cao.'
WHERE Slug = 'cap-treo-hon-thom';

UPDATE DiaDiem
SET MoTaNgan = N'Một trong những bãi biển đẹp nhất Phú Quốc với cát trắng mịn như kem và làn nước xanh trong màu ngọc bích.'
WHERE Slug = 'bai-sao';

UPDATE DiaDiem
SET MoTaNgan = N'Khu bảo tồn động vật bán hoang dã lớn nhất Việt Nam với hàng nghìn cá thể quý hiếm trong môi trường tự nhiên rộng lớn.'
WHERE Slug = 'safari-phu-quoc';

UPDATE DiaDiem
SET MoTaNgan = N'Thành phố không ngủ sôi động bậc nhất Phú Quốc với kiến trúc châu Âu, lễ hội đêm và các hoạt động giải trí đẳng cấp.'
WHERE Slug = 'grand-world';

UPDATE DiaDiem
SET MoTaNgan = N'Trung tâm quyền lực của triều Nguyễn xưa với kiến trúc cung đình uy nghi và quần thể di tích lịch sử nổi tiếng thế giới.'
WHERE Slug = 'dai-noi-hue';

UPDATE DiaDiem
SET MoTaNgan = N'Ngôi chùa cổ linh thiêng nằm bên dòng sông Hương thơ mộng, biểu tượng văn hóa và tâm linh đặc trưng của xứ Huế.'
WHERE Slug = 'chua-thien-mu';

UPDATE DiaDiem
SET MoTaNgan = N'Lăng tẩm nổi bật của vua Khải Định với kiến trúc giao thoa Á - Âu độc đáo và nghệ thuật khảm sành tinh xảo.'
WHERE Slug = 'lang-khai-dinh';

UPDATE DiaDiem
SET MoTaNgan = N'Khu lăng mộ rộng lớn hài hòa giữa kiến trúc cung đình và cảnh quan thiên nhiên xanh mát của vùng đất cố đô.'
WHERE Slug = 'lang-minh-mang';

UPDATE DiaDiem
SET MoTaNgan = N'Khu du lịch cáp treo hiện đại đưa du khách chinh phục đỉnh Fansipan – nóc nhà Đông Dương giữa biển mây Tây Bắc.'
WHERE Slug = 'fansipan';

UPDATE DiaDiem
SET MoTaNgan = N'Bản làng truyền thống của người H’Mông với nhà gỗ, ruộng bậc thang và không gian văn hóa vùng cao đặc sắc.'
WHERE Slug = 'ban-cat-cat';

UPDATE DiaDiem
SET MoTaNgan = N'Thung lũng nổi tiếng của Sa Pa với ruộng bậc thang trải dài tuyệt đẹp và bãi đá cổ bí ẩn giữa núi rừng Tây Bắc.'
WHERE Slug = 'thung-lung-muong-hoa';

UPDATE DiaDiem
SET MoTaNgan = N'Nhà thờ cổ mang phong cách Gothic Pháp nằm giữa trung tâm Sa Pa, biểu tượng kiến trúc nổi bật của thị trấn sương mù.'
WHERE Slug = 'nha-tho-da-sapa';

UPDATE DiaDiem
SET MoTaNgan = N'Khu nghỉ dưỡng nổi tiếng với khí hậu mát mẻ quanh năm, Cầu Vàng biểu tượng và kiến trúc châu Âu giữa núi rừng.'
WHERE Slug = 'ba-na-hills';

UPDATE DiaDiem
SET MoTaNgan = N'Cụm núi đá vôi linh thiêng với nhiều hang động huyền bí và chùa cổ nổi tiếng bên bờ biển Đà Nẵng.'
WHERE Slug = 'ngu-hanh-son';

UPDATE DiaDiem
SET MoTaNgan = N'Bán đảo xanh tuyệt đẹp của Đà Nẵng với rừng nguyên sinh, chùa Linh Ứng và những cung đường ven biển ngoạn mục.'
WHERE Slug = 'ban-dao-son-tra';

UPDATE DiaDiem
SET MoTaNgan = N'Bãi biển nổi tiếng thế giới với cát trắng mịn, sóng êm và không gian nghỉ dưỡng lý tưởng giữa lòng thành phố biển.'
WHERE Slug = 'bai-bien-my-khe';

UPDATE DiaDiem
SET MoTaNgan = N'Danh thắng nổi tiếng Ninh Bình với hành trình chèo thuyền xuyên hang động giữa cánh đồng lúa và núi đá vôi hùng vĩ.'
WHERE Slug = 'tam-coc';

UPDATE DiaDiem
SET MoTaNgan = N'Quần thể chùa lớn bậc nhất Đông Nam Á nổi tiếng với kiến trúc đồ sộ và không gian tâm linh thanh tịnh.'
WHERE Slug = 'chua-bai-dinh';

UPDATE DiaDiem
SET MoTaNgan = N'Điểm du lịch nổi tiếng với hành trình leo núi ngắm toàn cảnh Tam Cốc và Tràng An từ đỉnh núi ngoạn mục.'
WHERE Slug = 'hang-mua';

UPDATE DiaDiem
SET MoTaNgan = N'Kinh đô đầu tiên của nhà nước phong kiến tập quyền Việt Nam với nhiều đền đài và dấu tích lịch sử quan trọng.'
WHERE Slug = 'co-do-hoa-lu';

UPDATE DiaDiem
SET MoTaNgan = N'Khu đầm ngập nước lớn nhất đồng bằng Bắc Bộ với hệ sinh thái đa dạng và cảnh sắc thiên nhiên yên bình.'
WHERE Slug = 'dam-van-long';