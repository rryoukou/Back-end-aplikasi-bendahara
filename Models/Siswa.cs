using System.ComponentModel.DataAnnotations;

namespace be.Models
{
    public class Siswa
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Nama { get; set; } = string.Empty;

        /// <summary>
        /// Nomor absen siswa di dalam kelas (wajib diisi, dipakai sebagai urutan list).
        /// </summary>
        [Required]
        public int NoAbsen { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
