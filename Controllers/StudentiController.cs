using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Studenti;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Studenti/Details/{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
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

        // GET: Studenti/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View();
        }

        // POST: Studenti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JMBG,Ime,Prezime,Email,Lozinka,BrojIndeksa,GodinaStudija,PredhodnoObrazovanje,Uloga")] Student student)
        {
            // Provjeri da li već postoji korisnik sa datim JMBG
            var postojiKorisnik = await _context.Korisnici
                .AnyAsync(k => k.JMBG == student.JMBG);

            if (postojiKorisnik)
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(student);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Studenti.Add(student);
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
            return View(student);
        }

        // GET: Studenti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
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
                Uloga = student.Uloga
            };

            ViewBag.Uloge = Enum.GetValues(typeof(Uloga))
                .Cast<Uloga>()
                .Select(u => new SelectListItem
                {
                    Value = ((int)u).ToString(),
                    Text = u.ToString(),
                    Selected = u == student.Uloga
                });

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

            // Proverite validaciju modela
            if (!ModelState.IsValid)
            {
                // Ponovo generišite dropdown listu za prikaz u slučaju greške
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
                var existingStudent = await _context.Studenti.FindAsync(id);
                if (existingStudent == null)
                {
                    return NotFound();
                }

                // Ažuriranje lozinke samo ako je uneta nova
                if (!string.IsNullOrEmpty(model.Lozinka))
                {
                    existingStudent.Lozinka = model.Lozinka;
                }

                // Ažuriranje ostalih podataka
                existingStudent.Ime = model.Ime;
                existingStudent.Prezime = model.Prezime;
                existingStudent.Email = model.Email;
                existingStudent.JMBG = model.JMBG;
                existingStudent.BrojIndeksa = model.BrojIndeksa;
                existingStudent.PredhodnoObrazovanje = model.PredhodnoObrazovanje;
                existingStudent.GodinaStudija = model.GodinaStudija;
                existingStudent.Uloga = model.Uloga;

                // Sačuvajte promene u bazi
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
    }
}