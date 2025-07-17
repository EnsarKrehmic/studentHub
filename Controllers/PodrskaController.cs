using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentHub.Data;
using StudentHub.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

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

        // Lista upita - student vidi svoje, služba sve
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            bool isSluzba = korisnik.Uloga == Uloga.StudentskaSluzba;
            var upiti = isSluzba
                ? await _context.PodrskaUpiti.Include(u => u.Korisnik).OrderByDescending(u => u.DatumKreiranja).ToListAsync()
                : await _context.PodrskaUpiti.Include(u => u.Korisnik)
                    .Where(u => u.KorisnikId == korisnik.Id)
                    .OrderByDescending(u => u.DatumKreiranja)
                    .ToListAsync();

            return View(upiti);
        }

        // Prikaz detalja jednog upita
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var upit = await _context.PodrskaUpiti.Include(u => u.Korisnik).FirstOrDefaultAsync(u => u.Id == id);
            if (upit == null)
                return NotFound();

            var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

            if (upit.KorisnikId != korisnik.Id && korisnik.Uloga != Uloga.StudentskaSluzba)
                return Forbid();

            return View(upit);
        }

        // Kreiranje novog upita (samo studenti)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PodrskaUpit upit)
        {
            ModelState.Remove("Korisnik");
            if (ModelState.IsValid)
            {
                var aspNetUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

                upit.KorisnikId = korisnik.Id;
                upit.DatumKreiranja = DateTime.Now;
                upit.Status = UpitStatus.Podnesen;

                _context.Add(upit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(upit);
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
        [Authorize(Roles = "Studentska služba")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var upit = await _context.PodrskaUpiti.Include(u => u.Korisnik).FirstOrDefaultAsync(u => u.Id == id);
            if (upit == null)
                return NotFound();
            return View(upit);
        }

        [Authorize(Roles = "Studentska služba")]
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
