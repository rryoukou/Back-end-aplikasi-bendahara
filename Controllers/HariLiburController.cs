using be.Data;
using be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HariLiburController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HariLiburController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/HariLibur
        // Mengembalikan semua hari libur custom yang tersimpan di DB.
        // Sabtu & Minggu dianggap libur di sisi frontend, tidak perlu disimpan di DB.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var listLibur = await _context.HariLiburs
                .OrderBy(h => h.Tanggal)
                .ToListAsync();
            return Ok(listLibur);
        }

        // GET: api/HariLibur/is-libur?tanggal=2026-08-17
        // Mengecek apakah suatu tanggal adalah hari libur (custom DB atau Sabtu/Minggu)
        [HttpGet("is-libur")]
        public async Task<IActionResult> IsLibur([FromQuery] DateTime tanggal)
        {
            // Sabtu (6) dan Minggu (0) selalu libur
            if (tanggal.DayOfWeek == DayOfWeek.Saturday || tanggal.DayOfWeek == DayOfWeek.Sunday)
                return Ok(new { isLibur = true, alasan = "Hari akhir pekan" });

            var tanggalUtc = DateTime.SpecifyKind(tanggal.Date, DateTimeKind.Utc);
            var customLibur = await _context.HariLiburs
                .FirstOrDefaultAsync(h => h.Tanggal.Date == tanggalUtc.Date);

            if (customLibur != null)
                return Ok(new { isLibur = true, alasan = customLibur.Keterangan });

            return Ok(new { isLibur = false, alasan = "" });
        }

        // POST: api/HariLibur
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HariLibur hariLibur)
        {
            if (string.IsNullOrWhiteSpace(hariLibur.Keterangan))
                return BadRequest(new { message = "Keterangan libur tidak boleh kosong!" });

            hariLibur.Tanggal = DateTime.SpecifyKind(hariLibur.Tanggal.Date, DateTimeKind.Utc);
            hariLibur.IsCustomOverride = true; // Semua input manual = custom override

            // Cek duplikat tanggal
            bool isDuplikat = await _context.HariLiburs
                .AnyAsync(h => h.Tanggal.Date == hariLibur.Tanggal.Date);
            if (isDuplikat)
                return BadRequest(new { message = "Tanggal ini sudah terdaftar sebagai hari libur!" });

            _context.HariLiburs.Add(hariLibur);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hari libur berhasil ditambahkan!", data = hariLibur });
        }

        // DELETE: api/HariLibur/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.HariLiburs.FindAsync(id);
            if (item == null) return NotFound(new { message = "Data libur tidak ditemukan" });

            _context.HariLiburs.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hari libur berhasil dihapus!" });
        }
    }
}
