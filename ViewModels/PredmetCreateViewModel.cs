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

        [Required(ErrorMessage = "Semestar je obavezan.")]
        [Range(1, 2, ErrorMessage = "Semestar može biti samo 1 (zimski) ili 2 (ljetni).")]
        public int Semestar { get; set; } // promijeni u int i validiraj na 1 ili 2

        [Required(ErrorMessage = "Godina studija je obavezna.")]
        [Range(1, 6, ErrorMessage = "Godina studija mora biti između 1 i 6.")]
        public int GodinaStudija { get; set; } // NOVO

        [Required(ErrorMessage = "Broj sati predavanja je obavezan.")]
        [Range(0, 60, ErrorMessage = "Sati predavanja mora biti između 0 i 60.")]
        public int SatiPredavanja { get; set; } // NOVO

        [Required(ErrorMessage = "Broj sati vježbi je obavezan.")]
        [Range(0, 60, ErrorMessage = "Sati vježbi mora biti između 0 i 60.")]
        public int SatiVjezbi { get; set; } // NOVO

        public long StudijskiProgramId { get; set; } // OBAVEZNO u viewmodelu, jer je u modelu required

        // Liste za višestruki odabir (profesori/asistenti povezani sa predmetom)
        public List<long> ProfesorIds { get; set; } = new List<long>();
        public List<long> AsistentIds { get; set; } = new List<long>();
    }
}
