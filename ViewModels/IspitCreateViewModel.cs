using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class IspitCreateViewModel
    {
        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [Display(Name = "Studijski program")]
        public long StudijskiProgramId { get; set; }

        [Required(ErrorMessage = "Nastavni plan je obavezan.")]
        [Display(Name = "Nastavni plan")]
        public long NastavniPlanId { get; set; }

        [Required(ErrorMessage = "Predmet je obavezan.")]
        [Display(Name = "Predmet")]
        public long PredmetId { get; set; }

        [Required(ErrorMessage = "Datum održavanja je obavezan.")]
        [Display(Name = "Datum održavanja")]
        [DataType(DataType.Date)]
        public DateTime DatumOdrzavanja { get; set; }

        [Required(ErrorMessage = "Lokacija je obavezna.")]
        [StringLength(100, ErrorMessage = "Lokacija ne može biti duža od 100 karaktera.")]
        public string Lokacija { get; set; }

        [Required(ErrorMessage = "Broj bodova je obavezan.")]
        [Range(1, 100, ErrorMessage = "Broj bodova mora biti između 1 i 100.")]
        [Display(Name = "Broj bodova")]
        public int BrojBodova { get; set; }
    }
}
