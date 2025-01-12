using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Profesori;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Profesori/Details/{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
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

        // GET: Profesori/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View();
        }

        // POST: Profesori/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Uloga,Ime,Prezime,JMBG,Email,Lozinka,ProfesorTitula")] Profesor profesor)
        {
            // Provjera da li već postoji korisnik sa datim JMBG
            var postojiKorisnik = await _context.Korisnici
                .AnyAsync(k => k.JMBG == profesor.JMBG);

            if (postojiKorisnik)
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(profesor);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Profesori.Add(profesor);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Došlo je do greške: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom kreiranja profesora.");
                }
            }
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View(profesor);
        }

        // GET: Profesori/Edit/{id}
        [HttpGet("Edit/{id:long}")]
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
                JMBG = profesor.JMBG,
                Ime = profesor.Ime,
                Prezime = profesor.Prezime,
                Email = profesor.Email,
                ProfesorTitula = profesor.ProfesorTitula,
                Lozinka = null,
                Uloga = profesor.Uloga
            };

            ViewBag.Uloge = Enum.GetValues(typeof(Uloga))
                .Cast<Uloga>()
                .Select(u => new SelectListItem
                {
                    Value = ((int)u).ToString(),
                    Text = u.ToString(),
                    Selected = u == profesor.Uloga
                });

            return View(model);
        }

        // POST: Profesori/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, ProfesorEditViewModel model)
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
                var existingProfesor = await _context.Profesori.FindAsync(id);
                if (existingProfesor == null)
                {
                    return NotFound();
                }

                // Ažurirajte lozinku samo ako je uneta nova
                if (!string.IsNullOrEmpty(model.Lozinka))
                {
                    existingProfesor.Lozinka = model.Lozinka;
                }

                // Ažurirajte ostale podatke
                existingProfesor.Ime = model.Ime;
                existingProfesor.Prezime = model.Prezime;
                existingProfesor.Email = model.Email;
                existingProfesor.JMBG = model.JMBG;
                existingProfesor.ProfesorTitula = model.ProfesorTitula;
                existingProfesor.Uloga = model.Uloga;

                // Sačuvajte promene u bazi
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfesorExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // GET: Profesori/Delete/{id}
        [HttpGet("Delete/{id:long}")]
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

        private bool ProfesorExists(long id)
        {
            return _context.Profesori.Any(e => e.Id == id);
        }
    }
}
