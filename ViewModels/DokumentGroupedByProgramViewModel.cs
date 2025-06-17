using StudentHub.Models;
using System.Collections.Generic;

namespace StudentHub.ViewModels
{
    public class EvidencijaPrisustvaViewModel
    {
        public NastavnaAktivnost NastavnaAktivnost { get; set; }

        public List<Student> Studenti { get; set; }

        public List<long> PrisutniStudentiIds { get; set; } = new List<long>();
    }
}
