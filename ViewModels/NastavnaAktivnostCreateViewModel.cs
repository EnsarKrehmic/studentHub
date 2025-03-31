using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class NastavnaAktivnostCreateViewModel
    {
        [Required(ErrorMessage = "Naziv je obavezan")]
        [StringLength(100, ErrorMessage = "Naziv ne smije biti duži od 100 znakova")]
        public string Naziv { get; set; }

        [StringLength(500, ErrorMessage = "Opis ne smije biti duži od 500 znakova")]
        [DataType(DataType.MultilineText)]
        public string Opis { get; set; }

        [Required(ErrorMessage = "Tip aktivnosti je obavezan")]
        [Display(Name = "Tip aktivnosti")]
        public TipNastavneAktivnosti Tip { get; set; }

        [Required(ErrorMessage = "Datum i vrijeme održavanja su obavezni.")]
        [DisplayName("Datum i vrijeme održavanja")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime DatumVrijemeOdrzavanja { get; set; }

        [Display(Name = "Manuelno otključaj aktivnost")]
        public bool ManuelnoOtkljucano { get; set; }

        [Required]
        public long PredmetId { get; set; }
    }
}