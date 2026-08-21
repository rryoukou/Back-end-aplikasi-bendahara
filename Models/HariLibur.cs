using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace be.Models
{
    public class HariLibur
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Isolasi data — hari libur ini milik bendahara mana.</summary>
        [Required]
        public Guid BendaharaId { get; set; }

        [ForeignKey(nameof(BendaharaId))]
        public Bendahara? Bendahara { get; set; }

        [Required]
        public DateTime Tanggal { get; set; }

        public string Keterangan { get; set; } = string.Empty;

        /// <summary>
        /// false = libur nasional/pendidikan (default)
        /// true  = libur khusus yang ditambahkan manual oleh bendahara
        /// </summary>
        public bool IsCustomOverride { get; set; } = false;
    }
}
