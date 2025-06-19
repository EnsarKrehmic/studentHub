using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Linq;
using System.Security.Claims;

namespace StudentHub.Controllers
{
    [Route("Predmeti")]
    public class PredmetiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PredmetiController> _logger;
        public PredmetiController(ApplicationDbContext context, ILogger<PredmetiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Predmeti
        [HttpGet("")]
        public IActionResult Index(long? studijskiProgramId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var predmetiQuery = _context.Predmeti.AsQueryable();

            if (User.IsInRole("Student"))
            {
                predmetiQuery = predmetiQuery.Where(p => p.StudentNaPredmetima.Any(snp => snp.Student.AspNetUserId == userId));
            }
            else if (User.IsInRole("Profesor"))
            {
                predmetiQuery = predmetiQuery.Where(p => p.Profesor.AspNetUserId == userId || p.PredmetProfesori.Any(pp => pp.Profesor.AspNetUserId == userId));
            }
            else if (User.IsInRole("Asistent"))
            {
                predmetiQuery = predmetiQuery.Where(p => p.Asistent.AspNetUserId == userId || p.PredmetAsistenti.Any(pa => pa.Asistent.AspNetUserId == userId));
            }

            if (studijskiProgramId.HasValue)
            {
                predmetiQuery = predmetiQuery.Where(p => p.NastavniPlan.StudijskiProgramId == studijskiProgramId.Value);
            }

            var predmeti = predmetiQuery
                .Include(p => p.Profesor)
                .Include(p => p.Asistent)
                .Include(p => p.NastavniPlan.StudijskiProgram)
                .ToList();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(predmeti);
        }

        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public IActionResult Details(long id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var predmet = _context.Predmeti
                    .Include(p => p.NastavniPlan)
                    .Include(p => p.NastavneAktivnosti)
                        .ThenInclude(na => na.Prisustva)
                    .FirstOrDefault(p => p.Id == id);

                if (predmet == null)
                {
                    _logger.LogWarning("Predmet sa ID-om {PredmetId} nije pronađen.", id);
                    return NotFound();
                }

                var profesori = _context.PredmetProfesori
                    .Where(pp => pp.PredmetId == id)
                    .Include(pp => pp.Profesor)
                    .Where(pp => pp.Profesor.Uloga == Uloga.Profesor)
                    .ToList();

                var asistenti = _context.PredmetAsistenti
                    .Where(pa => pa.PredmetId == id)
                    .Include(pa => pa.Asistent)
                    .Where(pa => pa.Asistent.Uloga == Uloga.Asistent)
                    .ToList();

                var studentiNaPredmetu = _context.StudentiNaPredmetima
                    .Where(snp => snp.PredmetId == id)
                    .Include(snp => snp.Student)
                    .ToList();

                ViewBag.Studenti = _context.Studenti
                    .Where(s => !_context.StudentiNaPredmetima
                        .Any(snp => snp.StudentId == s.Id && snp.PredmetId == id))
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = $"{s.Ime} {s.Prezime} ({s.BrojIndeksa})"
                    }).ToList();

