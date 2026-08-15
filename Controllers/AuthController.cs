using be.Data;
using be.DTOs;
using be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Bendaharas.AnyAsync(b => b.Email == dto.Email))
                return BadRequest(new { message = "Email sudah terdaftar!" });

            var bendahara = new Bendahara
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                NamaLengkap = dto.NamaLengkap,
                IsOnboarded = true // wajib onboarding setelah registrasi
            };

            _context.Bendaharas.Add(bendahara);
            await _context.SaveChangesAsync();

            // Langsung kembalikan data agar frontend tidak perlu login ulang
            return Ok(new
            {
                message = "Registrasi bendahara berhasil!",
                data = new
                {
                    bendahara.Id,
                    bendahara.Email,
                    bendahara.NamaLengkap,
                    bendahara.IsOnboarded
                }
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var bendahara = await _context.Bendaharas.FirstOrDefaultAsync(b => b.Email == dto.Email);
            if (bendahara == null)
                return BadRequest(new { message = "Email atau password salah!" });

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, bendahara.PasswordHash);
            if (!isPasswordValid)
                return BadRequest(new { message = "Email atau password salah!" });

            return Ok(new
            {
                message = "Login berhasil!",
                data = new
                {
                    bendahara.Id,
                    bendahara.Email,
                    bendahara.NamaLengkap,
                    bendahara.IsOnboarded
                }
            });
        }

        // POST: api/Auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var bendahara = await _context.Bendaharas.FirstOrDefaultAsync(b => b.Email == dto.Email);
            if (bendahara == null)
                return NotFound(new { message = "Email tidak ditemukan!" });

            bendahara.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password berhasil diperbarui!" });
        }

        // POST: api/Auth/complete-onboarding
        // Dipanggil setelah user selesai mengatur pengaturan kas di OnboardingScreen
        [HttpPost("complete-onboarding")]
        public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingDto dto)
        {
            var bendahara = await _context.Bendaharas.FindAsync(dto.BendaharaId);
            if (bendahara == null)
                return NotFound(new { message = "Akun bendahara tidak ditemukan!" });

            bendahara.IsOnboarded = false; // false = sudah selesai onboarding
            await _context.SaveChangesAsync();

            return Ok(new { message = "Onboarding selesai!" });
        }

        // GET: api/Auth/profile/{id}
        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(Guid id)
        {
            var bendahara = await _context.Bendaharas.FindAsync(id);
            if (bendahara == null)
                return NotFound(new { message = "Akun bendahara tidak ditemukan!" });

            return Ok(new
            {
                bendahara.Id,
                bendahara.Email,
                bendahara.NamaLengkap,
                bendahara.CreatedAt,
                bendahara.IsOnboarded
            });
        }

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var bendahara = await _context.Bendaharas.FindAsync(dto.BendaharaId);
            if (bendahara == null)
                return NotFound(new { message = "Akun tidak ditemukan!" });

            bool isOldValid = BCrypt.Net.BCrypt.Verify(dto.OldPassword, bendahara.PasswordHash);
            if (!isOldValid)
                return BadRequest(new { message = "Password lama tidak sesuai!" });

            if (dto.NewPassword.Length < 6)
                return BadRequest(new { message = "Password baru minimal 6 karakter!" });

            bendahara.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password berhasil diubah!" });
        }
    }

    // DTO lokal untuk complete-onboarding
    public class CompleteOnboardingDto
    {
        public Guid BendaharaId { get; set; }
    }
}
