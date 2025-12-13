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

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<DanhGiaSanPham> DanhGiaSanPhams { get; set; }

    public virtual DbSet<DanhMucSanPham> DanhMucSanPhams { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<DonViVanChuyen> DonViVanChuyens { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<MaGiamGium> MaGiamGia { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=ONION\\SQLEXPRESS;Database=ScentoryPTUD;Integrated Security=true;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.IdBlog).HasName("PK__Blog__F1F67AB860B8F2CD");

            entity.ToTable("Blog");

            entity.Property(e => e.IdBlog)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_Blog");
            entity.Property(e => e.Alias).HasMaxLength(255);
            entity.Property(e => e.DanhMucBlog).HasMaxLength(50);
            entity.Property(e => e.MetaDesc).HasMaxLength(255);
            entity.Property(e => e.MetaKey).HasMaxLength(255);
            entity.Property(e => e.NoiDungNgan).HasMaxLength(255);
            entity.Property(e => e.TenBlog).HasMaxLength(255);
            entity.Property(e => e.ThoiGianCapNhatBlog).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianTaoBlog)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => new { e.IdDonHang, e.IdSanPham }).HasName("PK__ChiTietD__AFA0CC0043BDF397");

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
            entity.HasKey(e => new { e.IdGioHang, e.IdSanPham }).HasName("PK__ChiTietG__F624402EEB07E92E");

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
            entity.HasKey(e => e.IdDanhGia).HasName("PK__DanhGiaS__6C898AE1F93A7132");

            entity.ToTable("DanhGiaSanPham");

            entity.Property(e => e.IdDanhGia)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_DanhGia");
            entity.Property(e => e.IdKhachHang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_KhachHang");
            entity.Property(e => e.IdSanPham)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_SanPham");
            entity.Property(e => e.ThoiGianDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdKhachHangNavigation).WithMany(p => p.DanhGiaSanPhams)
                .HasForeignKey(d => d.IdKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK10_KH_DG");

            entity.HasOne(d => d.IdSanPhamNavigation).WithMany(p => p.DanhGiaSanPhams)
                .HasForeignKey(d => d.IdSanPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK9_SP_DG");
        });

        modelBuilder.Entity<DanhMucSanPham>(entity =>
        {
            entity.HasKey(e => e.IdDanhMucSanPham).HasName("PK__DanhMucS__95D6C4EF87CE35DE");

            entity.ToTable("DanhMucSanPham");

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
            entity.HasKey(e => e.IdDonHang).HasName("PK__DonHang__99B72639077C4BC6");

            entity.ToTable("DonHang");

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
            entity.Property(e => e.IdKhachHang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_KhachHang");
            entity.Property(e => e.IdMaGiamGia)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_MaGiamGia");
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

            entity.HasOne(d => d.IdKhachHangNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.IdKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK6_KH_GH");

            entity.HasOne(d => d.IdMaGiamGiaNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.IdMaGiamGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK12_DH_MGG");
        });

        modelBuilder.Entity<DonViVanChuyen>(entity =>
        {
            entity.HasKey(e => e.IdDonViVanChuyen).HasName("PK__DonViVan__54421B64F12C0592");

            entity.ToTable("DonViVanChuyen");

            entity.Property(e => e.IdDonViVanChuyen)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_DonViVanChuyen");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.IdGioHang).HasName("PK__GioHang__C033AA17B2C09F1C");

            entity.ToTable("GioHang");

            entity.Property(e => e.IdGioHang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_GioHang");
            entity.Property(e => e.IdKhachHang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_KhachHang");
            entity.Property(e => e.ThoiGianCapNhatGh)
                .HasColumnType("datetime")
                .HasColumnName("ThoiGianCapNhatGH");
            entity.Property(e => e.ThoiGianTaoGh)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ThoiGianTaoGH");

            entity.HasOne(d => d.IdKhachHangNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.IdKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK3_KH_GH");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.IdKhachHang).HasName("pk_KhachHang");

            entity.ToTable("KhachHang");

            entity.Property(e => e.IdKhachHang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_KhachHang");
            entity.Property(e => e.DiaChi).HasMaxLength(100);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(3);
            entity.Property(e => e.HoTen).HasMaxLength(50);
            entity.Property(e => e.IdTaiKhoan)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_TaiKhoan");
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.KhachHangs)
                .HasForeignKey(d => d.IdTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK2_TK_KH");
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

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.IdSanPham).HasName("PK__SanPham__617EA3922F572B66");

            entity.ToTable("SanPham");

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
            entity.HasKey(e => e.IdTaiKhoan).HasName("PK__TaiKhoan__0E3EC210B37C8C0D");

            entity.ToTable("TaiKhoan");

            entity.Property(e => e.IdTaiKhoan)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID_TaiKhoan");
            entity.Property(e => e.MatKhau).IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VaiTro).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