                ViewBag.Profesori = _context.Profesori
                    .Where(p => !_context.PredmetProfesori.Any(pp => pp.PredmetId == id && pp.ProfesorId == p.Id))
                    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.ProfesorTitula} {p.Ime} {p.Prezime}" })
                    .ToList();

                ViewBag.Asistenti = _context.Asistenti
                    .Where(a => !_context.PredmetAsistenti.Any(pa => pa.PredmetId == id && pa.AsistentId == a.Id))
                    .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.AsistentTitula} {a.Ime} {a.Prezime}" })
                    .ToList();

                // Ocjene tipa Predmet
                var ocjene = _context.Ocjene
                    .Where(o => o.PredmetId == id && o.Tip == TipOcjene.Predmet)
                    .ToList();

                var sveOcjene = studentiNaPredmetu.ToDictionary(
                    snp => snp.StudentId,
                    snp => ocjene.FirstOrDefault(o => o.StudentId == snp.StudentId)?.Vrijednost
                );

                var ocjenaIds = ocjene
                    .GroupBy(o => o.StudentId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.DatumUnosa).First().Id);

                // Nastavne aktivnosti – bez filtriranja zaključanih za studente
                var nastavneAktivnosti = predmet.NastavneAktivnosti != null
                    ? predmet.NastavneAktivnosti.OrderBy(a => a.DatumVrijemeOdrzavanja).ToList()
                    : new List<NastavnaAktivnost>();

                // Statistika prisustva
                var statistika = studentiNaPredmetu.Select(snp =>
                {
                    var prisustva = _context.PrisustvaNaAktivnostima
                        .Count(p => p.StudentId == snp.StudentId && p.NastavnaAktivnost.PredmetId == id);

                    return new StatistikaPrisustvaDTO
                    {
                        Student = snp.Student,
                        BrojPrisustava = prisustva,
                        UkupnoAktivnosti = nastavneAktivnosti.Count
                    };
                }).ToList();

                int ukupnoStudenata = studentiNaPredmetu.Count;
                int brojPolozili = ocjene.Count(o => o.Vrijednost >= 6);

                var viewModel = new PredmetDetailsViewModel
                {
                    Predmet = predmet,
                    Profesori = profesori,
                    Asistenti = asistenti,
                    StudentiNaPredmetu = studentiNaPredmetu,
                    Ocjene = sveOcjene.ToDictionary(kv => kv.Key, kv => (float?)kv.Value),
                    OcjenaIds = ocjenaIds,
                    NastavneAktivnosti = nastavneAktivnosti,
                    StatistikaPrisustva = statistika,
                    UkupnoStudenata = ukupnoStudenata,
                    BrojPoloziliPredmet = brojPolozili,
                    ProsjecnoPrisustvo = statistika.Count > 0 ? statistika.Average(s => s.Procenat) : null,
                    ProsjecnaOcjena = sveOcjene.Values.Where(v => v.HasValue).Average(v => v) ?? 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Došlo je do greške prilikom povlačenja podataka o predmetu sa ID-om {PredmetId}.", id);
                return View("Error");
            }
        }

        // GET: Predmeti/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            try
            {
                ViewBag.Profesori = _context.Profesori
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Ime} {p.Prezime}"
                    }).ToList();

                ViewBag.Asistenti = _context.Asistenti
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.Ime} {a.Prezime}"
                    }).ToList();

                ViewBag.NastavniPlanId = _context.NastavniPlanovi
                    .Include(np => np.StudijskiProgram)
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godina"
                    }).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Došlo je do greške prilikom pripremanja pogleda za formu Create.");
                return View("Error");
            }
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create(PredmetCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Nevažeće stanje modela za kreiranje predmeta.");

                // POPUNI ViewBag-ove jer ćeš vraćati View
                ViewBag.Profesori = _context.Profesori
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Ime} {p.Prezime}"
                    }).ToList();

                ViewBag.Asistenti = _context.Asistenti
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.Ime} {a.Prezime}"
                    }).ToList();

                ViewBag.NastavniPlanId = _context.NastavniPlanovi
                    .Include(np => np.StudijskiProgram)
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godina"
                    }).ToList();

                return View(model);
            }

            try
            {
                var predmet = new Predmet
                {
                    Naziv = model.Naziv,
                    Opis = model.Opis,
                    ECTS = model.ECTS,
                    TipPredmeta = model.TipPredmeta,
                    Semestar = model.Semestar,
                    NastavniPlanId = model.NastavniPlanId
                };

                _context.Predmeti.Add(predmet);
                _context.SaveChanges();

                if (model.ProfesorIds != null && model.ProfesorIds.Any())
                {
                    foreach (var profesorId in model.ProfesorIds)
                    {
                        var profesor = _context.Profesori.FirstOrDefault(p => p.Id == profesorId);

                        _context.PredmetProfesori.Add(new PredmetProfesor
                        {
                            PredmetId = predmet.Id,
                            ProfesorId = profesorId,
                            AspNetUserId = profesor?.AspNetUserId
                        });
                    }

                    predmet.ProfesorId = model.ProfesorIds.First();
                }

                if (model.AsistentIds != null && model.AsistentIds.Any())
                {
                    foreach (var asistentId in model.AsistentIds)
                    {
                        var asistent = _context.Asistenti.FirstOrDefault(a => a.Id == asistentId);

                        _context.PredmetAsistenti.Add(new PredmetAsistent
                        {
                            PredmetId = predmet.Id,
                            AsistentId = asistentId,
                            AspNetUserId = asistent?.AspNetUserId
                        });
                    }

                    predmet.AsistentId = model.AsistentIds.First();
                }

                _context.SaveChanges();
                _logger.LogInformation("Predmet sa ID-om {PredmetId} je uspješno kreiran.", predmet.Id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Došlo je do greške prilikom kreiranja predmeta.");

                // OPET POPUNI ViewBag-ove → jer ViewBag.Profesori ne postoji u catch bloku!
                ViewBag.Profesori = _context.Profesori
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Ime} {p.Prezime}"
                    }).ToList();

                ViewBag.Asistenti = _context.Asistenti
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.Ime} {a.Prezime}"
                    }).ToList();

                ViewBag.NastavniPlanId = _context.NastavniPlanovi
                    .Include(np => np.StudijskiProgram)
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godina"
                    }).ToList();

                ModelState.AddModelError("", "Došlo je do greške prilikom kreiranja. Molimo pokušajte ponovo.");
                return View(model);
            }
        }

        // GET: Predmeti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Edit(long id)
        {
            try
            {
                var predmet = _context.Predmeti.Find(id);
                if (predmet == null)
                {
                    _logger.LogWarning("Predmet sa ID-om {PredmetId} nije pronađen.", id);
                    return NotFound();
                }

                var model = new PredmetCreateViewModel
                {
                    Naziv = predmet.Naziv,
                    Opis = predmet.Opis,
                    ECTS = predmet.ECTS,
                    TipPredmeta = predmet.TipPredmeta,
                    ProfesorId = predmet.ProfesorId,
                    AsistentId = predmet.AsistentId,
                    NastavniPlanId = predmet.NastavniPlanId,
                    Semestar = predmet.Semestar,
                    ProfesorIds = _context.PredmetProfesori
                        .Where(pp => pp.PredmetId == id)
                        .Select(pp => pp.ProfesorId)
                        .ToList(),
                    AsistentIds = _context.PredmetAsistenti
                        .Where(pa => pa.PredmetId == id)
                        .Select(pa => pa.AsistentId)
                        .ToList()
                };

                ViewBag.Profesori = _context.Profesori
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Ime} {p.Prezime}, {p.ProfesorTitula}",
                        Selected = model.ProfesorIds.Contains(p.Id)
                    }).ToList();

                ViewBag.Asistenti = _context.Asistenti
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.Ime} {a.Prezime}, {a.AsistentTitula}",
                        Selected = model.AsistentIds.Contains(a.Id)
                    }).ToList();

                ViewBag.NastavniPlanId = _context.NastavniPlanovi
                    .Include(np => np.StudijskiProgram)
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godina",
                        Selected = np.Id == model.NastavniPlanId
                    }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Došlo je do greške prilikom pripreme prikaza za uređivanje za predmet sa ID-om {PredmetId}.", id);
                return View("Error");
            }
        }

        // POST: Predmeti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Edit(long id, PredmetCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Nevažeće stanje modela za uređivanje predmeta s ID-om {PredmetId}.", id);

                ViewBag.Profesori = _context.Profesori
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Ime} {p.Prezime}, {p.ProfesorTitula}",
                        Selected = model.ProfesorIds.Contains(p.Id)
                    }).ToList();

                ViewBag.Asistenti = _context.Asistenti
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.Ime} {a.Prezime}, {a.AsistentTitula}",
                        Selected = model.AsistentIds.Contains(a.Id)
                    }).ToList();

                ViewBag.NastavniPlanId = _context.NastavniPlanovi
                    .Include(np => np.StudijskiProgram)
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godina",
                        Selected = np.Id == model.NastavniPlanId
                    }).ToList();

                return View(model);
            }

            try
            {
                var predmet = _context.Predmeti.Find(id);
                if (predmet == null)
                {
                    _logger.LogWarning("Predmet sa ID-om {PredmetId} nije pronađen.", id);
                    return NotFound();
                }

                predmet.Naziv = model.Naziv;
                predmet.Opis = model.Opis;
                predmet.ECTS = model.ECTS;
                predmet.Semestar = model.Semestar;
                predmet.TipPredmeta = model.TipPredmeta;
                predmet.NastavniPlanId = model.NastavniPlanId.GetValueOrDefault();

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _context.PredmetProfesori.RemoveRange(_context.PredmetProfesori.Where(pp => pp.PredmetId == id));
                        _context.PredmetAsistenti.RemoveRange(_context.PredmetAsistenti.Where(pa => pa.PredmetId == id));

                        if (model.ProfesorIds != null && model.ProfesorIds.Any())
                        {
                            foreach (var profesorId in model.ProfesorIds)
                            {
                                var profesor = _context.Profesori.FirstOrDefault(p => p.Id == profesorId);

                                _context.PredmetProfesori.Add(new PredmetProfesor
                                {
                                    PredmetId = predmet.Id,
                                    ProfesorId = profesorId,
                                    AspNetUserId = profesor?.AspNetUserId
                                });
                            }

                            predmet.ProfesorId = model.ProfesorIds.First();
                        }

                        if (model.AsistentIds != null && model.AsistentIds.Any())
                        {
                            foreach (var asistentId in model.AsistentIds)
                            {
                                var asistent = _context.Asistenti.FirstOrDefault(a => a.Id == asistentId);

                                _context.PredmetAsistenti.Add(new PredmetAsistent
                                {
                                    PredmetId = predmet.Id,
                                    AsistentId = asistentId,
                                    AspNetUserId = asistent?.AspNetUserId
                                });
                            }

                            predmet.AsistentId = model.AsistentIds.First();
                        }

                        _context.SaveChanges();
                        transaction.Commit();

                        _logger.LogInformation("Predmet sa ID-om {PredmetId} je uspješno ažuriran.", id);
                        TempData["SuccessMessage"] = "Promjene su uspješno sačuvane.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex, "Došlo je do greške prilikom ažuriranja predmeta s ID-om {PredmetId}.", id);

                        ViewBag.Profesori = _context.Profesori
                            .Select(p => new SelectListItem
                            {
                                Value = p.Id.ToString(),
                                Text = $"{p.Ime} {p.Prezime}"
                            }).ToList();

                        ViewBag.Asistenti = _context.Asistenti
                            .Select(a => new SelectListItem
                            {
                                Value = a.Id.ToString(),
                                Text = $"{a.Ime} {a.Prezime}"
                            }).ToList();

                        ViewBag.NastavniPlanId = _context.NastavniPlanovi
                            .Include(np => np.StudijskiProgram)
                            .Select(np => new SelectListItem
                            {
                                Value = np.Id.ToString(),
                                Text = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godina"
                            }).ToList();

                        ModelState.AddModelError("", "Došlo je do greške prilikom ažuriranja. Molimo pokušajte ponovo.");
                        return View(model);
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Došlo je do greške prilikom uređivanja predmeta s ID-om {PredmetId}.", id);
                return View("Error");
            }
        }

        // GET: Predmet/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Delete(long id)
        {
            var predmet = _context.Predmeti
                .Include(p => p.Profesor)
                .Include(p => p.Asistent)
                .Include(p => p.NastavniPlan)
                .FirstOrDefault(p => p.Id == id);

            if (predmet == null)
                return NotFound();

            // Učitavanje profesora
            var profesori = _context.PredmetProfesori
                .Where(pp => pp.PredmetId == id)
                .Include(pp => pp.Profesor)
                .Where(pp => pp.Profesor.Uloga == Uloga.Profesor)
                .ToList();

            // Učitavanje asistenata
            var asistenti = _context.PredmetAsistenti
                .Where(pa => pa.PredmetId == id)
                .Include(pa => pa.Asistent)
                .Where(pa => pa.Asistent.Uloga == Uloga.Asistent)
                .ToList();

            // Formiraj ViewModel za prikaz
            var viewModel = new PredmetDetailsViewModel
            {
                Predmet = predmet,
                Profesori = profesori,
                Asistenti = asistenti
            };

            return View(viewModel);
        }

        // POST: Predmet/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult DeleteConfirmed(long id)
        {
            var predmet = _context.Predmeti.Find(id);
            if (predmet == null)
            {
                _logger.LogWarning("Predmet with ID {PredmetId} not found.", id);
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.PredmetProfesori.RemoveRange(_context.PredmetProfesori.Where(pp => pp.PredmetId == id));
                    _context.PredmetAsistenti.RemoveRange(_context.PredmetAsistenti.Where(pa => pa.PredmetId == id));
                    _context.Predmeti.Remove(predmet);
                    _context.SaveChanges();
                    transaction.Commit();

                    _logger.LogInformation("Predmet sa ID-om {PredmetId} je uspješno obrisan.", id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Došlo je do greške prilikom brisanja Predmeta s ID-om {PredmetId}.", id);
                    ModelState.AddModelError("", "Došlo je do greške prilikom brisanja. Molimo pokušajte ponovo.");
                    return View(predmet);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Predmeti/AddStudentToSubject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba, Profesor")]
        public IActionResult AddStudentToSubject(long predmetId, long studentId)
        {
            // Validate the StudentId
            if (studentId <= 0)
            {
                ModelState.AddModelError("StudentId", "Odabir studenta je obavezan.");
            }

            // Check if the student exists
            var student = _context.Studenti.Find(studentId);
            if (student == null)
            {
                ModelState.AddModelError("StudentId", "Odabrani student ne postoji.");
            }

            // Check if the subject exists
            var predmet = _context.Predmeti
                .Include(p => p.NastavniPlan)
                .FirstOrDefault(p => p.Id == predmetId);
            if (predmet == null)
            {
                TempData["ErrorMessage"] = "Odabrani predmet ne postoji.";
                return RedirectToAction("Index");
            }

            if (_context.StudentiNaPredmetima.Any(snp => snp.PredmetId == predmetId && snp.StudentId == studentId))
            {
                TempData["WarningMessage"] = "Student je već dodan na ovaj predmet.";
                return RedirectToAction("Details", new { id = predmetId });
            }

            if (!ModelState.IsValid)
            {
                // Reload the ViewBag.Studenti for the dropdown list
                ViewBag.Studenti = _context.Studenti
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = $"{s.Ime} {s.Prezime} ({s.BrojIndeksa})"
                    }).ToList();

                // Reload the view model
                var profesori = _context.PredmetProfesori
                    .Where(pp => pp.PredmetId == predmetId)
                    .Include(pp => pp.Profesor)
                    .Where(pp => pp.Profesor.Uloga == Uloga.Profesor)
                    .ToList();

                var asistenti = _context.PredmetAsistenti
                    .Where(pa => pa.PredmetId == predmetId)
                    .Include(pa => pa.Asistent)
                    .Where(pa => pa.Asistent.Uloga == Uloga.Asistent)
                    .ToList();

                var studentiNaPredmetu = _context.StudentiNaPredmetima
                    .Where(snp => snp.PredmetId == predmetId)
                    .Include(snp => snp.Student)
                    .ToList();

                var viewModel = new PredmetDetailsViewModel
                {
                    Predmet = predmet,
                    Profesori = profesori,
                    Asistenti = asistenti,
                    StudentiNaPredmetu = studentiNaPredmetu
                };

                return View("Details", viewModel);
            }

            // Dodavanje studenta na predmet
            var studentNaPredmetu = new StudentNaPredmetu
            {
                PredmetId = predmetId,
                StudentId = studentId,
                AspNetUserId = student.AspNetUserId,
                AkademskaGodina = DateTime.Now.Year.ToString()
            };

            _context.StudentiNaPredmetima.Add(studentNaPredmetu);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Student je uspješno dodan na predmet.";

            return RedirectToAction("Details", new { id = predmetId });
        }

        [HttpPost("RemoveStudentFromSubject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba,Profesor")]
        public async Task<IActionResult> RemoveStudentFromSubject(long predmetId, long studentId)
        {
            var studentNaPredmetu = await _context.StudentiNaPredmetima
                .FirstOrDefaultAsync(snp => snp.PredmetId == predmetId && snp.StudentId == studentId);
            if (studentNaPredmetu == null)
            {
                TempData["Error"] = "Student nije pronađen na predmetu.";
                return RedirectToAction("Details", new { id = predmetId });
            }

            _context.StudentiNaPredmetima.Remove(studentNaPredmetu);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Student je uspješno uklonjen s predmeta.";
            return RedirectToAction("Details", new { id = predmetId });
        }

        [HttpPost]
        [Route("Predmeti/AddProfesorToSubject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult AddProfesorToSubject(long predmetId, long profesorId)
        {
            if (profesorId <= 0)
            {
                ModelState.AddModelError("ProfesorId", "Odabir profesora je obavezan.");
            }

            var profesor = _context.Profesori.Find(profesorId);
            if (profesor == null)
            {
                ModelState.AddModelError("ProfesorId", "Odabrani profesor ne postoji.");
            }

            var predmet = _context.Predmeti.Include(p => p.NastavniPlan)
                                            .FirstOrDefault(p => p.Id == predmetId);
            if (predmet == null)
            {
                TempData["ErrorMessage"] = "Odabrani predmet ne postoji.";
                return RedirectToAction("Index");
            }

            if (_context.PredmetProfesori.Any(pp => pp.PredmetId == predmetId && pp.ProfesorId == profesorId))
            {
                TempData["WarningMessage"] = "Profesor je već dodan na ovaj predmet.";
                return RedirectToAction("Details", new { id = predmetId });
            }

            if (!ModelState.IsValid)
            {
                ReloadViewData(predmetId);
                return View("Details", LoadViewModel(predmetId));
            }

            _context.PredmetProfesori.Add(new PredmetProfesor
            {
                PredmetId = predmetId,
                ProfesorId = profesorId,
                AspNetUserId = profesor.AspNetUserId
            });
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Profesor je uspješno dodan na predmet.";
            return RedirectToAction("Details", new { id = predmetId });
        }

        [HttpPost]
        [Route("Predmeti/RemoveProfesor/{predmetId}/{profesorId}")]
        public async Task<IActionResult> RemoveProfesorFromSubject(long predmetId, long profesorId)

        {
            var predmetProfesor = _context.PredmetProfesori
                .FirstOrDefault(pp => pp.PredmetId == predmetId && pp.ProfesorId == profesorId);
            if (predmetProfesor != null)
            {
                _context.PredmetProfesori.Remove(predmetProfesor);
                await _context.SaveChangesAsync();
            }
            ViewBag.Profesori = new SelectList(
                _context.Profesori.Where(p => !_context.PredmetProfesori.Any(pp => pp.PredmetId == predmetId && pp.ProfesorId == p.Id)),
                "Id", "Ime");

            return RedirectToAction("Details", new { id = predmetId });
        }

        [HttpPost]
        [Route("Predmeti/AddAsistentToSubject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult AddAsistentToSubject(long predmetId, long asistentId)
        {
            if (asistentId <= 0)
            {
                ModelState.AddModelError("AsistentId", "Odabir asistenta je obavezan.");
            }

            var asistent = _context.Asistenti.Find(asistentId);
            if (asistent == null)
            {
                ModelState.AddModelError("AsistentId", "Odabrani asistent ne postoji.");
            }

            var predmet = _context.Predmeti.Include(p => p.NastavniPlan)
                                            .FirstOrDefault(p => p.Id == predmetId);
            if (predmet == null)
            {
                TempData["ErrorMessage"] = "Odabrani predmet ne postoji.";
                return RedirectToAction("Index");
            }

            if (_context.PredmetAsistenti.Any(pa => pa.PredmetId == predmetId && pa.AsistentId == asistentId))
            {
                TempData["WarningMessage"] = "Asistent je već dodan na ovaj predmet.";
                return RedirectToAction("Details", new { id = predmetId });
            }

            if (!ModelState.IsValid)
            {
                ReloadViewData(predmetId);
                return View("Details", LoadViewModel(predmetId));
            }

            _context.PredmetAsistenti.Add(new PredmetAsistent { 
                PredmetId = predmetId, 
                AsistentId = asistentId,
                AspNetUserId = asistent.AspNetUserId
            });
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Asistent je uspješno dodan na predmet.";
            return RedirectToAction("Details", new { id = predmetId });
        }


        [HttpPost]
        [Route("Predmeti/RemoveAsistent/{predmetId}/{asistentId}")]
        public async Task<IActionResult> RemoveAsistentFromSubject(long predmetId, long asistentId)
        {
            var predmetAsistent = _context.PredmetAsistenti
                .FirstOrDefault(pa => pa.PredmetId == predmetId && pa.AsistentId == asistentId);
            if (predmetAsistent != null)
            {
                _context.PredmetAsistenti.Remove(predmetAsistent);
                await _context.SaveChangesAsync();
            }
            ViewBag.Asistenti = new SelectList(
                _context.Asistenti.Where(a => !_context.PredmetAsistenti.Any(pa => pa.PredmetId == predmetId && pa.AsistentId == a.Id)),
                "Id", "Ime");

            return RedirectToAction("Details", new { id = predmetId });
        }


        [HttpGet("GetPredmetiByStudijskiProgramAndNastavniPlan")]
        public async Task<IActionResult> GetPredmetiByStudijskiProgramAndNastavniPlan(long studijskiProgramId, long nastavniPlanId)
        {
            var predmeti = await _context.Predmeti
                .Where(p => p.NastavniPlan.StudijskiProgramId == studijskiProgramId && p.NastavniPlanId == nastavniPlanId)
                .Select(p => new { id = p.Id, naziv = p.Naziv })
                .ToListAsync();

            return Json(predmeti);
        }

        [HttpGet("Prisustvo/{id:long}")]
        [Authorize(Roles = "Profesor,Asistent,Studentska služba")]
        public async Task<IActionResult> Prisustvo(long id)
        {
            var predmet = await _context.Predmeti
                .Include(p => p.NastavneAktivnosti)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (predmet == null) return NotFound();

            var aktivnosti = predmet.NastavneAktivnosti
                .OrderBy(a => a.DatumVrijemeOdrzavanja)
                .ToList();

            var studenti = await _context.StudentiNaPredmetima
                .Where(s => s.PredmetId == id)
                .Include(s => s.Student)
                .Select(s => s.Student)
                .ToListAsync();

            var prisustva = await _context.PrisustvaNaAktivnostima
                .Where(p => p.NastavnaAktivnost.PredmetId == id)
                .ToListAsync();

            // 1. Statusi prisustva po aktivnosti
            var statusi = new Dictionary<(long studentId, long aktivnostId), string>();
            foreach (var student in studenti)
            {
                foreach (var aktivnost in aktivnosti)
                {
                    var key = (student.Id, aktivnost.Id);
                    bool prisutan = prisustva.Any(p =>
                        p.StudentId == student.Id && p.NastavnaAktivnostId == aktivnost.Id);
                    statusi[key] = prisutan ? "Prisutan" : "Nije prisutan";
                }
            }

            // 2. Grupisanje aktivnosti po tipu
            var predavanja = aktivnosti.Where(a => a.Tip == TipNastavneAktivnosti.Predavanje).ToList();
            var vjezbe = aktivnosti.Where(a => a.Tip == TipNastavneAktivnosti.Vjezba).ToList();

            // 3. Statistika
            List<StudentPrisustvoStatistika> IzracunajStatistiku(List<NastavnaAktivnost> listaAktivnosti)
            {
                return studenti.Select(student =>
                {
                    var brojPrisustava = listaAktivnosti.Count(a =>
                        prisustva.Any(p => p.StudentId == student.Id && p.NastavnaAktivnostId == a.Id));

                    return new StudentPrisustvoStatistika
                    {
                        Student = student,
                        BrojPrisustava = brojPrisustava,
                        UkupnoAktivnosti = listaAktivnosti.Count
                    };
                }).ToList();
            }

            var statistikaUkupno = IzracunajStatistiku(aktivnosti);
            var statistikaPredavanja = IzracunajStatistiku(predavanja);
            var statistikaVjezbi = IzracunajStatistiku(vjezbe);

            var model = new PrisustvoPoPredmetuViewModel
            {
                Predmet = predmet,
                Aktivnosti = aktivnosti,
                Studenti = studenti,
                StatusiPrisustva = statusi,
                StatistikaUkupno = statistikaUkupno,
                StatistikaPredavanja = statistikaPredavanja,
                StatistikaVjezbe = statistikaVjezbi,
                PragPrisustvaPredavanja = predmet.PragPrisustvaPredavanja ?? 70,
                PragPrisustvaVjezbe = predmet.PragPrisustvaVjezbe ?? 70,
                PragPrisustvaUkupno = predmet.PragPrisustvaUkupno ?? 70
            };

            return View("PrisustvoPoPredmetu", model);
        }

        [HttpPost]
        [Route("Predmeti/PostaviPragovePrisustva")]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> PostaviPragovePrisustva(long predmetId, int pragPredavanja, int pragVjezbe)
        {
            if (!Enumerable.Range(0, 101).Contains(pragPredavanja) || !Enumerable.Range(0, 101).Contains(pragVjezbe))
            {
                TempData["ErrorMessage"] = "Pragovi moraju biti između 0 i 100.";
                return RedirectToAction("Prisustvo", new { id = predmetId });
            }

            var predmet = await _context.Predmeti.FindAsync(predmetId);
            if (predmet == null)
                return NotFound();

            predmet.PragPrisustvaPredavanja = pragPredavanja;
            predmet.PragPrisustvaVjezbe = pragVjezbe;

            // Automatski izračun ukupnog praga
            predmet.PragPrisustvaUkupno = (int)Math.Round((pragPredavanja + pragVjezbe) / 2.0);

            await _context.SaveChangesAsync();

            TempData["PragUpdateSuccess"] = $"Pragovi su uspješno ažurirani. Ukupni prag automatski postavljen na {predmet.PragPrisustvaUkupno}%.";

            return RedirectToAction("Prisustvo", new { id = predmetId });
        }

        [HttpGet("StudentDetails")]
        [Authorize(Roles = "Student, Profesor, Asistent, Studentska služba")]
        public async Task<IActionResult> StudentDetails(long predmetId, long studentId)
        {
            var student = await _context.Studenti
                .Include(s => s.StudentNaPredmetima)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            var predmet = await _context.Predmeti
                .Include(p => p.NastavneAktivnosti)
                .FirstOrDefaultAsync(p => p.Id == predmetId);

            if (student == null || predmet == null)
                return NotFound();

            var aktivnosti = predmet.NastavneAktivnosti
                .OrderBy(a => a.DatumVrijemeOdrzavanja)
                .ToList();

            var prisustva = await _context.PrisustvaNaAktivnostima
                .Where(p => p.StudentId == studentId && aktivnosti.Select(a => a.Id).Contains(p.NastavnaAktivnostId))
                .ToListAsync();

            var sveOcjene = await _context.Ocjene
                .Include(o => o.NastavnaAktivnost)
                .Include(o => o.Profesor)
                .Where(o => o.StudentId == studentId && o.PredmetId == predmetId)
                .ToListAsync();

            var glavneOcjene = sveOcjene.Where(o => o.ParentOcjenaId == null).ToList();
            var parcijalne = sveOcjene.Where(o => o.ParentOcjenaId != null).ToList();

            var grupisaneParcijalne = parcijalne
                .GroupBy(p => p.ParentOcjenaId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Novo: Prisustvo po aktivnosti
            var prisustvoPoAktivnosti = aktivnosti.ToDictionary(
                a => a.Id,
                a => prisustva.Any(p => p.NastavnaAktivnostId == a.Id)
            );

            // Novo: Ocjena po aktivnosti
            var ocjenePoAktivnosti = sveOcjene
                .Where(o => o.NastavnaAktivnostId.HasValue)
                .GroupBy(o => o.NastavnaAktivnostId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.DatumUnosa).FirstOrDefault());

            // Novo: Profesor po aktivnosti
            var profesorPoAktivnosti = ocjenePoAktivnosti
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Profesor != null
                        ? $"{kvp.Value.Profesor.ProfesorTitula} {kvp.Value.Profesor.Ime} {kvp.Value.Profesor.Prezime}"
                        : "Nepoznato"
                );

            var ukupno = aktivnosti.Count;
            var brojPrisustva = prisustva.Count;
            var procUkupno = ukupno == 0 ? 0 : (float)brojPrisustva / ukupno * 100;

            var jeVlasnik = student.AspNetUserId == User.FindFirstValue(ClaimTypes.NameIdentifier);
            var jeNastavnik = User.IsInRole("Profesor") || User.IsInRole("Asistent") || User.IsInRole("Studentska služba");

            if (!jeVlasnik && !jeNastavnik)
                return Forbid();

            // Učitaj sve prijave na ispite tog predmeta za datog studenta
            var prijave = await _context.Prijave
                .Include(p => p.Ispit)
                .Where(p => p.StudentId == studentId && p.Ispit.PredmetId == predmetId)
                .ToListAsync();

            // Opcionalno: Uslov za prolazak ako postoji bar jedan ispit
            var uslovZaPolaganje = prijave
                .Select(p => p.Ispit.UslovZaPolaganje)
                .FirstOrDefault();

            // Ponderisana ocjena (ako postoji zaključna)
            float? ponderisana = null;
            var predmetnaOcjena = glavneOcjene.FirstOrDefault(o => o.Tip == TipOcjene.Predmet);

            if (predmetnaOcjena != null && grupisaneParcijalne.TryGetValue(predmetnaOcjena.Id, out var djelimicne))
            {
                var ukupnaTezina = djelimicne.Sum(d => d.TezinaProcentualno ?? 0);
                if (ukupnaTezina > 0)
                {
                    var suma = djelimicne.Sum(d => d.Vrijednost * ((d.TezinaProcentualno ?? 0) / 100f));
                    ponderisana = suma;
                }
            }

            var model = new StudentNaPredmetuViewModel
            {
                Student = student,
                Predmet = predmet,
                Aktivnosti = aktivnosti,
                Prisustva = prisustva,
                Ocjene = glavneOcjene,
                ParcijalneOcjene = grupisaneParcijalne,
                ProcenatUkupno = procUkupno,
                ProcenatPredavanja = IzracunajProcenat(aktivnosti, prisustva, TipNastavneAktivnosti.Predavanje, student.Id),
                ProcenatVjezbi = IzracunajProcenat(aktivnosti, prisustva, TipNastavneAktivnosti.Vjezba, student.Id),
                ZakljucnaOcjena = predmetnaOcjena?.Vrijednost,
                PonderisanaOcjena = ponderisana,
                DozvoljenPristup = true,

                PrisustvoPoAktivnosti = prisustvoPoAktivnosti,
                OcjenaPoAktivnosti = ocjenePoAktivnosti,
                ProfesorPoAktivnosti = profesorPoAktivnosti,
                Prijave = prijave,
                UslovZaPolaganje = uslovZaPolaganje
            };

            return View("StudentNaPredmetu", model);
        }

        private void ReloadViewData(long predmetId)
        {
            ViewBag.Profesori = _context.Profesori
                .Where(p => !_context.PredmetProfesori.Any(pp => pp.PredmetId == predmetId && pp.ProfesorId == p.Id))
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Ime} {p.Prezime}" })
                .ToList();

            ViewBag.Asistenti = _context.Asistenti
                .Where(a => !_context.PredmetAsistenti.Any(pa => pa.PredmetId == predmetId && pa.AsistentId == a.Id))
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.Ime} {a.Prezime}" })
                .ToList();
        }

        private float IzracunajProcenat(List<NastavnaAktivnost> sve, List<PrisustvoNaAktivnosti> prisustva, TipNastavneAktivnosti tip, long studentId)
        {
            var filtrirane = sve.Where(a => a.Tip == tip).ToList();
            if (filtrirane.Count == 0) return 0;

            var broj = filtrirane.Count(a => prisustva.Any(p => p.StudentId == studentId && p.NastavnaAktivnostId == a.Id));
            return (float)broj / filtrirane.Count * 100;
        }

        private PredmetDetailsViewModel LoadViewModel(long predmetId)
        {
            var predmet = _context.Predmeti.Include(p => p.NastavniPlan).FirstOrDefault(p => p.Id == predmetId);

            var profesori = _context.PredmetProfesori.Where(pp => pp.PredmetId == predmetId)
                                                     .Include(pp => pp.Profesor).ToList();

            var asistenti = _context.PredmetAsistenti.Where(pa => pa.PredmetId == predmetId)
                                                     .Include(pa => pa.Asistent).ToList();

            var studentiNaPredmetu = _context.StudentiNaPredmetima.Where(snp => snp.PredmetId == predmetId)
                                                                  .Include(snp => snp.Student).ToList();

            return new PredmetDetailsViewModel
            {
                Predmet = predmet,
                Profesori = profesori,
                Asistenti = asistenti,
                StudentiNaPredmetu = studentiNaPredmetu
            };
        }
    }
}