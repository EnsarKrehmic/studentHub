using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Route("Profesori")]
    public class ProfesoriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfesoriController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Profesori
        [HttpGet("")]
        public async Task<IActionResult> Index(string sortOrder, string searchString, long? studijskiProgramId)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SurnameSortParm"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["JMBGSortParm"] = sortOrder == "jmbg_asc" ? "jmbg_desc" : "jmbg_asc";
            ViewData["EmailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewData["TitulaSortParm"] = sortOrder == "titula_asc" ? "titula_desc" : "titula_asc";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStudijskiProgramId"] = studijskiProgramId;

            var profesoriQuery = _context.Profesori.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                profesoriQuery = profesoriQuery.Where(p => p.Ime.Contains(searchString) || p.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                profesoriQuery = profesoriQuery.Where(p => _context.ProfesorStudijskiProgrami.Any(psp => psp.ProfesorId == p.Id && psp.StudijskiProgramId == studijskiProgramId.Value));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.Ime);
                    break;
                case "surname_asc":
                    profesoriQuery = profesoriQuery.OrderBy(p => p.Prezime);
                    break;
                case "surname_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.Prezime);
                    break;
                case "jmbg_asc":
                    profesoriQuery = profesoriQuery.OrderBy(p => p.JMBG);
                    break;
                case "jmbg_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.JMBG);
                    break;
                case "email_asc":
                    profesoriQuery = profesoriQuery.OrderBy(p => p.Email);
                    break;
                case "email_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.Email);
                    break;
                case "titula_asc":
                    profesoriQuery = profesoriQuery.OrderBy(p => p.ProfesorTitula);
                    break;
                case "titula_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.ProfesorTitula);
                    break;
                default:
                    profesoriQuery = profesoriQuery.OrderBy(p => p.Ime);
                    break;
            }

            var profesori = await profesoriQuery.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(profesori);
        }

        // GET: Profesori/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesori
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profesor == null)
            {
                return NotFound();
            }

            var studijskiProgrami = await _context.ProfesorStudijskiProgrami
                .Where(psp => psp.ProfesorId == id)
                .Select(psp => psp.StudijskiProgram)
                .ToListAsync();

            var predmeti = await _context.PredmetProfesori
                .Where(pp => pp.ProfesorId == id)
                .Select(pp => pp.Predmet)
                .ToListAsync();

            var viewModel = new ProfesorDetailsViewModel
            {
                Profesor = profesor,
                StudijskiProgrami = studijskiProgrami,
                Predmeti = predmeti
            };

            return View(viewModel);
        }

        // GET: Profesori/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var profesor = await _context.Profesori.FindAsync(id);
            if (profesor == null)
            {
                return NotFound();
            }

            var model = new ProfesorEditViewModel
            {
                Id = profesor.Id,
                Ime = profesor.Ime,
                Prezime = profesor.Prezime,
                JMBG = profesor.JMBG,
                Email = profesor.Email,
                ProfesorTitula = profesor.ProfesorTitula,
                Uloga = profesor.Uloga,
                StudijskiProgramIds = await _context.ProfesorStudijskiProgrami
                    .Where(psp => psp.ProfesorId == profesor.Id)
                    .Select(psp => psp.StudijskiProgramId)
                    .ToListAsync(),
                PredmetIds = await _context.PredmetProfesori
                    .Where(pp => pp.ProfesorId == profesor.Id)
                    .Select(pp => pp.PredmetId)
                    .ToListAsync()
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.Predmeti = new SelectList(_context.Predmeti, "Id", "Naziv");
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View(model);
        }

        // POST: Profesori/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, ProfesorEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var profesor = await _context.Profesori.FindAsync(id);
                if (profesor == null)
                {
                    return NotFound();
                }

                profesor.Ime = model.Ime;
                profesor.Prezime = model.Prezime;
                profesor.JMBG = model.JMBG;
                profesor.Email = model.Email;
                profesor.ProfesorTitula = model.ProfesorTitula;
                profesor.Uloga = model.Uloga;

                _context.Update(profesor);

                var existingStudijskiProgrami = await _context.ProfesorStudijskiProgrami
                    .Where(psp => psp.ProfesorId == profesor.Id)
                    .Select(psp => psp.StudijskiProgramId)
                    .ToListAsync();
                var newStudijskiProgrami = model.StudijskiProgramIds.Except(existingStudijskiProgrami).ToList();
                var removedStudijskiProgrami = existingStudijskiProgrami.Except(model.StudijskiProgramIds).ToList();

                foreach (var studijskiProgramId in newStudijskiProgrami)
                {
                    _context.ProfesorStudijskiProgrami.Add(new ProfesorStudijskiProgram
                    {
                        ProfesorId = profesor.Id,
                        StudijskiProgramId = studijskiProgramId
                    });
                }

                foreach (var studijskiProgramId in removedStudijskiProgrami)
                {
                    var profesorStudijskiProgram = await _context.ProfesorStudijskiProgrami
                        .FirstOrDefaultAsync(psp => psp.ProfesorId == profesor.Id && psp.StudijskiProgramId == studijskiProgramId);
                    if (profesorStudijskiProgram != null)
                    {
                        _context.ProfesorStudijskiProgrami.Remove(profesorStudijskiProgram);
                    }
                }

                var existingPredmeti = await _context.PredmetProfesori
                    .Where(pp => pp.ProfesorId == profesor.Id)
                    .Select(pp => pp.PredmetId)
                    .ToListAsync();
                var newPredmeti = model.PredmetIds.Except(existingPredmeti).ToList();
                var removedPredmeti = existingPredmeti.Except(model.PredmetIds).ToList();

                foreach (var predmetId in newPredmeti)
                {
                    _context.PredmetProfesori.Add(new PredmetProfesor
                    {
                        ProfesorId = profesor.Id,
                        PredmetId = predmetId
                    });
                }

                foreach (var predmetId in removedPredmeti)
                {
                    var predmetProfesor = await _context.PredmetProfesori
                        .FirstOrDefaultAsync(pp => pp.ProfesorId == profesor.Id && pp.PredmetId == predmetId);
                    if (predmetProfesor != null)
                    {
                        _context.PredmetProfesori.Remove(predmetProfesor);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.Predmeti = new SelectList(_context.Predmeti, "Id", "Naziv");
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View(model);
        }

        // GET: Profesori/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesori.FirstOrDefaultAsync(m => m.Id == id);
            if (profesor == null)
            {
                return NotFound();
            }

            return View(profesor);
        }

        // POST: Profesori/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var profesor = await _context.Profesori.FindAsync(id);
            if (profesor != null)
            {
                _context.Profesori.Remove(profesor);
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("GetPredmetiByStudijskiProgram")]
        public async Task<IActionResult> GetPredmetiByStudijskiProgram([FromBody] List<long> studijskiProgramIds)
        {
            if (studijskiProgramIds == null || !studijskiProgramIds.Any())
            {
                return Json(new List<object>());
            }

            var predmeti = await _context.Predmeti
                .Where(p => studijskiProgramIds.Contains(p.NastavniPlan.StudijskiProgramId))
                .Select(p => new { id = p.Id, naziv = p.Naziv })
                .ToListAsync();

            return Json(predmeti);
        }
    }
}