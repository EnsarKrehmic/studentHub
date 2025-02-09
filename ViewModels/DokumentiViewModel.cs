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
        public List<IFormFile> Slike { get; set; } = new List<IFormFile>();

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

        public List<IFormFile> NoveSlike { get; set; } = new List<IFormFile>();

        public List<DokumentSlike> PostojeceSlike { get; set; } = new List<DokumentSlike>();

        [Required(ErrorMessage = "Student je obavezan.")]
        public long StudentId { get; set; }

        [Required(ErrorMessage = "Studentska služba je obavezna.")]
        public long StudentskaSluzbaId { get; set; }
    }
}
