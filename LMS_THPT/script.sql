CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Khoi] (
    [Id] int NOT NULL IDENTITY,
    [TenKhoi] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Khoi] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey])
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name])
);
GO


CREATE TABLE [BaiGiang] (
    [Id] int NOT NULL IDENTITY,
    [TieuDe] nvarchar(max) NOT NULL,
    [MoTa] nvarchar(max) NULL,
    [ThuTu] int NOT NULL,
    [IsActive] bit NOT NULL,
    [NgayTao] datetime2 NOT NULL,
    [NgayCapNhat] datetime2 NULL,
    [MonHocId] int NOT NULL,
    [NguoiDungId] nvarchar(450) NULL,
    CONSTRAINT [PK_BaiGiang] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [BaiNop] (
    [Id] int NOT NULL IDENTITY,
    [NoiDung] nvarchar(max) NULL,
    [DuongDanFile] nvarchar(max) NULL,
    [NgayNop] datetime2 NOT NULL,
    [TrangThai] int NOT NULL,
    [Diem] float NULL,
    [NhanXet] nvarchar(max) NULL,
    [NgayCham] datetime2 NULL,
    [BaiTapId] int NOT NULL,
    [HocSinhId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_BaiNop] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [BaiTap] (
    [Id] int NOT NULL IDENTITY,
    [TieuDe] nvarchar(max) NOT NULL,
    [MoTa] nvarchar(max) NULL,
    [NoiDung] nvarchar(max) NULL,
    [HanNop] datetime2 NOT NULL,
    [DiemToiDa] int NOT NULL,
    [TrangThai] int NOT NULL,
    [NgayTao] datetime2 NOT NULL,
    [NgayCapNhat] datetime2 NULL,
    [MonHocId] int NOT NULL,
    [NguoiDungId] nvarchar(450) NULL,
    CONSTRAINT [PK_BaiTap] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [DangKyHoc] (
    [Id] int NOT NULL IDENTITY,
    [NgayDangKy] datetime2 NOT NULL,
    [TrangThai] int NOT NULL,
    [GhiChu] nvarchar(max) NULL,
    [MonHocId] int NOT NULL,
    [HocSinhId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_DangKyHoc] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [DanhSachBinhLuan] (
    [Id] int NOT NULL IDENTITY,
    [NoiDung] nvarchar(max) NOT NULL,
    [NgayTao] datetime2 NOT NULL,
    [NguoiDungId] nvarchar(450) NOT NULL,
    [BaiGiangId] int NULL,
    [BaiTapId] int NULL,
    [ParentId] int NULL,
    CONSTRAINT [PK_DanhSachBinhLuan] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DanhSachBinhLuan_BaiGiang_BaiGiangId] FOREIGN KEY ([BaiGiangId]) REFERENCES [BaiGiang] ([Id]),
    CONSTRAINT [FK_DanhSachBinhLuan_BaiTap_BaiTapId] FOREIGN KEY ([BaiTapId]) REFERENCES [BaiTap] ([Id]),
    CONSTRAINT [FK_DanhSachBinhLuan_DanhSachBinhLuan_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [DanhSachBinhLuan] ([Id])
);
GO


CREATE TABLE [DiemSo] (
    [Id] int NOT NULL IDENTITY,
    [Diem] float NOT NULL,
    [LoaiDiem] int NOT NULL,
    [NhanXet] nvarchar(max) NULL,
    [NgayNhap] datetime2 NOT NULL,
    [NgayCapNhat] datetime2 NULL,
    [MonHocId] int NOT NULL,
    [HocSinhId] nvarchar(450) NOT NULL,
    [NguoiDungId] nvarchar(450) NOT NULL,
    [GiaoVienId] nvarchar(450) NOT NULL,
    [DiemGiuaKy] float NULL,
    [DiemCuoiKy] float NULL,
    CONSTRAINT [PK_DiemSo] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [LichHoc] (
    [Id] int NOT NULL IDENTITY,
    [TieuDe] nvarchar(max) NULL,
    [LopId] int NOT NULL,
    [MonHocId] int NOT NULL,
    [GiaoVienId] nvarchar(450) NULL,
    [Thu] int NOT NULL,
    [TietHoc] int NOT NULL,
    [PhongHoc] nvarchar(max) NOT NULL,
    [NgayHoc] datetime2 NOT NULL,
    [GioBatDau] time NOT NULL,
    [GioKetThuc] time NOT NULL,
    CONSTRAINT [PK_LichHoc] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Lop] (
    [Id] int NOT NULL IDENTITY,
    [TenLop] nvarchar(max) NOT NULL,
    [MaKhoi] int NOT NULL,
    [GiaoVienChuNhiemId] nvarchar(450) NULL,
    CONSTRAINT [PK_Lop] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Lop_Khoi_MaKhoi] FOREIGN KEY ([MaKhoi]) REFERENCES [Khoi] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [NguoiDung] (
    [Id] nvarchar(450) NOT NULL,
    [HoTen] nvarchar(max) NOT NULL,
    [AnhDaiDien] nvarchar(max) NULL,
    [NgaySinh] datetime2 NULL,
    [GioiTinh] nvarchar(max) NULL,
    [DiaChi] nvarchar(max) NULL,
    [LopId] int NULL,
    [MaHocSinh] nvarchar(max) NULL,
    [ChuyenMon] nvarchar(max) NULL,
    [ChucVu] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [NgayTao] datetime2 NOT NULL,
    [NgayCapNhat] datetime2 NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_NguoiDung] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NguoiDung_Lop_LopId] FOREIGN KEY ([LopId]) REFERENCES [Lop] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [MonHoc] (
    [Id] int NOT NULL IDENTITY,
    [TenMonHoc] nvarchar(max) NOT NULL,
    [MoTa] nvarchar(max) NULL,
    [MucTieu] nvarchar(max) NULL,
    [NoiDung] nvarchar(max) NULL,
    [HinhAnh] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [NgayTao] datetime2 NOT NULL,
    [NgayCapNhat] datetime2 NULL,
    [KhoiId] int NOT NULL,
    [GiaoVienId] nvarchar(450) NULL,
    CONSTRAINT [PK_MonHoc] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MonHoc_Khoi_KhoiId] FOREIGN KEY ([KhoiId]) REFERENCES [Khoi] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MonHoc_NguoiDung_GiaoVienId] FOREIGN KEY ([GiaoVienId]) REFERENCES [NguoiDung] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [ThongBaos] (
    [Id] int NOT NULL IDENTITY,
    [TieuDe] nvarchar(200) NOT NULL,
    [NoiDung] nvarchar(max) NOT NULL,
    [NgayDang] datetime2 NOT NULL,
    [NguoiDangId] nvarchar(450) NULL,
    [HienThi] bit NOT NULL,
    CONSTRAINT [PK_ThongBaos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThongBaos_NguoiDung_NguoiDangId] FOREIGN KEY ([NguoiDangId]) REFERENCES [NguoiDung] ([Id])
);
GO


CREATE TABLE [YeuCauGiaoVien] (
    [Id] int NOT NULL IDENTITY,
    [LoaiYeuCau] int NOT NULL,
    [TieuDe] nvarchar(100) NOT NULL,
    [MoTa] nvarchar(max) NOT NULL,
    [NoiDung] nvarchar(max) NOT NULL,
    [TrangThai] int NOT NULL,
    [GhiChu] nvarchar(max) NULL,
    [GhiChuAdmin] nvarchar(max) NULL,
    [GiaoVienId] nvarchar(max) NOT NULL,
    [MaGiaoVien] nvarchar(450) NOT NULL,
    [NguoiXuLyId] nvarchar(450) NULL,
    [XuLyBoi] nvarchar(max) NULL,
    [LopId] int NULL,
    [NgayGui] datetime2 NOT NULL,
    [NgayXuLy] datetime2 NULL,
    [DuongDanTaiLieu] nvarchar(max) NULL,
    CONSTRAINT [PK_YeuCauGiaoVien] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_YeuCauGiaoVien_Lop_LopId] FOREIGN KEY ([LopId]) REFERENCES [Lop] ([Id]),
    CONSTRAINT [FK_YeuCauGiaoVien_NguoiDung_MaGiaoVien] FOREIGN KEY ([MaGiaoVien]) REFERENCES [NguoiDung] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_YeuCauGiaoVien_NguoiDung_NguoiXuLyId] FOREIGN KEY ([NguoiXuLyId]) REFERENCES [NguoiDung] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [LopMonHoc] (
    [Id] int NOT NULL IDENTITY,
    [LopId] int NOT NULL,
    [MonHocId] int NOT NULL,
    [GiaoVienId] nvarchar(450) NULL,
    CONSTRAINT [PK_LopMonHoc] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LopMonHoc_Lop_LopId] FOREIGN KEY ([LopId]) REFERENCES [Lop] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LopMonHoc_MonHoc_MonHocId] FOREIGN KEY ([MonHocId]) REFERENCES [MonHoc] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LopMonHoc_NguoiDung_GiaoVienId] FOREIGN KEY ([GiaoVienId]) REFERENCES [NguoiDung] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [MonHocGiaoViens] (
    [Id] int NOT NULL IDENTITY,
    [NguoiDungId] nvarchar(450) NOT NULL,
    [MonHocId] int NOT NULL,
    [LopId] int NULL,
    CONSTRAINT [PK_MonHocGiaoViens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MonHocGiaoViens_Lop_LopId] FOREIGN KEY ([LopId]) REFERENCES [Lop] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_MonHocGiaoViens_MonHoc_MonHocId] FOREIGN KEY ([MonHocId]) REFERENCES [MonHoc] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MonHocGiaoViens_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [TaiLieu] (
    [Id] int NOT NULL IDENTITY,
    [TenTaiLieu] nvarchar(max) NOT NULL,
    [DuongDanFile] nvarchar(max) NOT NULL,
    [LoaiTaiLieu] int NOT NULL,
    [KichThuocFile] bigint NOT NULL,
    [NgayTao] datetime2 NOT NULL,
    [NgayCapNhat] datetime2 NULL,
    [BaiGiangId] int NULL,
    [MonHocId] int NULL,
    CONSTRAINT [PK_TaiLieu] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TaiLieu_BaiGiang_BaiGiangId] FOREIGN KEY ([BaiGiangId]) REFERENCES [BaiGiang] ([Id]),
    CONSTRAINT [FK_TaiLieu_MonHoc_MonHocId] FOREIGN KEY ([MonHocId]) REFERENCES [MonHoc] ([Id])
);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [IX_BaiGiang_MonHocId] ON [BaiGiang] ([MonHocId]);
GO


