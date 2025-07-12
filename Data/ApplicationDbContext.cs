using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentHub.Models;

namespace StudentHub.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Definicije DbSet za svaku klasu
        public DbSet<Asistent> Asistenti { get; set; }
        public DbSet<AsistentStudijskiProgram> AsistentStudijskiProgrami { get; set; }
        public DbSet<Dokument> Dokumenti { get; set; }
        public DbSet<DokumentSlike> DokumentSlike { get; set; }
        public DbSet<Ispit> Ispiti { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Komentar> Komentari { get; set; }
        public DbSet<KomentarVidljivost> KomentarVidljivosti { get; set; }
        public DbSet<NastavniPlan> NastavniPlanovi { get; set; }
        public DbSet<NastavnaAktivnost> NastavneAktivnosti { get; set; }
        public DbSet<NastavniMaterijal> NastavniMaterijali { get; set; }
        public DbSet<NastavniMaterijalFajl> NastavniMaterijalFajlovi { get; set; }
        public DbSet<Obavjestenje> Obavjestenja { get; set; }
        public DbSet<ObavjestenjeStudijskiProgram> ObavjestenjeStudijskiProgrami { get; set; }
        public DbSet<Ocjena> Ocjene { get; set; }
        public DbSet<Predmet> Predmeti { get; set; }
        public DbSet<PredmetAsistent> PredmetAsistenti { get; set; }
        public DbSet<PredmetProfesor> PredmetProfesori { get; set; }
        public DbSet<Prijava> Prijave { get; set; }
        public DbSet<Profesor> Profesori { get; set; }
        public DbSet<ProfesorStudijskiProgram> ProfesorStudijskiProgrami { get; set; }
        public DbSet<PrisustvoNaAktivnosti> PrisustvaNaAktivnostima { get; set; }
        public DbSet<Raspored> Rasporedi { get; set; }
        public DbSet<Student> Studenti { get; set; }
        public DbSet<StudentStudijskiProgram> StudentStudijskiProgrami { get; set; }
        public DbSet<StudentNaPredmetu> StudentiNaPredmetima { get; set; }
        public DbSet<StudentskaSluzba> StudentskeSluzbe { get; set; }
        public DbSet<StudentskaSluzbaStudijskiProgram> StudentskaSluzbaStudijskiProgrami { get; set; }
        public DbSet<StudijskiProgram> StudijskiProgrami { get; set; }
        public DbSet<StudijskiProgramIzborniLimit> StudijskiProgramIzborniLimiti { get; set; }
        public DbSet<TerminNastave> TerminiNastave { get; set; }
        public DbSet<Uvjerenje> Uvjerenja { get; set; }
        public DbSet<Zahtjev> Zahtjevi { get; set; }
        public DbSet<ZahtjevZaPrisustvo> ZahtjeviZaPrisustvo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AsistentStudijskiProgram>().ToTable("AsistentStudijskiProgram");
            modelBuilder.Entity<Dokument>().ToTable("Dokument");
            modelBuilder.Entity<DokumentSlike>().ToTable("DokumentSlike");
            modelBuilder.Entity<Ispit>().ToTable("Ispit");
            modelBuilder.Entity<Korisnik>().ToTable("Korisnik");
            modelBuilder.Entity<Komentar>().ToTable("Komentar");
            modelBuilder.Entity<NastavniPlan>().ToTable("NastavniPlan");
            modelBuilder.Entity<NastavnaAktivnost>().ToTable("NastavnaAktivnost");
            modelBuilder.Entity<NastavniMaterijal>().ToTable("NastavniMaterijal");
            modelBuilder.Entity<NastavniMaterijalFajl>().ToTable("NastavniMaterijalFajl");
            modelBuilder.Entity<Obavjestenje>().ToTable("Obavjestenje");
            modelBuilder.Entity<ObavjestenjeStudijskiProgram>().ToTable("ObavjestenjeStudijskiProgram");
            modelBuilder.Entity<Ocjena>().ToTable("Ocjena");
            modelBuilder.Entity<Predmet>().ToTable("Predmet");
            modelBuilder.Entity<PredmetAsistent>().ToTable("PredmetAsistent");
            modelBuilder.Entity<PredmetProfesor>().ToTable("PredmetProfesor");
            modelBuilder.Entity<Prijava>().ToTable("Prijava");
            modelBuilder.Entity<ProfesorStudijskiProgram>().ToTable("ProfesorStudijskiProgram");
            modelBuilder.Entity<PrisustvoNaAktivnosti>().ToTable("PrisustvoNaAktivnosti");
            modelBuilder.Entity<Raspored>().ToTable("Raspored");
            modelBuilder.Entity<StudentStudijskiProgram>().ToTable("StudentStudijskiProgram");
            modelBuilder.Entity<StudentNaPredmetu>().ToTable("StudentNaPredmetu");
            modelBuilder.Entity<StudentskaSluzbaStudijskiProgram>().ToTable("StudentskaSluzbaStudijskiProgram");
            modelBuilder.Entity<StudijskiProgram>().ToTable("StudijskiProgram");
            modelBuilder.Entity<StudijskiProgramIzborniLimit>().ToTable("StudijskiProgramIzborniLimit");
            modelBuilder.Entity<TerminNastave>().ToTable("TerminNastave");
            modelBuilder.Entity<Uvjerenje>().ToTable("Uvjerenje");
            modelBuilder.Entity<Zahtjev>().ToTable("Zahtjev");
            modelBuilder.Entity<ZahtjevZaPrisustvo>().ToTable("ZahtjevZaPrisustvo");

            modelBuilder.Entity<Korisnik>()
                .HasKey(k => k.Id);

            modelBuilder.Entity<Korisnik>()
                .HasDiscriminator<Uloga>("Uloga")
                .HasValue<Korisnik>(Uloga.Osnovni)
                .HasValue<StudentskaSluzba>(Uloga.StudentskaSluzba)
                .HasValue<Student>(Uloga.Student)
                .HasValue<Profesor>(Uloga.Profesor)
                .HasValue<Asistent>(Uloga.Asistent);

            modelBuilder.Entity<Ispit>()
                .Property(i => i.BrojBodova)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Ispit>()
                .Property(i => i.UslovZaPolaganje)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Prijava>()
                .Property(p => p.Bodovi)
                .HasPrecision(18, 2);

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

            // Konfiguracija za PredmetProfesor -> Profesor
            modelBuilder.Entity<PredmetProfesor>()
                .HasOne(pp => pp.Profesor)
                .WithMany(p => p.Predmeti)
                .HasForeignKey(pp => pp.ProfesorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za PredmetProfesor -> Predmet
            modelBuilder.Entity<PredmetProfesor>()
                .HasOne(pp => pp.Predmet)
                .WithMany(p => p.PredmetProfesori)
                .HasForeignKey(pp => pp.PredmetId);

            modelBuilder.Entity<Profesor>()
                .Property(p => p.ProfesorTitula)
                .HasColumnName("ProfesorTitula");

            // Konfiguracija za PredmetAsistent -> Asistent
            modelBuilder.Entity<PredmetAsistent>()
                .HasOne(pa => pa.Asistent)
                .WithMany(a => a.Predmeti)
                .HasForeignKey(pa => pa.AsistentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za PredmetAsistent -> Predmet
            modelBuilder.Entity<PredmetAsistent>()
                .HasOne(pa => pa.Predmet)
                .WithMany(p => p.PredmetAsistenti)
                .HasForeignKey(pa => pa.PredmetId);

            modelBuilder.Entity<Asistent>()
                .Property(a => a.AsistentTitula)
                .HasColumnName("AsistentTitula");

            // DeleteBehavior.Cascade -> brisanje entiteta koji ima referencu na drugi entitet

            // Konfiguracija za StudentNaPredmetu -> Student
            modelBuilder.Entity<StudentNaPredmetu>()
                .HasOne(snp => snp.Student)
                .WithMany(s => s.StudentNaPredmetima)
                .HasForeignKey(snp => snp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za StudentNaPredmetu -> Predmet
            modelBuilder.Entity<StudentNaPredmetu>()
                .HasOne(snp => snp.Predmet)
                .WithMany(s => s.StudentNaPredmetima)
                .HasForeignKey(snp => snp.PredmetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za ProfesorStudijskiProgram -> Profesor
            modelBuilder.Entity<ProfesorStudijskiProgram>()
                .HasOne(psp => psp.Profesor)
                .WithMany(p => p.ProfesorStudijskiProgrami)
                .HasForeignKey(psp => psp.ProfesorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za ProfesorStudijskiProgram -> StudijskiProgram
            modelBuilder.Entity<ProfesorStudijskiProgram>()
                .HasOne(psp => psp.StudijskiProgram)
                .WithMany(sp => sp.ProfesorStudijskiProgrami)
                .HasForeignKey(psp => psp.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za AsistentStudijskiProgram -> Asistent
            modelBuilder.Entity<AsistentStudijskiProgram>()
                .HasOne(asp => asp.Asistent)
                .WithMany(a => a.AsistentStudijskiProgrami)
                .HasForeignKey(asp => asp.AsistentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za AsistentStudijskiProgram -> StudijskiProgram
            modelBuilder.Entity<AsistentStudijskiProgram>()
                .HasOne(asp => asp.StudijskiProgram)
                .WithMany(sp => sp.AsistentStudijskiProgrami)
                .HasForeignKey(asp => asp.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za StudentStudijskiProgram -> Student
            modelBuilder.Entity<StudentStudijskiProgram>()
                .HasOne(ssp => ssp.Student)
                .WithMany(s => s.StudentStudijskiProgrami)
                .HasForeignKey(ssp => ssp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za StudentStudijskiProgram -> StudijskiProgram
            modelBuilder.Entity<StudentStudijskiProgram>()
                .HasOne(ssp => ssp.StudijskiProgram)
                .WithMany(sp => sp.StudentStudijskiProgrami)
                .HasForeignKey(ssp => ssp.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za StudentskaSluzbaStudijskiProgram -> StudentskaSluzba
            modelBuilder.Entity<StudentskaSluzbaStudijskiProgram>()
                .HasOne(sssp => sssp.StudentskaSluzba)
                .WithMany(ss => ss.StudentskaSluzbaStudijskiProgrami)
                .HasForeignKey(sssp => sssp.StudentskaSluzbaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za StudentskaSluzbaStudijskiProgram -> StudijskiProgram
            modelBuilder.Entity<StudentskaSluzbaStudijskiProgram>()
                .HasOne(sssp => sssp.StudijskiProgram)
                .WithMany(sp => sp.StudentskaSluzbaStudijskiProgrami)
                .HasForeignKey(sssp => sssp.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacija ObavjestenjeStudijskiProgram → Obavjestenje
            modelBuilder.Entity<ObavjestenjeStudijskiProgram>()
                .HasOne(osp => osp.Obavjestenje)
                .WithMany(o => o.ObavjestenjeStudijskiProgrami)
                .HasForeignKey(osp => osp.ObavjestenjeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacija ObavjestenjeStudijskiProgram → StudijskiProgram
            modelBuilder.Entity<ObavjestenjeStudijskiProgram>()
                .HasOne(osp => osp.StudijskiProgram)
                .WithMany(sp => sp.ObavjestenjeStudijskiProgrami)
                .HasForeignKey(osp => osp.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za Predmet -> NastavniPlan
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.NastavniPlan)
                .WithMany()
                .HasForeignKey(p => p.NastavniPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za Predmet -> StudijskiProgram
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.StudijskiProgram)
                .WithMany()
                .HasForeignKey(p => p.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za NastavniPlan -> StudijskiProgram
            modelBuilder.Entity<NastavniPlan>()
                .HasOne(np => np.StudijskiProgram)
                .WithMany()
                .HasForeignKey(np => np.StudijskiProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // DeleteBehavior.Restrict -> ne dozvoljava brisanje entiteta ako postoji referenca na njega

            // Konfiguracija za Ocjena -> Predmet
            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Predmet)
                .WithMany()
                .HasForeignKey(o => o.PredmetId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // PredmetId je opcionalan

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
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // ProfesorId je opcionalan

            // Konfiguracija za Ocjena -> NastavnaAktivnost
            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.NastavnaAktivnost)
                .WithMany(na => na.Ocjene)
                .HasForeignKey(o => o.NastavnaAktivnostId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // NastavnaAktivnostId je opcionalan

            // Konfiguracija za NastavnaAktivnost -> Predmet
            modelBuilder.Entity<NastavnaAktivnost>()
                .HasOne(na => na.Predmet)
                .WithMany(p => p.NastavneAktivnosti)
                .HasForeignKey(na => na.PredmetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za NastavniMaterijal -> NastavnaAktivnost
            modelBuilder.Entity<NastavniMaterijal>()
                .HasOne(nm => nm.NastavnaAktivnost)
                .WithMany(na => na.NastavniMaterijali)
                .HasForeignKey(nm => nm.NastavnaAktivnostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za Komentar -> NastavnaAktivnost
            modelBuilder.Entity<Komentar>()
                .HasOne(k => k.NastavnaAktivnost)
                .WithMany(na => na.Komentari)
                .HasForeignKey(k => k.NastavnaAktivnostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Autor komentara (Korisnik)
            modelBuilder.Entity<Komentar>()
                .HasOne(k => k.Korisnik)
                .WithMany()
                .HasForeignKey(k => k.KorisnikId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student na koga se komentar odnosi
            modelBuilder.Entity<Komentar>()
                .HasOne(k => k.Student)
                .WithMany()
                .HasForeignKey(k => k.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za KomentarVidljivost
            modelBuilder.Entity<KomentarVidljivost>()
                .HasKey(kv => new { kv.KomentarId, kv.KorisnikId });

            modelBuilder.Entity<Komentar>()
                .HasOne(k => k.Ispit)
                .WithMany(i => i.Komentari)
                .HasForeignKey(k => k.IspitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za Zahtjev -> Student
            modelBuilder.Entity<Zahtjev>()
                .HasOne(z => z.Student)
                .WithMany()
                .HasForeignKey(z => z.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Uvjerenje -> Student
            modelBuilder.Entity<Uvjerenje>()
                .HasOne(u => u.Student)
                .WithMany()
                .HasForeignKey(u => u.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracija za Uvjerenje -> StudentskaSluzba
            modelBuilder.Entity<Uvjerenje>()
                .HasOne(u => u.StudentskaSluzba)
                .WithMany()
                .HasForeignKey(u => u.StudentskaSluzbaId)
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

            // Konfiguracija za Student -> NastavniPlan
            modelBuilder.Entity<Student>()
                .HasOne(s => s.NastavniPlan)
                .WithMany()
                .HasForeignKey(s => s.NastavniPlanId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // Konfiguracija za Ispit -> StudijskiProgram
            modelBuilder.Entity<Ispit>()
                .HasOne(i => i.StudijskiProgram)
                .WithMany()
                .HasForeignKey(i => i.StudijskiProgramId)
                .OnDelete(DeleteBehavior.NoAction);

            // Konfiguracija za Ispit -> NastavniPlan
            modelBuilder.Entity<Ispit>()
                .HasOne(i => i.NastavniPlan)
                .WithMany()
                .HasForeignKey(i => i.NastavniPlanId)
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

            // Konfiguracija za Raspored -> TerminiNastave
            modelBuilder.Entity<TerminNastave>()
                .HasOne(t => t.Raspored)
                .WithMany(r => r.Termini)
                .HasForeignKey(t => t.RasporedId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}