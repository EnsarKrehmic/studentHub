using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class StudentskaSluzba : Korisnik
    {
        [ForeignKey("Zahtjev")]
        public long? ZahtjevId { get; set; }
        public Zahtjev Zahtjev { get; set; }

        [ForeignKey("Predmet")]
        public long? PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        public StudentskaSluzba() { }
    }
}
