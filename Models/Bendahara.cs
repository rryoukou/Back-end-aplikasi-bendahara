using System.ComponentModel.DataAnnotations;

namespace be.Models
{
    public class Bendahara
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string NamaLengkap { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// true  = user baru, belum menyelesaikan onboarding → arahkan ke OnboardingScreen
        /// false = sudah onboarding → langsung ke DashboardScreen
        /// </summary>
        public bool IsOnboarded { get; set; } = true;
    }
}
