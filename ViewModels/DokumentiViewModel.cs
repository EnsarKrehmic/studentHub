using Microsoft.AspNetCore.Http;
using StudentHub.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class DokumentCreateViewModel
    {
        [Required(ErrorMessage = "Naziv dokumenta je obavezan.")]
        [MaxLength(200, ErrorMessage = "Naziv dokumenta ne smije biti duži od 200 karaktera.")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Datoteka je obavezna.")]
        public IFormFile Datoteka { get; set; }

        [Required(ErrorMessage = "Student je obavezan.")]
        public long StudentId { get; set; }

        [Required(ErrorMessage = "Studentska služba je obavezna.")]
        public long StudentskaSluzbaId { get; set; }
    }

    public class DokumentEditViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Naziv dokumenta je obavezan.")]
        [MaxLength(200, ErrorMessage = "Naziv dokumenta ne smije biti duži od 200 karaktera.")]
        public string Naziv { get; set; }

        public IFormFile? Datoteka { get; set; }

        [Required(ErrorMessage = "Student je obavezan.")]
        public long StudentId { get; set; }

        [Required(ErrorMessage = "Studentska služba je obavezna.")]
        public long StudentskaSluzbaId { get; set; }
    }
}
