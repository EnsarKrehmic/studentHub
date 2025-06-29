using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class RasporedDetailsViewModel
    {
        public Raspored Raspored { get; set; }
        public List<TerminNastave> Termini { get; set; }

        public bool PrikazSamoLični { get; set; } = false;
        public string KorisnickaUloga { get; set; }
        public long? KorisnikId { get; set; }
    }
}
