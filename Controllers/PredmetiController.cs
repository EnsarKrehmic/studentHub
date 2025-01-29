using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Linq;

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
        public IActionResult Index()
        {
            try
            {
                var predmeti = _context.Predmeti
                    .Include(p => p.Profesor)
                    .Include(p => p.Asistent)
                    .Include(p => p.NastavniPlan.StudijskiProgram)
                    .ToList();

                if (!predmeti.Any())
                {
                    _logger.LogInformation("Nije pronađen nijedan predmet.");
                }

                return View(predmeti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Došlo je do greške prilikom povlačenja predmeta.");
                return View("Error");
            }
        }

        // GET: Predmet/Details{id}
        [HttpGet("Details/{id:long}")]
        public IActionResult Details(long id)
        {
            try
            {
                var predmet = _context.Predmeti
                    .Include(p => p.NastavniPlan)
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

                var ocjene = _context.Ocjene
                    .Where(o => studentiNaPredmetu.Select(snp => snp.StudentId).Contains(o.StudentId) && o.PredmetId == id)
                    .ToDictionary(o => o.StudentId, o => (float?)o.Vrijednost);

                var viewModel = new PredmetDetailsViewModel
                {
                    Predmet = predmet,
                    Profesori = profesori,
                    Asistenti = asistenti,
                    StudentiNaPredmetu = studentiNaPredmetu,
                    Ocjene = ocjene
                };

                ViewBag.Studenti = _context.Studenti
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = $"{s.Ime} {s.Prezime} ({s.BrojIndeksa})"
                    }).ToList();

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

        [HttpPost("AddStudentToSubject")]
        [ValidateAntiForgeryToken]
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
                ModelState.AddModelError("PredmetId", "Odabrani predmet ne postoji.");
            }

            // Check if the student is already added to the subject
            var existingEntry = _context.StudentiNaPredmetima
                .FirstOrDefault(snp => snp.PredmetId == predmetId && snp.StudentId == studentId);
            if (existingEntry != null)
            {
                ModelState.AddModelError("StudentId", "Student je već dodan na ovaj predmet.");
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

            // Add the student to the subject
            var studentNaPredmetu = new StudentNaPredmetu
            {
                PredmetId = predmetId,
                StudentId = studentId,
                AkademskaGodina = DateTime.Now.Year.ToString()
            };

            _context.StudentiNaPredmetima.Add(studentNaPredmetu);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Student je uspješno dodan na predmet.";

            return RedirectToAction("Details", new { id = predmetId });
        }

        [HttpPost("RemoveStudentFromSubject")]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveStudentFromSubject(long predmetId, long studentId)
        {
            var studentNaPredmetu = _context.StudentiNaPredmetima
                .FirstOrDefault(snp => snp.PredmetId == predmetId && snp.StudentId == studentId);

            if (studentNaPredmetu == null)
            {
                ModelState.AddModelError("", "Student nije pronađen na ovom predmetu.");
                return RedirectToAction("Details", new { id = predmetId });
            }

            _context.StudentiNaPredmetima.Remove(studentNaPredmetu);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Student je uspješno uklonjen sa predmeta.";

            return RedirectToAction("Details", new { id = predmetId });
        }

        [HttpPost("AddGrade")]
        [ValidateAntiForgeryToken]
        public IActionResult AddGrade(long predmetId, long studentId, float ocjena)
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

            // Check if the professor exists
            var profesor = _context.Profesori.FirstOrDefault(p => p.Id == predmet.ProfesorId);
            if (profesor == null)
            {
                ModelState.AddModelError("ProfesorId", "Odabrani profesor ne postoji.");
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

            // Add the grade
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

        [HttpPost("EditGrade")]
        [ValidateAntiForgeryToken]
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
        public IActionResult RemoveGrade(long predmetId, long studentId)
        {
            // Check if the grade exists
            var ocjenaEntity = _context.Ocjene
                .FirstOrDefault(o => o.PredmetId == predmetId && o.StudentId == studentId);
            if (ocjenaEntity != null)
            {
                _context.Ocjene.Remove(ocjenaEntity);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Ocjena je uspješno uklonjena.";
            }
            else
            {
                ModelState.AddModelError("", "Ocjena nije pronađena.");
            }

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
    }
}