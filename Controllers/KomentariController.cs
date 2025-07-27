using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Authorize]
    public class KomentariController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public KomentariController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Komentari/Index?nastavnaAktivnostId=5
        public async Task<IActionResult> Index(long nastavnaAktivnostId)
        {
            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(n => n.Id == nastavnaAktivnostId);
            if (nastavnaAktivnost == null) return NotFound();

            // Provjera dostupnosti za studente
            if (!nastavnaAktivnost.JeDostupno
                && !User.IsInRole("Profesor")
                && !User.IsInRole("Asistent"))
            {
                TempData["Error"] = "Nastavna aktivnost nije dostupna.";
                return RedirectToAction("Index", "NastavneAktivnosti", new { predmetId = nastavnaAktivnost.PredmetId });
            }

            var komentari = _context.Komentari
                .Where(k => k.NastavnaAktivnostId == nastavnaAktivnostId)
                .Include(k => k.Student)
                .Include(k => k.Korisnik)
                .Include(k => k.NastavnaAktivnost);

            ViewBag.NastavnaAktivnostId = nastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = nastavnaAktivnost.Naziv;
            return View(await komentari.ToListAsync());
        }

        // GET: Komentari/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var komentar = await _context.Komentari
                .Include(k => k.NastavnaAktivnost).ThenInclude(n => n.Predmet)
                .Include(k => k.Ispit).ThenInclude(i => i.Predmet)
                .Include(k => k.Student)
                .Include(k => k.Korisnik)
                .FirstOrDefaultAsync(k => k.Id == id);
            if (komentar == null) return NotFound();

            if (!komentar.NastavnaAktivnost.JeDostupno
                && !User.IsInRole("Profesor")
                && !User.IsInRole("Asistent"))
            {
                TempData["Error"] = "Nastavna aktivnost nije dostupna.";
                return RedirectToAction("Index",
                    "NastavneAktivnosti",
                    new { predmetId = komentar.NastavnaAktivnost.PredmetId });
            }

            return View(komentar);
        }

        // GET: Komentari/Create?nastavnaAktivnostId=5
        [Authorize(Roles = "Student")]
        public IActionResult Create(long nastavnaAktivnostId)
        {
            var nastavnaAktivnost = _context.NastavneAktivnosti.Find(nastavnaAktivnostId);
            if (nastavnaAktivnost == null) return NotFound();

            if (!nastavnaAktivnost.JeDostupno)
            {
                TempData["Error"] = "Nastavna aktivnost nije dostupna za komentiranje.";
                return RedirectToAction("Details", "NastavneAktivnosti", new { id = nastavnaAktivnostId });
            }

            ViewBag.NastavnaAktivnostId = nastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = nastavnaAktivnost.Naziv;
            return View(new Komentar { NastavnaAktivnostId = nastavnaAktivnostId });
        }

        // POST: Komentari/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create([Bind("Sadrzaj,NastavnaAktivnostId")] Komentar komentar)
        {
            var nastavnaAktivnost = await _context.NastavneAktivnosti.FindAsync(komentar.NastavnaAktivnostId);
            if (nastavnaAktivnost == null || !nastavnaAktivnost.JeDostupno)
            {
                TempData["Error"] = "Nastavna aktivnost nije dostupna.";
                return RedirectToAction("Index", "NastavneAktivnosti", new { predmetId = nastavnaAktivnost?.PredmetId });
            }

            if (ModelState.IsValid)
            {
                // Postavljanje StudentId na osnovu trenutno prijavljenog korisnika
                var currentUser = await _userManager.GetUserAsync(User);
                var student = await _context.Studenti
                    .FirstOrDefaultAsync(s => s.AspNetUserId == currentUser.Id);
                if (student == null)
                {
                    TempData["Error"] = "Niste registrovani kao student.";
                    return RedirectToAction("Details", "NastavneAktivnosti", new { id = komentar.NastavnaAktivnostId });
                }

                komentar.StudentId = student.Id;
                komentar.DatumVrijeme = DateTime.Now;

                _context.Add(komentar);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "NastavneAktivnosti", new { id = komentar.NastavnaAktivnostId });
            }

            ViewBag.NastavnaAktivnostId = komentar.NastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = nastavnaAktivnost.Naziv;
            return View(komentar);
        }

        // GET: Komentari/Edit/5
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var komentar = await _context.Komentari
                .Include(k => k.NastavnaAktivnost)
                .FirstOrDefaultAsync(k => k.Id == id);
            if (komentar == null) return NotFound();

            ViewBag.NastavnaAktivnostId = komentar.NastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = komentar.NastavnaAktivnost.Naziv;
            return View(komentar);
        }

        // POST: Komentari/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Sadrzaj,NastavnaAktivnostId,StudentId,DatumVrijeme")] Komentar komentar)
        {
            if (id != komentar.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(komentar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KomentarExists(komentar.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Details", "NastavneAktivnosti", new { id = komentar.NastavnaAktivnostId });
            }

            ViewBag.NastavnaAktivnostId = komentar.NastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = _context.NastavneAktivnosti.Find(komentar.NastavnaAktivnostId)?.Naziv;
            return View(komentar);
        }

        // GET: Komentari/Delete/5
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var komentar = await _context.Komentari
                .Include(k => k.NastavnaAktivnost)
                .Include(k => k.Student)
                .FirstOrDefaultAsync(k => k.Id == id);
            if (komentar == null) return NotFound();

            return View(komentar);
        }

        // POST: Komentari/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var komentar = await _context.Komentari.FindAsync(id);
            if (komentar == null) return NotFound();

            _context.Komentari.Remove(komentar);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "NastavneAktivnosti", new { id = komentar.NastavnaAktivnostId });
        }

        private bool KomentarExists(long id)
        {
            return _context.Komentari.Any(e => e.Id == id);
        }
    }
}