CREATE INDEX [IX_BaiGiang_NguoiDungId] ON [BaiGiang] ([NguoiDungId]);
GO


CREATE INDEX [IX_BaiNop_BaiTapId] ON [BaiNop] ([BaiTapId]);
GO


CREATE INDEX [IX_BaiNop_HocSinhId] ON [BaiNop] ([HocSinhId]);
GO


CREATE INDEX [IX_BaiTap_MonHocId] ON [BaiTap] ([MonHocId]);
GO


CREATE INDEX [IX_BaiTap_NguoiDungId] ON [BaiTap] ([NguoiDungId]);
GO


CREATE INDEX [IX_DangKyHoc_HocSinhId] ON [DangKyHoc] ([HocSinhId]);
GO


CREATE INDEX [IX_DangKyHoc_MonHocId] ON [DangKyHoc] ([MonHocId]);
GO


CREATE INDEX [IX_DanhSachBinhLuan_BaiGiangId] ON [DanhSachBinhLuan] ([BaiGiangId]);
GO


CREATE INDEX [IX_DanhSachBinhLuan_BaiTapId] ON [DanhSachBinhLuan] ([BaiTapId]);
GO


CREATE INDEX [IX_DanhSachBinhLuan_NguoiDungId] ON [DanhSachBinhLuan] ([NguoiDungId]);
GO


