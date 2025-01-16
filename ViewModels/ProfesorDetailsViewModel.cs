using System.Collections.Generic;
using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class ProfesorDetailsViewModel
    {
        public Profesor Profesor { get; set; }
        public List<StudijskiProgram> StudijskiProgrami { get; set; }
        public List<Predmet> Predmeti { get; set; }
    }
}