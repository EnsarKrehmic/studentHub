using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class IspitGroupedViewModel
    {
        public StudijskiProgram StudijskiProgram { get; set; }
        public Predmet Predmet { get; set; }
        public List<Ispit> Ispiti { get; set; }
    }
}