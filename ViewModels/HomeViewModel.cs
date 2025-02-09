using StudentHub.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.ViewModels
{
    public class HomeViewModel
    {
        public int BrojAsistenata { get; set; }
        public int BrojProfesora { get; set; }
        public int BrojStudenata { get; set; }
        public int AktivniIspiti { get; set; }
        public List<ObavjestenjeViewModel> NajnovijeObavijesti { get; set; } = new();
        public List<StudijskiProgram> StudijskiProgrami { get; set; }
    }
}
