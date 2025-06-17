using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class NastavniMaterijalFajl
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string PutanjaDoFajla { get; set; }

        [Required]
        public string TipFajla { get; set; }

        [ForeignKey("NastavniMaterijal")]
        public long NastavniMaterijalId { get; set; }

        public NastavniMaterijal NastavniMaterijal { get; set; }
    }
}
