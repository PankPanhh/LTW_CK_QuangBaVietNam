CREATE TABLE LichTrinh (
    MaLichTrinh INT IDENTITY(1,1) PRIMARY KEY,
    TenLichTrinh NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(MAX),

    MaNguoiDung INT NOT NULL,

    SoNgay INT DEFAULT 1,

    TrangThai NVARCHAR(20) DEFAULT 'private',
    -- private | public

    AnhBia NVARCHAR(MAX),

    NgayBatDau DATE,
    NgayKetThuc DATE,

    LuotXem INT DEFAULT 0,
    LuotLike INT DEFAULT 0,

    NgayTao DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (MaNguoiDung)
    REFERENCES NguoiDung(MaNguoiDung)
);

CREATE TABLE NgayLichTrinh (
    MaNgay INT IDENTITY(1,1) PRIMARY KEY,

    MaLichTrinh INT NOT NULL,

    ThuTuNgay INT NOT NULL,
    TieuDe NVARCHAR(255),

    FOREIGN KEY (MaLichTrinh)
    REFERENCES LichTrinh(MaLichTrinh)
);

CREATE TABLE ChiTietLichTrinh (
    MaChiTiet INT IDENTITY(1,1) PRIMARY KEY,

    MaNgay INT NOT NULL,
    MaDiaDiem INT NOT NULL,

    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NULL,

    GhiChu NVARCHAR(500),

    ThuTu INT NOT NULL,

    FOREIGN KEY (MaNgay)
    REFERENCES NgayLichTrinh(MaNgay),

    FOREIGN KEY (MaDiaDiem)
    REFERENCES DiaDiem(MaDiaDiem)
);