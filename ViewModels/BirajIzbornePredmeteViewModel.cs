using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class BirajIzbornePredmeteViewModel
    {
        public long StudentId { get; set; }
        public string ImePrezime { get; set; }
        public int GodinaStudija { get; set; }
        public long StudijskiProgramId { get; set; }
        public string StudijskiProgramNaziv { get; set; }
        public int MinIzborniPredmeti { get; set; }
        public int MaxIzborniPredmeti { get; set; }
        public int BrojVecOdabranihPredmeta { get; set; }
        public bool IsMaxLimitReached => BrojVecOdabranihPredmeta >= MaxIzborniPredmeti;
        public bool IsLocked { get; set; }

        public List<PredmetCheckboxViewModel> Predmeti { get; set; } = new();
        public List<long> SelectedPredmetiIds { get; set; } = new();
    }

    public class PredmetCheckboxViewModel
    {
        public long PredmetId { get; set; }
        public string Naziv { get; set; }
        public bool IsSelected { get; set; }
    }
}
