using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Route("StudentskaSluzba")]
    public class StudentskaSluzbaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentskaSluzbaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudentskaSluzba
        [HttpGet("")]
        public async Task<IActionResult> Index(string sortOrder, string searchString, long? studijskiProgramId)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SurnameSortParm"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["JMBGSortParm"] = sortOrder == "jmbg_asc" ? "jmbg_desc" : "jmbg_asc";
            ViewData["EmailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStudijskiProgramId"] = studijskiProgramId;

            var studentskesluzbequery = _context.StudentskeSluzbe.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentskesluzbequery = studentskesluzbequery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                studentskesluzbequery = studentskesluzbequery.Where(p => _context.StudentskaSluzbaStudijskiProgrami.Any(psp => psp.StudentskaSluzbaId == p.Id && psp.StudijskiProgramId == studijskiProgramId.Value));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    studentskesluzbequery = studentskesluzbequery.OrderByDescending(p => p.Ime);
                    break;
                case "surname_asc":
                    studentskesluzbequery = studentskesluzbequery.OrderBy(p => p.Prezime);
                    break;
                case "surname_desc":
                    studentskesluzbequery = studentskesluzbequery.OrderByDescending(p => p.Prezime);
                    break;
                case "jmbg_asc":
                    studentskesluzbequery = studentskesluzbequery.OrderBy(p => p.JMBG);
                    break;
                case "jmbg_desc":
                    studentskesluzbequery = studentskesluzbequery.OrderByDescending(p => p.JMBG);
                    break;
                case "email_asc":
                    studentskesluzbequery = studentskesluzbequery.OrderBy(p => p.Email);
                    break;
                case "email_desc":
                    studentskesluzbequery = studentskesluzbequery.OrderByDescending(p => p.Email);
                    break;
                default:
                    studentskesluzbequery = studentskesluzbequery.OrderBy(p => p.Ime);
                    break;
            }

            var studentskaSluzba = await studentskesluzbequery.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentskasluzba = await _context.StudentskeSluzbe
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studentskasluzba == null)
            {
                return NotFound();
            }

            var studijskiProgrami = await _context.StudentskaSluzbaStudijskiProgrami
                .Where(psp => psp.StudentskaSluzbaId == id)
                .Select(psp => psp.StudijskiProgram)
                .ToListAsync();

            var viewModel = new StudentskaSluzbaDetailsViewModel
            {
                StudentskaSluzba = studentskasluzba,
                StudijskiProgrami = studijskiProgrami,
            };

            return View(viewModel);
        }

        // GET: StudentskaSluzba/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var studentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
            if (studentskaSluzba == null)
            {
                return NotFound();
            }

            var model = new StudentskaSluzbaEditViewModel
            {
                Id = studentskaSluzba.Id,
                JMBG = studentskaSluzba.JMBG,
                Ime = studentskaSluzba.Ime,
                Prezime = studentskaSluzba.Prezime,
                Email = studentskaSluzba.Email,
                Uloga = studentskaSluzba.Uloga,
                StudijskiProgramId = studentskaSluzba.StudijskiProgramId
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studentskaSluzba.StudijskiProgramId);
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());

            return View(model);
        }

        // POST: StudentskaSluzba/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, StudentskaSluzbaEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var studentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
                if (studentskaSluzba == null)
                {
                    return NotFound();
                }

                studentskaSluzba.JMBG = model.JMBG;
                studentskaSluzba.Ime = model.Ime;
                studentskaSluzba.Prezime = model.Prezime;
                studentskaSluzba.Email = model.Email;
                studentskaSluzba.Uloga = model.Uloga;
                studentskaSluzba.StudijskiProgramId = model.StudijskiProgramId;

                // Lozinku ne menjamo!
                _context.Update(studentskaSluzba);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());

            return View(model);
        }

        // GET: StudentskaSluzba/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync(m => m.Id == id);
            if (studentskaSluzba == null)
            {
                return NotFound();
            }

            return View(studentskaSluzba);
        }

        // POST: StudentskaSluzba/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var studentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
            if (studentskaSluzba != null)
            {
                _context.StudentskeSluzbe.Remove(studentskaSluzba);
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentskaSluzbaExists(long id)
        {
            return _context.StudentskeSluzbe.Any(e => e.Id == id);
        }
    }
}
