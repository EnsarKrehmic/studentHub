using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Route("Asistenti")]
    public class AsistentiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsistentiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Asistenti
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Asistenti;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Asistenti/Details/{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistent = await _context.Asistenti.FirstOrDefaultAsync(m => m.Id == id);
            if (asistent == null)
            {
                return NotFound();
            }

            return View(asistent);
        }

        // GET: Asistenti/Create{id}
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View();
        }

        // POST: Asistenti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JMBG,Ime,Prezime,Email,Lozinka,AsistentTitula,Uloga")] Asistent asistent)
        {
            // Provjeri da li već postoji korisnik sa datim JMBG
            var postojiKorisnik = await _context.Korisnici
                .AnyAsync(k => k.JMBG == asistent.JMBG);

            if (postojiKorisnik)
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(asistent);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Asistenti.Add(asistent);
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
            return View(asistent);
        }

        // GET: Asistenti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var asistent = await _context.Asistenti.FindAsync(id);
            if (asistent == null)
            {
                return NotFound();
            }

            var model = new AsistentEditViewModel
            {
                Id = asistent.Id,
                JMBG = asistent.JMBG,
                Ime = asistent.Ime,
                Prezime = asistent.Prezime,
                Email = asistent.Email,
                AsistentTitula = asistent.AsistentTitula,
                Lozinka = null,
                Uloga = asistent.Uloga
            };

            ViewBag.Uloge = Enum.GetValues(typeof(Uloga))
                .Cast<Uloga>()
                .Select(u => new SelectListItem
                {
                    Value = ((int)u).ToString(),
                    Text = u.ToString(),
                    Selected = u == asistent.Uloga
                });

            return View(model);
        }

        // POST: Asistenti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AsistentEditViewModel model)
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
                var existingAsistent = await _context.Asistenti.FindAsync(id);
                if (existingAsistent == null)
                {
                    return NotFound();
                }

                // Ažuriranje lozinke samo ako je uneta nova
                if (!string.IsNullOrEmpty(model.Lozinka))
                {
                    existingAsistent.Lozinka = model.Lozinka;
                }

                // Ažuriranje ostalih podataka
                existingAsistent.Ime = model.Ime;
                existingAsistent.Prezime = model.Prezime;
                existingAsistent.Email = model.Email;
                existingAsistent.JMBG = model.JMBG;
                existingAsistent.AsistentTitula = model.AsistentTitula;
                existingAsistent.Uloga = model.Uloga;

                // Sačuvajte promene u bazi
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AsistentExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // GET: Asistenti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistent = await _context.Asistenti.FirstOrDefaultAsync(m => m.Id == id);
            if (asistent == null)
            {
                return NotFound();
            }

            return View(asistent);
        }

        // POST: Asistenti/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var asistent = await _context.Asistenti.FindAsync(id);
            if (asistent != null)
            {
                _context.Asistenti.Remove(asistent);
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsistentExists(long id)
        {
            return _context.Asistenti.Any(e => e.Id == id);
        }
    }
}