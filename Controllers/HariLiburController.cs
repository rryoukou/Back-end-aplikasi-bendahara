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

        private Guid? GetBendaharaId()
        {
            if (Request.Headers.TryGetValue("X-Bendahara-Id", out var value) &&
                Guid.TryParse(value, out var id))
                return id;
            return null;
        }

        // GET: api/HariLibur
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bendaharaId = GetBendaharaId();
            if (bendaharaId == null)
                return BadRequest(new { message = "Header X-Bendahara-Id wajib disertakan." });

            var listLibur = await _context.HariLiburs
                .Where(h => h.BendaharaId == bendaharaId)
                .OrderBy(h => h.Tanggal)
                .ToListAsync();
            return Ok(listLibur);
        }

        // GET: api/HariLibur/is-libur?tanggal=2026-08-17
        [HttpGet("is-libur")]
        public async Task<IActionResult> IsLibur([FromQuery] DateTime tanggal)
        {
            var bendaharaId = GetBendaharaId();

            if (tanggal.DayOfWeek == DayOfWeek.Saturday || tanggal.DayOfWeek == DayOfWeek.Sunday)
                return Ok(new { isLibur = true, alasan = "Hari akhir pekan" });

            // Jika tidak ada header, hanya cek weekend
            if (bendaharaId == null)
                return Ok(new { isLibur = false, alasan = "" });

            var tanggalUtc = DateTime.SpecifyKind(tanggal.Date, DateTimeKind.Utc);
            var customLibur = await _context.HariLiburs
                .FirstOrDefaultAsync(h => h.BendaharaId == bendaharaId
                                       && h.Tanggal.Date == tanggalUtc.Date);

            if (customLibur != null)
                return Ok(new { isLibur = true, alasan = customLibur.Keterangan });

            return Ok(new { isLibur = false, alasan = "" });
        }

        // POST: api/HariLibur
        public class CreateHariLiburDto
        {
            public Guid BendaharaId { get; set; }
            public DateTime Tanggal { get; set; }
            public string Keterangan { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHariLiburDto dto)
        {
            if (dto.BendaharaId == Guid.Empty)
                return BadRequest(new { message = "BendaharaId wajib diisi." });

            if (string.IsNullOrWhiteSpace(dto.Keterangan))
                return BadRequest(new { message = "Keterangan libur tidak boleh kosong!" });

            var tanggalUtc = DateTime.SpecifyKind(dto.Tanggal.Date, DateTimeKind.Utc);

            bool isDuplikat = await _context.HariLiburs
                .AnyAsync(h => h.BendaharaId == dto.BendaharaId
                            && h.Tanggal.Date == tanggalUtc.Date);
            if (isDuplikat)
                return BadRequest(new { message = "Tanggal ini sudah terdaftar sebagai hari libur!" });

            var hariLibur = new HariLibur
            {
                BendaharaId = dto.BendaharaId,
                Tanggal = tanggalUtc,
                Keterangan = dto.Keterangan,
                IsCustomOverride = true
            };

            _context.HariLiburs.Add(hariLibur);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hari libur berhasil ditambahkan!", data = hariLibur });
        }

        // DELETE: api/HariLibur/{id}?bendaharaId={id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] Guid bendaharaId)
        {
            if (bendaharaId == Guid.Empty)
                return BadRequest(new { message = "bendaharaId wajib disertakan." });

            var item = await _context.HariLiburs
                .FirstOrDefaultAsync(h => h.Id == id && h.BendaharaId == bendaharaId);
            if (item == null) return NotFound(new { message = "Data libur tidak ditemukan" });

            _context.HariLiburs.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hari libur berhasil dihapus!" });
        }
    }
}
