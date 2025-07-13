using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class FaqPitanjeViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Kategorija je obavezna.")]
        [StringLength(100)]
        public string Kategorija { get; set; }

        [Required(ErrorMessage = "Pitanje je obavezno.")]
        [StringLength(300)]
        public string Pitanje { get; set; }

        [Required(ErrorMessage = "Odgovor je obavezan.")]
        [StringLength(2000)]
        public string Odgovor { get; set; }

        public bool Preporuceno { get; set; }

        public List<string> SveKategorije { get; set; } = new List<string>();
    }
}
