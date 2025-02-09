using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class DokumentSlike
    {
        public long Id { get; set; }
        public long DokumentId { get; set; }
        public Dokument Dokument { get; set; } 
        public string Putanja { get; set; }

    }

}
