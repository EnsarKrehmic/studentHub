using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class PredmetCreateViewModel
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Naziv predmeta je obavezan.")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Opis predmeta je obavezan.")]
        public string Opis { get; set; }

        [Required(ErrorMessage = "Vrijednost ECTS bodova predmeta je obavezna.")]
        [Range(1, 30, ErrorMessage = "Broj ECTS bodova mora biti između 1 i 30.")]
        public int ECTS { get; set; }

        // Za odabir profesora i asistenata
        public List<long> ProfesorIds { get; set; } = new List<long>();
        public List<long> AsistentIds { get; set; } = new List<long>();

        // Jedan profesor i asistent za FK
        public long? ProfesorId { get; set; }
        public long? AsistentId { get; set; }

        // Nastavni plan
        public long? NastavniPlanId { get; set; }

        // Semestar
        [Required(ErrorMessage = "Semestar je obavezan.")]
        [Range(1, 12, ErrorMessage = "Semestar mora biti između 1 i 12.")]
        public int Semestar { get; set; }
    }
}