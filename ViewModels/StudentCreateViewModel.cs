using StudentHub.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class StudentCreateViewModel
    {
        // Identity podaci
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite validnu email adresu.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Lozinka mora imati najmanje {2} karaktera.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Lozinka i potvrda lozinke se ne podudaraju.")]
        public string ConfirmPassword { get; set; }

        // Poslovni podaci
        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tačno 13 karaktera.")]
        public string? JMBG { get; set; }

        [MaxLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
        public string? Ime { get; set; }

        [MaxLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
        public string? Prezime { get; set; }

        [MaxLength(20, ErrorMessage = "Broj indeksa ne može biti duži od 20 karaktera.")]
        [Display(Name = "Broj indeksa")]
        public string? BrojIndeksa { get; set; }

        [MaxLength(200, ErrorMessage = "Prethodno obrazovanje ne može biti duže od 200 karaktera.")]
        public string? PrethodnoObrazovanje { get; set; }

        [Range(1, 6, ErrorMessage = "Godina studija mora biti između 1 i 6.")]
        public int? GodinaStudija { get; set; }

        [Range(1, 12, ErrorMessage = "Semestar mora biti između 1 i 12.")]
        public int? Semestar { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        public long StudijskiProgramId { get; set; }

        public long? NastavniPlanId { get; set; }

        public List<long> PredmetIds { get; set; } = new List<long>();
    }
}
