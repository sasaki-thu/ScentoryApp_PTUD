using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ScentoryApp.Models;

public partial class ScentoryPtudContext : DbContext
{
    public ScentoryPtudContext()
    {
    }

    public ScentoryPtudContext(DbContextOptions<ScentoryPtudContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<DanhGiaSanPham> DanhGiaSanPhams { get; set; }

    public virtual DbSet<DanhMucSanPham> DanhMucSanPhams { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<DonViVanChuyen> DonViVanChuyens { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<MaGiamGium> MaGiamGia { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DAT;Database=ScentoryPTUD;Integrated Security=true;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => new { e.IdDonHang, e.IdSanPham }).HasName("PK__ChiTietD__AFA0CC00374D76B1");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.IdDonHang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_DonHang");
            entity.Property(e => e.IdSanPham)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_SanPham");
            entity.Property(e => e.DonGia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.IdDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK7_DH_CTDH");

            entity.HasOne(d => d.IdSanPhamNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.IdSanPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK8_SP_CTDH");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => new { e.IdGioHang, e.IdSanPham }).HasName("PK__ChiTietG__F624402EF5628835");

            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.IdGioHang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_GioHang");
            entity.Property(e => e.IdSanPham)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_SanPham");

            entity.HasOne(d => d.IdGioHangNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.IdGioHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK4_GH_CTGH");

            entity.HasOne(d => d.IdSanPhamNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.IdSanPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK5_SP_CTGH");
        });

        modelBuilder.Entity<DanhGiaSanPham>(entity =>
        {
            entity.HasKey(e => e.IdDanhGia).HasName("PK__DanhGiaS__6C898AE15C3A2D5B");

            entity.ToTable("DanhGiaSanPham");

            entity.Property(e => e.IdDanhGia)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_DanhGia");
            entity.Property(e => e.IdNguoiDung)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_NguoiDung");
            entity.Property(e => e.IdSanPham)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_SanPham");
            entity.Property(e => e.NoiDung).HasMaxLength(1);
            entity.Property(e => e.ThoiGianDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdNguoiDungNavigation).WithMany(p => p.DanhGiaSanPhams)
                .HasForeignKey(d => d.IdNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK10_ND_DG");

            entity.HasOne(d => d.IdSanPhamNavigation).WithMany(p => p.DanhGiaSanPhams)
                .HasForeignKey(d => d.IdSanPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK9_SP_DG");
        });

        modelBuilder.Entity<DanhMucSanPham>(entity =>
        {
            entity.HasKey(e => e.IdDanhMucSanPham).HasName("PK__DanhMucS__95D6C4EF0E8DF4DB");

            entity.ToTable("DanhMucSanPham", tb => tb.HasTrigger("trg_DM_UpdateTimestamp"));

            entity.Property(e => e.IdDanhMucSanPham)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_DanhMucSanPham");
            entity.Property(e => e.ThoiGianCapNhatDm)
                .HasColumnType("datetime")
                .HasColumnName("ThoiGianCapNhatDM");
            entity.Property(e => e.ThoiGianTaoDm)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ThoiGianTaoDM");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.IdDonHang).HasName("PK__DonHang__99B72639FA2F4692");

            entity.ToTable("DonHang", tb => tb.HasTrigger("trg_DonHang_Update"));

            entity.Property(e => e.IdDonHang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_DonHang");
            entity.Property(e => e.IdDonViVanChuyen)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_DonViVanChuyen");
            entity.Property(e => e.IdMaGiamGia)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_MaGiamGia");
            entity.Property(e => e.IdNguoiDung)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_NguoiDung");
            entity.Property(e => e.PhiVanChuyen).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ThoiGianCapNhat).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianDatHang)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ThoiGianHoanTatDonHang).HasColumnType("datetime");
            entity.Property(e => e.ThueBanHang).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TinhTrangDonHang).HasMaxLength(20);
            entity.Property(e => e.TongTienDonHang).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdDonViVanChuyenNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.IdDonViVanChuyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK11_DVVC_DH");

            entity.HasOne(d => d.IdMaGiamGiaNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.IdMaGiamGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK12_DH_MGG");

            entity.HasOne(d => d.IdNguoiDungNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.IdNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK6_ND_GH");
        });

        modelBuilder.Entity<DonViVanChuyen>(entity =>
        {
            entity.HasKey(e => e.IdDonViVanChuyen).HasName("PK__DonViVan__54421B6470EDEEA2");

            entity.ToTable("DonViVanChuyen");

            entity.Property(e => e.IdDonViVanChuyen)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_DonViVanChuyen");
            entity.Property(e => e.TenDonViVanChuyen).HasMaxLength(1);
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.IdGioHang).HasName("PK__GioHang__C033AA174E3D4ABD");

            entity.ToTable("GioHang", tb => tb.HasTrigger("trg_GioHang_UpdateTimestamp"));

            entity.Property(e => e.IdGioHang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_GioHang");
            entity.Property(e => e.IdNguoiDung)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_NguoiDung");
            entity.Property(e => e.ThoiGianCapNhatGh)
                .HasColumnType("datetime")
                .HasColumnName("ThoiGianCapNhatGH");
            entity.Property(e => e.ThoiGianTaoGh)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ThoiGianTaoGH");

            entity.HasOne(d => d.IdNguoiDungNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.IdNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK3_ND_GH");
        });

        modelBuilder.Entity<MaGiamGium>(entity =>
        {
            entity.HasKey(e => e.IdMaGiamGia).HasName("pk_MaGiamGia");

            entity.Property(e => e.IdMaGiamGia)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_MaGiamGia");
            entity.Property(e => e.GiaGiamToiDa).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaTriGiam).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaTriToiThieu).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.LoaiGiam).HasMaxLength(3);
            entity.Property(e => e.ThoiGianBatDau).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianKetThuc).HasColumnType("datetime");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.IdNguoiDung).HasName("PK__NguoiDun__5282D3EB88980677");

            entity.ToTable("NguoiDung");

            entity.Property(e => e.IdNguoiDung)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_NguoiDung");
            entity.Property(e => e.DiaChi).HasMaxLength(1);
            entity.Property(e => e.Email)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(3);
            entity.Property(e => e.HoTen).HasMaxLength(1);
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.IdSanPham).HasName("PK__SanPham__617EA392C31436A7");

            entity.ToTable("SanPham", tb => tb.HasTrigger("trg_SanPham_UpdateTimestamp"));

            entity.Property(e => e.IdSanPham)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_SanPham");
            entity.Property(e => e.GiaNiemYet).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IdDanhMucSanPham)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_DanhMucSanPham");
            entity.Property(e => e.ThoiGianCapNhat).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianTaoSp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ThoiGianTaoSP");
            entity.Property(e => e.TrangThaiSp).HasColumnName("TrangThaiSP");

            entity.HasOne(d => d.IdDanhMucSanPhamNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.IdDanhMucSanPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK1_DM_SP");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.IdTaiKhoan).HasName("PK__TaiKhoan__0E3EC2101597A738");

            entity.ToTable("TaiKhoan");

            entity.Property(e => e.IdTaiKhoan)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_TaiKhoan");
            entity.Property(e => e.IdNguoiDung)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_NguoiDung");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.VaiTro).HasMaxLength(1);

            entity.HasOne(d => d.IdNguoiDungNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.IdNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK2_ND_TK");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
