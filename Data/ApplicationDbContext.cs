using Microsoft.EntityFrameworkCore;
using StudentHub.Models;

namespace StudentHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Definicije DbSet za svaku klasu
        public DbSet<Student> Studenti { get; set; }
        public DbSet<Predmet> Predmeti { get; set; }
        public DbSet<NastavniPlan> NastavniPlanovi { get; set; }
        public DbSet<StudijskiProgram> StudijskiProgrami { get; set; }
        public DbSet<Profesor> Profesori { get; set; }
        public DbSet<Asistent> Asistenti { get; set; }
        public DbSet<PredmetProfesor> PredmetProfesori { get; set; }
        public DbSet<PredmetAsistent> PredmetAsistenti { get; set; }
        public DbSet<Zahtjev> Zahtjevi { get; set; }
        public DbSet<Uvjerenje> Uvjerenja { get; set; }
        public DbSet<Dokument> Dokumenti { get; set; }
        public DbSet<Obavjestenje> Obavjestenja { get; set; }
        public DbSet<StudentskaSluzba> StudentskeSluzbe { get; set; }
        public DbSet<Ispit> Ispiti { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Ocjena> Ocjene { get; set; }
        public DbSet<Prijava> Prijave { get; set; }
        public DbSet<StudentNaPredmetu> StudentiNaPredmetima { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Mapiranje klasa na tabele (opciono: prilagoditi nazive tabela ako je potrebno)
            modelBuilder.Entity<Student>().ToTable("Student");
            modelBuilder.Entity<Predmet>().ToTable("Predmet");
            modelBuilder.Entity<NastavniPlan>().ToTable("NastavniPlan");
            modelBuilder.Entity<StudijskiProgram>().ToTable("StudijskiProgram");
            modelBuilder.Entity<Zahtjev>().ToTable("Zahtjev");
            modelBuilder.Entity<Profesor>().ToTable("Profesor");
            modelBuilder.Entity<Asistent>().ToTable("Asistent");
            modelBuilder.Entity<PredmetProfesor>().ToTable("PredmetProfesor");
            modelBuilder.Entity<PredmetAsistent>().ToTable("PredmetAsistent");
            modelBuilder.Entity<Uvjerenje>().ToTable("Uvjerenje");
            modelBuilder.Entity<Dokument>().ToTable("Dokument");
            modelBuilder.Entity<Obavjestenje>().ToTable("Obavjestenje");
            modelBuilder.Entity<StudentskaSluzba>().ToTable("StudentskaSluzba");
            modelBuilder.Entity<Ispit>().ToTable("Ispit");
            modelBuilder.Entity<Korisnik>().ToTable("Korisnik");
            modelBuilder.Entity<Ocjena>().ToTable("Ocjena");
            modelBuilder.Entity<Prijava>().ToTable("Prijava");
            modelBuilder.Entity<StudentNaPredmetu>().ToTable("StudentNaPredmetu");

            modelBuilder.Entity<Korisnik>()
                .HasKey(k => k.JMBG);

            modelBuilder.Entity<StudentskaSluzba>()
                .HasBaseType<Korisnik>();

            modelBuilder.Entity<Asistent>()
                .HasBaseType<Korisnik>();

            modelBuilder.Entity<Profesor>()
                .HasBaseType<Korisnik>();

            // Konfiguracija za Predmet -> Profesor
            modelBuilder.Entity<PredmetProfesor>()
            .HasOne(pp => pp.Profesor)
            .WithMany()
            .HasForeignKey(pp => pp.ProfesorId);

            modelBuilder.Entity<PredmetProfesor>()
                .HasOne(pp => pp.Predmet)
                .WithMany()
                .HasForeignKey(pp => pp.PredmetId);

            // Konfiguracija za Predmet -> Asistent
            modelBuilder.Entity<PredmetAsistent>()
            .HasOne(pp => pp.Asistent)
            .WithMany()
            .HasForeignKey(pp => pp.AsistentId);

            modelBuilder.Entity<PredmetAsistent>()
                .HasOne(pp => pp.Predmet)
                .WithMany()
                .HasForeignKey(pp => pp.PredmetId);


            // DeleteBehavior.Cascade -> brisanje entiteta koji ima referencu na drugi entitet

            // Konfiguracija za Uvjerenje -> Zahtjev
            modelBuilder.Entity<Uvjerenje>()
                .HasOne(u => u.StudentskaSluzba)
                .WithMany()
                .HasForeignKey(u => u.StudentskaSluzbaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za StudentskaSluzba -> Zahtjev
            modelBuilder.Entity<StudentskaSluzba>()
                .HasOne(s => s.Zahtjev)
                .WithMany()
                .HasForeignKey(s => s.ZahtjevId)
                .OnDelete(DeleteBehavior.Cascade);

            // DeleteBehavior.Restrict -> ne dozvoljava brisanje entiteta ako postoji referenca na njega

            // Konfiguracija za Ispit -> Asistent
            modelBuilder.Entity<Ispit>()
            .HasOne(i => i.Asistent)
            .WithMany()
            .HasForeignKey(i => i.AsistentId)
            .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Ispit -> Predmet
            modelBuilder.Entity<Ispit>()
            .HasOne(i => i.Predmet)
            .WithMany()
            .HasForeignKey(i => i.PredmetId)
            .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Ispit -> Profesor
            modelBuilder.Entity<Ispit>()
            .HasOne(i => i.Profesor)
            .WithMany()
            .HasForeignKey(i => i.ProfesorId)
            .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Predmet -> NastavniPlan
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.NastavniPlan)
                .WithMany()
                .HasForeignKey(p => p.NastavniPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Ocjena -> Predmet
            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Predmet)
                .WithMany()
                .HasForeignKey(o => o.PredmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Ocjena -> Student
            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Student)
                .WithMany()
                .HasForeignKey(o => o.brojIndeksa)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Ocjena -> NastavnoOsoblje
            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Profesor)
                .WithMany()
                .HasForeignKey(o => o.ProfesorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Zahtjev -> Student
            modelBuilder.Entity<Zahtjev>()
                .HasOne(z => z.Student)
                .WithMany()
                .HasForeignKey(z => z.brojIndeksa)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Zahtjev -> StudentskaSluzba
            modelBuilder.Entity<Zahtjev>()
                .HasOne(z => z.Student)
                .WithMany()
                .HasForeignKey(z => z.brojIndeksa)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za StudentskaSluzba -> Korisnik
            modelBuilder.Entity<StudentskaSluzba>()
                .HasOne<Korisnik>()
                .WithOne()
                .HasForeignKey<StudentskaSluzba>(s => s.JMBG)  
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Profesor -> Korisnik
            modelBuilder.Entity<Profesor>()
                .HasOne<Korisnik>()
                .WithOne()
                .HasForeignKey<Profesor>(p => p.JMBG)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Asistent -> Korisnik
            modelBuilder.Entity<Asistent>()
                .HasOne<Korisnik>()
                .WithOne()
                .HasForeignKey<Asistent>(a => a.JMBG)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Student -> Korisnik
            modelBuilder.Entity<Student>()
                .HasOne<Korisnik>()
                .WithOne()
                .HasForeignKey<Student>(s => s.JMBG)
                .OnDelete(DeleteBehavior.Restrict);

            // DeleteBehavior.NoAction -> ne dozvoljava brisanje entiteta ako postoji referenca na njega

            // Konfiguracija za Predmet -> Profesor
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.Profesor)
                .WithMany()
                .HasForeignKey(p => p.ProfesorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Predmet -> Asistent
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.Asistent)
                .WithMany()
                .HasForeignKey(p => p.AsistentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Predmet -> NastavniPlan
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.NastavniPlan)
                .WithMany()
                .HasForeignKey(p => p.NastavniPlanId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Obavještenje -> StudentskaSluzba
            modelBuilder.Entity<Obavjestenje>()
                .HasOne(o => o.StudentskaSluzba)
                .WithMany()
                .HasForeignKey(o => o.StudentskaSluzbaId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Obavještenje -> Profesor
            modelBuilder.Entity<Obavjestenje>()
                .HasOne(o => o.Profesor)
                .WithMany()
                .HasForeignKey(o => o.ProfesorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Obavještenje -> Asistent
            modelBuilder.Entity<Obavjestenje>()
                .HasOne(o => o.Asistent)
                .WithMany()
                .HasForeignKey(o => o.AsistentId)
                .OnDelete(DeleteBehavior.NoAction);
                base.OnModelCreating(modelBuilder);
        }
    }
}