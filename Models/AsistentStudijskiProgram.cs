using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;

namespace StudentHub.Models
{
    public class AsistentStudijskiProgram
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Asistent je obavezan.")]
        [ForeignKey("Asistent")]
        public long AsistentId { get; set; }
        public Asistent Asistent { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }
    }
}