using be.Data;
using be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiswaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SiswaController(AppDbContext context)
        {
            _context = context;
        }

        // Ambil BendaharaId dari header X-Bendahara-Id
        private Guid? GetBendaharaId()
        {
            if (Request.Headers.TryGetValue("X-Bendahara-Id", out var value) &&
                Guid.TryParse(value, out var id))
                return id;
            return null;
        }

        // GET: api/Siswa — diurutkan ascending berdasarkan NoAbsen
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bendaharaId = GetBendaharaId();
            if (bendaharaId == null)
                return BadRequest(new { message = "Header X-Bendahara-Id wajib disertakan." });

            var siswas = await _context.Siswas
                .Where(s => s.IsActive && s.BendaharaId == bendaharaId)
                .OrderBy(s => s.NoAbsen)
                .ToListAsync();
            return Ok(siswas);
        }

        // POST: api/Siswa
        public class CreateSiswaDto
        {
            public Guid BendaharaId { get; set; }
            public string Nama { get; set; } = string.Empty;
            public int NoAbsen { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSiswaDto dto)
        {
            if (dto.BendaharaId == Guid.Empty)
                return BadRequest(new { message = "BendaharaId wajib diisi." });

            if (string.IsNullOrWhiteSpace(dto.Nama))
                return BadRequest(new { message = "Nama siswa tidak boleh kosong!" });

            if (dto.NoAbsen <= 0)
                return BadRequest(new { message = "Nomor absen harus diisi dan bernilai positif!" });

            // Cek duplikat nomor absen dalam kelas bendahara yang sama
            bool isNoAbsenDuplikat = await _context.Siswas
                .AnyAsync(s => s.NoAbsen == dto.NoAbsen && s.IsActive
                            && s.BendaharaId == dto.BendaharaId);
            if (isNoAbsenDuplikat)
                return BadRequest(new { message = $"Nomor absen {dto.NoAbsen} sudah digunakan siswa lain!" });

            var siswa = new Siswa
            {
                Id = Guid.NewGuid(),
                BendaharaId = dto.BendaharaId,
                Nama = dto.Nama,
                NoAbsen = dto.NoAbsen,
                IsActive = true
            };

            _context.Siswas.Add(siswa);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Siswa berhasil ditambahkan!", data = siswa });
        }

        // PUT: api/Siswa/{id}
        public class UpdateSiswaDto
        {
            public Guid BendaharaId { get; set; }
            public string? Nama { get; set; }
            public int NoAbsen { get; set; }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSiswaDto dto)
        {
            var siswa = await _context.Siswas
                .FirstOrDefaultAsync(s => s.Id == id && s.BendaharaId == dto.BendaharaId);
            if (siswa == null) return NotFound(new { message = "Siswa tidak ditemukan" });

            if (dto.NoAbsen > 0)
            {
                bool isDuplikat = await _context.Siswas
                    .AnyAsync(s => s.NoAbsen == dto.NoAbsen && s.IsActive
                               && s.BendaharaId == dto.BendaharaId && s.Id != id);
                if (isDuplikat)
                    return BadRequest(new { message = $"Nomor absen {dto.NoAbsen} sudah digunakan siswa lain!" });
                siswa.NoAbsen = dto.NoAbsen;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nama))
                siswa.Nama = dto.Nama;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Data siswa berhasil diupdate!", data = siswa });
        }

        // DELETE: api/Siswa/{id}?bendaharaId={id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid bendaharaId)
        {
            if (bendaharaId == Guid.Empty)
                return BadRequest(new { message = "bendaharaId wajib disertakan." });

            var siswa = await _context.Siswas
                .FirstOrDefaultAsync(s => s.Id == id && s.BendaharaId == bendaharaId);
            if (siswa == null) return NotFound(new { message = "Siswa tidak ditemukan" });

            siswa.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Siswa berhasil dihapus!" });
        }
    }
}