CREATE INDEX [IX_DanhSachBinhLuan_ParentId] ON [DanhSachBinhLuan] ([ParentId]);
GO


CREATE INDEX [IX_DiemSo_GiaoVienId] ON [DiemSo] ([GiaoVienId]);
GO


CREATE INDEX [IX_DiemSo_HocSinhId] ON [DiemSo] ([HocSinhId]);
GO


CREATE INDEX [IX_DiemSo_MonHocId] ON [DiemSo] ([MonHocId]);
GO


CREATE INDEX [IX_DiemSo_NguoiDungId] ON [DiemSo] ([NguoiDungId]);
GO


CREATE INDEX [IX_LichHoc_GiaoVienId] ON [LichHoc] ([GiaoVienId]);
GO


CREATE INDEX [IX_LichHoc_LopId] ON [LichHoc] ([LopId]);
GO


CREATE INDEX [IX_LichHoc_MonHocId] ON [LichHoc] ([MonHocId]);
GO


CREATE INDEX [IX_Lop_GiaoVienChuNhiemId] ON [Lop] ([GiaoVienChuNhiemId]);
GO


CREATE INDEX [IX_Lop_MaKhoi] ON [Lop] ([MaKhoi]);
GO


CREATE INDEX [IX_LopMonHoc_GiaoVienId] ON [LopMonHoc] ([GiaoVienId]);
GO


