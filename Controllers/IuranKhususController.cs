using be.Data;
using be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IuranKhususController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IuranKhususController(AppDbContext context)
        {
            _context = context;
        }

        // DTO buat event
        public class CreateEventDto
        {
            public string JudulIuran { get; set; } = string.Empty;
            public decimal TargetNominalPerSiswa { get; set; }
            public DateTime TanggalMulai { get; set; } = DateTime.UtcNow;
            public string? Keterangan { get; set; }
            public List<Guid>? SiswaExcluded { get; set; } // ID siswa yang dikecualikan
        }

        // DTO bayar cicilan
        public class BayarIuranDto
        {
            public Guid IuranKhususId { get; set; }
            public Guid SiswaId { get; set; }
            public decimal NominalBayar { get; set; }
        }

        // 1. GET ALL EVENT IURAN KHUSUS
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.IuranKhususs
                .OrderByDescending(i => i.TanggalDibuat)
                .ToListAsync();
            return Ok(events);
        }

        // 2. POST CREATE EVENT BARU
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.JudulIuran) || dto.TargetNominalPerSiswa <= 0)
                return BadRequest(new { message = "Judul iuran dan target nominal harus diisi!" });

            var iuran = new IuranKhusus
            {
                Id = Guid.NewGuid(),
                JudulIuran = dto.JudulIuran,
                TargetNominalPerSiswa = dto.TargetNominalPerSiswa,
                TanggalMulai = DateTime.SpecifyKind(dto.TanggalMulai, DateTimeKind.Utc),
                TanggalDibuat = DateTime.UtcNow,
                Keterangan = dto.Keterangan
            };

            _context.IuranKhususs.Add(iuran);

            // Tambahkan exclusion untuk siswa yang dikecualikan
            if (dto.SiswaExcluded != null && dto.SiswaExcluded.Any())
            {
                foreach (var siswaId in dto.SiswaExcluded)
                {
                    _context.ExclusionIuranSiswas.Add(new ExclusionIuranSiswa
                    {
                        Id = Guid.NewGuid(),
                        IuranKhususId = iuran.Id,
                        SiswaId = siswaId,
                        TanggalDikecualikan = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event iuran berhasil dibuat!", data = iuran });
        }

        // 3. GET DETAIL PEMBAYARAN EVENT PER SISWA (dengan info exclusion)
        [HttpGet("{eventId}/status-siswa")]
        public async Task<IActionResult> GetStatusPembayaran(Guid eventId)
        {
            var eventInfo = await _context.IuranKhususs.FindAsync(eventId);
            if (eventInfo == null) return NotFound(new { message = "Event tidak ditemukan" });

            var listSiswa = await _context.Siswas
                .Where(s => s.IsActive)
                .OrderBy(s => s.NoAbsen)
                .ToListAsync();

            var listBayar = await _context.PembayaranIuranKhususs
                .Where(p => p.IuranKhususId == eventId)
                .ToListAsync();

            var listExclusion = await _context.ExclusionIuranSiswas
                .Where(e => e.IuranKhususId == eventId)
                .Select(e => e.SiswaId)
                .ToListAsync();

            var result = listSiswa.Select(s =>
            {
                bool isDikecualikan = listExclusion.Contains(s.Id);
                var bayar = listBayar.FirstOrDefault(b => b.SiswaId == s.Id);
                var totalTerbayar = bayar?.TotalTerbayar ?? 0;
                var isLunas = totalTerbayar >= eventInfo.TargetNominalPerSiswa;

                return new
                {
                    SiswaId = s.Id,
                    NamaSiswa = s.Nama,
                    NoAbsen = s.NoAbsen,
                    TotalTerbayar = totalTerbayar,
                    TargetNominal = isDikecualikan ? 0 : eventInfo.TargetNominalPerSiswa,
                    IsLunas = isDikecualikan ? true : isLunas,
                    IsDikecualikan = isDikecualikan
                };
            });

            return Ok(result);
        }

        // 4. POST CATAT PEMBAYARAN / CICILAN IURAN SISWA
        [HttpPost("bayar")]
        public async Task<IActionResult> BayarIuran([FromBody] BayarIuranDto dto)
        {
            var eventInfo = await _context.IuranKhususs.FindAsync(dto.IuranKhususId);
            if (eventInfo == null) return NotFound(new { message = "Event tidak ditemukan" });

            if (dto.NominalBayar <= 0)
                return BadRequest(new { message = "Nominal bayar harus lebih dari 0!" });

            // Cek apakah siswa dikecualikan
            bool isDikecualikan = await _context.ExclusionIuranSiswas
                .AnyAsync(e => e.IuranKhususId == dto.IuranKhususId && e.SiswaId == dto.SiswaId);
            if (isDikecualikan)
                return BadRequest(new { message = "Siswa ini dikecualikan dari event iuran!" });

            // Upsert: tambah cicilan atau buat baru
            var bayar = await _context.PembayaranIuranKhususs
                .FirstOrDefaultAsync(p => p.IuranKhususId == dto.IuranKhususId && p.SiswaId == dto.SiswaId);

            if (bayar == null)
            {
                bayar = new PembayaranIuranKhusus
                {
                    Id = Guid.NewGuid(),
                    IuranKhususId = dto.IuranKhususId,
                    SiswaId = dto.SiswaId,
                    TotalTerbayar = dto.NominalBayar,
                    TanggalBayar = DateTime.UtcNow,
                    IsLunas = dto.NominalBayar >= eventInfo.TargetNominalPerSiswa
                };
                _context.PembayaranIuranKhususs.Add(bayar);
            }
            else
            {
                bayar.TotalTerbayar += dto.NominalBayar;
                bayar.TanggalBayar = DateTime.UtcNow;
                bayar.IsLunas = bayar.TotalTerbayar >= eventInfo.TargetNominalPerSiswa;
                _context.PembayaranIuranKhususs.Update(bayar);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Pembayaran berhasil dicatat!",
                totalTerbayar = bayar.TotalTerbayar,
                isLunas = bayar.IsLunas
            });
        }

        // 5. POST TOGGLE EXCLUSION SISWA DARI EVENT
        [HttpPost("{eventId}/exclusion")]
        public async Task<IActionResult> ToggleExclusion(Guid eventId, [FromBody] ToggleExclusionDto dto)
        {
            var eventInfo = await _context.IuranKhususs.FindAsync(eventId);
            if (eventInfo == null) return NotFound(new { message = "Event tidak ditemukan" });

            var existing = await _context.ExclusionIuranSiswas
                .FirstOrDefaultAsync(e => e.IuranKhususId == eventId && e.SiswaId == dto.SiswaId);

            if (existing != null)
            {
                _context.ExclusionIuranSiswas.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Siswa berhasil diikutsertakan kembali!", isDikecualikan = false });
            }
            else
            {
                _context.ExclusionIuranSiswas.Add(new ExclusionIuranSiswa
                {
                    Id = Guid.NewGuid(),
                    IuranKhususId = eventId,
                    SiswaId = dto.SiswaId,
                    TanggalDikecualikan = DateTime.UtcNow,
                    Alasan = dto.Alasan
                });
                await _context.SaveChangesAsync();
                return Ok(new { message = "Siswa berhasil dikecualikan!", isDikecualikan = true });
            }
        }
    }

    public class ToggleExclusionDto
    {
        public Guid SiswaId { get; set; }
        public string? Alasan { get; set; }
    }
}
