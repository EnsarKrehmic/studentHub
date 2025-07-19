using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class Pravilnik
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Naslov { get; set; }

        [Required]
        [StringLength(500)]
        public string Opis { get; set; }

        public DateTime DatumKreiranja { get; set; } = DateTime.Now;

        // Svi članovi/dijelovi pravilnika
        public List<PravilnikClanak> Clanovi { get; set; } = new();
    }

    public class PravilnikClanak
    {
        public int Id { get; set; }

        [Required]
        public int PravilnikId { get; set; }
        public Pravilnik Pravilnik { get; set; }

        [Required]
        [StringLength(300)]
        public string NaslovClanka { get; set; }

        [Required]
        [StringLength(4000)]
        public string Sadrzaj { get; set; }

        public int RedniBroj { get; set; } // Za sortiranje (npr. član 1, član 2...)
    }
}
