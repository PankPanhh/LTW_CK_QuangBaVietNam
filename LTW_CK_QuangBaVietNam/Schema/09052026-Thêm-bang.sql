CREATE TABLE YeuThich (
    MaNguoiDung INT NOT NULL,
    MaDiaDiem   INT NOT NULL,
    NgayLuu     DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_YeuThich PRIMARY KEY (MaNguoiDung, MaDiaDiem),

    CONSTRAINT FK_YeuThich_NguoiDung
        FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),

    CONSTRAINT FK_YeuThich_DiaDiem
        FOREIGN KEY (MaDiaDiem) REFERENCES dbo.DiaDiem(MaDiaDiem)
        ON DELETE CASCADE
);

CREATE TABLE BoSuuTap (
        MaBoSuuTap INT IDENTITY(1,1) PRIMARY KEY,
        MaNguoiDung INT NOT NULL,
        TenBoSuuTap NVARCHAR(150) NOT NULL,
        MoTa NVARCHAR(500) NULL,
        NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_BoSuuTap_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
    );

    CREATE TABLE BoSuuTapDiaDiem (
        MaBoSuuTap INT NOT NULL,
        MaDiaDiem INT NOT NULL,
        NgayThem DATETIME NOT NULL DEFAULT GETDATE(),
        PRIMARY KEY (MaBoSuuTap, MaDiaDiem),
        CONSTRAINT FK_BSTD_BoSuuTap FOREIGN KEY (MaBoSuuTap) REFERENCES BoSuuTap(MaBoSuuTap),
        CONSTRAINT FK_BSTD_DiaDiem FOREIGN KEY (MaDiaDiem) REFERENCES DiaDiem(MaDiaDiem)
    );

