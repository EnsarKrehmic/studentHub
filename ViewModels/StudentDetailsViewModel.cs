using StudentHub.Models;
using X.PagedList;

namespace StudentHub.ViewModels
{
    public class StudentDetailsViewModel
    {
        public Student Student { get; set; }
        public StudijskiProgramIzborniLimit? StudijskiProgramIzborniLimit { get; set; }
        public List<StudentiGroupedByProgramViewModel> GroupedStudents { get; set; }
        public List<Predmet> Predmeti { get; set; }
        public Dictionary<long?, float?> Ocjene { get; set; }
        public string CurrentSort { get; set; }
        public string SearchString { get; set; }
        public long? StudijskiProgramId { get; set; }
    }

    public class StudentiGroupedByProgramViewModel
    {
        public StudijskiProgram StudijskiProgram { get; set; }
        public List<Student> Studenti { get; set; }
    }
}