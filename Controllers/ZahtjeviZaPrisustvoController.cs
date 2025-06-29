using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Authorize(Roles = "Profesor,Asistent")]
    [Route("ZahtjeviZaPrisustvo")]
    public class ZahtjeviZaPrisustvoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZahtjeviZaPrisustvoController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public ZahtjeviZaPrisustvoController(
            ApplicationDbContext context,
            ILogger<ZahtjeviZaPrisustvoController> logger,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var aspUserId = _userManager.GetUserId(User);
            long? korisnikId = null;

            if (User.IsInRole("Profesor"))
            {
                korisnikId = await _context.Profesori
                    .Where(p => p.AspNetUserId == aspUserId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();
            }
            else if (User.IsInRole("Asistent"))
            {
                korisnikId = await _context.Asistenti
                    .Where(a => a.AspNetUserId == aspUserId)
                    .Select(a => (long?)a.Id)
                    .FirstOrDefaultAsync();
            }

            if (korisnikId == null)
            {
                _logger.LogWarning("Nije moguće pronaći korisnika po AspNetUserId.");
                return Forbid();
            }

            var zahtjevi = await _context.ZahtjeviZaPrisustvo
                .Include(z => z.Student)
                .Include(z => z.NastavnaAktivnost).ThenInclude(na => na.Predmet)
                .Where(z =>
                    !z.Obradjen &&
                    ((User.IsInRole("Profesor") && z.NastavnaAktivnost.Predmet.ProfesorId == korisnikId) ||
                     (User.IsInRole("Asistent") && z.NastavnaAktivnost.Predmet.AsistentId == korisnikId)))
                .OrderByDescending(z => z.VrijemePodnosenja)
                .ToListAsync();

            _logger.LogInformation("Otvorena stranica zahtjeva za prisustvo. Ukupno: {Broj}", zahtjevi.Count);
            return View(zahtjevi);
        }

        [HttpPost("ObradiOznaceneZahtjeve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObradiOznaceneZahtjeve(List<long> oznaceniZahtjevi)
        {
            _logger.LogInformation("Primljeno oznacenih zahtjeva za obradu count: {Count}", oznaceniZahtjevi?.Count ?? 0);

            if (oznaceniZahtjevi == null || !oznaceniZahtjevi.Any())
            {
                TempData["Error"] = "Niste označili nijedan zahtjev za obradu.";
                return RedirectToAction(nameof(Index));
            }

            var aspUserId = _userManager.GetUserId(User);
            long? korisnikId = null;

            if (User.IsInRole("Profesor"))
            {
                korisnikId = await _context.Profesori
                    .Where(p => p.AspNetUserId == aspUserId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();
            }
            else if (User.IsInRole("Asistent"))
            {
                korisnikId = await _context.Asistenti
                    .Where(a => a.AspNetUserId == aspUserId)
                    .Select(a => (long?)a.Id)
                    .FirstOrDefaultAsync();
            }

            if (korisnikId == null)
                return Forbid();

            var zahtjevi = await _context.ZahtjeviZaPrisustvo
                .Include(z => z.NastavnaAktivnost).ThenInclude(na => na.Predmet)
                .Where(z => oznaceniZahtjevi.Contains(z.Id) &&
                            !z.Obradjen &&
                            ((User.IsInRole("Profesor") && z.NastavnaAktivnost.Predmet.ProfesorId == korisnikId) ||
                             (User.IsInRole("Asistent") && z.NastavnaAktivnost.Predmet.AsistentId == korisnikId)))
                .ToListAsync();

            int potvrdeni = 0, odbijeni = 0;

            foreach (var zahtjev in zahtjevi)
            {
                var key = $"napomena_{zahtjev.Id}";
                var napomena = Request.Form[key];

                var kodValidan = zahtjev.NastavnaAktivnost.KodAktivanDo > DateTime.Now;
                var vecPrisutan = await _context.PrisustvaNaAktivnostima
                    .AnyAsync(p => p.StudentId == zahtjev.StudentId && p.NastavnaAktivnostId == zahtjev.NastavnaAktivnostId);

                if (kodValidan && !vecPrisutan)
                {
                    _context.PrisustvaNaAktivnostima.Add(new PrisustvoNaAktivnosti
                    {
                        StudentId = zahtjev.StudentId,
                        NastavnaAktivnostId = zahtjev.NastavnaAktivnostId,
                        VrijemeEvidentiranja = DateTime.Now
                    });

                    _logger.LogInformation("Prisustvo potvrđeno za zahtjev ID {ZahtjevId}", zahtjev.Id);
                    potvrdeni++;
                }
                else
                {
                    zahtjev.Odbijen = true;
                    zahtjev.Napomena = napomena;
                    _logger.LogWarning("Zahtjev ID {ZahtjevId} odbijen. Napomena: {Napomena}", zahtjev.Id, napomena);
                    odbijeni++;
                }

                zahtjev.Obradjen = true;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Uspješno obrađeno: potvrđeno {potvrdeni}, odbijeno {odbijeni}";
            return RedirectToAction(nameof(Index));
        }
    }
}
