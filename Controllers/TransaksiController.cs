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

        // DTO bayar kas batch (mendukung periode spesifik)
        public class BayarKasBatchDto
        {
            public List<Guid> SiswaIds { get; set; } = new();
            public decimal Nominal { get; set; }
            public string? BulanPeriode { get; set; }   // format: "yyyy-MM"
            public int? MingguKe { get; set; }          // 1-4 (untuk mode Mingguan)
            public string? TanggalBayarSpec { get; set; } // "yyyy-MM-dd" (untuk mode Harian)
        }

        // DTO transaksi lain dengan bukti foto
        public class TransaksiLainDto
        {
            public string Judul { get; set; } = string.Empty;
            public string Tipe { get; set; } = "Pengeluaran";
            public decimal Nominal { get; set; }
            public string? Keterangan { get; set; }
            public string? BuktiFoto { get; set; } // Base64 string
        }

        // DTO item riwayat untuk response
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

        // POST: api/Transaksi/bayar-kas-batch
        [HttpPost("bayar-kas-batch")]
        public async Task<IActionResult> BayarKasBatch([FromBody] BayarKasBatchDto dto)
        {
            if (dto.SiswaIds == null || !dto.SiswaIds.Any())
                return BadRequest(new { message = "Pilih minimal 1 siswa!" });

            var listTransaksi = new List<TransaksiKas>();
            foreach (var siswaId in dto.SiswaIds)
            {
                // Cek apakah periode ini sudah dibayar (hindari double payment)
                if (!string.IsNullOrEmpty(dto.BulanPeriode))
                {
                    bool sudahBayar = false;

                    if (dto.TanggalBayarSpec != null)
                    {
                        // Mode Harian: cek per tanggal spesifik
                        sudahBayar = await _context.TransaksiKas.AnyAsync(t =>
                            t.SiswaId == siswaId &&
                            t.BulanPeriode == dto.BulanPeriode &&
                            t.TanggalBayarSpec == dto.TanggalBayarSpec);
                    }
                    else if (dto.MingguKe != null)
                    {
                        // Mode Mingguan: cek per minggu
                        sudahBayar = await _context.TransaksiKas.AnyAsync(t =>
                            t.SiswaId == siswaId &&
                            t.BulanPeriode == dto.BulanPeriode &&
                            t.MingguKe == dto.MingguKe);
                    }

                    if (sudahBayar) continue; // skip siswa yang sudah bayar periode ini
                }

                listTransaksi.Add(new TransaksiKas
                {
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

        // GET: api/Transaksi/status-kas?bulanPeriode=2026-08&tipeJadwal=Mingguan
        // Mengambil status bayar per siswa untuk periode tertentu
        [HttpGet("status-kas")]
        public async Task<IActionResult> GetStatusKas(
            [FromQuery] string bulanPeriode,
            [FromQuery] string tipeJadwal = "Mingguan")
        {
            var listSiswa = await _context.Siswas
                .Where(s => s.IsActive)
                .OrderBy(s => s.NoAbsen)
                .ToListAsync();

            var transaksiPeriode = await _context.TransaksiKas
                .Where(t => t.BulanPeriode == bulanPeriode)
                .ToListAsync();

            if (tipeJadwal == "Mingguan")
            {
                var result = listSiswa.Select(s =>
                {
                    var dibayar = transaksiPeriode
                        .Where(t => t.SiswaId == s.Id && t.MingguKe != null)
                        .Select(t => t.MingguKe!.Value)
                        .ToList();

                    return new
                    {
                        SiswaId = s.Id,
                        NamaSiswa = s.Nama,
                        NoAbsen = s.NoAbsen,
                        MingguSudahBayar = dibayar
                    };
                });
                return Ok(result);
            }
            else if (tipeJadwal == "Harian")
            {
                var result = listSiswa.Select(s =>
                {
                    var dibayar = transaksiPeriode
                        .Where(t => t.SiswaId == s.Id && t.TanggalBayarSpec != null)
                        .Select(t => t.TanggalBayarSpec!)
                        .ToList();

                    return new
                    {
                        SiswaId = s.Id,
                        NamaSiswa = s.Nama,
                        NoAbsen = s.NoAbsen,
                        TanggalSudahBayar = dibayar
                    };
                });
                return Ok(result);
            }
            else
            {
                // Bulanan: bulanPeriode bisa "yyyy" (query per tahun) atau "yyyy-MM" (per bulan).
                // Jika format "yyyy" → query semua transaksi tahun itu, kembalikan
                // daftar bulan (int 1-12) yang sudah dibayar.
                // Jika format "yyyy-MM" → backward compat, kembalikan SudahBayar bool.
                bool isTahunOnly = bulanPeriode.Length == 4 &&
                                   int.TryParse(bulanPeriode, out _);

                if (isTahunOnly)
                {
                    // Query semua transaksi yang BulanPeriode di-awali "yyyy-"
                    var tahunPrefix = bulanPeriode + "-";
                    var transaksiTahun = await _context.TransaksiKas
                        .Where(t => t.BulanPeriode != null &&
                                    t.BulanPeriode.StartsWith(tahunPrefix))
                        .ToListAsync();

                    var result = listSiswa.Select(s =>
                    {
                        // Ekstrak nomor bulan dari BulanPeriode "yyyy-MM"
                        var bulanDibayar = transaksiTahun
                            .Where(t => t.SiswaId == s.Id &&
                                        t.BulanPeriode != null &&
                                        t.BulanPeriode.Length == 7)
                            .Select(t => int.Parse(t.BulanPeriode!.Substring(5, 2)))
                            .Distinct()
                            .OrderBy(b => b)
                            .ToList();

                        return new
                        {
                            SiswaId = s.Id,
                            NamaSiswa = s.Nama,
                            NoAbsen = s.NoAbsen,
                            BulanSudahBayar = bulanDibayar
                        };
                    });
                    return Ok(result);
                }
                else
                {
                    // Backward compat: cek apakah sudah bayar bulan ini
                    var result = listSiswa.Select(s =>
                    {
                        bool sudahBayar = transaksiPeriode.Any(t => t.SiswaId == s.Id);
                        return new
                        {
                            SiswaId = s.Id,
                            NamaSiswa = s.Nama,
                            NoAbsen = s.NoAbsen,
                            SudahBayar = sudahBayar
                        };
                    });
                    return Ok(result);
                }
            }
        }

        // POST: api/Transaksi/transaksi-lain
        [HttpPost("transaksi-lain")]
        public async Task<IActionResult> CreateTransaksiLain([FromBody] TransaksiLainDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Judul) || dto.Nominal <= 0)
                return BadRequest(new { message = "Judul dan nominal harus diisi dengan benar!" });

            var transaksi = new TransaksiLain
            {
                Id = Guid.NewGuid(),
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

        // GET: api/Transaksi/total-saldo
        [HttpGet("total-saldo")]
        public async Task<IActionResult> GetTotalSaldo()
        {
            var totalKas = await _context.TransaksiKas.SumAsync(t => (decimal?)t.Nominal) ?? 0;
            var totalPemasukanLain = await _context.TransaksiLains
                .Where(t => t.Tipe == "Pemasukan")
                .SumAsync(t => (decimal?)t.Nominal) ?? 0;
            var totalPengeluaran = await _context.TransaksiLains
                .Where(t => t.Tipe == "Pengeluaran")
                .SumAsync(t => (decimal?)t.Nominal) ?? 0;

            var saldoAkhir = (totalKas + totalPemasukanLain) - totalPengeluaran;

            return Ok(new { totalSaldo = saldoAkhir });
        }

        // GET: api/Transaksi/riwayat?page=1&pageSize=10&filter=Semua
        [HttpGet("riwayat")]
        public async Task<IActionResult> GetRiwayat(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string filter = "Semua",   // "Semua", "Masuk", "Keluar"
            [FromQuery] int limit = 0)             // backward compat
        {
            // Support legacy ?limit=N parameter
            if (limit > 0 && pageSize == 10) pageSize = limit;

            // Ambil Kas Rutin Siswa
            var listKas = await _context.TransaksiKas
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

            // Ambil Transaksi Lain
            var listLain = await _context.TransaksiLains
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

            // Gabungkan
            var gabungan = listKas.Concat(listLain)
                .OrderByDescending(t => t.Tanggal)
                .AsQueryable();

            // Filter
            if (filter == "Masuk")
                gabungan = gabungan.Where(t => t.Tipe == "Masuk");
            else if (filter == "Keluar")
                gabungan = gabungan.Where(t => t.Tipe == "Keluar");

            var totalCount = gabungan.Count();

            // Pagination (skip jika limit > 0 untuk backward compat)
            List<RiwayatItemDto> result;
            if (limit > 0)
            {
                result = gabungan.Take(limit).ToList();
            }
            else
            {
                result = gabungan
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

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
