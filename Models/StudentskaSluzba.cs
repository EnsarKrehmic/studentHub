using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class StudentskaSluzba : Korisnik
    {
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }

        public List<StudentskaSluzbaStudijskiProgram> StudentskaSluzbaStudijskiProgrami { get; set; }
        public StudentskaSluzba() { }
    }
}
