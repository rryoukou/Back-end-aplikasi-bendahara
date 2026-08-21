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

        private Guid? GetBendaharaId()
        {
            if (Request.Headers.TryGetValue("X-Bendahara-Id", out var value) &&
                Guid.TryParse(value, out var id))
                return id;
            return null;
        }

        // GET: api/PengaturanKas
        [HttpGet]
        public async Task<IActionResult> GetPengaturan()
        {
            var bendaharaId = GetBendaharaId();
            if (bendaharaId == null)
                return BadRequest(new { message = "Header X-Bendahara-Id wajib disertakan." });

            var config = await _context.PengaturanKas
                .FirstOrDefaultAsync(p => p.BendaharaId == bendaharaId);

            if (config == null)
            {
                return Ok(new
                {
                    id = 0,
                    tipeJadwal = "Mingguan",
                    nominalKas = 5000
                });
            }
            return Ok(config);
        }

        // POST: api/PengaturanKas — upsert per bendahara
        public class SavePengaturanDto
        {
            public Guid BendaharaId { get; set; }
            public string TipeJadwal { get; set; } = "Mingguan";
            public decimal NominalKas { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SavePengaturan([FromBody] SavePengaturanDto dto)
        {
            if (dto.BendaharaId == Guid.Empty)
                return BadRequest(new { message = "BendaharaId wajib diisi." });

            var validTipe = new[] { "Harian", "Mingguan", "Bulanan" };
            if (!validTipe.Contains(dto.TipeJadwal))
                return BadRequest(new { message = "Tipe jadwal tidak valid! Gunakan: Harian, Mingguan, atau Bulanan." });

            if (dto.NominalKas <= 0)
                return BadRequest(new { message = "Nominal kas harus lebih dari 0!" });

            var config = await _context.PengaturanKas
                .FirstOrDefaultAsync(p => p.BendaharaId == dto.BendaharaId);

            if (config == null)
            {
                _context.PengaturanKas.Add(new PengaturanKas
                {
                    BendaharaId = dto.BendaharaId,
                    TipeJadwal = dto.TipeJadwal,
                    NominalKas = dto.NominalKas
                });
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
                data = new { tipeJadwal = dto.TipeJadwal, nominalKas = dto.NominalKas }
            });
        }
    }
}
