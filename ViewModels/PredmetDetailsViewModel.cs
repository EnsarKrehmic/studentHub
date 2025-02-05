using System.Collections.Generic;
using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class PredmetDetailsViewModel
    {
        public Predmet Predmet { get; set; }
        public List<PredmetProfesor> Profesori { get; set; }
        public List<PredmetAsistent> Asistenti { get; set; }
        public List<StudentNaPredmetu> StudentiNaPredmetu { get; set; }
        public Dictionary<long, float?> Ocjene { get; set; }
        public long StudentId { get; set; }
        public long ProfesorId { get; set; }
        public long AsistentId { get; set; }
        public float? Ocjena { get; set; }
    }
}