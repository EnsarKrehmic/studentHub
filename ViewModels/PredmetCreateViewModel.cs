using StudentHub.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class PredmetCreateViewModel
    {
        [Required(ErrorMessage = "Naziv predmeta je obavezan.")]
        [MaxLength(100, ErrorMessage = "Naziv ne može biti duži od 100 karaktera.")]
        public string Naziv { get; set; }

        [MaxLength(500, ErrorMessage = "Opis ne može biti duži od 500 karaktera.")]
        public string Opis { get; set; }

        [Required(ErrorMessage = "Vrijednost ECTS bodova je obavezna.")]
        [Range(1, 30, ErrorMessage = "Broj ECTS bodova mora biti između 1 i 30.")]
        public int ECTS { get; set; }

        [Required(ErrorMessage = "Tip predmeta je obavezan.")]
        public TipPredmeta TipPredmeta { get; set; }

        public long? ProfesorId { get; set; }
        public long? AsistentId { get; set; }

        [Display(Name = "Nastavni plan")]
        public long? NastavniPlanId { get; set; }

        [Range(1, 12, ErrorMessage = "Semestar mora biti između 1 i 12.")]
        public int? Semestar { get; set; }

        // Liste za višestruki odabir (profesori/asistenti povezani sa predmetom)
        public List<long> ProfesorIds { get; set; } = new List<long>();
        public List<long> AsistentIds { get; set; } = new List<long>();
    }
}
