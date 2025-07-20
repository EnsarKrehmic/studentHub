using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentHub.Data;
using StudentHub.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class BugReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BugReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTA svih prijava - student vidi svoje, služba vidi sve
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            bool isSluzba = korisnik.Uloga == Uloga.StudentskaSluzba;
            var prijave = isSluzba
                ? await _context.BugReporti.Include(b => b.Korisnik).OrderByDescending(b => b.DatumPrijave).ToListAsync()
                : await _context.BugReporti.Include(b => b.Korisnik)
                    .Where(b => b.KorisnikId == korisnik.Id)
                    .OrderByDescending(b => b.DatumPrijave)
                    .ToListAsync();

            return View(prijave);
        }

        // DETALJI pojedinačne prijave
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var prijava = await _context.BugReporti.Include(b => b.Korisnik).FirstOrDefaultAsync(b => b.Id == id);
            if (prijava == null)
                return NotFound();

            var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            if (prijava.KorisnikId != korisnik.Id && korisnik.Uloga != Uloga.StudentskaSluzba)
                return Forbid();

            return View(prijava);
        }

        // KREIRANJE nove prijave (bug/prijedlog)
        [HttpGet]
        public IActionResult Create()
        {
            return View(new BugReport());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BugReport prijava, IFormFile? slika)
        {
            ModelState.Remove("Korisnik");
            if (ModelState.IsValid)
            {
                var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

                prijava.KorisnikId = korisnik.Id;
                prijava.DatumPrijave = DateTime.Now;
                prijava.Status = BugStatus.Podnesen;

                // Upload slike
                if (slika != null && slika.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bugreport");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(slika.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await slika.CopyToAsync(stream);
                    }
                    prijava.Slika = fileName;
                }

                _context.Add(prijava);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(prijava);
        }

        // UREDI prijavu (student može dok je status Podnesen)
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            var prijava = await _context.BugReporti.FirstOrDefaultAsync(b => b.Id == id);
            if (prijava == null)
                return NotFound();

            if (prijava.KorisnikId != korisnik.Id || prijava.Status != BugStatus.Podnesen)
                return Forbid();

            return View(prijava);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BugReport izmjena, IFormFile? slika)
        {
            ModelState.Remove("Korisnik");
            if (ModelState.IsValid)
            {
                var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

                var prijava = await _context.BugReporti.FirstOrDefaultAsync(b => b.Id == id);
                if (prijava == null)
                    return NotFound();

                if (prijava.KorisnikId != korisnik.Id || prijava.Status != BugStatus.Podnesen)
                    return Forbid();

                prijava.Tip = izmjena.Tip;
                prijava.Naslov = izmjena.Naslov;
                prijava.Opis = izmjena.Opis;

                // Ako želiš, dozvoli upload nove slike (opciono)
                if (slika != null && slika.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bugreport");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(slika.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await slika.CopyToAsync(stream);
                    }
                    prijava.Slika = fileName;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = prijava.Id });
            }
            return View(izmjena);
        }

        // Služba POSTAVLJA status "U obradi"
        [Authorize(Roles = "Studentska služba")]
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostaviUObradi(int id)
        {
            var prijava = await _context.BugReporti.FindAsync(id);
            if (prijava == null)
                return NotFound();

            if (prijava.Status == BugStatus.Podnesen)
            {
                prijava.Status = BugStatus.UObradi;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // Služba ODGOVARA (zatvara prijavu)
        [Authorize(Roles = "Studentska služba")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Odgovori(int id)
        {
            var prijava = await _context.BugReporti.Include(b => b.Korisnik).FirstOrDefaultAsync(b => b.Id == id);
            if (prijava == null)
                return NotFound();
            if (prijava.Status == BugStatus.Zatvoren)
                return RedirectToAction(nameof(Details), new { id });
            return View(prijava);
        }

        [Authorize(Roles = "Studentska služba")]
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Odgovori(int id, string odgovor)
        {
            var prijava = await _context.BugReporti.FindAsync(id);
            if (prijava == null)
                return NotFound();

            prijava.Odgovor = odgovor;
            prijava.DatumOdgovora = DateTime.Now;
            prijava.Status = BugStatus.Zatvoren;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // Služba UREDI odgovor (kad je Zatvoreno)
        [Authorize(Roles = "Studentska služba")]
        [HttpGet("{id}")]
        public async Task<IActionResult> EditOdgovor(int id)
        {
            var prijava = await _context.BugReporti.Include(b => b.Korisnik).FirstOrDefaultAsync(b => b.Id == id);
            if (prijava == null || prijava.Status != BugStatus.Zatvoren)
                return NotFound();
            return View(prijava);
        }

        [Authorize(Roles = "Studentska služba")]
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOdgovor(int id, string odgovor)
        {
            var prijava = await _context.BugReporti.FindAsync(id);
            if (prijava == null || prijava.Status != BugStatus.Zatvoren)
                return NotFound();

            prijava.Odgovor = odgovor;
            prijava.DatumOdgovora = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // Opciono: Delete (samo služba)
        [Authorize(Roles = "Studentska služba")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var prijava = await _context.BugReporti.Include(b => b.Korisnik).FirstOrDefaultAsync(b => b.Id == id);
            if (prijava == null)
                return NotFound();
            return View(prijava);
        }

        [Authorize(Roles = "Studentska služba")]
        [HttpPost("{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prijava = await _context.BugReporti.FindAsync(id);
            if (prijava == null)
                return NotFound();
            _context.BugReporti.Remove(prijava);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
