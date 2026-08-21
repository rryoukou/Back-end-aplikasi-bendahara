using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace be.Models
{
    public class IuranKhusus
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Isolasi data — event iuran ini milik bendahara mana.</summary>
        [Required]
        public Guid BendaharaId { get; set; }

        [ForeignKey(nameof(BendaharaId))]
        public Bendahara? Bendahara { get; set; }

        /// <summary>Judul/nama event iuran (ex: "Classmeeting", "Piknik Kelas").</summary>
        [Required]
        public string JudulIuran { get; set; } = string.Empty;

        /// <summary>Target nominal yang harus dibayar per siswa.</summary>
        [Required]
        public decimal TargetNominalPerSiswa { get; set; }

        public DateTime TanggalMulai { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Batas waktu pengumpulan iuran (opsional/nullable).
        /// Hanya berfungsi sebagai indikator deadline — pembayaran tetap
        /// dapat dicatat meskipun sudah melewati TanggalSelesai.
        /// </summary>
        public DateTime? TanggalSelesai { get; set; }

        public DateTime TanggalDibuat { get; set; } = DateTime.UtcNow;

        public string? Keterangan { get; set; }
    }
}
