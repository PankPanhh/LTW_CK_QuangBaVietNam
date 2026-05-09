CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    MatKhauHash NVARCHAR(255) NOT NULL,
    SoDienThoai VARCHAR(20),
    
    -- Các trường bổ sung dựa trên UI --
    NgaySinh DATE NULL,                -- Khớp với field 'Ngày sinh'
    TieuSu NVARCHAR(500) NULL,         -- Khớp với field 'Tiểu sử'
    ThanhPho NVARCHAR(100) NULL,       -- Khớp với field 'Thành phố'
    QuocGia NVARCHAR(100) NULL,        -- Khớp với field 'Quốc gia'
    -----------------------------------

     AnhDaiDien NVARCHAR(MAX),

    VaiTro INT NOT NULL DEFAULT 2, -- 1: Admin, 2: User
    TrangThai BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NULL
);