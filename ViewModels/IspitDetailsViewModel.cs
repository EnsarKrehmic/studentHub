using StudentHub.Models;
using System.Collections.Generic;

namespace StudentHub.ViewModels
{
    public class IspitDetailsViewModel
    {
        public long IspitId { get; set; }
        public bool Prijavljen { get; set; }
        public bool Arhivirano { get; set; }
        public string CurrentSort { get; set; }
        public string DateSortParm { get; set; }
        public string LocationSortParm { get; set; }
        public string PointsSortParm { get; set; }
        public decimal BrojBodova { get; set; }
        public decimal UslovZaPolaganje { get; set; }
        public decimal? Bodovi { get; set; }
        public DateTime DatumIspita { get; set; }
        public long? StudentId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }
        public List<PredmetIspitViewModel> Predmeti { get; set; } = new List<PredmetIspitViewModel>();
        public List<NastavniPlanIspitViewModel> NastavniPlanovi { get; set; } = new List<NastavniPlanIspitViewModel>();
        public List<long> PrijavljeniIspitiIds { get; set; } = new List<long>();
        public Dictionary<long?, List<Ocjena>> Ocjene { get; set; } = new Dictionary<long?, List<Ocjena>>();
        public List<Student> PrijavljeniStudenti { get; set; } = new List<Student>();
        public List<RezultatIspitaViewModel> RezultatiIspita { get; set; } = new List<RezultatIspitaViewModel>();
        public List<Prijava> Prijave { get; set; } = new List<Prijava>();
        public List<Komentar> Komentari { get; set; } = new List<Komentar>();
    }

    public class NastavniPlanIspitViewModel
    {
        public NastavniPlan NastavniPlan { get; set; }
        public List<PredmetIspitViewModel> Predmeti { get; set; } = new List<PredmetIspitViewModel>();
    }

    public class PredmetIspitViewModel
    {
        public Predmet Predmet { get; set; }
        public List<Ispit> Ispiti { get; set; } = new List<Ispit>();
    }
}