CREATE INDEX [IX_LopMonHoc_LopId] ON [LopMonHoc] ([LopId]);
GO


CREATE INDEX [IX_LopMonHoc_MonHocId] ON [LopMonHoc] ([MonHocId]);
GO


CREATE INDEX [IX_MonHoc_GiaoVienId] ON [MonHoc] ([GiaoVienId]);
GO


CREATE INDEX [IX_MonHoc_KhoiId] ON [MonHoc] ([KhoiId]);
GO


CREATE INDEX [IX_MonHocGiaoViens_LopId] ON [MonHocGiaoViens] ([LopId]);
GO


CREATE UNIQUE INDEX [IX_MonHocGiaoViens_MonHocId_LopId] ON [MonHocGiaoViens] ([MonHocId], [LopId]) WHERE [LopId] IS NOT NULL;
GO


CREATE INDEX [IX_MonHocGiaoViens_NguoiDungId] ON [MonHocGiaoViens] ([NguoiDungId]);
GO


CREATE INDEX [EmailIndex] ON [NguoiDung] ([NormalizedEmail]);
GO


CREATE INDEX [IX_NguoiDung_LopId] ON [NguoiDung] ([LopId]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [NguoiDung] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_TaiLieu_BaiGiangId] ON [TaiLieu] ([BaiGiangId]);
GO


CREATE INDEX [IX_TaiLieu_MonHocId] ON [TaiLieu] ([MonHocId]);
GO


CREATE INDEX [IX_ThongBaos_NguoiDangId] ON [ThongBaos] ([NguoiDangId]);
GO


CREATE INDEX [IX_YeuCauGiaoVien_LopId] ON [YeuCauGiaoVien] ([LopId]);
GO


CREATE INDEX [IX_YeuCauGiaoVien_MaGiaoVien] ON [YeuCauGiaoVien] ([MaGiaoVien]);
GO


CREATE INDEX [IX_YeuCauGiaoVien_NguoiXuLyId] ON [YeuCauGiaoVien] ([NguoiXuLyId]);
GO


