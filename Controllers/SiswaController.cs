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

        // GET: api/Siswa — diurutkan ascending berdasarkan NoAbsen
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var siswas = await _context.Siswas
                .Where(s => s.IsActive)
                .OrderBy(s => s.NoAbsen)
                .ToListAsync();
            return Ok(siswas);
        }

        // POST: api/Siswa
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Siswa siswa)
        {
            if (string.IsNullOrWhiteSpace(siswa.Nama))
                return BadRequest(new { message = "Nama siswa tidak boleh kosong!" });

            if (siswa.NoAbsen <= 0)
                return BadRequest(new { message = "Nomor absen harus diisi dan bernilai positif!" });

            // Cek duplikat nomor absen
            bool isNoAbsenDuplikat = await _context.Siswas
                .AnyAsync(s => s.NoAbsen == siswa.NoAbsen && s.IsActive);
            if (isNoAbsenDuplikat)
                return BadRequest(new { message = $"Nomor absen {siswa.NoAbsen} sudah digunakan siswa lain!" });

            siswa.Id = Guid.NewGuid();
            siswa.IsActive = true;

            _context.Siswas.Add(siswa);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Siswa berhasil ditambahkan!", data = siswa });
        }

        // PUT: api/Siswa/{id} — update nama atau nomor absen
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Siswa dto)
        {
            var siswa = await _context.Siswas.FindAsync(id);
            if (siswa == null) return NotFound(new { message = "Siswa tidak ditemukan" });

            if (dto.NoAbsen > 0)
            {
                bool isDuplikat = await _context.Siswas
                    .AnyAsync(s => s.NoAbsen == dto.NoAbsen && s.IsActive && s.Id != id);
                if (isDuplikat)
                    return BadRequest(new { message = $"Nomor absen {dto.NoAbsen} sudah digunakan siswa lain!" });
                siswa.NoAbsen = dto.NoAbsen;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nama))
                siswa.Nama = dto.Nama;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Data siswa berhasil diupdate!", data = siswa });
        }

        // DELETE: api/Siswa/{id} — soft delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var siswa = await _context.Siswas.FindAsync(id);
            if (siswa == null) return NotFound(new { message = "Siswa tidak ditemukan" });

            siswa.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Siswa berhasil dihapus!" });
        }
    }
}
