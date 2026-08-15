using Microsoft.EntityFrameworkCore;
using be.Models;

namespace be.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Bendahara> Bendaharas { get; set; }
        public DbSet<PengaturanKas> PengaturanKas { get; set; }
        public DbSet<Siswa> Siswas { get; set; }
        public DbSet<HariLibur> HariLiburs { get; set; }
        public DbSet<TransaksiKas> TransaksiKas { get; set; }
        public DbSet<TransaksiLain> TransaksiLains { get; set; }
        public DbSet<IuranKhusus> IuranKhususs { get; set; }
        public DbSet<PembayaranIuranKhusus> PembayaranIuranKhususs { get; set; }
        public DbSet<ExclusionIuranSiswa> ExclusionIuranSiswas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Pastikan kombinasi IuranKhususId + SiswaId unik di tabel exclusion
            modelBuilder.Entity<ExclusionIuranSiswa>()
                .HasIndex(e => new { e.IuranKhususId, e.SiswaId })
                .IsUnique();

            // Pastikan kombinasi IuranKhususId + SiswaId unik di tabel pembayaran iuran
            modelBuilder.Entity<PembayaranIuranKhusus>()
                .HasIndex(p => new { p.IuranKhususId, p.SiswaId })
                .IsUnique();
        }
    }
}
