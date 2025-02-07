using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class StudentskaSluzba : Korisnik
    {
        public List<StudentskaSluzbaStudijskiProgram> StudentskaSluzbaStudijskiProgrami { get; set; }
        public StudentskaSluzba() { }
    }
}
