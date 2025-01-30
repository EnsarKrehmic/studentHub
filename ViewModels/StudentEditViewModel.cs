using StudentHub.Models;
using System.ComponentModel.DataAnnotations;


namespace StudentHub.ViewModels
{
    public class StudentEditViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "JMBG je obavezan.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tačno 13 karaktera.")]
        public string JMBG { get; set; }

        [Required(ErrorMessage = "Ime je obavezno.")]
        [MaxLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [MaxLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
        public string Prezime { get; set; }

        [EmailAddress(ErrorMessage = "Unesite validnu email adresu.")]
        public string? Email { get; set; }

        public string? Lozinka { get; set; }

        [MaxLength(20, ErrorMessage = "Broj indeksa ne može biti duži od 20 karaktera.")]
        [Display(Name = "Broj indeksa")]
        public string? BrojIndeksa { get; set; }

        [MaxLength(200, ErrorMessage = "Prethodno obrazovanje ne može biti duže od 200 karaktera.")]
        [Display(Name = "Predhodno obrazovanje")]
        public string? PredhodnoObrazovanje { get; set; }

        [Range(1, 6, ErrorMessage = "Godina studija mora biti između 1 i 6.")]
        [Display(Name = "Godina studija")]
        public int? GodinaStudija { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [Display(Name = "Studijski program")]
        public long StudijskiProgramId { get; set; }

        [Display(Name = "Nastavni plan")]
        public long? NastavniPlanId { get; set; }

        [Range(1, 12, ErrorMessage = "Semestar mora biti između 1 i 12.")]
        public int? Semestar { get; set; }

        [Display(Name = "Predmet")]
        public long? PredmetId { get; set; }
        public List<long> PredmetIds { get; set; } = new List<long>();

        [Required(ErrorMessage = "Uloga je obavezna.")]
        [EnumDataType(typeof(Uloga))]
        public Uloga Uloga { get; set; }
    }
}