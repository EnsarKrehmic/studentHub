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

                // Popunjavamo ocjene za sve studente na predmetu
                var ocjene = _context.Ocjene
                    .Where(o => o.PredmetId == id)
                    .ToDictionary(o => o.StudentId, o => (float?)o.Vrijednost);

                var sveOcjene = studentiNaPredmetu.ToDictionary(
                    snp => snp.StudentId,
                    snp => ocjene.ContainsKey(snp.StudentId) ? ocjene[snp.StudentId] : null
                );

                // Filtriranje nastavnih aktivnosti za studente ili postavljanje na praznu listu ako je null
                var nastavneAktivnosti = predmet.NastavneAktivnosti != null
                    ? predmet.NastavneAktivnosti.ToList()
                    : new List<NastavnaAktivnost>();

                if (User.IsInRole("Student"))
                {
                    nastavneAktivnosti = nastavneAktivnosti.Where(na => na.JeDostupno).ToList();
                }

                var viewModel = new PredmetDetailsViewModel
                {
                    Predmet = predmet,
                    Profesori = profesori,
                    Asistenti = asistenti,
                    StudentiNaPredmetu = studentiNaPredmetu,
                    Ocjene = sveOcjene,
                    NastavneAktivnosti = nastavneAktivnosti
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Došlo je do greške prilikom povlačenja podataka o predmetu sa ID-om {PredmetId}.", id);
                return View("Error");
            }
        }

        // GET: Predmet/Create
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

        // POST: Predmet/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create(PredmetCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Nevažeće stanje modela za kreiranje subjekta.");
                ViewBag.Profesori = _context.Profesori.ToList();
                ViewBag.Asistenti = _context.Asistenti.ToList();
                ViewBag.NastavniPlanId = _context.NastavniPlanovi
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = np.GodinaStudija.ToString()
                    })
                    .ToList();
                return View(model);
            }

            try
            {
                var predmet = new Predmet
                {
                    Naziv = model.Naziv,
                    Opis = model.Opis,
                    ECTS = model.ECTS,
                    ProfesorId = model.ProfesorId,
                    AsistentId = model.AsistentId,
                    NastavniPlanId = model.NastavniPlanId.GetValueOrDefault(),
                    Semestar = model.Semestar
                };

                _context.Predmeti.Add(predmet);
                _context.SaveChanges();

                if (model.ProfesorIds != null && model.ProfesorIds.Any())
                {
                    foreach (var profesorId in model.ProfesorIds)
                    {
                        _context.PredmetProfesori.Add(new PredmetProfesor
                        {
                            PredmetId = predmet.Id,
                            ProfesorId = profesorId
                        });
                    }

                    predmet.ProfesorId = model.ProfesorIds.First();
                }

                if (model.AsistentIds != null && model.AsistentIds.Any())
                {
                    foreach (var asistentId in model.AsistentIds)
                    {
                        _context.PredmetAsistenti.Add(new PredmetAsistent
                        {
                            PredmetId = predmet.Id,
                            AsistentId = asistentId
                        });
                    }

                    predmet.AsistentId = model.AsistentIds.First();
                }

                _context.SaveChanges();
                _logger.LogInformation("Subjekt sa ID-om {PredmetId} je uspješno kreiran.", predmet.Id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Došlo je do greške prilikom kreiranja predmeta.");
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
                    _logger.LogWarning("Subjekt sa ID-om {PredmetId} nije pronađen.", id);
                    return NotFound();
                }

                var model = new PredmetCreateViewModel
                {
                    Naziv = predmet.Naziv,
                    Opis = predmet.Opis,
                    ECTS = predmet.ECTS,
                    ProfesorId = predmet.ProfesorId,
                    AsistentId = predmet.AsistentId,
                    NastavniPlanId = predmet.NastavniPlanId,
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
                        Text = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godina"
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
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = $"Nastavni plan za {np.GodinaStudija}. godinu",
                        Selected = np.Id == model.NastavniPlanId
                    }).ToList();

                return View(model);
            }

            try
            {
                var predmet = _context.Predmeti.Find(id);
                if (predmet == null)
                {
                    _logger.LogWarning("Subjekt sa ID-om {PredmetId} nije pronađen.", id);
                    return NotFound();
                }

                predmet.Naziv = model.Naziv;
                predmet.Opis = model.Opis;
                predmet.ECTS = model.ECTS;
                predmet.Semestar = model.Semestar;
                predmet.NastavniPlanId = model.NastavniPlanId.GetValueOrDefault();

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _context.PredmetProfesori.RemoveRange(_context.PredmetProfesori.Where(pp => pp.PredmetId == id));
                        _context.PredmetAsistenti.RemoveRange(_context.PredmetAsistenti.Where(pa => pa.PredmetId == id));

                        if (model.ProfesorIds != null)
                        {
                            foreach (var profesorId in model.ProfesorIds)
                            {
                                _context.PredmetProfesori.Add(new PredmetProfesor
                                {
                                    PredmetId = id,
                                    ProfesorId = profesorId
                                });
                            }

                            predmet.ProfesorId = model.ProfesorIds.First();
                        }

                        if (model.AsistentIds != null)
                        {
                            foreach (var asistentId in model.AsistentIds)
                            {
                                _context.PredmetAsistenti.Add(new PredmetAsistent
                                {
                                    PredmetId = id,
                                    AsistentId = asistentId
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
        [Route("Predmeti/AddGrade")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public IActionResult AddGrade(long predmetId, long studentId, float ocjena)
        {
            // Validacija ocjene
            if (ocjena < 5 || ocjena > 10)
            {
                ModelState.AddModelError("Ocjena", "Ocjena mora biti između 5 i 10.");
            }

            // Provjeri da li student postoji
            var student = _context.Studenti.Find(studentId);
            if (student == null)
            {
                ModelState.AddModelError("StudentId", "Odabrani student ne postoji.");
            }

            // Provjeri da li predmet postoji
            var predmet = _context.Predmeti
                .Include(p => p.NastavniPlan)
                .FirstOrDefault(p => p.Id == predmetId);
            if (predmet == null)
            {
                ModelState.AddModelError("PredmetId", "Odabrani predmet ne postoji.");
            }

            // Check if the professor exists
            var profesor = _context.Profesori.FirstOrDefault(p => p.Id == predmet.ProfesorId);
            if (profesor == null)
            {
                ModelState.AddModelError("ProfesorId", "Odabrani profesor ne postoji.");
            }

            if (!ModelState.IsValid)
            {
                // Osvježi ViewBag.Studenti za dropdown listu
                ViewBag.Studenti = _context.Studenti
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = $"{s.Ime} {s.Prezime} ({s.BrojIndeksa})"
                    }).ToList();

                // Osvježi ViewModel
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

                var ocjene = _context.Ocjene
                    .Where(o => studentiNaPredmetu.Select(snp => snp.StudentId).Contains(o.StudentId) && o.PredmetId == predmetId)
                    .ToDictionary(o => o.StudentId, o => (float?)o.Vrijednost);

                var viewModel = new PredmetDetailsViewModel
                {
                    Predmet = predmet,
                    Profesori = profesori,
                    Asistenti = asistenti,
                    StudentiNaPredmetu = studentiNaPredmetu,
                    Ocjene = ocjene
                };

                return View("Details", viewModel);
            }

            // Dodaj ocjenu studentu
            var ocjenaEntity = new Ocjena
            {
                PredmetId = predmetId,
                StudentId = studentId,
                ProfesorId = profesor.Id,
                Vrijednost = ocjena
            };

            _context.Ocjene.Add(ocjenaEntity);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Ocjena je uspješno dodana.";

            return RedirectToAction("Details", new { id = predmetId });
        }

        [HttpPost]
        [Route("Predmeti/EditGrade")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public IActionResult EditGrade(long predmetId, long studentId, float ocjena)
        {
            // Validate the grade
            if (ocjena < 5 || ocjena > 10)
            {
                ModelState.AddModelError("Ocjena", "Ocjena mora biti između 5 i 10.");
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
                ModelState.AddModelError("PredmetId", "Odabrani predmet ne postoji.");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", new { id = predmetId });
            }

            // Update the grade
            var ocjenaEntity = _context.Ocjene
                .FirstOrDefault(o => o.PredmetId == predmetId && o.StudentId == studentId);
            if (ocjenaEntity != null)
            {
                ocjenaEntity.Vrijednost = ocjena;
                _context.Ocjene.Update(ocjenaEntity);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Ocjena je uspješno ažurirana.";
            }
            else
            {
                ModelState.AddModelError("", "Ocjena nije pronađena.");
            }

            return RedirectToAction("Details", new { id = predmetId });
        }

        [HttpPost("RemoveGrade")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> RemoveGrade(long predmetId, long studentId)
        {
            var ocjena = await _context.Ocjene
                .FirstOrDefaultAsync(o => o.PredmetId == predmetId && o.StudentId == studentId);
            if (ocjena == null)
            {
                TempData["Error"] = "Ocjena nije pronađena.";
                return RedirectToAction("Details", new { id = predmetId });
            }

            _context.Ocjene.Remove(ocjena);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ocjena je uspješno uklonjena.";
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