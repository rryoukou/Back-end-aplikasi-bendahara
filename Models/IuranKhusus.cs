using System.ComponentModel.DataAnnotations;

namespace be.Models
{
    public class IuranKhusus
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Judul/nama event iuran (ex: "Classmeeting", "Piknik Kelas", "Baju Angkatan").
        /// </summary>
        [Required]
        public string JudulIuran { get; set; } = string.Empty;

        /// <summary>
        /// Target nominal yang harus dibayar per siswa.
        /// </summary>
        [Required]
        public decimal TargetNominalPerSiswa { get; set; }

        public DateTime TanggalMulai { get; set; } = DateTime.UtcNow;

        public DateTime TanggalDibuat { get; set; } = DateTime.UtcNow;

        public string? Keterangan { get; set; }
    }
}
