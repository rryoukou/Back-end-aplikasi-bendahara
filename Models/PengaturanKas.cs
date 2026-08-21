using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace be.Models
{
    public class PengaturanKas
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Isolasi data — pengaturan kas milik bendahara mana.</summary>
        [Required]
        public Guid BendaharaId { get; set; }

        [ForeignKey(nameof(BendaharaId))]
        public Bendahara? Bendahara { get; set; }

        // Tipe Jadwal: "Harian", "Mingguan", "Bulanan"
        [Required]
        public string TipeJadwal { get; set; } = "Mingguan";

        [Required]
        public decimal NominalKas { get; set; } = 0;
    }
}
