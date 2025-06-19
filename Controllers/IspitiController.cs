using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using Microsoft.AspNetCore.SignalR;
using StudentHub.Hubs;

namespace StudentHub.Controllers
{
    [Route("Ispiti")]
    public class IspitiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IspitiController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<ExamHub> _hubContext;

        public IspitiController(ApplicationDbContext context, ILogger<IspitiController> logger, UserManager<IdentityUser> userManager, IHubContext<ExamHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Index(string sortOrder, bool? showArchived, string searchPredmet = "")
        {
            var studijskiProgrami = await _context.StudijskiProgrami.ToListAsync();
            var nastavniPlanovi = await _context.NastavniPlanovi.ToListAsync();
            var predmeti = await _context.Predmeti.ToListAsync();
            var ispiti = await _context.Ispiti.ToListAsync();
            bool showArchivedBool = showArchived ?? false;

            var userId = _userManager.GetUserId(User);

            // Filtriranje po ulozi
            if (User.IsInRole("Studentska služba"))
            {
                // Studentska služba vidi ispite zavisno od filtera
                if (!showArchivedBool)
                {
                    ispiti = ispiti.Where(i => !i.Arhivirano).ToList();
                }
                // ako je showArchived == true → prikazuje sve ispite (ne filtrira dodatno)

                if (!string.IsNullOrEmpty(searchPredmet))
                {
                    predmeti = predmeti.Where(p => p.Naziv.Contains(searchPredmet, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return View(await CreateViewModel(studijskiProgrami, nastavniPlanovi, predmeti, ispiti, sortOrder));
            }

            if (User.IsInRole("Profesor"))
            {
                // Profesori vide samo svoje predmete
                var predmetiIds = await _context.PredmetProfesori
                    .Where(pp => pp.Profesor.AspNetUserId == userId)
                    .Select(pp => pp.PredmetId)
                    .ToListAsync();

                if (!string.IsNullOrEmpty(searchPredmet))
                {
                    predmeti = predmeti.Where(p => p.Naziv.Contains(searchPredmet, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!showArchivedBool)
                {
                    ispiti = ispiti.Where(i => predmetiIds.Contains(i.PredmetId) && !i.Arhivirano).ToList();
                }
                else
                {
                    ispiti = ispiti.Where(i => predmetiIds.Contains(i.PredmetId)).ToList();
                }
            }

            if (User.IsInRole("Asistent"))
            {
                // Asistenti vide samo svoje predmete
                var predmetiIds = await _context.PredmetAsistenti
                    .Where(pa => pa.Asistent.AspNetUserId == userId)
                    .Select(pa => pa.PredmetId)
                    .ToListAsync();

                if (!string.IsNullOrEmpty(searchPredmet))
                {
                    predmeti = predmeti.Where(p => p.Naziv.Contains(searchPredmet, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!showArchivedBool)
                {
                    ispiti = ispiti.Where(i => predmetiIds.Contains(i.PredmetId) && !i.Arhivirano).ToList();
                }
                else
                {
                    ispiti = ispiti.Where(i => predmetiIds.Contains(i.PredmetId)).ToList();
                }
            }

            if (User.IsInRole("Student"))
            {
                // Studenti vide samo svoje predmete i samo ispite koji NISU arhivirani
                var predmetiIds = await _context.StudentiNaPredmetima
                    .Where(snp => snp.Student.AspNetUserId == userId)
                    .Select(snp => snp.PredmetId)
                    .ToListAsync();

                predmeti = predmeti.Where(p => predmetiIds.Contains(p.Id)).ToList();
                ispiti = ispiti.Where(i => predmetiIds.Contains(i.PredmetId) && !i.Arhivirano).ToList();
            }
            ViewBag.ShowArchived = showArchivedBool;
            return View(await CreateViewModel(studijskiProgrami, nastavniPlanovi, predmeti, ispiti, sortOrder));
        }

        private async Task<List<IspitDetailsViewModel>> CreateViewModel(
            List<StudijskiProgram> studijskiProgrami,
            List<NastavniPlan> nastavniPlanovi,
            List<Predmet> predmeti,
            List<Ispit> ispiti,
            string sortOrder)
        {
            var userId = _userManager.GetUserId(User);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            var ocjene = new Dictionary<long?, Ocjena>();
            var prijavljeniIspitiIds = new List<long>();

            // Id-jevi ispita koje prikazujemo
            var ispitIds = ispiti.Select(i => i.Id).ToList();

            // SVE Prijave za prikazane ispite
            var prijave = await _context.Prijave
                .Where(p => ispitIds.Contains(p.IspitId))
                .ToListAsync();

            if (student != null)
            {
                // OCJENE studenta
                ocjene = await _context.Ocjene
                    .Where(o => o.StudentId == student.Id)
                    .ToDictionaryAsync(o => o.PredmetId, o => o);

                // PRIJAVLJENI ISPITI za studenta
                prijavljeniIspitiIds = prijave
                    .Where(p => p.StudentId == student.Id)
                    .Select(p => p.IspitId)
                    .ToList();
            }

            // ViewModel punjenje
            var viewModel = studijskiProgrami.Select(sp => new IspitDetailsViewModel
            {
                StudijskiProgram = sp,
                NastavniPlanovi = nastavniPlanovi
                    .Where(np => np.StudijskiProgramId == sp.Id && ispiti.Any(i => i.NastavniPlanId == np.Id))
                    .Select(np => new NastavniPlanIspitViewModel
                    {
                        NastavniPlan = np,
                        Predmeti = predmeti
                        .Where(p => p.NastavniPlanId == np.Id && ispiti.Any(i => i.PredmetId == p.Id))
                        .Select(p => new PredmetIspitViewModel
                        {
                            Predmet = p,
                            Ispiti = ispiti
                                .Where(i => i.PredmetId == p.Id)
                                .ToList()
                        })
                        .ToList()
                    })
                    .ToList(),
                CurrentSort = sortOrder,
                DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "",
                LocationSortParm = sortOrder == "Location" ? "location_desc" : "Location",
                PointsSortParm = sortOrder == "Points" ? "points_desc" : "Points",
                Ocjene = ocjene,
                PrijavljeniIspitiIds = prijavljeniIspitiIds,
                Prijave = prijave
            }).ToList();

            // Sortiranje ispita
            foreach (var program in viewModel)
            {
                foreach (var plan in program.NastavniPlanovi)
                {
                    foreach (var predmet in plan.Predmeti)
                    {
                        predmet.Ispiti = predmet.Ispiti switch
                        {
                            var list when sortOrder == "date_desc" => list.OrderByDescending(i => i.DatumOdrzavanja).ToList(),
                            var list when sortOrder == "Location" => list.OrderBy(i => i.Lokacija).ToList(),
                            var list when sortOrder == "location_desc" => list.OrderByDescending(i => i.Lokacija).ToList(),
                            var list when sortOrder == "Points" => list.OrderBy(i => i.BrojBodova).ToList(),
                            var list when sortOrder == "points_desc" => list.OrderByDescending(i => i.BrojBodova).ToList(),
                            _ => predmet.Ispiti.OrderBy(i => i.DatumOdrzavanja).ToList()
                        };
                    }
                }
            }

            return viewModel;
        }

        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .Include(i => i.StudijskiProgram)
                .Include(i => i.Komentari)
                    .ThenInclude(k => k.Korisnik)
                .Include(i => i.Komentari)
                    .ThenInclude(k => k.VidljivostKorisnici)
                .Include(i => i.Prijave)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ispit == null)
            {
                return NotFound();
            }

            // Dobavljanje trenutnog studenta
            var userId = _userManager.GetUserId(User);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);
            // Dobavljanje trenutnog korisnika
            var korisnikId = GetTrenutniKorisnikId();
            ViewBag.TrenutniKorisnikId = korisnikId;
            bool prijavljen = student != null && await _context.Prijave
                .AnyAsync(p => p.StudentId == student.Id && p.IspitId == ispit.Id);

            // Dohvaćanje prijavljenih studenata
            var prijavljeniStudenti = await _context.Prijave
                .Where(p => p.IspitId == id)
                .Select(p => p.Student)
                .ToListAsync();

            // Dohvaćanje bodova studenta
            var prijava = student != null ? await _context.Prijave
                .FirstOrDefaultAsync(p => p.StudentId == student.Id && p.IspitId == ispit.Id) : null;
            var bodovi = prijava?.Bodovi;

            var viewModel = new IspitDetailsViewModel
            {
                IspitId = ispit.Id,
                Arhivirano = ispit.Arhivirano,
                StudijskiProgram = ispit.StudijskiProgram,
                Predmeti = new List<PredmetIspitViewModel>
                {
                    new PredmetIspitViewModel
                    {
                        Predmet = ispit.Predmet,
                        Ispiti = new List<Ispit> { ispit }
                    }
                },
                BrojBodova = ispit.BrojBodova,
                UslovZaPolaganje = ispit.UslovZaPolaganje,
                Prijavljen = prijavljen,
                PrijavljeniStudenti = prijavljeniStudenti,
                Bodovi = bodovi,
                Komentari = ispit.Komentari.ToList(),
                Prijave = ispit.Prijave.ToList()
            };

            ViewBag.JePrijavljen = prijavljen;

            return View(viewModel);
        }

        // GET: Ispiti/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public IActionResult Create()
        {
            try
            {
                ViewBag.StudijskiProgramId = _context.StudijskiProgrami
                    .Select(sp => new SelectListItem
                    {
                        Value = sp.Id.ToString(),
                        Text = $"Studijski program: {sp.Naziv}."
                    }).ToList();
                ViewBag.NastavniPlanId = _context.NastavniPlanovi
                    .Include(np => np.StudijskiProgram)
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godina"
                    }).ToList();
                ViewBag.PredmetId = _context.Predmeti
                    .Include(p => p.NastavniPlan)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"Predmet: {p.Naziv}: ({p.NastavniPlan.StudijskiProgram.Naziv})"
                    }).ToList();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nastala je greška prilikom učitavanja forme za kreiranje ispita..");
                return View("Error");
            }
        }

        // POST: Ispiti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Create(IspitCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning("Missing or invalid attribute: {Key}", state.Key);
                    }
                }

                // Repopulate dropdowns
                ViewBag.StudijskiProgramId = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);
                ViewBag.NastavniPlanId = new SelectList(
                    _context.NastavniPlanovi.Where(np => np.StudijskiProgramId == model.StudijskiProgramId),
                    "Id",
                    "GodinaStudija",
                    model.NastavniPlanId
                );
                ViewBag.PredmetId = new SelectList(
                    _context.Predmeti.Where(p => p.NastavniPlanId == model.NastavniPlanId),
                    "Id",
                    "Naziv",
                    model.PredmetId
                );
                return View(model);
            }

            var ispit = new Ispit
            {
                StudijskiProgramId = model.StudijskiProgramId,
                NastavniPlanId = model.NastavniPlanId,
                PredmetId = model.PredmetId,
                DatumOdrzavanja = model.DatumOdrzavanja,
                Lokacija = model.Lokacija,
                BrojBodova = model.BrojBodova,
                UslovZaPolaganje = model.UslovZaPolaganje
            };

            _context.Add(ispit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ispit = await _context.Ispiti
                .Include(i => i.StudijskiProgram)
                .Include(i => i.NastavniPlan)
                .Include(i => i.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ispit == null)
            {
                return NotFound();
            }

            var model = new IspitCreateViewModel
            {
                StudijskiProgramId = ispit.StudijskiProgramId,
                NastavniPlanId = ispit.NastavniPlanId,
                PredmetId = ispit.PredmetId,
                DatumOdrzavanja = ispit.DatumOdrzavanja,
                Lokacija = ispit.Lokacija,
                BrojBodova = ispit.BrojBodova,
                UslovZaPolaganje = ispit.UslovZaPolaganje
            };

            ViewBag.StudijskiProgramId = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", ispit.StudijskiProgramId);
            ViewBag.NastavniPlanId = new SelectList(_context.NastavniPlanovi.Where(np => np.StudijskiProgramId == ispit.StudijskiProgramId), "Id", "GodinaStudija", ispit.NastavniPlanId);
            ViewBag.PredmetId = new SelectList(_context.Predmeti.Where(p => p.NastavniPlanId == ispit.NastavniPlanId), "Id", "Naziv", ispit.PredmetId);

            return View(model);
        }

        // POST: Ispiti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Edit(long id, IspitCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudijskiProgramId = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);
                ViewBag.NastavniPlanId = new SelectList(_context.NastavniPlanovi.Where(np => np.StudijskiProgramId == model.StudijskiProgramId), "Id", "GodinaStudija", model.NastavniPlanId);
                ViewBag.PredmetId = new SelectList(_context.Predmeti.Where(p => p.NastavniPlanId == model.NastavniPlanId), "Id", "Naziv", model.PredmetId);
                return View(model);
            }

            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit == null)
            {
                return NotFound();
            }

            ispit.StudijskiProgramId = model.StudijskiProgramId;
            ispit.NastavniPlanId = model.NastavniPlanId;
            ispit.PredmetId = model.PredmetId;
            ispit.DatumOdrzavanja = model.DatumOdrzavanja;
            ispit.Lokacija = model.Lokacija;
            ispit.BrojBodova = model.BrojBodova;
            ispit.UslovZaPolaganje = model.UslovZaPolaganje;

            _context.Update(ispit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Ispiti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .Include(i => i.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ispit == null) return NotFound();
            return View(ispit);
        }

        // POST: Ispiti/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit != null)
            {
                _context.Ispiti.Remove(ispit);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Prijavi/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Prijavi(long id)
        {
            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ispit == null)
            {
                return NotFound("Ispit ne postoji.");
            }

            if (ispit.DatumOdrzavanja.AddDays(-3) <= DateTime.Now)
            {
                return BadRequest("Rok za prijavu ispita je istekao.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            if (student == null)
            {
                return BadRequest("Ne možete se prijaviti na ispit jer niste registrovani kao student.");
            }

            // PROVJERA DA LI IMA OCJENU
            var ocjena = await _context.Ocjene
                .FirstOrDefaultAsync(o => o.StudentId == student.Id && o.PredmetId == ispit.PredmetId);

            if (ocjena != null)
            {
                return BadRequest("Ne možete se prijaviti na ispit jer već imate ocjenu iz ovog predmeta.");
            }

            // PROVJERA DA LI JE STUDENT NA OVOM PREDMETU (da ne može manipulirati ručno)
            bool studentNaPredmetu = await _context.StudentiNaPredmetima
                .AnyAsync(snp => snp.StudentId == student.Id && snp.PredmetId == ispit.PredmetId);

            if (!studentNaPredmetu)
            {
                return BadRequest("Ne možete se prijaviti na ispit jer niste upisani na ovaj predmet.");
            }

            // PROVJERA DA LI JE VEĆ PRIJAVLJEN
            bool alreadyRegistered = await _context.Prijave
                .AnyAsync(p => p.StudentId == student.Id && p.IspitId == id);

            if (alreadyRegistered)
            {
                return BadRequest("Već ste prijavljeni na ovaj ispit.");
            }

            // PRIJAVI ISPIT
            var prijava = new Prijava
            {
                IspitId = id,
                StudentId = student.Id,
                DatumPrijave = DateTime.Now
            };

            _context.Prijave.Add(prijava);
            await _context.SaveChangesAsync();

            // NOTIFIKACIJA
            await _hubContext.Clients.All.SendAsync("UpdateRegisteredStudents", id);

            TempData["Message"] = "Uspješno ste prijavili ispit.";

            return RedirectToAction("Details", new { id });
        }

        [HttpPost("Odjavi/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Odjavi(long id)
        {
            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ispit == null)
            {
                return NotFound("Ispit ne postoji.");
            }

            if (ispit.DatumOdrzavanja.AddDays(-2) <= DateTime.Now)
            {
                return BadRequest("Rok za odjavu ispita je istekao.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            if (student == null)
            {
                return BadRequest("Ne možete se odjaviti jer niste registrovani kao student.");
            }

            // PROVJERA: DA LI JE IMAO OCJENU → NE TREBA prikazivati dugme ali dodatno provjerimo
            var ocjena = await _context.Ocjene
                .FirstOrDefaultAsync(o => o.StudentId == student.Id && o.PredmetId == ispit.PredmetId);

            if (ocjena != null)
            {
                return BadRequest("Ne možete se odjaviti jer već imate ocjenu iz ovog predmeta.");
            }

            var prijava = await _context.Prijave
                .FirstOrDefaultAsync(p => p.StudentId == student.Id && p.IspitId == id);

            if (prijava == null)
            {
                return NotFound("Niste prijavljeni na ovaj ispit.");
            }

            _context.Prijave.Remove(prijava);
            await _context.SaveChangesAsync();

            // NOTIFIKACIJA
            await _hubContext.Clients.All.SendAsync("UpdateRegisteredStudents", id);

            TempData["Message"] = "Uspješno ste odjavili ispit.";

            return RedirectToAction("Details", new { id });
        }

        [HttpPost("UnesiBodove")]
        [Authorize(Roles = "Profesor, Asistent")]
        public async Task<IActionResult> UnesiBodove([FromBody] UnosBodovaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Neispravni podaci." });
            }

            var prijava = await _context.Prijave
                .Include(p => p.Ispit)
                .FirstOrDefaultAsync(p => p.IspitId == model.IspitId && p.StudentId == model.StudentId);

            if (prijava == null)
            {
                return NotFound(new { success = false, message = "Prijava nije pronađena." });
            }

            prijava.Bodovi = model.Bodovi;
            await _context.SaveChangesAsync();

            var bodovi = prijava.Bodovi;
            var ispit = prijava.Ispit;
            var uslovZaPolaganje = ispit.UslovZaPolaganje;

            bool polozen = bodovi.HasValue && bodovi >= uslovZaPolaganje;

            _logger.LogInformation($"Uneseno {bodovi} bodova za studenta {prijava.StudentId} na ispitu {model.IspitId}. Položen: {polozen}");

            return Ok(new
            {
                success = true,
                bodovi = bodovi,
                polozen = polozen
            });
        }

        private float MapirajBodoveUBrojOcjenu(decimal bodovi, decimal ukupnoBodova)
        {
            var procenat = (double)bodovi / (double)ukupnoBodova * 100;
            if (procenat >= 95) return 10;
            if (procenat >= 85) return 9;
            if (procenat >= 75) return 8;
            if (procenat >= 65) return 7;
            if (procenat >= 55) return 6;
            return 5; // Minimalna prolazna ocjena u sistemu
        }

        private long GetTrenutniProfesorIliAsistentId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Pokušaj kao Profesor
            var profesor = _context.Profesori.FirstOrDefault(p => p.AspNetUserId == userId);
            if (profesor != null)
            {
                return profesor.Id;
            }

            // Pokušaj kao Asistent
            var asistent = _context.Asistenti.FirstOrDefault(a => a.AspNetUserId == userId);
            if (asistent != null)
            {
                return asistent.Id;
            }

            return 0; // Ako nije pronađen, što ne bi smjelo biti slučaj
        }

        [HttpGet("PrikaziRezultate/{ispitId}")]
        public IActionResult PrikaziRezultate(int ispitId)
        {
            var korisnikId = GetTrenutniKorisnikId();
            var korisnikUloga = GetTrenutnaUloga();

            var prijave = _context.Prijave
                .Include(p => p.Ispit)
                .Include(p => p.Student)
                    .Where(p => p.IspitId == ispitId &&
                        (korisnikUloga == "Profesor" || korisnikUloga == "Asistent" || korisnikId == p.StudentId))
                .Select(p => new
                {
                    StudentId = p.Student.Id,
                    Ime = p.Student.Ime,
                    Prezime = p.Student.Prezime,
                    BrojIndeksa = p.Student.BrojIndeksa,
                    Bodovi = p.Bodovi,
                    Polozen = p.Bodovi.HasValue && p.Bodovi >= p.Ispit.UslovZaPolaganje
                })
                .ToList();

            return Ok(prijave);
        }

        [HttpGet("GetNastavniPlanoviByStudijskiProgram/{studijskiProgramId}")]
        public async Task<IActionResult> GetNastavniPlanoviByStudijskiProgram(long studijskiProgramId)
        {
            var nastavniPlanovi = await _context.NastavniPlanovi
                .Where(np => np.StudijskiProgramId == studijskiProgramId)
                .Select(np => new { id = np.Id, godinaStudija = np.GodinaStudija })
                .ToListAsync();

            return Json(nastavniPlanovi);
        }

        [HttpGet("GetPredmetiByNastavniPlan/{nastavniPlanId}")]
        public async Task<IActionResult> GetPredmetiByNastavniPlan(long nastavniPlanId)
        {
            var predmeti = await _context.Predmeti
                .Where(p => p.NastavniPlanId == nastavniPlanId)
                .Select(p => new { id = p.Id, naziv = p.Naziv })
                .ToListAsync();

            return Json(predmeti);
        }

        [HttpGet("GetRegisteredStudents/{examId}")]
        public async Task<IActionResult> GetRegisteredStudents(long examId)
        {
            var students = await _context.Prijave
                .Where(p => p.IspitId == examId)
                .Select(p => new
                {
                    p.Student.Ime,
                    p.Student.Prezime,
                    p.Student.Email,
                    p.Student.BrojIndeksa
                })
                .ToListAsync();

            return Json(students);
        }

        [HttpPost("DodajKomentar")]
        [Authorize(Roles = "Student, Profesor, Asistent")]
        public async Task<IActionResult> DodajKomentar(long ispitId, string sadrzaj, VidljivostKomentara vidljivost, List<long> odabraniKorisnici, IFormFile prilog)
        {
            var korisnikId = GetTrenutniKorisnikId();

            var ispit = await _context.Ispiti.FindAsync(ispitId);
            if (ispit == null)
                return NotFound();

            if (ispit.Arhivirano)
            {
                TempData["Error"] = "Nije moguće dodati komentar na arhivirani ispit.";
                return RedirectToAction("Details", new { id = ispitId });
            }

            var komentar = new Komentar
            {
                Sadrzaj = sadrzaj,
                DatumVrijeme = DateTime.Now,
                KorisnikId = korisnikId,
                IspitId = ispitId,
                Vidljivost = vidljivost,
                MentionedUserId = null
            };

            if (prilog != null && prilog.Length > 0)
            {
                var folder = Path.Combine("wwwroot", "prilozi", ispitId.ToString());
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(prilog.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await prilog.CopyToAsync(stream);
                }

                komentar.PrilogPath = $"/prilozi/{ispitId}/{fileName}";
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

            TempData["Message"] = "Komentar uspješno dodan.";

            return RedirectToAction("Details", new { id = ispitId });
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

            if (_context.Ispiti.Any(i => i.Id == komentar.IspitId && i.Arhivirano))
            {
                TempData["Error"] = "Ispit je arhiviran. Nije moguće uređivati komentar.";
                return RedirectToAction("Details", new { id = komentar.IspitId });
            }

            if (komentar.KorisnikId != korisnikId && !User.IsInRole("Studentska služba"))
            {
                return Forbid();
            }

            // Dohvati samo korisnike vezane za predmet komentara
            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .FirstOrDefaultAsync(i => i.Id == komentar.IspitId);

            var predmetId = ispit.PredmetId;

            // STUDENTI
            var studentiIds = await _context.StudentiNaPredmetima
                .Where(snp => snp.PredmetId == predmetId)
                .Select(snp => snp.StudentId)
                .ToListAsync();

            var studenti = await _context.Studenti
                .Where(s => studentiIds.Contains(s.Id))
                .Select(s => new StudentHub.Models.Korisnik
                {
                    Id = s.Id, // Student.Id jer Student *nasljeđuje* Korisnik
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

            return View(komentar);
        }

        [HttpPost("EditKomentar/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student, Profesor, Asistent")]
        public async Task<IActionResult> EditKomentar(long id, string sadrzaj, VidljivostKomentara vidljivost, List<long> odabraniKorisnici)
        {
            var komentar = await _context.Komentari
                .Include(k => k.VidljivostKorisnici)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (komentar == null)
            {
                return NotFound();
            }

            var korisnikId = GetTrenutniKorisnikId();

            if (_context.Ispiti.Any(i => i.Id == komentar.IspitId && i.Arhivirano))
            {
                TempData["Error"] = "Ispit je arhiviran. Nije moguće uređivati komentar.";
                return RedirectToAction("Details", new { id = komentar.IspitId });
            }

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

            await _context.SaveChangesAsync();

            TempData["Message"] = "Komentar uspješno ažuriran.";

            return RedirectToAction("Details", new { id = komentar.IspitId });
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

            if (_context.Ispiti.Any(i => i.Id == komentar.IspitId && i.Arhivirano))
            {
                TempData["Error"] = "Ispit je arhiviran. Nije moguće uređivati komentar.";
                return RedirectToAction("Details", new { id = komentar.IspitId });
            }

            if (komentar.KorisnikId != korisnikId && !User.IsInRole("Studentska služba"))
            {
                return Forbid();
            }

            _context.Komentari.Remove(komentar);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Komentar uspješno obrisan.";

            return RedirectToAction("Details", new { id = komentar.IspitId });
        }

        [HttpGet("GetVidljiviKorisnici")]
        public async Task<IActionResult> GetVidljiviKorisnici()
        {
            var korisnici = await _context.Korisnici
                .Where(k => k.Uloga == Uloga.Student || k.Uloga == Uloga.Profesor || k.Uloga == Uloga.Asistent)
                .Select(k => new
                {
                    Id = k.Id,
                    Ime = k.Ime + " " + k.Prezime,
                    Uloga = k.Uloga.ToString()
                })
                .ToListAsync();

            return Json(korisnici);
        }

        [HttpGet("GetVidljiviKorisnici/{ispitId}")]
        public async Task<IActionResult> GetVidljiviKorisnici(long ispitId)
        {
            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .FirstOrDefaultAsync(i => i.Id == ispitId);

            if (ispit == null) return Json(new List<object>());

            var predmetId = ispit.PredmetId;

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

            var sviKorisnici = studenti.Concat(profesori).Concat(asistenti).ToList();

            return Json(sviKorisnici);
        }

        [HttpPost("Arhiviraj/{id}")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Arhiviraj(long id)
        {
            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit == null)
            {
                return NotFound();
            }

            ispit.Arhivirano = true;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Ispit je uspješno arhiviran.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("VratiIzArhive/{id}")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VratiIzArhive(long id)
        {
            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit == null)
            {
                return NotFound();
            }

            ispit.Arhivirano = false;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Ispit je uspješno vraćen iz arhive.";

            return RedirectToAction(nameof(Index));
        }

        private bool UserBelongsToStudijskiProgramAndPredmet(long studentId, long? studijskiProgramId, long? predmetId)
        {
            var student = _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .FirstOrDefault(s => s.Id == studentId);

            if (student == null || studijskiProgramId == null || predmetId == null)
            {
                return false;
            }

            bool belongsToStudijskiProgram = student.StudentStudijskiProgrami
                .Any(ssp => ssp.StudijskiProgramId == studijskiProgramId);

            return belongsToStudijskiProgram && student.IsEnrolledInPredmet(predmetId.Value, _context);
        }

        private long GetTrenutniKorisnikId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var korisnik = _context.Korisnici.FirstOrDefault(s => s.AspNetUserId == userId);
            return korisnik?.Id ?? 0;
        }

        private string GetTrenutnaUloga()
        {
            if (User.IsInRole("Profesor"))
                return "Profesor";
            if (User.IsInRole("Student"))
                return "Student";
            if (User.IsInRole("Asistent"))
                return "Asistent";
            if (User.IsInRole("Studentska služba"))
                return "Studentska služba";

            return "Nepoznata";
        }

        private bool IspitExists(long id)
        {
            return _context.Ispiti.Any(e => e.Id == id);
        }
    }
}
