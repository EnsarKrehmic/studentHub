using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class StudijskiProgramDetailsViewModel
    {
        public StudijskiProgram StudijskiProgram { get; set; }
        public List<Obavjestenje> Obavjestenja { get; set; }
        public int BrojStudenata { get; set; }
        public int BrojProfesora { get; set; }
        public int BrojAsistenata { get; set; }
    }
}
