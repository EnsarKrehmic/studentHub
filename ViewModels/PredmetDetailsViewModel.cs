using System.Collections.Generic;
using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class PredmetDetailsViewModel
    {
        public Predmet Predmet { get; set; }
        public List<PredmetProfesor> Profesori { get; set; } = new List<PredmetProfesor>();
        public List<PredmetAsistent> Asistenti { get; set; } = new List<PredmetAsistent>();
        public List<StudentNaPredmetu> StudentiNaPredmetu { get; set; } = new List<StudentNaPredmetu>();
        public List<NastavnaAktivnost> NastavneAktivnosti { get; set; } = new List<NastavnaAktivnost>();
        public Dictionary<long, float?> Ocjene { get; set; }
        public long StudentId { get; set; }
        public long ProfesorId { get; set; }
        public long AsistentId { get; set; }
        public float? Ocjena { get; set; }
    }
}