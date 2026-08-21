using be.Data;
using be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransaksiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransaksiController(AppDbContext context)
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

        // ── DTOs ──────────────────────────────────────────────────────────────

        public class BayarKasBatchDto
        {
            public Guid BendaharaId { get; set; }
            public List<Guid> SiswaIds { get; set; } = new();
            public decimal Nominal { get; set; }
            public string? BulanPeriode { get; set; }
            public int? MingguKe { get; set; }
            public string? TanggalBayarSpec { get; set; }
        }

        public class TransaksiLainDto
        {
            public Guid BendaharaId { get; set; }
            public string Judul { get; set; } = string.Empty;
            public string Tipe { get; set; } = "Pengeluaran";
            public decimal Nominal { get; set; }
            public string? Keterangan { get; set; }
            public string? BuktiFoto { get; set; }
        }

        public class RiwayatItemDto
        {
            public Guid Id { get; set; }
            public string Judul { get; set; } = string.Empty;
            public string Tipe { get; set; } = string.Empty;
            public decimal Nominal { get; set; }
            public DateTime Tanggal { get; set; }
            public string? Keterangan { get; set; }
            public string? BuktiFotoUrl { get; set; }
        }

        // ── POST: bayar-kas-batch ─────────────────────────────────────────────

        [HttpPost("bayar-kas-batch")]
        public async Task<IActionResult> BayarKasBatch([FromBody] BayarKasBatchDto dto)
        {
            if (dto.BendaharaId == Guid.Empty)
                return BadRequest(new { message = "BendaharaId wajib diisi." });
            if (dto.SiswaIds == null || !dto.SiswaIds.Any())
                return BadRequest(new { message = "Pilih minimal 1 siswa!" });

            var listTransaksi = new List<TransaksiKas>();
            foreach (var siswaId in dto.SiswaIds)
            {
                if (!string.IsNullOrEmpty(dto.BulanPeriode))
                {
                    bool sudahBayar = false;
                    if (dto.TanggalBayarSpec != null)
                    {
                        sudahBayar = await _context.TransaksiKas.AnyAsync(t =>
                            t.BendaharaId == dto.BendaharaId &&
                            t.SiswaId == siswaId &&
                            t.BulanPeriode == dto.BulanPeriode &&
                            t.TanggalBayarSpec == dto.TanggalBayarSpec);
                    }
                    else if (dto.MingguKe != null)
                    {
                        sudahBayar = await _context.TransaksiKas.AnyAsync(t =>
                            t.BendaharaId == dto.BendaharaId &&
                            t.SiswaId == siswaId &&
                            t.BulanPeriode == dto.BulanPeriode &&
                            t.MingguKe == dto.MingguKe);
                    }
                    if (sudahBayar) continue;
                }

                listTransaksi.Add(new TransaksiKas
                {
                    BendaharaId = dto.BendaharaId,
                    SiswaId = siswaId,
                    Nominal = dto.Nominal,
                    TanggalBayar = DateTime.UtcNow,
                    Keterangan = "Pembayaran Kas Rutin",
                    BulanPeriode = dto.BulanPeriode,
                    MingguKe = dto.MingguKe,
                    TanggalBayarSpec = dto.TanggalBayarSpec
                });
            }

            if (!listTransaksi.Any())
                return Ok(new { message = "Semua siswa yang dipilih sudah membayar periode ini." });

            _context.TransaksiKas.AddRange(listTransaksi);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Berhasil mencatat kas untuk {listTransaksi.Count} siswa!" });
        }

        // ── GET: status-kas ───────────────────────────────────────────────────

        [HttpGet("status-kas")]
        public async Task<IActionResult> GetStatusKas(
            [FromQuery] string bulanPeriode,
            [FromQuery] string tipeJadwal = "Mingguan")
        {
            var bendaharaId = GetBendaharaId();
            if (bendaharaId == null)
                return BadRequest(new { message = "Header X-Bendahara-Id wajib disertakan." });

            var listSiswa = await _context.Siswas
                .Where(s => s.IsActive && s.BendaharaId == bendaharaId)
                .OrderBy(s => s.NoAbsen)
                .ToListAsync();

            var transaksiPeriode = await _context.TransaksiKas
                .Where(t => t.BendaharaId == bendaharaId && t.BulanPeriode == bulanPeriode)
                .ToListAsync();

            if (tipeJadwal == "Mingguan")
            {
                var result = listSiswa.Select(s => new
                {
                    SiswaId = s.Id,
                    NamaSiswa = s.Nama,
                    NoAbsen = s.NoAbsen,
                    MingguSudahBayar = transaksiPeriode
                        .Where(t => t.SiswaId == s.Id && t.MingguKe != null)
                        .Select(t => t.MingguKe!.Value).ToList()
                });
                return Ok(result);
            }
            else if (tipeJadwal == "Harian")
            {
                var result = listSiswa.Select(s => new
                {
                    SiswaId = s.Id,
                    NamaSiswa = s.Nama,
                    NoAbsen = s.NoAbsen,
                    TanggalSudahBayar = transaksiPeriode
                        .Where(t => t.SiswaId == s.Id && t.TanggalBayarSpec != null)
                        .Select(t => t.TanggalBayarSpec!).ToList()
                });
                return Ok(result);
            }
            else
            {
                bool isTahunOnly = bulanPeriode.Length == 4 && int.TryParse(bulanPeriode, out _);

                if (isTahunOnly)
                {
                    var tahunPrefix = bulanPeriode + "-";
                    var transaksiTahun = await _context.TransaksiKas
                        .Where(t => t.BendaharaId == bendaharaId &&
                                    t.BulanPeriode != null &&
                                    t.BulanPeriode.StartsWith(tahunPrefix))
                        .ToListAsync();

                    var result = listSiswa.Select(s => new
                    {
                        SiswaId = s.Id,
                        NamaSiswa = s.Nama,
                        NoAbsen = s.NoAbsen,
                        BulanSudahBayar = transaksiTahun
                            .Where(t => t.SiswaId == s.Id &&
                                        t.BulanPeriode != null &&
                                        t.BulanPeriode.Length == 7)
                            .Select(t => int.Parse(t.BulanPeriode!.Substring(5, 2)))
                            .Distinct().OrderBy(b => b).ToList()
                    });
                    return Ok(result);
                }
                else
                {
                    var result = listSiswa.Select(s => new
                    {
                        SiswaId = s.Id,
                        NamaSiswa = s.Nama,
                        NoAbsen = s.NoAbsen,
                        SudahBayar = transaksiPeriode.Any(t => t.SiswaId == s.Id)
                    });
                    return Ok(result);
                }
            }
        }

        // ── POST: transaksi-lain ──────────────────────────────────────────────

        [HttpPost("transaksi-lain")]
        public async Task<IActionResult> CreateTransaksiLain([FromBody] TransaksiLainDto dto)
        {
            if (dto.BendaharaId == Guid.Empty)
                return BadRequest(new { message = "BendaharaId wajib diisi." });
            if (string.IsNullOrWhiteSpace(dto.Judul) || dto.Nominal <= 0)
                return BadRequest(new { message = "Judul dan nominal harus diisi dengan benar!" });

            var transaksi = new TransaksiLain
            {
                Id = Guid.NewGuid(),
                BendaharaId = dto.BendaharaId,
                Judul = dto.Judul,
                Tipe = dto.Tipe,
                Nominal = dto.Nominal,
                Tanggal = DateTime.UtcNow,
                Keterangan = dto.Keterangan,
                BuktiFoto = dto.BuktiFoto
            };

            _context.TransaksiLains.Add(transaksi);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Transaksi berhasil disimpan!" });
        }

        // ── GET: total-saldo (legacy) ─────────────────────────────────────────

        [HttpGet("total-saldo")]
        public async Task<IActionResult> GetTotalSaldo()
        {
            var bendaharaId = GetBendaharaId();
            if (bendaharaId == null)
                return BadRequest(new { message = "Header X-Bendahara-Id wajib disertakan." });

            var totalKas = await _context.TransaksiKas
                .Where(t => t.BendaharaId == bendaharaId)
                .SumAsync(t => (decimal?)t.Nominal) ?? 0;
            var totalMasuk = await _context.TransaksiLains
                .Where(t => t.BendaharaId == bendaharaId && t.Tipe == "Pemasukan")
                .SumAsync(t => (decimal?)t.Nominal) ?? 0;
            var totalKeluar = await _context.TransaksiLains
                .Where(t => t.BendaharaId == bendaharaId && t.Tipe == "Pengeluaran")
                .SumAsync(t => (decimal?)t.Nominal) ?? 0;

            return Ok(new { totalSaldo = (totalKas + totalMasuk) - totalKeluar });
        }

        // ── GET: summary (baru) ───────────────────────────────────────────────
        // GET /api/Transaksi/summary?bendaharaId={id}

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid bendaharaId)
        {
            if (bendaharaId == Guid.Empty)
                return BadRequest(new { message = "bendaharaId wajib disertakan." });

            var totalKas = await _context.TransaksiKas
                .Where(t => t.BendaharaId == bendaharaId)
                .SumAsync(t => (decimal?)t.Nominal) ?? 0;

            var totalMasuk = await _context.TransaksiLains
                .Where(t => t.BendaharaId == bendaharaId && t.Tipe == "Pemasukan")
                .SumAsync(t => (decimal?)t.Nominal) ?? 0;

            var totalKeluar = await _context.TransaksiLains
                .Where(t => t.BendaharaId == bendaharaId && t.Tipe == "Pengeluaran")
                .SumAsync(t => (decimal?)t.Nominal) ?? 0;

            // Pemasukan = kas rutin + pemasukan lain
            var totalPemasukan = totalKas + totalMasuk;
            var totalSaldo = totalPemasukan - totalKeluar;

            return Ok(new
            {
                totalSaldo,
                totalPemasukan,
                totalPengeluaran = totalKeluar
            });
        }

        // ── GET: riwayat ──────────────────────────────────────────────────────

        [HttpGet("riwayat")]
        public async Task<IActionResult> GetRiwayat(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string filter = "Semua",
            [FromQuery] int limit = 0)
        {
            var bendaharaId = GetBendaharaId();
            if (bendaharaId == null)
                return BadRequest(new { message = "Header X-Bendahara-Id wajib disertakan." });

            if (limit > 0 && pageSize == 10) pageSize = limit;

            var listKas = await _context.TransaksiKas
                .Where(t => t.BendaharaId == bendaharaId)
                .Include(t => t.Siswa)
                .Select(t => new RiwayatItemDto
                {
                    Id = t.Id,
                    Judul = "Kas - " + (t.Siswa != null ? t.Siswa.Nama : "Siswa"),
                    Tipe = "Masuk",
                    Nominal = t.Nominal,
                    Tanggal = t.TanggalBayar,
                    Keterangan = t.Keterangan,
                    BuktiFotoUrl = null
                })
                .ToListAsync();

            var listLain = await _context.TransaksiLains
                .Where(t => t.BendaharaId == bendaharaId)
                .Select(t => new RiwayatItemDto
                {
                    Id = t.Id,
                    Judul = t.Judul,
                    Tipe = t.Tipe == "Pemasukan" ? "Masuk" : "Keluar",
                    Nominal = t.Nominal,
                    Tanggal = t.Tanggal,
                    Keterangan = t.Keterangan,
                    BuktiFotoUrl = t.BuktiFoto
                })
                .ToListAsync();

            var gabungan = listKas.Concat(listLain)
                .OrderByDescending(t => t.Tanggal)
                .AsQueryable();

            if (filter == "Masuk") gabungan = gabungan.Where(t => t.Tipe == "Masuk");
            else if (filter == "Keluar") gabungan = gabungan.Where(t => t.Tipe == "Keluar");

            var totalCount = gabungan.Count();

            List<RiwayatItemDto> result;
            if (limit > 0)
                result = gabungan.Take(limit).ToList();
            else
                result = gabungan.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                data = result,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
    }
}
