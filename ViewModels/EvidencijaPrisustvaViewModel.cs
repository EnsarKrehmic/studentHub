using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class DokumentGroupedByProgramViewModel
    {
        public StudijskiProgram StudijskiProgram { get; set; }
        public List<Dokument> Dokumenti { get; set; }
    }
}
