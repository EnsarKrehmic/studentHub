using System.Collections.Generic;
using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class AsistentDetailsViewModel
    {
        public Asistent Asistent { get; set; }
        public List<StudijskiProgram> StudijskiProgrami { get; set; }
        public List<Predmet> Predmeti { get; set; }
    }
}