using System.Collections.Generic;
using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class PredmetDetailsViewModel
    {
        public Predmet Predmet { get; set; }
        public List<PredmetProfesor> Profesori { get; set; }
        public List<PredmetAsistent> Asistenti { get; set; }
    }
}