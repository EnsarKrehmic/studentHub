using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class PodrskaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PodrskaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string status)
        {
            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            bool isSluzba = korisnik.Uloga == Uloga.StudentskaSluzba;
            IQueryable<PodrskaUpit> query = _context.PodrskaUpiti.Include(u => u.Korisnik);

            if (!isSluzba)
                query = query.Where(u => u.KorisnikId == korisnik.Id);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<UpitStatus>(status, out var parsedStatus))
                query = query.Where(u => u.Status == parsedStatus);

            var upiti = await query.OrderByDescending(u => u.DatumKreiranja).ToListAsync();
            return View(upiti);
        }

        // Prikaz detalja jednog upita
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var upit = await _context.PodrskaUpiti
                             .Include(u => u.Korisnik)
                             .FirstOrDefaultAsync(u => u.Id == id);
            if (upit == null) return NotFound();

            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var korisnik = await _context.Korisnici
                                      .FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);
            if (korisnik == null) return Forbid();

            var isAutor = upit.KorisnikId == korisnik.Id;
            var isSluzba = korisnik.Uloga == Uloga.StudentskaSluzba;
            // ako niste autor i niste služba, zabranjujemo
            if (!isAutor && !isSluzba)
                return Forbid();

            return View(upit);
        }

        // Kreiranje novog upita (samo studenti)
        [HttpGet]
        public IActionResult Create()
        {
            return View(new PodrskaUpit());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PodrskaUpit upit)
        {
            ModelState.Remove(nameof(upit.Korisnik));
            if (!ModelState.IsValid)
                return View(upit);

            var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var korisnik = await _context.Korisnici
                .FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            upit.KorisnikId = korisnik!.Id;
            upit.DatumKreiranja = DateTime.Now;
            upit.Status = UpitStatus.Podnesen;

            _context.Add(upit);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Uređivanje upita (student dok je status Podnesen)
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            var upit = await _context.PodrskaUpiti.FirstOrDefaultAsync(u => u.Id == id);
            if (upit == null)
                return NotFound();

            if (upit.KorisnikId != korisnik.Id || upit.Status != UpitStatus.Podnesen)
                return Forbid();

            return View(upit);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PodrskaUpit izmenjenUpit)
        {
            ModelState.Remove("Korisnik");
            if (ModelState.IsValid)
            {
                var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

                var upit = await _context.PodrskaUpiti.FirstOrDefaultAsync(u => u.Id == id);
                if (upit == null)
                    return NotFound();

                if (upit.KorisnikId != korisnik.Id || upit.Status != UpitStatus.Podnesen)
                    return Forbid();

                upit.Naslov = izmenjenUpit.Naslov;
                upit.Opis = izmenjenUpit.Opis;
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = upit.Id });
            }
            return View(izmenjenUpit);
        }

        // Služba postavlja status u "U obradi"
        [Authorize(Roles = "Studentska služba")]
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostaviUObradi(int id)
        {
            var upit = await _context.PodrskaUpiti.FindAsync(id);
            if (upit == null)
                return NotFound();

            if (upit.Status == UpitStatus.Podnesen)
            {
                upit.Status = UpitStatus.UObradi;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // Služba odgovara na upit i zatvara ga
        [Authorize(Roles = "Studentska služba")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Odgovori(int id)
        {
            var upit = await _context.PodrskaUpiti.Include(u => u.Korisnik).FirstOrDefaultAsync(u => u.Id == id);
            if (upit == null)
                return NotFound();

            // Dozvoljeno odgovarati samo dok nije zatvoren
            if (upit.Status == UpitStatus.Zatvoren)
                return RedirectToAction(nameof(Details), new { id });

            return View(upit);
        }

        [Authorize(Roles = "Studentska služba")]
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Odgovori(int id, string odgovor)
        {
            var upit = await _context.PodrskaUpiti.FindAsync(id);
            if (upit == null)
                return NotFound();

            upit.Odgovor = odgovor;
            upit.DatumOdgovora = DateTime.Now;
            upit.Status = UpitStatus.Zatvoren;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // Služba uređuje odgovor kad je status Zatvoren
        [Authorize(Roles = "Studentska služba")]
        [HttpGet("{id}")]
        public async Task<IActionResult> EditOdgovor(int id)
        {
            var upit = await _context.PodrskaUpiti.Include(u => u.Korisnik).FirstOrDefaultAsync(u => u.Id == id);
            if (upit == null || upit.Status != UpitStatus.Zatvoren)
                return NotFound();
            return View(upit);
        }

        [Authorize(Roles = "Studentska služba")]
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOdgovor(int id, string odgovor)
        {
            var upit = await _context.PodrskaUpiti.FindAsync(id);
            if (upit == null || upit.Status != UpitStatus.Zatvoren)
                return NotFound();

            upit.Odgovor = odgovor;
            upit.DatumOdgovora = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // DELETE akcija
        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var upit = await _context.PodrskaUpiti.Include(u => u.Korisnik).FirstOrDefaultAsync(u => u.Id == id);
            if (upit == null)
                return NotFound();
            return View(upit);
        }

        [HttpPost("{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var upit = await _context.PodrskaUpiti.FindAsync(id);
            if (upit == null)
                return NotFound();
            _context.PodrskaUpiti.Remove(upit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
