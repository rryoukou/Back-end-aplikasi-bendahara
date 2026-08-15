using System.ComponentModel.DataAnnotations;

namespace be.Models
{
    public class HariLibur
    {
        [Key]
        public int Id { get; set; }

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
