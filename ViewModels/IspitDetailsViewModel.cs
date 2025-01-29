using StudentHub.Models;
using System.Collections.Generic;

namespace StudentHub.ViewModels
{
    public class IspitDetailsViewModel
    {
        public long IspitId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }
        public List<PredmetIspitViewModel> Predmeti { get; set; } = new List<PredmetIspitViewModel>();
        public List<NastavniPlanIspitViewModel> NastavniPlanovi { get; set; } = new List<NastavniPlanIspitViewModel>();
        public string CurrentSort { get; set; }
        public string DateSortParm { get; set; }
        public string LocationSortParm { get; set; }
        public string PointsSortParm { get; set; }
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