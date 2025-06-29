using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Authorize]
    [Route("MojiPodaci")]
    public class MojiPodaciController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MojiPodaciController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public MojiPodaciController(ApplicationDbContext context, ILogger<MojiPodaciController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet("")]
        [Authorize(Roles = "Student,Profesor,Asistent,Studentska služba")]
        public async Task<IActionResult> Index()
        {
            var uloga = User.IsInRole("Student") ? "Student"
                     : User.IsInRole("Profesor") ? "Profesor"
                     : User.IsInRole("Asistent") ? "Asistent"
                     : User.IsInRole("Studentska služba") ? "Studentska služba"
                     : null;

            if (uloga == null)
            {
                _logger.LogWarning("Neautorizovan pokušaj pristupa stranici MojiPodaci.");
                return Forbid();
            }

            var aspNetUserId = _userManager.GetUserId(User); // string AspNetUserId

            if (string.IsNullOrEmpty(aspNetUserId))
            {
                _logger.LogWarning("Nije moguće pronaći identitet korisnika.");
                return Forbid();
            }

            long? korisnikId = null;

            if (uloga == "Student")
            {
                korisnikId = await _context.Studenti
                    .Where(s => s.AspNetUserId == aspNetUserId)
                    .Select(s => (long?)s.Id)
                    .FirstOrDefaultAsync();
            }
            else if (uloga == "Profesor")
            {
                korisnikId = await _context.Profesori
                    .Where(p => p.AspNetUserId == aspNetUserId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();
            }
            else if (uloga == "Asistent")
            {
                korisnikId = await _context.Asistenti
                    .Where(a => a.AspNetUserId == aspNetUserId)
                    .Select(a => (long?)a.Id)
                    .FirstOrDefaultAsync();
            }

            if ((uloga != "Studentska služba") && korisnikId == null)
            {
                _logger.LogWarning("Nije pronađen ID korisnika u bazi.");
                return Forbid();
            }

            var model = new MojiPodaciIndexViewModel
            {
                Uloga = uloga,
                KorisnikId = korisnikId ?? 0
            };

            if (uloga == "Studentska služba")
            {
                model.BrojNerijesenihZahtjeva = await _context.Zahtjevi
                    .CountAsync(z => z.StatusZahtjeva == StatusZahtjeva.Podnešen);
            }
            else if (uloga == "Profesor")
            {
                model.BrojZahtjevaZaPrisustvo = await _context.ZahtjeviZaPrisustvo
                    .Include(z => z.NastavnaAktivnost)
                    .ThenInclude(na => na.Predmet)
                    .Where(z => !z.Obradjen && z.NastavnaAktivnost.Predmet.ProfesorId == korisnikId)
                    .CountAsync();
            }
            else if (uloga == "Asistent")
            {
                model.BrojZahtjevaZaPrisustvo = await _context.ZahtjeviZaPrisustvo
                    .Include(z => z.NastavnaAktivnost)
                    .ThenInclude(na => na.Predmet)
                    .Where(z => !z.Obradjen && z.NastavnaAktivnost.Predmet.AsistentId == korisnikId)
                    .CountAsync();
            }

            return View(model);
        }

        [HttpGet("Raspored")]
        [Authorize(Roles = "Student,Profesor,Asistent")]
        public async Task<IActionResult> Raspored()
        {
            var uloga = User.IsInRole("Student") ? "Student"
                     : User.IsInRole("Profesor") ? "Profesor"
                     : User.IsInRole("Asistent") ? "Asistent"
                     : null;

            if (uloga == null)
            {
                _logger.LogWarning("Neautorizovan pristup rasporedu.");
                return Forbid();
            }

            var aspNetUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(aspNetUserId)) return Forbid();

            long? korisnikId = null;

            if (uloga == "Student")
            {
                korisnikId = await _context.Studenti
                    .Where(s => s.AspNetUserId == aspNetUserId)
                    .Select(s => (long?)s.Id)
                    .FirstOrDefaultAsync();
            }
            else if (uloga == "Profesor")
            {
                korisnikId = await _context.Profesori
                    .Where(p => p.AspNetUserId == aspNetUserId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();
            }
            else if (uloga == "Asistent")
            {
                korisnikId = await _context.Asistenti
                    .Where(a => a.AspNetUserId == aspNetUserId)
                    .Select(a => (long?)a.Id)
                    .FirstOrDefaultAsync();
            }

            if (korisnikId == null) return Forbid();

            List<TerminNastave> termini = new();

            if (uloga == "Student")
            {
                var student = await _context.Studenti
                    .Include(s => s.StudentStudijskiProgrami)
                    .FirstOrDefaultAsync(s => s.Id == korisnikId);

                if (student != null)
                {
                    var programIds = student.StudentStudijskiProgrami
                        .Select(sp => sp.StudijskiProgramId)
                        .ToList();

                    termini = await _context.TerminiNastave
                        .Include(t => t.Predmet)
                        .Include(t => t.Raspored)
                                .ThenInclude(r => r.StudijskiProgram)
                        .Where(t => t.Raspored != null && programIds.Contains(t.Raspored.StudijskiProgramId))
                        .ToListAsync();
                }
            }
            else if (uloga == "Profesor")
            {
                termini = await _context.TerminiNastave
                    .Include(t => t.Predmet)
                    .Include(t => t.Raspored)
                            .ThenInclude(r => r.StudijskiProgram)
                    .Where(t => t.Predmet.ProfesorId == korisnikId)
                    .ToListAsync();
            }
            else if (uloga == "Asistent")
            {
                termini = await _context.TerminiNastave
                    .Include(t => t.Predmet)
                    .Include(t => t.Raspored)
                            .ThenInclude(r => r.StudijskiProgram)
                    .Where(t => t.Predmet.AsistentId == korisnikId)
                    .ToListAsync();
            }

            var viewModel = new MojRasporedViewModel
            {
                Termini = termini.OrderBy(t => t.Dan).ThenBy(t => t.VrijemeOd).ToList(),
                Uloga = uloga
            };

            return View(viewModel);
        }

        [HttpGet("ZahtjeviZaPrisustvo")]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> ZahtjeviZaPrisustvo()
        {
            var aspNetUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(aspNetUserId)) return Forbid();

            long? korisnikId = null;

            if (User.IsInRole("Profesor"))
            {
                korisnikId = await _context.Profesori
                    .Where(p => p.AspNetUserId == aspNetUserId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();
            }
            else if (User.IsInRole("Asistent"))
            {
                korisnikId = await _context.Asistenti
                    .Where(a => a.AspNetUserId == aspNetUserId)
                    .Select(a => (long?)a.Id)
                    .FirstOrDefaultAsync();
            }

            if (korisnikId == null) return Forbid();

            var zahtjevi = await _context.ZahtjeviZaPrisustvo
                .Where(z => !z.Obradjen &&
                    ((User.IsInRole("Profesor") && z.NastavnaAktivnost.Predmet.ProfesorId == korisnikId) ||
                     (User.IsInRole("Asistent") && z.NastavnaAktivnost.Predmet.AsistentId == korisnikId)))
                .Include(z => z.Student)
                .Include(z => z.NastavnaAktivnost).ThenInclude(na => na.Predmet)
                .OrderByDescending(z => z.VrijemePodnosenja)
                .ToListAsync();

            return View(zahtjevi);
        }

        [HttpPost("ObradiOznaceneZahtjeve")]
        [Authorize(Roles = "Profesor,Asistent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObradiOznaceneZahtjeve(List<long> oznaceniZahtjevi)
        {
            if (oznaceniZahtjevi == null || !oznaceniZahtjevi.Any())
            {
                TempData["Poruka"] = "Niste označili nijedan zahtjev za obradu.";
                return RedirectToAction("ZahtjeviZaPrisustvo");
            }

            var aspNetUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(aspNetUserId)) return Forbid();

            long? korisnikId = null;

            if (User.IsInRole("Profesor"))
            {
                korisnikId = await _context.Profesori
                    .Where(p => p.AspNetUserId == aspNetUserId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();
            }
            else if (User.IsInRole("Asistent"))
            {
                korisnikId = await _context.Asistenti
                    .Where(a => a.AspNetUserId == aspNetUserId)
                    .Select(a => (long?)a.Id)
                    .FirstOrDefaultAsync();
            }

            if (korisnikId == null) return Forbid();

            var zahtjevi = await _context.ZahtjeviZaPrisustvo
                .Where(z => oznaceniZahtjevi.Contains(z.Id) &&
                            !z.Obradjen &&
                           ((User.IsInRole("Profesor") && z.NastavnaAktivnost.Predmet.ProfesorId == korisnikId) ||
                            (User.IsInRole("Asistent") && z.NastavnaAktivnost.Predmet.AsistentId == korisnikId)))
                .Include(z => z.NastavnaAktivnost)
                .ToListAsync();

            foreach (var zahtjev in zahtjevi)
            {
                var key = $"napomena_{zahtjev.Id}";
                var napomena = Request.Form[key];

                if (!string.IsNullOrWhiteSpace(napomena))
                {
                    zahtjev.Odbijen = true;
                    zahtjev.Napomena = napomena;
                }
                else
                {
                    var postojiPrisustvo = await _context.PrisustvaNaAktivnostima.AnyAsync(p =>
                        p.StudentId == zahtjev.StudentId &&
                        p.NastavnaAktivnostId == zahtjev.NastavnaAktivnostId);

                    if (!postojiPrisustvo)
                    {
                        _context.PrisustvaNaAktivnostima.Add(new PrisustvoNaAktivnosti
                        {
                            StudentId = zahtjev.StudentId,
                            NastavnaAktivnostId = zahtjev.NastavnaAktivnostId,
                            VrijemeEvidentiranja = DateTime.Now
                        });
                    }

                    zahtjev.Odbijen = false;
                    zahtjev.Napomena = null;
                }

                zahtjev.Obradjen = true;
            }

            await _context.SaveChangesAsync();
            TempData["Poruka"] = $"Obrađeno zahtjeva: {zahtjevi.Count}";

            return RedirectToAction("ZahtjeviZaPrisustvo");
        }
    }
}
