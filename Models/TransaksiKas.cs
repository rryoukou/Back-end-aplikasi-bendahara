using System.ComponentModel.DataAnnotations;

namespace be.Models
{
    public class TransaksiKas
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SiswaId { get; set; }

        public Siswa? Siswa { get; set; }

        [Required]
        public decimal Nominal { get; set; }

        [Required]
        public DateTime TanggalBayar { get; set; } = DateTime.UtcNow;

        public string Keterangan { get; set; } = "Pembayaran Kas Rutin";

        /// <summary>
        /// Bulan transaksi ini berlaku (format: yyyy-MM, contoh: "2026-08").
        /// Digunakan untuk validasi matrix checkbox agar tidak double-bayar.
        /// </summary>
        public string? BulanPeriode { get; set; }

        /// <summary>
        /// Minggu ke berapa dalam bulan (1-4).
        /// Diisi jika TipeJadwal = "Mingguan".
        /// </summary>
        public int? MingguKe { get; set; }

        /// <summary>
        /// Tanggal spesifik periode pembayaran (yyyy-MM-dd).
        /// Diisi jika TipeJadwal = "Harian" agar bisa cek hari mana sudah dibayar.
        /// </summary>
        public string? TanggalBayarSpec { get; set; }
    }
}
