-- ============================================================
-- SCRIPT CẬP NHẬT KINH ĐỘ, VĨ ĐỘ CHÍNH XÁC CHO BẢNG DiaDiem
-- Dựa theo vị trí thực tế từng địa danh nổi tiếng Việt Nam
-- ============================================================

-- 1. Vịnh Hạ Long (Quảng Ninh)
UPDATE DiaDiem SET KinhDo = 107.0833, ViDo = 20.9500
WHERE Slug = 'vinh-ha-long';

-- 2. Phố cổ Hội An (Quảng Nam)
UPDATE DiaDiem SET KinhDo = 108.3280, ViDo = 15.8800
WHERE Slug = 'pho-co-hoi-an';

-- 3. Đà Lạt (Lâm Đồng)
UPDATE DiaDiem SET KinhDo = 108.4500, ViDo = 11.9400
WHERE Slug = 'da-lat';

-- 4. Đảo Phú Quốc (Kiên Giang)
UPDATE DiaDiem SET KinhDo = 103.9840, ViDo = 10.2890
WHERE Slug = 'dao-phu-quoc';

-- 5. Cố đô Huế (Thừa Thiên Huế)
UPDATE DiaDiem SET KinhDo = 107.5833, ViDo = 16.4667
WHERE Slug = 'co-do-hue';

-- 6. Sa Pa (Lào Cai)
UPDATE DiaDiem SET KinhDo = 103.8440, ViDo = 22.3360
WHERE Slug = 'sapa';

-- 7. Đà Nẵng (trung tâm thành phố)
UPDATE DiaDiem SET KinhDo = 108.2022, ViDo = 16.0544
WHERE Slug = 'da-nang';

-- 8. Tràng An (Ninh Bình)
UPDATE DiaDiem SET KinhDo = 105.8944, ViDo = 20.2522
WHERE Slug = 'trang-an';

-- 9. Thung lũng Tình Yêu (Đà Lạt)
UPDATE DiaDiem SET KinhDo = 108.4333, ViDo = 11.9833
WHERE Slug = 'thung-lung-tinh-yeu';

-- 10. Núi Langbiang (Lạc Dương, Lâm Đồng)
UPDATE DiaDiem SET KinhDo = 108.4167, ViDo = 12.0500
WHERE Slug = 'nui-langbiang';

-- 11. Thác Datanla (Đà Lạt)
UPDATE DiaDiem SET KinhDo = 108.4300, ViDo = 11.9000
WHERE Slug = 'thac-datanla';

-- 12. Hồ Tuyền Lâm & Thiền Viện Trúc Lâm (Đà Lạt)
UPDATE DiaDiem SET KinhDo = 108.4167, ViDo = 11.8800
WHERE Slug = 'ho-tuyen-lam';

-- 13. Biệt thự Hằng Nga - Crazy House (Đà Lạt)
UPDATE DiaDiem SET KinhDo = 108.4500, ViDo = 11.9300
WHERE Slug = 'crazy-house';

-- 14. Vườn Hoa Thành Phố Đà Lạt
UPDATE DiaDiem SET KinhDo = 108.4400, ViDo = 11.9400
WHERE Slug = 'vuon-hoa-da-lat';

-- 15. Sun World Hạ Long Park (Bãi Cháy, Hạ Long)
UPDATE DiaDiem SET KinhDo = 107.0500, ViDo = 20.9500
WHERE Slug = 'sun-world-ha-long';

-- 16. Đảo Tuần Châu (Hạ Long, Quảng Ninh)
UPDATE DiaDiem SET KinhDo = 107.0167, ViDo = 20.9167
WHERE Slug = 'dao-tuan-chau';

-- 17. Khu di tích Yên Tử (Uông Bí, Quảng Ninh)
UPDATE DiaDiem SET KinhDo = 106.7333, ViDo = 21.1167
WHERE Slug = 'yen-tu';

-- 18. Bãi Cháy (Hạ Long, Quảng Ninh)
UPDATE DiaDiem SET KinhDo = 107.0500, ViDo = 20.9600
WHERE Slug = 'bai-chay';

-- 19. Vịnh Bái Tử Long (Vân Đồn, Quảng Ninh)
UPDATE DiaDiem SET KinhDo = 107.5000, ViDo = 21.0000
WHERE Slug = 'vinh-bai-tu-long';

-- 20. Cù Lao Chàm (Hội An, Quảng Nam)
UPDATE DiaDiem SET KinhDo = 108.5000, ViDo = 15.9500
WHERE Slug = 'cu-lao-cham';

-- 21. Rừng dừa Bảy Mẫu (Hội An, Quảng Nam)
UPDATE DiaDiem SET KinhDo = 108.5000, ViDo = 15.8333
WHERE Slug = 'rung-dua-bay-mau';

-- 22. VinWonders Nam Hội An (Thăng Bình, Quảng Nam)
UPDATE DiaDiem SET KinhDo = 108.3500, ViDo = 15.8500
WHERE Slug = 'vinwonders-nam-...';

-- 23. Thánh địa Mỹ Sơn (Duy Xuyên, Quảng Nam)
UPDATE DiaDiem SET KinhDo = 108.1167, ViDo = 15.7667
WHERE Slug = 'my-son';

