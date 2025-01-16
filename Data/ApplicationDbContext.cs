using Microsoft.EntityFrameworkCore;
using StudentHub.Models;

namespace StudentHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Definicije DbSet za svaku klasu
        public DbSet<Asistent> Asistenti { get; set; }
        public DbSet<AsistentStudijskiProgram> AsistentStudijskiProgrami { get; set; }
        public DbSet<Dokument> Dokumenti { get; set; }
        public DbSet<Ispit> Ispiti { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<NastavniPlan> NastavniPlanovi { get; set; }
        public DbSet<Obavjestenje> Obavjestenja { get; set; }
        public DbSet<Ocjena> Ocjene { get; set; }
        public DbSet<Predmet> Predmeti { get; set; }
        public DbSet<PredmetAsistent> PredmetAsistenti { get; set; }
        public DbSet<PredmetProfesor> PredmetProfesori { get; set; }
        public DbSet<Prijava> Prijave { get; set; }
        public DbSet<Profesor> Profesori { get; set; }
        public DbSet<ProfesorStudijskiProgram> ProfesorStudijskiProgrami { get; set; }
        public DbSet<Student> Studenti { get; set; }
        public DbSet<StudentNaPredmetu> StudentiNaPredmetima { get; set; }
        public DbSet<StudentskaSluzba> StudentskeSluzbe { get; set; }
        public DbSet<StudijskiProgram> StudijskiProgrami { get; set; }
        public DbSet<Uvjerenje> Uvjerenja { get; set; }
        public DbSet<Zahtjev> Zahtjevi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AsistentStudijskiProgram>().ToTable("AsistentStudijskiProgram");
            modelBuilder.Entity<Dokument>().ToTable("Dokument");
            modelBuilder.Entity<Ispit>().ToTable("Ispit");
            modelBuilder.Entity<Korisnik>().ToTable("Korisnik");
            modelBuilder.Entity<NastavniPlan>().ToTable("NastavniPlan");
            modelBuilder.Entity<Obavjestenje>().ToTable("Obavjestenje");
            modelBuilder.Entity<Ocjena>().ToTable("Ocjena");
            modelBuilder.Entity<Predmet>().ToTable("Predmet");
            modelBuilder.Entity<PredmetAsistent>().ToTable("PredmetAsistent");
            modelBuilder.Entity<PredmetProfesor>().ToTable("PredmetProfesor");
            modelBuilder.Entity<ProfesorStudijskiProgram>().ToTable("ProfesorStudijskiProgram");
            modelBuilder.Entity<Prijava>().ToTable("Prijava");
            modelBuilder.Entity<StudentNaPredmetu>().ToTable("StudentNaPredmetu");
            modelBuilder.Entity<StudijskiProgram>().ToTable("StudijskiProgram");
            modelBuilder.Entity<Uvjerenje>().ToTable("Uvjerenje");
            modelBuilder.Entity<Zahtjev>().ToTable("Zahtjev");

            modelBuilder.Entity<Korisnik>()
                .HasKey(k => k.Id);

            modelBuilder.Entity<Korisnik>()
                .HasDiscriminator<Uloga>("Uloga")
                .HasValue<Korisnik>(Uloga.Osnovni)
                .HasValue<StudentskaSluzba>(Uloga.StudentskaSluzba)
                .HasValue<Student>(Uloga.Student)
                .HasValue<Profesor>(Uloga.Profesor)
                .HasValue<Asistent>(Uloga.Asistent);

            // Konfiguracija za Predmet -> Profesor
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.Profesor)
                .WithMany()
                .HasForeignKey(p => p.ProfesorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Predmet -> Asistent
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.Asistent)
                .WithMany()
                .HasForeignKey(p => p.AsistentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Predmet -> Profesor
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.Profesor)
                .WithMany()
                .HasForeignKey(p => p.ProfesorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Konfiguracija za PredmetProfesor -> Profesor
            modelBuilder.Entity<PredmetProfesor>()
                .HasOne(pp => pp.Profesor)
                .WithMany()
                .HasForeignKey(pp => pp.ProfesorId);

            // Konfiguracija za PredmetProfesor -> Predmet
            modelBuilder.Entity<PredmetProfesor>()
                .HasOne(pp => pp.Predmet)
                .WithMany()
                .HasForeignKey(pp => pp.PredmetId);

            modelBuilder.Entity<Profesor>()
                .Property(p => p.ProfesorTitula)
                .HasColumnName("ProfesorTitula");

            // Konfiguracija za PredmetAsistent -> Asistent
            modelBuilder.Entity<PredmetAsistent>()
                .HasOne(pa => pa.Asistent)
                .WithMany()
                .HasForeignKey(pa => pa.AsistentId);

            // Konfiguracija za PredmetAsistent -> Predmet
            modelBuilder.Entity<PredmetAsistent>()
                .HasOne(pa => pa.Predmet)
                .WithMany()
                .HasForeignKey(pa => pa.PredmetId);

            modelBuilder.Entity<Asistent>()
                .Property(a => a.AsistentTitula)
                .HasColumnName("AsistentTitula");

            // DeleteBehavior.Cascade -> brisanje entiteta koji ima referencu na drugi entitet

            // Konfiguracija za Uvjerenje -> Zahtjev
            modelBuilder.Entity<Uvjerenje>()
                .HasOne(u => u.StudentskaSluzba)
                .WithMany()
                .HasForeignKey(u => u.StudentskaSluzbaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za Predmet -> NastavniPlan
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.NastavniPlan)
                .WithMany()
                .HasForeignKey(p => p.NastavniPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za NastavniPlan -> StudijskiProgram
            modelBuilder.Entity<NastavniPlan>()
                .HasOne(np => np.StudijskiProgram)
                .WithMany()
                .HasForeignKey(np => np.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za StudentNaPredmetu -> Student
            modelBuilder.Entity<StudentNaPredmetu>()
                .HasOne(snp => snp.Student)
                .WithMany()
                .HasForeignKey(snp => snp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za StudentNaPredmetu -> Predmet
            modelBuilder.Entity<StudentNaPredmetu>()
                .HasOne(snp => snp.Predmet)
                .WithMany()
                .HasForeignKey(snp => snp.PredmetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za ProfesorStudijskiProgram -> Profesor
            modelBuilder.Entity<ProfesorStudijskiProgram>()
                .HasOne(psp => psp.Profesor)
                .WithMany()
                .HasForeignKey(psp => psp.ProfesorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za ProfesorStudijskiProgram -> StudijskiProgram
            modelBuilder.Entity<ProfesorStudijskiProgram>()
                .HasOne(psp => psp.StudijskiProgram)
                .WithMany()
                .HasForeignKey(psp => psp.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za AsistentStudijskiProgram -> Asistent
            modelBuilder.Entity<AsistentStudijskiProgram>()
                .HasOne(psp => psp.Asistent)
                .WithMany()
                .HasForeignKey(psp => psp.AsistentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za AsistentStudijskiProgram -> StudijskiProgram
            modelBuilder.Entity<AsistentStudijskiProgram>()
                .HasOne(psp => psp.StudijskiProgram)
                .WithMany()
                .HasForeignKey(psp => psp.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // DeleteBehavior.Restrict -> ne dozvoljava brisanje entiteta ako postoji referenca na njega

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
                .HasForeignKey(o => o.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Ocjena -> NastavnoOsoblje
            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Profesor)
                .WithMany()
                .HasForeignKey(o => o.ProfesorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Obavještenje -> StudijskiProgram
            modelBuilder.Entity<Obavjestenje>()
                .HasOne(o => o.StudijskiProgram)
                .WithMany()
                .HasForeignKey(o => o.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Zahtjev -> Student
            modelBuilder.Entity<Zahtjev>()
                .HasOne(z => z.Student)
                .WithMany()
                .HasForeignKey(z => z.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Zahtjev -> StudentskaSluzba
            modelBuilder.Entity<Zahtjev>()
                .HasOne(z => z.Student)
                .WithMany()
                .HasForeignKey(z => z.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za StudentskaSluzba -> Korisnik
            modelBuilder.Entity<StudentskaSluzba>()
                .HasOne<Korisnik>()
                .WithOne()
                .HasForeignKey<StudentskaSluzba>(s => s.Id)  
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Profesor -> Korisnik
            modelBuilder.Entity<Profesor>()
                .HasOne<Korisnik>()
                .WithOne()
                .HasForeignKey<Profesor>(p => p.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Asistent -> Korisnik
            modelBuilder.Entity<Asistent>()
                .HasOne<Korisnik>()
                .WithOne()
                .HasForeignKey<Asistent>(a => a.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Student -> Korisnik
            modelBuilder.Entity<Student>()
                .HasOne<Korisnik>()
                .WithOne()
                .HasForeignKey<Student>(s => s.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Student -> StudijskiProgram
            modelBuilder.Entity<Student>()
                .HasOne(s => s.StudijskiProgram)
                .WithMany()
                .HasForeignKey(s => s.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Student -> NastavniPlan
            modelBuilder.Entity<Student>()
                .HasOne(s => s.NastavniPlan)
                .WithMany()
                .HasForeignKey(s => s.NastavniPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Student -> Predmet
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Predmet)
                .WithMany()
                .HasForeignKey(s => s.PredmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // DeleteBehavior.NoAction -> ne dozvoljava brisanje entiteta ako postoji referenca na njega

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

            // Konfiguracija za Ispit -> Predmet
            modelBuilder.Entity<Ispit>()
                .HasOne(i => i.Predmet)
                .WithMany()
                .HasForeignKey(i => i.PredmetId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Ispit -> Profesor
            modelBuilder.Entity<Ispit>()
                .HasOne(i => i.Profesor)
                .WithMany()
                .HasForeignKey(i => i.ProfesorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Ispit -> Asistent
            modelBuilder.Entity<Ispit>()
                .HasOne(i => i.Asistent)
                .WithMany()
                .HasForeignKey(i => i.AsistentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Dokument -> Student
            modelBuilder.Entity<Dokument>()
                .HasOne(d => d.Student)
                .WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Dokument -> StudentskaSluzba
            modelBuilder.Entity<Dokument>()
                .HasOne(d => d.StudentskaSluzba)
                .WithMany()
                .HasForeignKey(d => d.StudentskaSluzbaId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Uvjerenje -> Student
            modelBuilder.Entity<Uvjerenje>()
                .HasOne(d => d.Student)
                .WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Uvjerenje -> StudentskaSluzba
            modelBuilder.Entity<Uvjerenje>()
                .HasOne(d => d.StudentskaSluzba)
                .WithMany()
                .HasForeignKey(d => d.StudentskaSluzbaId)
                .OnDelete(DeleteBehavior.NoAction);
            base.OnModelCreating(modelBuilder);
        }
    }
}