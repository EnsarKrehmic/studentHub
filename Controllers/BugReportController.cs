using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("BugSuggestionReport")]
    [Authorize]
    public class BugReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BugReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BugSuggestionReport?tip=Bug&status=UObradi
        [HttpGet("")]
        public async Task<IActionResult> Index(string tip, string status)
        {
            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);
            bool isSluzba = korisnik?.Uloga == Uloga.StudentskaSluzba;

            IQueryable<BugReport> query = _context.BugReporti
                .Include(b => b.Korisnik);

            if (!isSluzba)
            {
                query = query.Where(b => b.KorisnikId == korisnik.Id);
            }

            if (!string.IsNullOrEmpty(tip) && Enum.TryParse<BugTip>(tip, out var parsedTip))
            {
                query = query.Where(b => b.Tip == parsedTip);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BugStatus>(status, out var parsedStatus))
            {
                query = query.Where(b => b.Status == parsedStatus);
            }

            var prijave = await query
                .OrderByDescending(b => b.DatumPrijave)
                .ToListAsync();

            return View(prijave);
        }

        // GET: BugSuggestionReport/Details/5
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var prijava = await _context.BugReporti
                .Include(b => b.Korisnik)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (prijava == null) return NotFound();

            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            if (prijava.KorisnikId != korisnik.Id && korisnik.Uloga != Uloga.StudentskaSluzba)
                return Forbid();

            return View(prijava);
        }

        // GET: BugSuggestionReport/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new BugReport());
        }

        // POST: BugSuggestionReport/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BugReport prijava, IFormFile? slika)
        {
            ModelState.Remove(nameof(prijava.Korisnik));
            if (!ModelState.IsValid) return View(prijava);

            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            prijava.KorisnikId = korisnik.Id;
            prijava.DatumPrijave = DateTime.Now;
            prijava.Status = BugStatus.Podnesen;

            if (slika != null && slika.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bugreport");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid() + Path.GetExtension(slika.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await slika.CopyToAsync(stream);
                prijava.Slika = fileName;
            }

            _context.BugReporti.Add(prijava);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: BugSuggestionReport/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            var prijava = await _context.BugReporti.FindAsync(id);
            if (prijava == null) return NotFound();
            if (prijava.KorisnikId != korisnik.Id || prijava.Status != BugStatus.Podnesen)
                return Forbid();

            return View(prijava);
        }

        // POST: BugSuggestionReport/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BugReport izmjena, IFormFile? slika)
        {
            ModelState.Remove(nameof(izmjena.Korisnik));
            if (!ModelState.IsValid) return View(izmjena);

            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            var prijava = await _context.BugReporti.FindAsync(id);
            if (prijava == null) return NotFound();
            if (prijava.KorisnikId != korisnik.Id || prijava.Status != BugStatus.Podnesen)
                return Forbid();

            prijava.Tip = izmjena.Tip;
            prijava.Naslov = izmjena.Naslov;
            prijava.Opis = izmjena.Opis;

            if (slika != null && slika.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bugreport");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid() + Path.GetExtension(slika.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await slika.CopyToAsync(stream);
                prijava.Slika = fileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = prijava.Id });
        }

        // POST: BugSuggestionReport/InProcess/5
        [HttpPost("InProcess/{id:int}")]
        [Authorize(Roles = "Studentska služba")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostaviUObradi(int id)
        {
            var prijava = await _context.BugReporti.FindAsync(id);
            if (prijava == null) return NotFound();

            if (prijava.Status == BugStatus.Podnesen)
            {
                prijava.Status = BugStatus.UObradi;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: BugSuggestionReport/Answer/5
        [HttpGet("Answer/{id:int}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Odgovori(int id)
        {
            var prijava = await _context.BugReporti
                .Include(b => b.Korisnik)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (prijava == null) return NotFound();
            if (prijava.Status == BugStatus.Zatvoren)
                return RedirectToAction(nameof(Details), new { id });
            return View(prijava);
        }

        // POST: BugSuggestionReport/Answer/5
        [HttpPost("Answer/{id:int}")]
        [Authorize(Roles = "Studentska služba")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Odgovori(int id, string odgovor)
        {
            var prijava = await _context.BugReporti.FindAsync(id);
            if (prijava == null) return NotFound();

            prijava.Odgovor = odgovor;
            prijava.DatumOdgovora = DateTime.Now;
            prijava.Status = BugStatus.Zatvoren;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: BugSuggestionReport/EditAnswer/5
        [HttpGet("EditAnswer/{id:int}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> EditOdgovor(int id)
        {
            var prijava = await _context.BugReporti
                .Include(b => b.Korisnik)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (prijava == null || prijava.Status != BugStatus.Zatvoren)
                return NotFound();
            return View(prijava);
        }

        // POST: BugSuggestionReport/EditAnswer/5
        [HttpPost("EditAnswer/{id:int}")]
        [Authorize(Roles = "Studentska služba")]
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

        // GET: BugSuggestionReport/Delete/5
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var prijava = await _context.BugReporti
                .Include(b => b.Korisnik)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (prijava == null) return NotFound();
            return View(prijava);
        }

        // POST: BugSuggestionReport/Delete/5
        [HttpPost("Delete/{id:int}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prijava = await _context.BugReporti.FindAsync(id);
            if (prijava != null)
            {
                _context.BugReporti.Remove(prijava);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
