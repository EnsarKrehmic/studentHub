using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class IspitViewModel
    {
        public long Id { get; set; }
        public Predmet Predmet { get; set; }
        public DateTime DatumOdrzavanja { get; set; }
        public bool JePrijavljen { get; set; }
    }

}
