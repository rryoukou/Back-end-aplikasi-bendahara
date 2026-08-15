using System.ComponentModel.DataAnnotations;

namespace be.Models
{
    public class PengaturanKas
    {
        [Key]
        public int Id { get; set; }

        // Tipe Jadwal: "Harian", "Mingguan", "Bulanan"
        [Required]
        public string TipeJadwal { get; set; } = "Mingguan";

        [Required]
        public decimal NominalKas { get; set; } = 0;
    }
}