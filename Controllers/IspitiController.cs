using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("Ispiti")]
    public class IspitiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IspitiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ispiti
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ispiti
                .Include(i => i.Asistent)
                .Include(i => i.Predmet)
                .Include(i => i.Profesor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ispiti/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ispit = await _context.Ispiti
                .Include(i => i.Asistent)
                .Include(i => i.Predmet)
                .Include(i => i.Profesor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ispit == null) return NotFound();

            return View(ispit);
        }

        // GET: Ispiti/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv");
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime");
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Ime");
            return View();
        }

        // POST: Ispiti/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("datumOdrzavanja,Lokacija,brojBodova,PredmetId,ProfesorId,AsistentId")] Ispit ispit)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine($"Predmet: {ispit.PredmetId}, Datum održavanja: {ispit.datumOdrzavanja}, Lokacija: {ispit.Lokacija}, Broj bodova: {ispit.brojBodova}");
                ispit.datumObjave = DateTime.Now;

                // Pronalaženje korisnika u bazi prema User.Identity.Name
                if (User.Identity?.IsAuthenticated == true)
                {
                    if (long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out long userId))
                    {
                        var korisnik = await _context.Korisnici
                            .FirstOrDefaultAsync(k => k.Id == userId);

                        if (korisnik != null)
                        {
                            ispit.KorisnikId = korisnik.Id; // Postavljanje povezanog entiteta
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Autentifikovani korisnik '{User.Identity.Name}' nije pronađen u bazi.");
                    }
                }
                else
                {
                    Console.WriteLine("Korisnik nije autentifikovan. Obavještenje će biti kreirano bez korisnika.");
                }

                // Postavljanje dodatnih vrijednosti
                SetUserRoleIds(ispit);

                _context.Add(ispit);
                await _context.SaveChangesAsync();
                Console.WriteLine("Ispit uspješno kreiran.");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("ModelState nije validan. Greške:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ispit.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", ispit.ProfesorId);
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Ime", ispit.AsistentId);
            return View(ispit);
        }

        // GET: Ispiti/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit == null) return NotFound();
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ispit.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", ispit.ProfesorId);
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Ime", ispit.AsistentId);

            return View(ispit);
        }

        // POST: Ispiti/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,datumOdrzavanja,Lokacija,brojBodova,PredmetId,ProfesorId,AsistentId")] Ispit ispit)
        {
            if (id != ispit.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    ispit.datumObjave = DateTime.Now;
                    SetUserRoleIds(ispit);

                    _context.Update(ispit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IspitExists(ispit.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ispit.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", ispit.ProfesorId);
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Ime", ispit.AsistentId);
            return View(ispit);
        }

        // GET: Ispiti/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var ispit = await _context.Ispiti
                .Include(i => i.Asistent)
                .Include(i => i.Predmet)
                .Include(i => i.Profesor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ispit == null) return NotFound();
            return View(ispit);
        }

        // POST: Ispiti/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit != null)
            {
                _context.Ispiti.Remove(ispit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IspitExists(long id)
        {
            return _context.Ispiti.Any(e => e.Id == id);
        }

        // Pomoćna metoda za postavljanje korisničkih ID-ova i uloga
        private void SetUserRoleIds(Ispit ispit)
        {
            Console.WriteLine("Postavljanje korisnika za ispit...");
            if (User.IsInRole("Profesor"))
            {
                ispit.ProfesorId = GetCurrentUserId();
            }
            else if (User.IsInRole("Asistent"))
            {
                ispit.AsistentId = GetCurrentUserId();
            }
        }

        // Metoda za dohvaćanje trenutnog korisničkog ID-a
        private long GetCurrentUserId()
        {
            return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
