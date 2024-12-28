using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("Obavjestenja")]
    public class ObavjestenjaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ObavjestenjaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Obavjestenja
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Obavjestenja
                .Include(o => o.Asistent)
                .Include(o => o.Profesor)
                .Include(o => o.StudentskaSluzba);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Obavjestenja/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var obavjestenje = await _context.Obavjestenja
                .Include(o => o.Asistent)
                .Include(o => o.Profesor)
                .Include(o => o.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (obavjestenje == null) return NotFound();

            return View(obavjestenje);
        }

        // GET: Obavjestenja/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Obavjestenja/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Create([Bind("Naslov,Sadrzaj")] Obavjestenje obavjestenje)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine($"Naslov: {obavjestenje.Naslov}, Sadrzaj: {obavjestenje.Sadrzaj}");
                obavjestenje.datumObjave = DateTime.Now;

                // Pronalaženje korisnika u bazi prema User.Identity.Name
                if (User.Identity?.IsAuthenticated == true)
                {
                    if (long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out long userId))
                    {
                        var korisnik = await _context.Korisnici
                            .FirstOrDefaultAsync(k => k.Id == userId);

                        if (korisnik != null)
                        {
                            obavjestenje.KorisnikId = korisnik.Id; // Postavljanje povezanog entiteta
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
                SetUserRoleIds(obavjestenje);

                _context.Add(obavjestenje);
                await _context.SaveChangesAsync();
                Console.WriteLine("Obavještenje uspješno kreirano.");
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
            return View(obavjestenje);
        }

        // GET: Obavjestenja/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var obavjestenje = await _context.Obavjestenja.FindAsync(id);
            if (obavjestenje == null) return NotFound();

            return View(obavjestenje);
        }

        // POST: Obavjestenja/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naslov,Sadrzaj")] Obavjestenje obavjestenje)
        {
            if (id != obavjestenje.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    obavjestenje.datumObjave = DateTime.Now;
                    SetUserRoleIds(obavjestenje);

                    _context.Update(obavjestenje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ObavjestenjeExists(obavjestenje.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(obavjestenje);
        }

        // GET: Obavjestenja/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var obavjestenje = await _context.Obavjestenja
                .Include(o => o.Asistent)
                .Include(o => o.Profesor)
                .Include(o => o.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (obavjestenje == null) return NotFound();
            return View(obavjestenje);
        }

        // POST: Obavjestenja/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var obavjestenje = await _context.Obavjestenja.FindAsync(id);
            if (obavjestenje != null)
            {
                _context.Obavjestenja.Remove(obavjestenje);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ObavjestenjeExists(long id)
        {
            return _context.Obavjestenja.Any(e => e.Id == id);
        }

        // Pomoćna metoda za postavljanje korisničkih ID-ova i uloga
        private void SetUserRoleIds(Obavjestenje obavjestenje)
        {
            Console.WriteLine("Postavljanje korisnika za obavještenje...");
            if (User.IsInRole("StudentskaSluzba"))
            {
                obavjestenje.StudentskaSluzbaId = GetCurrentUserId();
            }
            else if (User.IsInRole("Profesor"))
            {
                obavjestenje.ProfesorId = GetCurrentUserId();
            }
            else if (User.IsInRole("Asistent"))
            {
                obavjestenje.AsistentId = GetCurrentUserId();
            }
        }

        // Metoda za dohvaćanje trenutnog korisničkog ID-a
        private long GetCurrentUserId()
        {
            return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}