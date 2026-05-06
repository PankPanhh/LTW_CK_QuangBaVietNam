CREATE TABLE BaiViet (
    MaBaiViet INT IDENTITY(1,1) PRIMARY KEY,
    TieuDe NVARCHAR(255) NOT NULL,
    NoiDung NVARCHAR(MAX) NOT NULL,

    MaDiaDiem INT NULL, -- gắn với địa điểm (map)
    MaNguoiDung INT NOT NULL,

    -- Trạng thái duyệt
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'pending',
    -- pending | approved | rejected

    LyDoTuChoi NVARCHAR(500) NULL, -- nếu bị từ chối

    LuotLike INT DEFAULT 0,

    NgayDang DATETIME DEFAULT GETDATE(),
    NgayDuyet DATETIME NULL,
    NguoiDuyet INT NULL, -- admin duyệt

    FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiDuyet) REFERENCES NguoiDung(MaNguoiDung)
);

CREATE TABLE AnhBaiViet (
    MaAnh INT IDENTITY(1,1) PRIMARY KEY,
    MaBaiViet INT NOT NULL,
    DuongDanAnh NVARCHAR(MAX) NOT NULL,
    ThuTu INT DEFAULT 0,

    FOREIGN KEY (MaBaiViet) REFERENCES BaiViet(MaBaiViet)
);

CREATE TABLE LikeBaiViet (
    MaLike INT IDENTITY(1,1) PRIMARY KEY,
    MaBaiViet INT NOT NULL,
    MaNguoiDung INT NOT NULL,
    NgayLike DATETIME DEFAULT GETDATE(),

    CONSTRAINT UQ_Like UNIQUE (MaBaiViet, MaNguoiDung),

    FOREIGN KEY (MaBaiViet) REFERENCES BaiViet(MaBaiViet),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);

CREATE TABLE BinhLuan (
    MaBinhLuan INT IDENTITY(1,1) PRIMARY KEY,
    MaBaiViet INT NOT NULL,
    MaNguoiDung INT NOT NULL,
    NoiDung NVARCHAR(MAX) NOT NULL,
    ParentId INT NULL,
    NgayDang DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (MaBaiViet) REFERENCES BaiViet(MaBaiViet),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (ParentId) REFERENCES BinhLuan(MaBinhLuan)
);

CREATE INDEX IDX_Post_Status ON BaiViet(TrangThai);
CREATE INDEX IDX_Post_Place ON BaiViet(MaDiaDiem);