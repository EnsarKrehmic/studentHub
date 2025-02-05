using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Route("Ocjene")]
    [Authorize]
    public class OcjeneController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OcjeneController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ocjene
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var ocjene = await _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Profesor)
                .Include(o => o.Student)
                .ToListAsync();

            return View(ocjene);
        }

        // GET: Ocjene/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Profesor)
                .Include(o => o.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ocjena == null)
            {
                return NotFound();
            }

            return View(ocjena);
        }

        // GET: Ocjene/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Profesor")]
        public IActionResult Create()
        {
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv");
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime");
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "brojIndeksa");
            return View();
        }

        // POST: Ocjene/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Create([Bind("Id,Vrijednost,PredmetId,brojIndeksa,StudentId,ProfesorId")] Ocjena ocjena)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ocjena);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ocjena.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", ocjena.ProfesorId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "brojIndeksa", ocjena.StudentId);
            return View(ocjena);
        }

        // GET: Ocjene/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjene.FindAsync(id);
            if (ocjena == null)
            {
                return NotFound();
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ocjena.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", ocjena.ProfesorId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "brojIndeksa", ocjena.StudentId);
            return View(ocjena);
        }

        // POST: Ocjene/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Vrijednost,PredmetId,brojIndeksa,StudentId,ProfesorId")] Ocjena ocjena)
        {
            if (id != ocjena.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ocjena);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OcjenaExists(ocjena.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ocjena.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", ocjena.ProfesorId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "brojIndeksa", ocjena.StudentId);
            return View(ocjena);
        }

        // GET: Ocjene/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Profesor)
                .Include(o => o.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ocjena == null)
            {
                return NotFound();
            }

            return View(ocjena);
        }

        // POST: Ocjene/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ocjena = await _context.Ocjene.FindAsync(id);
            if (ocjena != null)
            {
                _context.Ocjene.Remove(ocjena);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OcjenaExists(long id)
        {
            return _context.Ocjene.Any(e => e.Id == id);
        }

        // GET: Ocjene/Student/{studentId}
        [HttpGet("Student/{studentId:long}")]
        public async Task<IActionResult> Student(long studentId)
        {
            var student = await _context.Studenti
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound();
            }

            var ocjene = await _context.Ocjene
                .Where(o => o.StudentId == studentId)
                .Include(o => o.Predmet)
                .ToListAsync();

            var ocjeneViewModel = new OcjeneViewModel
            {
                StudentId = student.Id,
                StudentIme = student.Ime,
                StudentPrezime = student.Prezime,
                Ocjene = ocjene.Select(o => new OcjenaPredmetViewModel
                {
                    PredmetNaziv = o.Predmet.Naziv,
                    OcjenaVrijednost = o.Vrijednost
                }).ToList(),
                Prosjek = ocjene.Any() ? ocjene.Average(o => o.Vrijednost) : 0
            };

            return View(ocjeneViewModel);
        }
    }
}