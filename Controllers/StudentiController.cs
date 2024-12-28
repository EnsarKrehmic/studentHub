using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

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
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Studenti;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Studenti/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Studenti/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Korisnici, "Id", "Id");
            return View();
        }

        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JMBG,Ime,Prezime,Email,brojIndeksa,Lozinka,godinaStudija,studijskiProgram,predhodnoObrazovanje,podaciUplata")] Student student)
        {
            if (await _context.Korisnici.AnyAsync(k => k.JMBG == student.JMBG))
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(student);
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}, Error: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return View(student);
            }

            try
            {
                student.Uloga = Uloga.Student;
                _context.Studenti.Add(student);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Došlo je do greške: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom kreiranja studenta.");
                return View(student);
            }
        }

        // GET: Studenti/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Korisnici, "Id", "Id", student.Id);
            return View(student);
        }

        // POST: Studenti/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("JMBG,Ime,Prezime,Email,brojIndeksa,Lozinka,GodinaStudija,StudijskiProgram,PredhodnoObrazovanje,PodaciUplata")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            // Provjera da li postoji drugi korisnik sa istim JMBG osim trenutnog
            if (_context.Korisnici.Any(k => k.JMBG == student.JMBG && k.Id != id))
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(student);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Došlo je do greške: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom ažuriranja studenta.");
                }
            }
            ViewData["Id"] = new SelectList(_context.Korisnici, "Id", "Id", student.Id);
            return View(student);
        }

        // GET: Studenti/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Studenti/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student != null)
            {
                _context.Studenti.Remove(student);
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