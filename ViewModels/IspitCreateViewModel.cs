using System.ComponentModel;
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
        [Range(0, 100, ErrorMessage = "Ukupni broj bodova mora biti između 0 i 100.")]
        [Display(Name = "Ukupni bodovi ispita")]
        public decimal BrojBodova { get; set; }

        [Range(0, 100, ErrorMessage = "Uslov za polaganje mora biti između 0 i 100.")]
        [Display(Name = "Uslov za polaganje (minimalni broj bodova)")]
        public decimal UslovZaPolaganje { get; set; }
    }
}
