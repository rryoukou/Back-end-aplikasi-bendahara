using be.Data;
using be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PengaturanKasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PengaturanKasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PengaturanKas — ambil konfigurasi kas saat ini
        [HttpGet]
        public async Task<IActionResult> GetPengaturan()
        {
            var config = await _context.PengaturanKas.FirstOrDefaultAsync();
            if (config == null)
            {
                // Return default jika belum ada konfigurasi
                return Ok(new
                {
                    id = 0,
                    tipeJadwal = "Mingguan",
                    nominalKas = 5000
                });
            }
            return Ok(config);
        }

        // POST: api/PengaturanKas — simpan/update konfigurasi (upsert)
        [HttpPost]
        public async Task<IActionResult> SavePengaturan([FromBody] PengaturanKas dto)
        {
            var validTipe = new[] { "Harian", "Mingguan", "Bulanan" };
            if (!validTipe.Contains(dto.TipeJadwal))
                return BadRequest(new { message = "Tipe jadwal tidak valid! Gunakan: Harian, Mingguan, atau Bulanan." });

            if (dto.NominalKas <= 0)
                return BadRequest(new { message = "Nominal kas harus lebih dari 0!" });

            var config = await _context.PengaturanKas.FirstOrDefaultAsync();
            if (config == null)
            {
                _context.PengaturanKas.Add(dto);
            }
            else
            {
                config.TipeJadwal = dto.TipeJadwal;
                config.NominalKas = dto.NominalKas;
            }

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Pengaturan kas berhasil disimpan!",
                data = new
                {
                    tipeJadwal = dto.TipeJadwal,
                    nominalKas = dto.NominalKas
                }
            });
        }
    }
}
