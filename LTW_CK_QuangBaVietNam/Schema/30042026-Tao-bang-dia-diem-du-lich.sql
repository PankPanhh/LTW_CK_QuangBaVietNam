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