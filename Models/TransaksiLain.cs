using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace be.Models
{
    public class TransaksiLain
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Isolasi data — transaksi ini milik bendahara mana.</summary>
        [Required]
        public Guid BendaharaId { get; set; }

        [ForeignKey(nameof(BendaharaId))]
        public Bendahara? Bendahara { get; set; }

        [Required]
        public string Judul { get; set; } = string.Empty;

        /// <summary>Tipe: "Pemasukan" atau "Pengeluaran"</summary>
        [Required]
        public string Tipe { get; set; } = "Pengeluaran";

        [Required]
        public decimal Nominal { get; set; }

        public DateTime Tanggal { get; set; } = DateTime.UtcNow;

        public string? Keterangan { get; set; }

        /// <summary>Gambar bukti transaksi dalam format Base64 string (nullable).</summary>
        public string? BuktiFoto { get; set; }
    }
}