ALTER TABLE [AspNetUserClaims] ADD CONSTRAINT [FK_AspNetUserClaims_NguoiDung_UserId] FOREIGN KEY ([UserId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [AspNetUserLogins] ADD CONSTRAINT [FK_AspNetUserLogins_NguoiDung_UserId] FOREIGN KEY ([UserId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [AspNetUserRoles] ADD CONSTRAINT [FK_AspNetUserRoles_NguoiDung_UserId] FOREIGN KEY ([UserId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [AspNetUserTokens] ADD CONSTRAINT [FK_AspNetUserTokens_NguoiDung_UserId] FOREIGN KEY ([UserId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [BaiGiang] ADD CONSTRAINT [FK_BaiGiang_MonHoc_MonHocId] FOREIGN KEY ([MonHocId]) REFERENCES [MonHoc] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [BaiGiang] ADD CONSTRAINT [FK_BaiGiang_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]);
GO


ALTER TABLE [BaiNop] ADD CONSTRAINT [FK_BaiNop_BaiTap_BaiTapId] FOREIGN KEY ([BaiTapId]) REFERENCES [BaiTap] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [BaiNop] ADD CONSTRAINT [FK_BaiNop_NguoiDung_HocSinhId] FOREIGN KEY ([HocSinhId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [BaiTap] ADD CONSTRAINT [FK_BaiTap_MonHoc_MonHocId] FOREIGN KEY ([MonHocId]) REFERENCES [MonHoc] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [BaiTap] ADD CONSTRAINT [FK_BaiTap_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]);
GO


ALTER TABLE [DangKyHoc] ADD CONSTRAINT [FK_DangKyHoc_MonHoc_MonHocId] FOREIGN KEY ([MonHocId]) REFERENCES [MonHoc] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [DangKyHoc] ADD CONSTRAINT [FK_DangKyHoc_NguoiDung_HocSinhId] FOREIGN KEY ([HocSinhId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [DanhSachBinhLuan] ADD CONSTRAINT [FK_DanhSachBinhLuan_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [DiemSo] ADD CONSTRAINT [FK_DiemSo_MonHoc_MonHocId] FOREIGN KEY ([MonHocId]) REFERENCES [MonHoc] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [DiemSo] ADD CONSTRAINT [FK_DiemSo_NguoiDung_GiaoVienId] FOREIGN KEY ([GiaoVienId]) REFERENCES [NguoiDung] ([Id]) ON DELETE NO ACTION;
GO


ALTER TABLE [DiemSo] ADD CONSTRAINT [FK_DiemSo_NguoiDung_HocSinhId] FOREIGN KEY ([HocSinhId]) REFERENCES [NguoiDung] ([Id]) ON DELETE NO ACTION;
GO


ALTER TABLE [DiemSo] ADD CONSTRAINT [FK_DiemSo_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [LichHoc] ADD CONSTRAINT [FK_LichHoc_Lop_LopId] FOREIGN KEY ([LopId]) REFERENCES [Lop] ([Id]) ON DELETE NO ACTION;
GO


ALTER TABLE [LichHoc] ADD CONSTRAINT [FK_LichHoc_MonHoc_MonHocId] FOREIGN KEY ([MonHocId]) REFERENCES [MonHoc] ([Id]) ON DELETE NO ACTION;
GO


ALTER TABLE [LichHoc] ADD CONSTRAINT [FK_LichHoc_NguoiDung_GiaoVienId] FOREIGN KEY ([GiaoVienId]) REFERENCES [NguoiDung] ([Id]) ON DELETE SET NULL;
GO


ALTER TABLE [Lop] ADD CONSTRAINT [FK_Lop_NguoiDung_GiaoVienChuNhiemId] FOREIGN KEY ([GiaoVienChuNhiemId]) REFERENCES [NguoiDung] ([Id]) ON DELETE SET NULL;
GO


