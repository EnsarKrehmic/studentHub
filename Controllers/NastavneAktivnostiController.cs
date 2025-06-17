using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Route("NastavneAktivnosti")]
    public class NastavneAktivnostiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NastavneAktivnostiController> _logger;
        private readonly IWebHostEnvironment _environment;

        public NastavneAktivnostiController(ApplicationDbContext context, ILogger<NastavneAktivnostiController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // GET: NastavneAktivnosti/Index?predmetId=5
        [HttpGet("Index")]
        public async Task<IActionResult> Index(long predmetId)
        {
            var predmet = await _context.Predmeti.FindAsync(predmetId);
            if (predmet == null) return NotFound();

            var nastavneAktivnosti = _context.NastavneAktivnosti
                .Where(n => n.PredmetId == predmetId)
                    .Include(n => n.Predmet)
                    .Include(n => n.NastavniMaterijali)
                    .Include(n => n.Komentari)
                    .Include(n => n.Ocjene)
                    .OrderBy(n => n.DatumVrijemeOdrzavanja);

            ViewBag.PredmetNaziv = predmet.Naziv;
            ViewBag.PredmetId = predmetId;
            return View(await nastavneAktivnosti.ToListAsync());
        }

        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .Include(n => n.NastavniMaterijali)
                        .ThenInclude(m => m.Fajlovi)
                .Include(n => n.Komentari).ThenInclude(k => k.Student)
                .Include(n => n.Komentari).ThenInclude(k => k.Korisnik)
                .Include(n => n.Komentari).ThenInclude(k => k.VidljivostKorisnici)
                .Include(n => n.Ocjene).ThenInclude(o => o.Student)
                .Include(n => n.Prisustva).ThenInclude(p => p.Student)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nastavnaAktivnost == null) return NotFound();

            // Studentima zabrani pristup nedostupnoj aktivnosti
            if (!nastavnaAktivnost.JeDostupno && User.IsInRole("Student"))
            {
                TempData["Error"] = "Ova aktivnost još nije dostupna";
                return RedirectToAction("Details", "Predmet", new { id = nastavnaAktivnost.PredmetId });
            }

            // Svi studenti na predmetu
            var sviStudenti = await _context.StudentiNaPredmetima
                .Where(x => x.PredmetId == nastavnaAktivnost.PredmetId)
                .Include(x => x.Student)
                .Select(x => x.Student)
                .ToListAsync();

            // Prisustva i zahtjevi
            var prisustva = nastavnaAktivnost.Prisustva.Select(p => p.StudentId).ToHashSet();

            var zahtjevi = await _context.ZahtjeviZaPrisustvo
                .Include(z => z.Student)
                .Where(z => z.NastavnaAktivnostId == nastavnaAktivnost.Id && !z.Odbijen)
                .ToListAsync();

            var zahtjeviMap = zahtjevi.ToDictionary(z => z.StudentId, z => z);

            // Priprema statusa za sve studente
            var studentiSaStatusima = sviStudenti.Select(s =>
            {
                if (prisustva.Contains(s.Id))
                    return (student: s, status: "Prisutan", zahtjev: (ZahtjevZaPrisustvo?)null);
                else if (zahtjeviMap.ContainsKey(s.Id))
                    return (student: s, status: "Čeka potvrdu", zahtjev: zahtjeviMap[s.Id]);
                else
                    return (student: s, status: "Nije prisutan", zahtjev: (ZahtjevZaPrisustvo?)null);
            }).ToList();

            ViewBag.BrojUkupnoStudenata = sviStudenti.Count;
            ViewBag.BrojPrisutnih = prisustva.Count;
            ViewBag.StudentStatusi = studentiSaStatusima;

            // Status zahtjeva za trenutnog studenta
            if (User.IsInRole("Student"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

                // ... unutar if (User.IsInRole("Student"))
                if (student != null)
                {
                    var prijavljen = await _context.ZahtjeviZaPrisustvo
                        .Where(z => z.StudentId == student.Id && z.NastavnaAktivnostId == nastavnaAktivnost.Id)
                        .OrderByDescending(z => z.VrijemePodnosenja)
                        .FirstOrDefaultAsync();

                    if (prijavljen != null)
                    {
                        ViewBag.StatusZahtjeva = prijavljen.Odbijen
                            ? "Vaš prethodni zahtjev je odbijen. Možete ponovo poslati zahtjev."
                            : "Vaš zahtjev je već poslan i čeka potvrdu.";
                        ViewBag.PrisustvoPotvrđeno = false;
                    }
                    else if (prisustva.Contains(student.Id))
                    {
                        ViewBag.StatusZahtjeva = "Vaše prisustvo je potvrđeno.";
                        ViewBag.PrisustvoPotvrđeno = true;
                    }
                    else
                    {
                        ViewBag.PrisustvoPotvrđeno = false;
                    }
                }
            }

            ViewBag.TrenutniKorisnikId = GetTrenutniKorisnikId();

            return View(nastavnaAktivnost);
        }

        [HttpGet("Create")]
        [Authorize(Roles = "Profesor,Asistent")]
        public IActionResult Create(long predmetId)
        {
            var predmet = _context.Predmeti.Find(predmetId);
            if (predmet == null) return NotFound();

            var viewModel = new NastavnaAktivnostCreateViewModel
            {
                PredmetId = predmetId,
                DatumVrijemeOdrzavanja = DateTime.Now
            };

            ViewBag.PredmetNaziv = predmet.Naziv;
            return View(viewModel);
        }

        // POST: NastavneAktivnosti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Create(NastavnaAktivnostCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var nastavnaAktivnost = new NastavnaAktivnost
                {
                    Naziv = viewModel.Naziv,
                    Opis = viewModel.Opis,
                    Tip = viewModel.Tip,
                    DatumVrijemeOdrzavanja = viewModel.DatumVrijemeOdrzavanja,
                    ManuelnoOtkljucano = viewModel.ManuelnoOtkljucano,
                    PredmetId = viewModel.PredmetId
                };
                _context.Add(nastavnaAktivnost);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Nastavna aktivnost uspješno kreirana.";
                return RedirectToAction(nameof(Index), new { predmetId = viewModel.PredmetId });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
            TempData["Error"] = "Greška pri kreiranju: " + string.Join(", ", errors);

            ViewBag.PredmetNaziv = _context.Predmeti.Find(viewModel.PredmetId)?.Naziv;
            return View(viewModel);
        }

        // GET: NastavneAktivnosti/Edit/5
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (nastavnaAktivnost == null) return NotFound();

            ViewBag.PredmetId = nastavnaAktivnost.PredmetId;
            ViewBag.PredmetNaziv = nastavnaAktivnost.Predmet.Naziv;
            return View(nastavnaAktivnost);
        }

        // POST: NastavneAktivnosti/Edit/5
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Opis,Tip,DatumVrijemeOdrzavanja,ManuelnoOtkljucano,PredmetId")] NastavnaAktivnost nastavnaAktivnost)
        {
            if (id != nastavnaAktivnost.Id)
            {
                TempData["Error"] = "Neispravan ID aktivnosti";
                return RedirectToAction(nameof(Index));
            }

            var originalAktivnost = await _context.NastavneAktivnosti
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);

            if (originalAktivnost == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nastavnaAktivnost);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Promjene uspješno spremljene";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Konfliktn ažuriranje aktivnosti ID: {Id}", id);
                    TempData["Error"] = "Došlo je do konflikta prilikom ažuriranja";
                }
                return RedirectToAction(nameof(Index), new { predmetId = nastavnaAktivnost.PredmetId });
            }

            ViewBag.PredmetId = nastavnaAktivnost.PredmetId;
            ViewBag.PredmetNaziv = _context.Predmeti.Find(nastavnaAktivnost.PredmetId)?.Naziv;
            return View(nastavnaAktivnost);
        }

        // GET: NastavneAktivnosti/Delete/5
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nastavnaAktivnost == null) return NotFound();

            return View(nastavnaAktivnost);
        }

        // POST: NastavneAktivnosti/Delete/5
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.NastavniMaterijali)
                .Include(n => n.Komentari)
                .Include(n => n.Ocjene)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nastavnaAktivnost == null)
            {
                TempData["Error"] = "Aktivnost nije pronađena";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Brišemo sve povezane resurse
                _context.RemoveRange(nastavnaAktivnost.NastavniMaterijali);
                _context.RemoveRange(nastavnaAktivnost.Komentari);
                _context.RemoveRange(nastavnaAktivnost.Ocjene);

                _context.NastavneAktivnosti.Remove(nastavnaAktivnost);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Aktivnost i svi povezani resursi uspješno obrisani";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Greška pri brisanju aktivnosti ID: {Id}", id);
                TempData["Error"] = "Došlo je do greške prilikom brisanja aktivnosti";
            }

            return RedirectToAction(nameof(Index), new { predmetId = nastavnaAktivnost.PredmetId });
        }

        // POST: NastavneAktivnosti/ToggleLock/5
        [HttpPost("ToggleLock")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> ToggleLock(long id)
        {
            var aktivnost = await _context.NastavneAktivnosti.FindAsync(id);
            if (aktivnost == null) return NotFound();

            if (aktivnost.DatumVrijemeOdrzavanja > DateTime.Now)
            {
                aktivnost.ManuelnoOtkljucano = !aktivnost.ManuelnoOtkljucano;
            }
            else
            {
                aktivnost.ManuelnoZakljucano = !aktivnost.ManuelnoZakljucano;
            }

            _context.Update(aktivnost);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet("AddComment/{nastavnaAktivnostId}")]
        [Authorize(Roles = "Student, Profesor, Asistent")]
        public async Task<IActionResult> AddComment(long nastavnaAktivnostId)
        {
            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(n => n.Id == nastavnaAktivnostId);

            if (nastavnaAktivnost == null) return NotFound();

            if (!nastavnaAktivnost.JeDostupno && User.IsInRole("Student"))
            {
                TempData["Error"] = "Nastavna aktivnost još nije dostupna.";
                return RedirectToAction("Details", new { id = nastavnaAktivnostId });
            }

            var komentar = new Komentar
            {
                NastavnaAktivnostId = nastavnaAktivnostId
            };

            // Po defaultu prikazujemo sve korisnike da možeš napraviti izbor
            ViewBag.SviKorisnici = await DohvatiVidljiveKorisnike(nastavnaAktivnostId);

            return View(komentar);
        }

        [HttpPost("AddComment/{nastavnaAktivnostId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student, Profesor, Asistent")]
        public async Task<IActionResult> AddComment(long nastavnaAktivnostId, string sadrzaj, VidljivostKomentara vidljivost, List<long> odabraniKorisnici, IFormFile prilog)
        {
            var korisnikId = GetTrenutniKorisnikId();

            var komentar = new Komentar
            {
                Sadrzaj = sadrzaj,
                DatumVrijeme = DateTime.Now,
                KorisnikId = korisnikId,
                NastavnaAktivnostId = nastavnaAktivnostId,
                Vidljivost = vidljivost,
                MentionedUserId = null
            };

            if (prilog != null && prilog.Length > 0)
            {
                var folder = Path.Combine("wwwroot", "prilozi-nastavne-aktivnosti", nastavnaAktivnostId.ToString());
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(prilog.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await prilog.CopyToAsync(stream);
                }
                komentar.PrilogPath = $"/prilozi-nastavne-aktivnosti/{nastavnaAktivnostId}/{fileName}";
            }

            if (vidljivost == VidljivostKomentara.Privatno && odabraniKorisnici != null)
            {
                foreach (var korisnik in odabraniKorisnici)
                {
                    komentar.VidljivostKorisnici.Add(new KomentarVidljivost
                    {
                        KorisnikId = korisnik
                    });
                }
            }

            _context.Komentari.Add(komentar);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Komentar uspješno dodan.";

            return RedirectToAction("Details", new { id = nastavnaAktivnostId });
        }

        [HttpGet("EditKomentar/{id}")]
        [Authorize(Roles = "Student, Profesor, Asistent")]
        public async Task<IActionResult> EditKomentar(long id)
        {
            var komentar = await _context.Komentari
                .Include(k => k.VidljivostKorisnici)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (komentar == null)
            {
                return NotFound();
            }

            var korisnikId = GetTrenutniKorisnikId();

            if (komentar.KorisnikId != korisnikId && !User.IsInRole("Studentska služba"))
            {
                return Forbid();
            }

            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(n => n.Id == komentar.NastavnaAktivnostId);

            var predmetId = nastavnaAktivnost.PredmetId;

            // STUDENTI
            var studentiIds = await _context.StudentiNaPredmetima
                .Where(snp => snp.PredmetId == predmetId)
                .Select(snp => snp.StudentId)
                .ToListAsync();

            var studenti = await _context.Studenti
                .Where(s => studentiIds.Contains(s.Id))
                .Select(s => new StudentHub.Models.Korisnik
                {
                    Id = s.Id,
                    Ime = s.Ime,
                    Prezime = s.Prezime,
                    Uloga = Uloga.Student
                })
                .ToListAsync();

            // PROFESORI
            var profesori = await _context.PredmetProfesori
                .Where(pp => pp.PredmetId == predmetId)
                .Select(pp => new StudentHub.Models.Korisnik
                {
                    Id = pp.Profesor.Id,
                    Ime = pp.Profesor.Ime,
                    Prezime = pp.Profesor.Prezime,
                    Uloga = Uloga.Profesor
                })
                .ToListAsync();

            // ASISTENTI
            var asistenti = await _context.PredmetAsistenti
                .Where(pa => pa.PredmetId == predmetId)
                .Select(pa => new StudentHub.Models.Korisnik
                {
                    Id = pa.Asistent.Id,
                    Ime = pa.Asistent.Ime,
                    Prezime = pa.Asistent.Prezime,
                    Uloga = Uloga.Asistent
                })
                .ToListAsync();

            var sviKorisnici = studenti.Concat(profesori).Concat(asistenti).ToList();

            ViewBag.SviKorisnici = sviKorisnici;
            
            ModelState.Clear();
            return View(komentar);
        }

        [HttpPost("EditKomentar/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student, Profesor, Asistent")]
        public async Task<IActionResult> EditKomentar(long id, string sadrzaj, VidljivostKomentara vidljivost, List<long> odabraniKorisnici, IFormFile prilog)
        {
            var komentar = await _context.Komentari
                .Include(k => k.VidljivostKorisnici)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (komentar == null)
            {
                return NotFound();
            }

            var korisnikId = GetTrenutniKorisnikId();

            if (komentar.KorisnikId != korisnikId && !User.IsInRole("Studentska služba"))
            {
                return Forbid();
            }

            komentar.Sadrzaj = sadrzaj;
            komentar.Vidljivost = vidljivost;

            if (komentar.VidljivostKorisnici == null)
            {
                komentar.VidljivostKorisnici = new List<KomentarVidljivost>();
            }
            komentar.VidljivostKorisnici.Clear();

            if (vidljivost == VidljivostKomentara.Privatno && odabraniKorisnici != null)
            {
                foreach (var korisnik in odabraniKorisnici)
                {
                    komentar.VidljivostKorisnici.Add(new KomentarVidljivost
                    {
                        KorisnikId = korisnik
                    });
                }
            }

            if (prilog != null && prilog.Length > 0)
            {
                var folder = Path.Combine("wwwroot", "prilozi-nastavne-aktivnosti", komentar.NastavnaAktivnostId.ToString());
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(prilog.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await prilog.CopyToAsync(stream);
                }
                komentar.PrilogPath = $"/prilozi-nastavne-aktivnosti/{komentar.NastavnaAktivnostId}/{fileName}";
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Komentar uspješno ažuriran.";

            return RedirectToAction("Details", new { id = komentar.NastavnaAktivnostId });
        }

        [HttpPost("DeleteKomentar/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student, Profesor, Asistent", Policy = "MozeBrisatiKomentar")]
        public async Task<IActionResult> DeleteKomentar(long id)
        {
            var komentar = await _context.Komentari
                .Include(k => k.VidljivostKorisnici)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (komentar == null)
            {
                return NotFound();
            }

            var korisnikId = GetTrenutniKorisnikId();

            if (komentar.KorisnikId != korisnikId && !User.IsInRole("Studentska služba"))
            {
                return Forbid();
            }

            _context.Komentari.Remove(komentar);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Komentar uspješno obrisan.";

            return RedirectToAction("Details", new { id = komentar.NastavnaAktivnostId });
        }

        [HttpGet("GetVidljiviKorisnici/{nastavnaAktivnostId}")]
        public async Task<IActionResult> GetVidljiviKorisnici(long nastavnaAktivnostId)
        {
            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(n => n.Id == nastavnaAktivnostId);

            if (nastavnaAktivnost == null) return Json(new List<object>());

            var predmetId = nastavnaAktivnost.PredmetId;

            // STUDENTI
            var studentiIds = await _context.StudentiNaPredmetima
                .Where(snp => snp.PredmetId == predmetId)
                .Select(snp => snp.StudentId)
                .ToListAsync();

            var studenti = await _context.Studenti
                .Where(s => studentiIds.Contains(s.Id))
                .Select(s => new
                {
                    Id = s.Id,
                    Ime = s.Ime + " " + s.Prezime,
                    Uloga = "Student"
                })
                .ToListAsync();

            // PROFESORI
            var profesori = await _context.PredmetProfesori
                .Where(pp => pp.PredmetId == predmetId)
                .Select(pp => new
                {
                    Id = pp.Profesor.Id,
                    Ime = pp.Profesor.Ime + " " + pp.Profesor.Prezime,
                    Uloga = "Profesor"
                })
                .ToListAsync();

            // ASISTENTI
            var asistenti = await _context.PredmetAsistenti
                .Where(pa => pa.PredmetId == predmetId)
                .Select(pa => new
                {
                    Id = pa.Asistent.Id,
                    Ime = pa.Asistent.Ime + " " + pa.Asistent.Prezime,
                    Uloga = "Asistent"
                })
                .ToListAsync();

            return Json(await DohvatiVidljiveKorisnike(nastavnaAktivnostId));
        }

        [HttpGet("EvidentirajPrisustvo/{nastavnaAktivnostId}")]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> EvidentirajPrisustvo(long nastavnaAktivnostId)
        {
            var aktivnost = await _context.NastavneAktivnosti
                .Include(a => a.Predmet)
                .FirstOrDefaultAsync(a => a.Id == nastavnaAktivnostId);

            if (aktivnost == null) return NotFound();

            var studentiIds = await _context.StudentiNaPredmetima
                .Where(x => x.PredmetId == aktivnost.PredmetId)
                .Select(x => x.StudentId)
                .ToListAsync();

            var studenti = await _context.Studenti
                .Where(s => studentiIds.Contains(s.Id))
                .ToListAsync();

            var prisutniIds = await _context.PrisustvaNaAktivnostima
                .Where(p => p.NastavnaAktivnostId == nastavnaAktivnostId)
                .Select(p => p.StudentId)
                .ToListAsync();

            var viewModel = new EvidencijaPrisustvaViewModel
            {
                NastavnaAktivnost = aktivnost,
                Studenti = studenti,
                PrisutniStudentiIds = prisutniIds
            };

            return View("EvidentirajPrisustvo", viewModel);
        }

        [HttpPost("EvidentirajPrisustvo/{nastavnaAktivnostId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> EvidentirajPrisustvo(long nastavnaAktivnostId, List<long> prisutniStudentiIds)
        {
            var aktivnost = await _context.NastavneAktivnosti
                .FirstOrDefaultAsync(a => a.Id == nastavnaAktivnostId);

            if (aktivnost == null) return NotFound();

            var postojece = await _context.PrisustvaNaAktivnostima
                .Where(p => p.NastavnaAktivnostId == nastavnaAktivnostId)
                .ToListAsync();

            _context.PrisustvaNaAktivnostima.RemoveRange(postojece);

            foreach (var studentId in prisutniStudentiIds ?? new List<long>())
            {
                _context.PrisustvaNaAktivnostima.Add(new PrisustvoNaAktivnosti
                {
                    StudentId = studentId,
                    NastavnaAktivnostId = nastavnaAktivnostId,
                    VrijemeEvidentiranja = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Prisustvo uspješno evidentirano.";

            return RedirectToAction("Details", new { id = nastavnaAktivnostId });
        }

        [HttpPost("GenerisiKodPrisustva/{aktivnostId}")]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> GenerisiKodPrisustva(long aktivnostId)
        {
            var aktivnost = await _context.NastavneAktivnosti.FindAsync(aktivnostId);
            if (aktivnost == null) return NotFound();

            var random = new Random();
            aktivnost.KodZaPrisustvo = random.Next(100000, 999999).ToString();
            aktivnost.VrijemeGenerisanjaKoda = DateTime.Now;
            aktivnost.KodAktivanDo = DateTime.Now.AddMinutes(45);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Kod za prisustvo je generisan i važi do {aktivnost.KodAktivanDo.Value.ToString("HH:mm")}.";

            return RedirectToAction("Details", new { id = aktivnostId });
        }

        [HttpGet("PrijavaPrisustva/{nastavnaAktivnostId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> PrijavaPrisustva(long nastavnaAktivnostId)
        {
            var aktivnost = await _context.NastavneAktivnosti.FindAsync(nastavnaAktivnostId);
            if (aktivnost == null || !aktivnost.JeDostupno)
                return NotFound();

            ViewBag.NazivAktivnosti = aktivnost.Naziv;
            ViewBag.AktivnostId = nastavnaAktivnostId;
            return View();
        }

        [HttpPost("PrijavaPrisustva/{nastavnaAktivnostId}")]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrijavaPrisustva(long nastavnaAktivnostId, string kod)
        {
            var aktivnost = await _context.NastavneAktivnosti.FindAsync(nastavnaAktivnostId);
            if (aktivnost == null || !aktivnost.JeDostupno)
                return NotFound();

            // Provjera ispravnosti i trajanja koda
            if (string.IsNullOrEmpty(aktivnost.KodZaPrisustvo) || aktivnost.KodAktivanDo < DateTime.Now || kod != aktivnost.KodZaPrisustvo)
            {
                TempData["StatusZahtjeva"] = "Neispravan ili istekao kod.";
                return RedirectToAction("Details", new { id = nastavnaAktivnostId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);
            if (student == null) return Forbid();

            var postojećiZahtjev = await _context.ZahtjeviZaPrisustvo
                .FirstOrDefaultAsync(z => z.StudentId == student.Id && z.NastavnaAktivnostId == nastavnaAktivnostId);

            // Ako postoji aktivan zahtjev (nije odbijen) – ne dozvoljavamo ponovni unos
            if (postojećiZahtjev != null && !postojećiZahtjev.Odbijen)
            {
                TempData["StatusZahtjeva"] = "Vaš zahtjev za prisustvo je već poslan i čeka potvrdu.";
                return RedirectToAction("Details", new { id = nastavnaAktivnostId });
            }

            // Ako je prethodni zahtjev odbijen – brišemo ga i unosimo novi
            if (postojećiZahtjev != null && postojećiZahtjev.Odbijen)
            {
                _context.ZahtjeviZaPrisustvo.Remove(postojećiZahtjev);
            }

            _context.ZahtjeviZaPrisustvo.Add(new ZahtjevZaPrisustvo
            {
                StudentId = student.Id,
                NastavnaAktivnostId = nastavnaAktivnostId,
                KodUnesen = kod,
                VrijemePodnosenja = DateTime.Now,
                Odbijen = false
            });

            await _context.SaveChangesAsync();

            TempData["StatusZahtjeva"] = "Zahtjev za evidenciju prisustva je uspješno poslan i čeka potvrdu.";

            return RedirectToAction("Details", new { id = nastavnaAktivnostId });
        }

        [HttpPost("PotvrdiPrisustvoStudentu/{zahtjevId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> PotvrdiPrisustvoStudentu(long zahtjevId)
        {
            var zahtjev = await _context.ZahtjeviZaPrisustvo
                .Include(z => z.Student)
                .FirstOrDefaultAsync(z => z.Id == zahtjevId);

            if (zahtjev == null) return NotFound();

            var postoji = await _context.PrisustvaNaAktivnostima.AnyAsync(p =>
                p.NastavnaAktivnostId == zahtjev.NastavnaAktivnostId &&
                p.StudentId == zahtjev.StudentId);

            if (!postoji)
            {
                _context.PrisustvaNaAktivnostima.Add(new PrisustvoNaAktivnosti
                {
                    StudentId = zahtjev.StudentId,
                    NastavnaAktivnostId = zahtjev.NastavnaAktivnostId,
                    VrijemeEvidentiranja = DateTime.Now
                });
            }

            _context.ZahtjeviZaPrisustvo.Remove(zahtjev); // izbriši jer je potvrđen
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = zahtjev.NastavnaAktivnostId });
        }

        [HttpPost("OdbijPrisustvoStudentu/{zahtjevId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> OdbijPrisustvoStudentu(long zahtjevId)
        {
            var zahtjev = await _context.ZahtjeviZaPrisustvo.FindAsync(zahtjevId);
            if (zahtjev == null) return NotFound();

            zahtjev.Odbijen = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = zahtjev.NastavnaAktivnostId });
        }

        private async Task<List<object>> DohvatiVidljiveKorisnike(long nastavnaAktivnostId)
        {
            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(n => n.Id == nastavnaAktivnostId);

            if (nastavnaAktivnost == null) return new List<object>();

            var predmetId = nastavnaAktivnost.PredmetId;

            // STUDENTI
            var studentiIds = await _context.StudentiNaPredmetima
                .Where(snp => snp.PredmetId == predmetId)
                .Select(snp => snp.StudentId)
                .ToListAsync();

            var studenti = await _context.Studenti
                .Where(s => studentiIds.Contains(s.Id))
                .Select(s => new
                {
                    Id = s.Id,
                    Ime = s.Ime + " " + s.Prezime,
                    Uloga = "Student"
                })
                .ToListAsync();

            // PROFESORI
            var profesori = await _context.PredmetProfesori
                .Where(pp => pp.PredmetId == predmetId)
                .Select(pp => new
                {
                    Id = pp.Profesor.Id,
                    Ime = pp.Profesor.Ime + " " + pp.Profesor.Prezime,
                    Uloga = "Profesor"
                })
                .ToListAsync();

            // ASISTENTI
            var asistenti = await _context.PredmetAsistenti
                .Where(pa => pa.PredmetId == predmetId)
                .Select(pa => new
                {
                    Id = pa.Asistent.Id,
                    Ime = pa.Asistent.Ime + " " + pa.Asistent.Prezime,
                    Uloga = "Asistent"
                })
                .ToListAsync();

            var sviKorisnici = studenti.Concat(profesori).Concat(asistenti).ToList<object>();

            return sviKorisnici;
        }

        private long GetTrenutniKorisnikId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var korisnik = _context.Korisnici.FirstOrDefault(s => s.AspNetUserId == userId);
            return korisnik?.Id ?? 0;
        }

        private bool NastavnaAktivnostExists(long id)
        {
            return _context.NastavneAktivnosti.Any(e => e.Id == id);
        }
    }
}