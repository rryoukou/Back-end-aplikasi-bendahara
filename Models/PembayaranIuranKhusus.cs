using System.ComponentModel.DataAnnotations;

namespace be.Models
{
    public class PembayaranIuranKhusus
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid IuranKhususId { get; set; }
        public IuranKhusus? IuranKhusus { get; set; }

        [Required]
        public Guid SiswaId { get; set; }
        public Siswa? Siswa { get; set; }

        /// <summary>
        /// Total akumulasi cicilan yang sudah dibayarkan.
        /// </summary>
        public decimal TotalTerbayar { get; set; } = 0;

        public DateTime TanggalBayar { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Otomatis true jika TotalTerbayar >= TargetNominalPerSiswa.
        /// </summary>
        public bool IsLunas { get; set; } = false;
    }
}