-- 24. Làng rau Trà Quế (Hội An, Quảng Nam)
UPDATE DiaDiem SET KinhDo = 108.3667, ViDo = 15.9167
WHERE Slug = 'lang-rau-tra-que';

-- 25. VinWonders Phú Quốc (Kiên Giang)
UPDATE DiaDiem SET KinhDo = 103.9500, ViDo = 10.3500
WHERE Slug = 'vinwonders-phu-...';

-- 26. Cáp treo Hòn Thơm (An Thới, Phú Quốc)
UPDATE DiaDiem SET KinhDo = 103.9800, ViDo = 10.2500
WHERE Slug = 'cap-treo-hon-thom';

-- 27. Bãi Sao (Phú Quốc, Kiên Giang)
UPDATE DiaDiem SET KinhDo = 104.0500, ViDo = 10.1167
WHERE Slug = 'bai-sao';

-- 28. Vinpearl Safari Phú Quốc
UPDATE DiaDiem SET KinhDo = 103.9000, ViDo = 10.4000
WHERE Slug = 'safari-phu-quoc';

-- 29. Grand World Phú Quốc
UPDATE DiaDiem SET KinhDo = 103.9600, ViDo = 10.3500
WHERE Slug = 'grand-world';

-- 30. Đại Nội – Hoàng Thành Huế (TP. Huế)
UPDATE DiaDiem SET KinhDo = 107.5772, ViDo = 16.4698
WHERE Slug = 'dai-noi-hue';

-- 31. Chùa Thiên Mụ (Huế)
UPDATE DiaDiem SET KinhDo = 107.5500, ViDo = 16.4500
WHERE Slug = 'chua-thien-mu';

-- 32. Lăng Khải Định (Huế)
UPDATE DiaDiem SET KinhDo = 107.6000, ViDo = 16.4000
WHERE Slug = 'lang-khai-dinh';

-- 33. Lăng Minh Mạng (Huế)
UPDATE DiaDiem SET KinhDo = 107.5500, ViDo = 16.4000
WHERE Slug = 'lang-minh-mang';

-- 34. Fansipan Legend (Sa Pa, Lào Cai)
UPDATE DiaDiem SET KinhDo = 103.7750, ViDo = 22.3000
WHERE Slug = 'fansipan';

-- 35. Bản Cát Cát (Sa Pa, Lào Cai)
UPDATE DiaDiem SET KinhDo = 103.8167, ViDo = 22.3333
WHERE Slug = 'ban-cat-cat';

-- 36. Thung lũng Mường Hoa (Sa Pa, Lào Cai)
UPDATE DiaDiem SET KinhDo = 103.8500, ViDo = 22.3200
WHERE Slug = 'thung-lung-muon...';

-- 37. Nhà thờ đá Sa Pa
UPDATE DiaDiem SET KinhDo = 103.8440, ViDo = 22.3367
WHERE Slug = 'nha-tho-da-sapa';

-- 38. Bà Nà Hills (Đà Nẵng)
UPDATE DiaDiem SET KinhDo = 107.9833, ViDo = 15.9833
WHERE Slug = 'ba-na-hills';

-- 39. Ngũ Hành Sơn (Đà Nẵng)
UPDATE DiaDiem SET KinhDo = 108.2500, ViDo = 16.0000
WHERE Slug = 'ngu-hanh-son';

-- 40. Bán đảo Sơn Trà (Đà Nẵng)
UPDATE DiaDiem SET KinhDo = 108.2800, ViDo = 16.1000
WHERE Slug = 'ban-dao-son-tra';

-- 41. Bãi biển Mỹ Khê (Đà Nẵng)
UPDATE DiaDiem SET KinhDo = 108.2467, ViDo = 16.0567
WHERE Slug = 'bai-bien-my-khe';

-- 42. Tam Cốc – Bích Động (Ninh Bình)
UPDATE DiaDiem SET KinhDo = 105.9000, ViDo = 20.2500
WHERE Slug = 'tam-coc';

-- 43. Chùa Bái Đính (Ninh Bình)
UPDATE DiaDiem SET KinhDo = 105.8500, ViDo = 20.2500
WHERE Slug = 'chua-bai-dinh';

-- 44. Hang Múa (Ninh Bình)
UPDATE DiaDiem SET KinhDo = 105.9167, ViDo = 20.2500
WHERE Slug = 'hang-mua';

-- 45. Cố đô Hoa Lư (Ninh Bình)
UPDATE DiaDiem SET KinhDo = 105.8800, ViDo = 20.2500
WHERE Slug = 'co-do-hoa-lu';

-- 46. Đầm Vân Long (Ninh Bình)
UPDATE DiaDiem SET KinhDo = 105.8500, ViDo = 20.3000
WHERE Slug = 'dam-van-long';

-- ============================================================
-- KIỂM TRA SAU KHI CẬP NHẬT
-- ============================================================
SELECT MaDiaDiem, TenDiaDiem, Slug, KinhDo, ViDo
FROM DiaDiem
ORDER BY MaDiaDiem;