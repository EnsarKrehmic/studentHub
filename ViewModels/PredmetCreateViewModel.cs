using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class PredmetCreateViewModel
    {
        [Required(ErrorMessage = "Naziv predmeta je obavezan.")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Opis predmeta je obavezan.")]
        public string Opis { get; set; }

        [Required(ErrorMessage = "Vrijednost ECTS bodova predmeta je obavezna.")]
        public int ECTS { get; set; }

        // Za odabir profesora i asistenata
        public List<long> ProfesorIds { get; set; } = new List<long>();
        public List<long> AsistentIds { get; set; } = new List<long>();

        // Jedan profesor i asistent za FK (ako se koriste)
        public long? ProfesorId { get; set; }
        public long? AsistentId { get; set; }
        public long? NastavniPlanId { get; set; }

    }
}