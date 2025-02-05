using Microsoft.AspNetCore.Authorization;
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
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SurnameSortParm"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["JMBGSortParm"] = sortOrder == "jmbg_asc" ? "jmbg_desc" : "jmbg_asc";
            ViewData["IndexSortParm"] = sortOrder == "index_asc" ? "index_desc" : "index_asc";
            ViewData["EmailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStudijskiProgramId"] = studijskiProgramId;

            var studentiQuery = _context.Studenti.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentiQuery = studentiQuery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                studentiQuery = studentiQuery.Where(p => _context.StudentStudijskiProgrami.Any(psp => psp.StudentId == p.Id && psp.StudijskiProgramId == studijskiProgramId.Value));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Ime);
                    break;
                case "surname_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.Prezime);
                    break;
                case "surname_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Prezime);
                    break;
                case "jmbg_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.JMBG);
                    break;
                case "jmbg_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.JMBG);
                    break;
                case "index_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.BrojIndeksa);
                    break;
                case "index_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.BrojIndeksa);
                    break;
                default:
                    studentiQuery = studentiQuery.OrderBy(s => s.Ime);
                    break;
            }

            var students = await studentiQuery.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(students);
        }

        // GET: Studenti/Details{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id, string sortOrder, string searchString, long? studijskiProgramId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id)
                ?? throw new InvalidOperationException("Student nije pronađen.");

            // Uzmite prvi (ili glavni) studijski program ako postoji
            var studijskiProgram = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram;

            var predmeti = await _context.StudentiNaPredmetima
                .Include(snp => snp.Predmet)
                .Where(snp => snp.StudentId == id)
                .Select(snp => snp.Predmet)
                .ToListAsync();

            var ocjene = await _context.Ocjene
                .Where(o => o.StudentId == id)
                .ToDictionaryAsync(o => o.PredmetId, o => (float?)o.Vrijednost);

            var studentsQuery = _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                studentsQuery = studentsQuery
                    .Where(s => s.StudentStudijskiProgrami.Any(ssp => ssp.StudijskiProgramId == studijskiProgramId.Value));
            }

            var viewModel = new StudentDetailsViewModel
            {
                Student = student,
                CurrentSort = sortOrder,
                SearchString = searchString,
                StudijskiProgramId = studijskiProgramId,
                Predmeti = predmeti,
                Ocjene = ocjene
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(viewModel);
        }

        // GET: Studenti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var studijskiProgramId = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgramId ?? 0;

            var model = new StudentEditViewModel
            {
                Id = student.Id,
                Ime = student.Ime,
                Prezime = student.Prezime,
                JMBG = student.JMBG,
                Email = student.Email,
                BrojIndeksa = student.BrojIndeksa,
                StudijskiProgramId = studijskiProgramId,
                GodinaStudija = student.GodinaStudija,
                Semestar = student.Semestar,
                PrethodnoObrazovanje = student.PrethodnoObrazovanje,
                Uloga = student.Uloga,
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

            ViewBag.StudijskiProgrami = new SelectList(
                _context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);
            ViewBag.NastavniPlanovi = new SelectList(
                _context.NastavniPlanovi, "Id", "GodinaStudija", student.NastavniPlanId);
            ViewBag.Predmeti = new SelectList(
                _context.Predmeti.Where(p => _context.StudentiNaPredmetima
                    .Any(snp => snp.StudentId == student.Id && snp.PredmetId == p.Id)),
                "Id", "Naziv");

            return View(model);
        }

        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, StudentEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.StudijskiProgrami = new SelectList(await _context.StudijskiProgrami.ToListAsync(), "Id", "Naziv");
                ViewBag.Predmeti = new SelectList(await _context.Predmeti.ToListAsync(), "Id", "Naziv");
                return View(model);
            }

            var existingStudent = await _context.Studenti.Include(s => s.StudentStudijskiProgrami)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStudent == null)
            {
                return NotFound();
            }

            existingStudent.Ime = model.Ime;
            existingStudent.Prezime = model.Prezime;
            existingStudent.Email = model.Email;
            existingStudent.BrojIndeksa = model.BrojIndeksa;
            existingStudent.PrethodnoObrazovanje = model.PrethodnoObrazovanje;
            existingStudent.GodinaStudija = model.GodinaStudija;
            existingStudent.Uloga = model.Uloga;
            existingStudent.Semestar = model.Semestar;

            var nastavniPlan = await _context.NastavniPlanovi
                .FirstOrDefaultAsync(np => np.StudijskiProgramId == model.StudijskiProgramId
                                           && np.GodinaStudija == model.GodinaStudija.ToString());

            if (nastavniPlan == null)
            {
                ModelState.AddModelError("NastavniPlanId", "Nastavni plan za odabranu godinu studija nije pronađen.");
                return View(model);
            }

            existingStudent.NastavniPlanId = nastavniPlan.Id;

            var stariPredmeti = await _context.StudentiNaPredmetima
                .Where(snp => snp.StudentId == existingStudent.Id)
                .ToListAsync();

            _context.StudentiNaPredmetima.RemoveRange(stariPredmeti);

            if (model.PredmetIds != null && model.PredmetIds.Any())
            {
                foreach (var predmetId in model.PredmetIds)
                {
                    _context.StudentiNaPredmetima.Add(new StudentNaPredmetu
                    {
                        StudentId = existingStudent.Id,
                        PredmetId = predmetId,
                        AkademskaGodina = DateTime.Now.Year.ToString()
                    });
                }
            }

            // Ažuriranje pomoćne tabele StudentStudijskiProgram
            var existingEntries = _context.StudentStudijskiProgrami
                .Where(ssp => ssp.StudentId == existingStudent.Id)
                .ToList();

            _context.StudentStudijskiProgrami.RemoveRange(existingEntries);

            if (model.StudijskiProgramId != 0)
            {
                _context.StudentStudijskiProgrami.Add(new StudentStudijskiProgram
                {
                    StudentId = existingStudent.Id,
                    StudijskiProgramId = model.StudijskiProgramId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Studenti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
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
        [Authorize(Roles = "Studentska služba")]
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
                .Include(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .SelectMany(s => s.StudentStudijskiProgrami)
                .GroupBy(ssp => ssp.StudijskiProgram)
                .Select(g => new StudentiGroupedByProgramViewModel
                {
                    StudijskiProgram = g.Key,
                    Studenti = g.Select(ssp => ssp.Student).ToList()
                })
                .ToListAsync();


            return View(groupedStudents);
        }

        [HttpGet("GetPredmetiByStudijskiProgram/{studijskiProgramId}")]
        public async Task<IActionResult> GetPredmetiByStudijskiProgram(long studijskiProgramId)
        {
            var predmeti = await _context.Predmeti
                .Where(p => p.NastavniPlan.StudijskiProgramId == studijskiProgramId)
                .Select(p => new
                {
                    id = p.Id,
                    naziv = p.Naziv
                })
                .ToListAsync();

            return Json(predmeti);
        }

        [HttpGet("GetNastavniPlanovi/{studijskiProgramId}")]
        public async Task<IActionResult> GetNastavniPlanovi(long studijskiProgramId)
        {
            var nastavaniPlanovi = await _context.NastavniPlanovi
                .Where(np => np.StudijskiProgramId == studijskiProgramId)
                .Select(np => new
                {
                    id = np.Id,
                    naziv = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godinu"
                })
                .ToListAsync();

            return Json(nastavaniPlanovi);
        }

    }
}