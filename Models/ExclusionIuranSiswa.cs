using System.ComponentModel.DataAnnotations;

namespace be.Models
{
    /// <summary>
    /// Menyimpan daftar siswa yang DIKECUALIKAN dari event iuran khusus tertentu.
    /// Jika SiswaId ada di tabel ini untuk suatu IuranKhususId, 
    /// maka siswa tersebut tidak diwajibkan membayar event itu.
    /// </summary>
    public class ExclusionIuranSiswa
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid IuranKhususId { get; set; }
        public IuranKhusus? IuranKhusus { get; set; }

        [Required]
        public Guid SiswaId { get; set; }
        public Siswa? Siswa { get; set; }

        public DateTime TanggalDikecualikan { get; set; } = DateTime.UtcNow;
        public string? Alasan { get; set; }
    }
}
