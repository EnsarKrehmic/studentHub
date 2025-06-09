using StudentHub.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class StudentEditViewModel
    {
        public long Id { get; set; }

        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tačno 13 karaktera.")]
        public string? JMBG { get; set; }

        [MaxLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
        public string? Ime { get; set; }

        [MaxLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
        public string? Prezime { get; set; }

        [EmailAddress(ErrorMessage = "Unesite validnu email adresu.")]
        public string? Email { get; set; }

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
        public bool IzborIzbornihPredmetaZakljucan { get; set; }

        public List<long> PredmetIds { get; set; } = new List<long>();

        [Required(ErrorMessage = "Uloga je obavezna.")]
        public Uloga Uloga { get; set; }
    }
}
