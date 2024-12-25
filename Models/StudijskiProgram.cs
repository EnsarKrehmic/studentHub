using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class StudijskiProgram
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Naziv { get; set; }

        [MaxLength(500)]
        public string Opis { get; set; }

        [Required]
        [Range(1, 6)]
        public int trajanjeUGodinama { get; set; }

        public StudijskiProgram() { }
    }
}