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
        public List<StatistikaPrisustvaDTO> StatistikaPrisustva { get; set; } = new();
        public float? ProsjecnoPrisustvo { get; set; }
        public float? ProsjecnaOcjena { get; set; }
        public long StudentId { get; set; }
        public long ProfesorId { get; set; }
        public long AsistentId { get; set; }
        public float? Ocjena { get; set; }
    }

    public class StatistikaPrisustvaDTO
    {
        public Student Student { get; set; }
        public int BrojPrisustava { get; set; }
        public int UkupnoAktivnosti { get; set; }
        public float Procenat => UkupnoAktivnosti == 0 ? 0 : (float)BrojPrisustava / UkupnoAktivnosti * 100;
    }
}