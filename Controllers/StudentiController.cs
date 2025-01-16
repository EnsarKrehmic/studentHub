using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using X.PagedList.Extensions;

namespace StudentHub.Controllers
{
    [Route("Studenti")]
    public class StudentiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Studenti
        [HttpGet("")]
        public async Task<IActionResult> Index(string sortOrder, string searchString, long? studijskiProgramId)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["IndexSortParm"] = sortOrder == "index_asc" ? "index_desc" : "index_asc";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStudijskiProgramId"] = studijskiProgramId;

            var studentsQuery = _context.Studenti
                .Include(s => s.StudijskiProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.StudijskiProgramId == studijskiProgramId.Value);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    studentsQuery = studentsQuery.OrderByDescending(s => s.Ime);
                    break;
                case "index_asc":
                    studentsQuery = studentsQuery.OrderBy(s => s.BrojIndeksa);
                    break;
                case "index_desc":
                    studentsQuery = studentsQuery.OrderByDescending(s => s.BrojIndeksa);
                    break;
                default:
                    studentsQuery = studentsQuery.OrderBy(s => s.Ime);
                    break;
            }

            var students = await studentsQuery.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(students);
        }

        // GET: Studenti/Details{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id, string sortOrder, string searchString, long? studijskiProgramId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti
                .Include(s => s.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            var predmeti = await _context.StudentiNaPredmetima
                .Where(snp => snp.StudentId == id)
                .Select(snp => snp.Predmet)
                .ToListAsync();

            var ocjene = await _context.Ocjene
                .Where(o => o.StudentId == id)
                .ToDictionaryAsync(o => o.PredmetId, o => (float?)o.Vrijednost);

            var studentsQuery = _context.Studenti
                .Include(s => s.StudijskiProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.StudijskiProgramId == studijskiProgramId.Value);
            }

            var groupedStudents = await studentsQuery
                .GroupBy(s => s.StudijskiProgram)
                .Select(g => new StudentiGroupedByProgramViewModel
                {
                    StudijskiProgram = g.Key,
                    Studenti = g.ToList()
                })
                .ToListAsync();

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    groupedStudents.ForEach(g => g.Studenti = g.Studenti.OrderByDescending(s => s.Ime).ToList());
                    break;
                case "index_asc":
                    groupedStudents.ForEach(g => g.Studenti = g.Studenti.OrderBy(s => s.BrojIndeksa).ToList());
                    break;
                case "index_desc":
                    groupedStudents.ForEach(g => g.Studenti = g.Studenti.OrderByDescending(s => s.BrojIndeksa).ToList());
                    break;
                default:
                    groupedStudents.ForEach(g => g.Studenti = g.Studenti.OrderBy(s => s.Ime).ToList());
                    break;
            }

            var viewModel = new StudentDetailsViewModel
            {
                Student = student,
                GroupedStudents = groupedStudents,
                CurrentSort = sortOrder,
                SearchString = searchString,
                StudijskiProgramId = studijskiProgramId,
                Predmeti = predmeti,
                Ocjene = ocjene
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(viewModel);
        }

        // GET: Studenti/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.NastavniPlanovi = new SelectList(_context.NastavniPlanovi, "Id", "GodinaStudija");
            ViewBag.Predmeti = new SelectList(_context.Predmeti, "Id", "Naziv");
            return View();
        }

        // POST: Studenti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JMBG,Ime,Prezime,Email,Lozinka,BrojIndeksa,GodinaStudija,PredhodnoObrazovanje,Uloga,StudijskiProgramId,NastavniPlanId,Semestar,PredmetIds")] StudentCreateViewModel model)
        {
            // Provjeri da li već postoji korisnik sa datim JMBG
            var postojiKorisnik = await _context.Korisnici
                .AnyAsync(k => k.JMBG == model.JMBG);

            if (postojiKorisnik)
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var student = new Student
                    {
                        JMBG = model.JMBG,
                        Ime = model.Ime,
                        Prezime = model.Prezime,
                        Email = model.Email,
                        Lozinka = model.Lozinka,
                        BrojIndeksa = model.BrojIndeksa,
                        GodinaStudija = model.GodinaStudija,
                        PredhodnoObrazovanje = model.PredhodnoObrazovanje,
                        Uloga = model.Uloga,
                        StudijskiProgramId = model.StudijskiProgramId,
                        NastavniPlanId = model.NastavniPlanId,
                        Semestar = model.Semestar
                    };

                    _context.Studenti.Add(student);
                    await _context.SaveChangesAsync();

                    if (model.PredmetIds != null && model.PredmetIds.Any())
                    {
                        foreach (var predmetId in model.PredmetIds)
                        {
                            _context.StudentiNaPredmetima.Add(new StudentNaPredmetu
                            {
                                StudentId = student.Id,
                                PredmetId = predmetId,
                                AkademskaGodina = DateTime.Now.Year.ToString()
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Došlo je do greške: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom kreiranja asistenta.");
                }
            }
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.NastavniPlanovi = new SelectList(_context.NastavniPlanovi, "Id", "GodinaStudija");
            ViewBag.Predmeti = new SelectList(_context.Predmeti, "Id", "Naziv");
            return View(model);
        }

        // GET: Studenti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var student = await _context.Studenti
                .FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            var model = new StudentEditViewModel
            {
                Id = student.Id,
                JMBG = student.JMBG,
                Ime = student.Ime,
                Prezime = student.Prezime,
                Email = student.Email,
                BrojIndeksa = student.BrojIndeksa,
                PredhodnoObrazovanje = student.PredhodnoObrazovanje,
                GodinaStudija = student.GodinaStudija,
                Lozinka = null,
                Uloga = student.Uloga,
                StudijskiProgramId = student.StudijskiProgramId,
                NastavniPlanId = student.NastavniPlanId,
                Semestar = student.Semestar,
                PredmetIds = await _context.StudentiNaPredmetima
                    .Where(snp => snp.StudentId == student.Id)
                    .Select(snp => snp.PredmetId)
                    .ToListAsync()
            };

            ViewBag.Uloge = Enum.GetValues(typeof(Uloga))
                .Cast<Uloga>()
                .Select(u => new SelectListItem
                {
                    Value = ((int)u).ToString(),
                    Text = u.ToString(),
                    Selected = u == student.Uloga
                });

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", student.StudijskiProgramId);
            ViewBag.NastavniPlanovi = new SelectList(_context.NastavniPlanovi, "Id", "GodinaStudija", student.NastavniPlanId);
            ViewBag.Predmeti = new SelectList(_context.Predmeti.Where(p => p.NastavniPlan.StudijskiProgramId == student.StudijskiProgramId), "Id", "Naziv");

            return View(model);
        }

        // POST: Studenti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, StudentEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Uloge = Enum.GetValues(typeof(Uloga))
                    .Cast<Uloga>()
                    .Select(u => new SelectListItem
                    {
                        Value = ((int)u).ToString(),
                        Text = u.ToString(),
                        Selected = u == model.Uloga
                    });
                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);
                ViewBag.NastavniPlanovi = new SelectList(_context.NastavniPlanovi, "Id", "GodinaStudija", model.NastavniPlanId);
                ViewBag.Predmeti = new SelectList(_context.Predmeti.Where(p => p.NastavniPlan.StudijskiProgramId == model.StudijskiProgramId), "Id", "Naziv");
                return View(model);
            }

            if (!Enum.IsDefined(typeof(Uloga), model.Uloga))
            {
                ModelState.AddModelError(nameof(model.Uloga), "Izabrana uloga nije validna.");
                return View(model);
            }

            try
            {
                var existingStudent = await _context.Studenti
                    .FirstOrDefaultAsync(s => s.Id == id);
                if (existingStudent == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(model.Lozinka))
                {
                    existingStudent.Lozinka = model.Lozinka;
                }

                existingStudent.Ime = model.Ime;
                existingStudent.Prezime = model.Prezime;
                existingStudent.Email = model.Email;
                existingStudent.JMBG = model.JMBG;
                existingStudent.BrojIndeksa = model.BrojIndeksa;
                existingStudent.PredhodnoObrazovanje = model.PredhodnoObrazovanje;
                existingStudent.GodinaStudija = model.GodinaStudija;
                existingStudent.Uloga = model.Uloga;
                existingStudent.StudijskiProgramId = model.StudijskiProgramId;
                existingStudent.NastavniPlanId = model.NastavniPlanId;
                existingStudent.Semestar = model.Semestar;

                var existingPredmeti = await _context.StudentiNaPredmetima
                    .Where(snp => snp.StudentId == existingStudent.Id)
                    .Select(snp => snp.PredmetId)
                    .ToListAsync();
                var newPredmeti = model.PredmetIds.Except(existingPredmeti).ToList();
                var removedPredmeti = existingPredmeti.Except(model.PredmetIds).ToList();

                foreach (var predmetId in newPredmeti)
                {
                    _context.StudentiNaPredmetima.Add(new StudentNaPredmetu
                    {
                        StudentId = existingStudent.Id,
                        PredmetId = predmetId,
                        AkademskaGodina = DateTime.Now.Year.ToString()
                    });
                }

                foreach (var predmetId in removedPredmeti)
                {
                    var studentNaPredmetu = await _context.StudentiNaPredmetima
                        .FirstOrDefaultAsync(snp => snp.StudentId == existingStudent.Id && snp.PredmetId == predmetId);
                    if (studentNaPredmetu != null)
                    {
                        _context.StudentiNaPredmetima.Remove(studentNaPredmetu);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // GET: Studenti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Studenti/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student != null)
            {
                _context.Studenti.Remove(student);
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id)
        {
            return _context.Studenti.Any(e => e.Id == id);
        }

        [HttpGet("GroupedByProgram")]
        public async Task<IActionResult> GroupedByProgram()
        {
            var groupedStudents = await _context.Studenti
                .Include(s => s.StudijskiProgram)
                .GroupBy(s => s.StudijskiProgram)
                .Select(g => new StudentiGroupedByProgramViewModel
                {
                    StudijskiProgram = g.Key,
                    Studenti = g.ToList()
                })
                .ToListAsync();

            return View(groupedStudents);
        }

        [HttpGet("GetPredmetiByStudijskiProgram/{studijskiProgramId}")]
        public async Task<IActionResult> GetPredmetiByStudijskiProgram(long studijskiProgramId)
        {
            var predmeti = await _context.Predmeti
                .Where(p => p.NastavniPlan.StudijskiProgramId == studijskiProgramId)
                .Select(p => new { id = p.Id, naziv = p.Naziv })
                .ToListAsync();

            return Json(predmeti);
        }
    }
}