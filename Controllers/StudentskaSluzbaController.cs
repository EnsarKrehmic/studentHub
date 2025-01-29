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
        public async Task<IActionResult> Index(string searchString, long? studijskiProgramId)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStudijskiProgramId"] = studijskiProgramId;

            var query = _context.StudentskeSluzbe.Include(s => s.StudijskiProgram).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                query = query.Where(s => s.StudijskiProgramId == studijskiProgramId.Value);
            }

            var studentskaSluzba = await query.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Details/{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
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

        // GET: StudentskaSluzba/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View();
        }

        // POST: StudentskaSluzba/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentskaSluzbaEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var studentskaSluzba = new StudentskaSluzba
                {
                    JMBG = model.JMBG,
                    Ime = model.Ime,
                    Prezime = model.Prezime,
                    Email = model.Email,
                    Lozinka = model.Lozinka,
                    Uloga = model.Uloga,
                    StudijskiProgramId = model.StudijskiProgramId
                };

                _context.StudentskeSluzbe.Add(studentskaSluzba);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View(model);
        }

        // GET: StudentskaSluzba/Edit/{id}
        [HttpGet("Edit/{id:long}")]
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
                Lozinka = studentskaSluzba.Lozinka,
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
                studentskaSluzba.Lozinka = model.Lozinka;
                studentskaSluzba.Uloga = model.Uloga;
                studentskaSluzba.StudijskiProgramId = model.StudijskiProgramId;

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
