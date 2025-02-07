using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class StudentGroupedByProgramViewModel
    {
        public StudijskiProgram StudijskiProgram { get; set; }
        public NastavniPlan NastavniPlan { get; set; }
        public List<Student> Studenti { get; set; }
    }

}
