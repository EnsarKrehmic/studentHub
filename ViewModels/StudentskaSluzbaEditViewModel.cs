using StudentHub.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class StudentskaSluzbaEditViewModel
    {
        public long Id { get; set; }

        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tačno 13 karaktera.")]
        public string? JMBG { get; set; }

        [MaxLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
        public string? Ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [MaxLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
        public string? Prezime { get; set; }

        [EmailAddress(ErrorMessage = "Unesite validnu email adresu.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Uloga je obavezna.")]
        [EnumDataType(typeof(Uloga))]
        public Uloga Uloga { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        public long StudijskiProgramId { get; set; }
    }
}
