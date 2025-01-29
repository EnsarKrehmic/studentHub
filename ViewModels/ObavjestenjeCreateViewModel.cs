using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class ObavjestenjeCreateViewModel
    {
        [Required(ErrorMessage = "Naslov je obavezan.")]
        [MaxLength(200, ErrorMessage = "Naslov ne sme biti duži od 200 karaktera.")]
        public string Naslov { get; set; }

        [Required(ErrorMessage = "Sadržaj je obavezan.")]
        [MaxLength(1000, ErrorMessage = "Sadržaj ne sme biti duži od 1000 karaktera.")]
        public string Sadrzaj { get; set; }

        [Required]
        public DateTime DatumObjave { get; set; } = DateTime.Now;

        public long StudijskiProgramId { get; set; }
    }
}
