using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace be.Models
{
    public class Siswa
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Isolasi data — siswa ini milik bendahara mana.</summary>
        [Required]
        public Guid BendaharaId { get; set; }

        [ForeignKey(nameof(BendaharaId))]
        public Bendahara? Bendahara { get; set; }

        [Required]
        public string Nama { get; set; } = string.Empty;

        [Required]
        public int NoAbsen { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
