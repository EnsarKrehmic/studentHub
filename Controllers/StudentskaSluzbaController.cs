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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.StudentskeSluzbe;
            return View(await applicationDbContext.ToListAsync());
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
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View();
        }

        // POST: StudentskaSluzba/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Uloga,Ime,Prezime,JMBG,Email,Lozinka")] StudentskaSluzba studentskaSluzba)
        {
            // Provjera da li već postoji korisnik sa datim JMBG
            var postojiKorisnik = await _context.Korisnici
                .AnyAsync(k => k.JMBG == studentskaSluzba.JMBG);

            if (postojiKorisnik)
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(studentskaSluzba);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.StudentskeSluzbe.Add(studentskaSluzba);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Došlo je do greške: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom kreiranja asistenta.");
                }
            }
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View(studentskaSluzba);
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
                Ime = studentskaSluzba.Ime,
                Prezime = studentskaSluzba.Prezime,
                JMBG = studentskaSluzba.JMBG,
                Email = studentskaSluzba.Email,
                Lozinka = null,
                Uloga = studentskaSluzba.Uloga
            };

            ViewBag.Uloge = Enum.GetValues(typeof(Uloga))
                .Cast<Uloga>()
                .Select(u => new SelectListItem
                {
                    Value = ((int)u).ToString(),
                    Text = u.ToString(),
                    Selected = u == studentskaSluzba.Uloga
                });

            return View(model);
        }

        // POST: StudentskaSluzba/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, StudentEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Provjera validacije modela
            if (!ModelState.IsValid)
            {
                // Generisanje dropdown liste za prikaz u slučaju greške
                ViewBag.Uloge = Enum.GetValues(typeof(Uloga))
                    .Cast<Uloga>()
                    .Select(u => new SelectListItem
                    {
                        Value = ((int)u).ToString(),
                        Text = u.ToString(),
                        Selected = u == model.Uloga
                    });
                Console.WriteLine($"Uloga iz forme: {model.Uloga}");
                return View(model);
            }

            if (!Enum.IsDefined(typeof(Uloga), model.Uloga))
            {
                ModelState.AddModelError(nameof(model.Uloga), "Izabrana uloga nije validna.");
                return View(model);
            }

            try
            {
                var existingStudentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
                if (existingStudentskaSluzba == null)
                {
                    return NotFound();
                }

                // Ažurirajte lozinku samo ako je uneta nova
                if (!string.IsNullOrEmpty(model.Lozinka))
                {
                    existingStudentskaSluzba.Lozinka = model.Lozinka;
                }

                // Ažurirajte ostale podatke
                existingStudentskaSluzba.Ime = model.Ime;
                existingStudentskaSluzba.Prezime = model.Prezime;
                existingStudentskaSluzba.Email = model.Email;
                existingStudentskaSluzba.JMBG = model.JMBG;
                existingStudentskaSluzba.Uloga = model.Uloga;

                // Sačuvajte promene u bazi
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentskaSluzbaExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
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
