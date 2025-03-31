using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using StudentHub.Hubs;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Authorize(Roles = "Student, Studentska služba, Profesor")]
    [Route("Ocjene")]
    public class OcjeneController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public OcjeneController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Ocjene
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<Ocjena> ocjeneQuery = _context.Ocjene
                .Include(o => o.Predmet)
                .ThenInclude(p => p.NastavniPlan)
                .ThenInclude(np => np.StudijskiProgram)
                .Include(o => o.Student)
                .ThenInclude(s => s.StudentStudijskiProgrami)
                .Include(o => o.Profesor)
                .Include(o => o.NastavnaAktivnost);

            List<OcjenaViewModel> ocjeneViewModel;

            if (User.IsInRole("Student"))
            {
                ocjeneQuery = ocjeneQuery.Where(o => o.Student.AspNetUserId == userId);
                var ocjene = await ocjeneQuery.ToListAsync();
                double prosjekOcjena = ocjene.Any() ? ocjene.Average(o => o.Vrijednost) : 0;

                ocjeneViewModel = ocjene.Select(o => new OcjenaViewModel
                {
                    Id = o.Id,
                    PredmetId = o.PredmetId ?? 0,
                    PredmetNaziv = o.Predmet?.Naziv,
                    NastavnaAktivnostNaziv = o.NastavnaAktivnost?.Naziv,
                    Tip = o.Tip.ToString(),
                    StudentIme = o.Student.Ime,
                    StudentPrezime = o.Student.Prezime,
                    ProfesorIme = o.Profesor?.Ime,
                    ProfesorPrezime = o.Profesor?.Prezime,
                    ProfesorTitula = o.Profesor?.ProfesorTitula,
                    Vrijednost = o.Vrijednost,
                    ProsjekOcjena = prosjekOcjena,
                    StudentStudijskiProgramNaziv = o.Predmet?.NastavniPlan?.StudijskiProgram?.Naziv ??
                        o.Student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram.Naziv ?? "Nepoznato",
                }).ToList();
            }
            else if (User.IsInRole("Profesor"))
            {
                ocjeneQuery = ocjeneQuery.Where(o => o.Profesor.AspNetUserId == userId && o.Tip == TipOcjene.Predmet);
                var ocjene = await ocjeneQuery.ToListAsync();

                var prosjekPoPredmetu = ocjene
                    .GroupBy(o => o.PredmetId)
                    .Select(g => new { PredmetId = g.Key, Prosjek = g.Average(o => o.Vrijednost) })
                    .ToDictionary(x => x.PredmetId, x => x.Prosjek);

                ocjeneViewModel = ocjene.Select(o => new OcjenaViewModel
                {
                    Id = o.Id,
                    PredmetId = o.PredmetId ?? 0,
                    PredmetNaziv = o.Predmet?.Naziv ?? "Nepoznat predmet",
                    Tip = o.Tip.ToString(),
                    StudentIme = o.Student?.Ime ?? "Nepoznato",
                    StudentPrezime = o.Student?.Prezime ?? "Nepoznato",
                    StudentBrojIndeksa = o.Student?.BrojIndeksa ?? "Nepoznat",
                    ProfesorIme = o.Profesor?.Ime ?? "Nepoznato",
                    ProfesorPrezime = o.Profesor?.Prezime ?? "Nepoznato",
                    ProfesorTitula = o.Profesor?.ProfesorTitula ?? "Nepoznata titula",
                    Vrijednost = o.Vrijednost,
                    ProsjekPoPredmetu = prosjekPoPredmetu.ContainsKey(o.PredmetId ?? 0) ? prosjekPoPredmetu[o.PredmetId ?? 0] : 0,
                    StudentStudijskiProgramNaziv = o.Predmet?.NastavniPlan?.StudijskiProgram?.Naziv ??
                        o.Student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram.Naziv ?? "Nepoznato",
                }).ToList();
            }
            else if (User.IsInRole("Studentska služba"))
            {
                var ocjene = await ocjeneQuery.ToListAsync();

                var prosjekPoPredmetu = ocjene.Where(o => o.Tip == TipOcjene.Predmet)
                    .GroupBy(o => o.Predmet?.Naziv)
                    .Select(g => new { Predmet = g.Key, Prosjek = g.Average(o => o.Vrijednost) })
                    .ToDictionary(x => x.Predmet, x => x.Prosjek);

                var prosjekPoStudijskomProgramu = ocjene
                    .Where(o => o.Tip == TipOcjene.Predmet && o.Student != null &&
                                o.Student.StudentStudijskiProgrami != null &&
                                o.Student.StudentStudijskiProgrami.Any() &&
                                o.Student.StudentStudijskiProgrami.First().StudijskiProgram != null)
                    .GroupBy(o => o.Student.StudentStudijskiProgrami.First().StudijskiProgram.Naziv)
                    .Select(g => new { StudijskiProgram = g.Key, Prosjek = g.Average(o => o.Vrijednost) })
                    .ToDictionary(x => x.StudijskiProgram, x => x.Prosjek);

                ocjeneViewModel = ocjene.Select(o => new OcjenaViewModel
                {
                    Id = o.Id,
                    PredmetId = o.PredmetId ?? 0,
                    PredmetNaziv = o.Predmet?.Naziv,
                    NastavnaAktivnostNaziv = o.NastavnaAktivnost?.Naziv,
                    Tip = o.Tip.ToString(),
                    StudentIme = o.Student?.Ime ?? "Nepoznato",
                    StudentPrezime = o.Student?.Prezime ?? "Nepoznato",
                    StudentBrojIndeksa = o.Student?.BrojIndeksa ?? "Nepoznat",
                    ProfesorIme = o.Profesor?.Ime ?? "Nepoznato",
                    ProfesorPrezime = o.Profesor?.Prezime ?? "Nepoznato",
                    ProfesorTitula = o.Profesor?.ProfesorTitula ?? "Nepoznata titula",
                    Vrijednost = o.Vrijednost,

                    ProsjekPoPredmetu = (o.Predmet != null && prosjekPoPredmetu.ContainsKey(o.Predmet.Naziv)) 
                    ? prosjekPoPredmetu[o.Predmet.Naziv] : 0,

                    ProsjekPoStudijskomProgramu = (o.Predmet != null &&
                        o.Predmet.NastavniPlan != null &&
                        o.Predmet.NastavniPlan.StudijskiProgram != null &&
                        prosjekPoStudijskomProgramu.ContainsKey(o.Predmet.NastavniPlan.StudijskiProgram.Naziv))
                        ? prosjekPoStudijskomProgramu[o.Predmet.NastavniPlan.StudijskiProgram.Naziv]
                        : 0,

                    ProsjekOcjena = (
                        ((o.Predmet != null && prosjekPoPredmetu.ContainsKey(o.Predmet.Naziv)) ? prosjekPoPredmetu[o.Predmet.Naziv] : 0) +
                        ((o.Predmet != null &&
                        o.Predmet.NastavniPlan != null &&
                        o.Predmet.NastavniPlan.StudijskiProgram != null &&
                        prosjekPoStudijskomProgramu.ContainsKey(o.Predmet.NastavniPlan.StudijskiProgram.Naziv))
                        ? prosjekPoStudijskomProgramu[o.Predmet.NastavniPlan.StudijskiProgram.Naziv]
                        : 0)
                    ) / 2,

                    ProsjekPrikaz = $"Predmet '{(o.Predmet != null ? o.Predmet.Naziv : "Nepoznat predmet")}': " +
                        $"{((o.Predmet != null && prosjekPoPredmetu.ContainsKey(o.Predmet.Naziv))
                        ? prosjekPoPredmetu[o.Predmet.Naziv] : 0):0.00}, " +
                        ((o.Predmet != null &&
                        o.Predmet.NastavniPlan != null &&
                        o.Predmet.NastavniPlan.StudijskiProgram != null)
                            ? $"Studijski program '{o.Predmet.NastavniPlan.StudijskiProgram.Naziv}': " +
                                $"{(prosjekPoStudijskomProgramu.ContainsKey(o.Predmet.NastavniPlan.StudijskiProgram.Naziv)
                                ? prosjekPoStudijskomProgramu[o.Predmet.NastavniPlan.StudijskiProgram.Naziv] : 0):0.00}, "
                            : "") +
                        $"Ukupno: {(((o.Predmet != null && prosjekPoPredmetu.ContainsKey(o.Predmet.Naziv))
                        ? prosjekPoPredmetu[o.Predmet.Naziv] : 0) +
                        ((o.Predmet != null &&
                        o.Predmet.NastavniPlan != null &&
                        o.Predmet.NastavniPlan.StudijskiProgram != null &&
                        prosjekPoStudijskomProgramu.ContainsKey(o.Predmet.NastavniPlan.StudijskiProgram.Naziv))
                        ? prosjekPoStudijskomProgramu[o.Predmet.NastavniPlan.StudijskiProgram.Naziv] : 0)) / 2:0.00}",

                    StudentStudijskiProgramNaziv = (o.Predmet != null &&
                        o.Predmet.NastavniPlan != null &&
                        o.Predmet.NastavniPlan.StudijskiProgram != null)
                        ? o.Predmet.NastavniPlan.StudijskiProgram.Naziv
                        : o.Student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram.Naziv ?? "Nepoznato",

                }).ToList();
            }
            else
            {
                return Forbid();
            }

            return View(ocjeneViewModel);
        }

        // GET: Ocjene/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjene
                .Include(o => o.Predmet)
                    .ThenInclude(p => p.NastavniPlan)
                        .ThenInclude(np => np.StudijskiProgram)
                .Include(o => o.Student)
                .Include(o => o.Profesor)
                .Include(o => o.NastavnaAktivnost)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ocjena == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Student") && ocjena.Student.AspNetUserId != userId)
            {
                return Forbid();
            }
            else if (User.IsInRole("Profesor") && ocjena.Profesor.AspNetUserId != userId)
            {
                return Forbid();
            }

            var ocjenaViewModel = new OcjenaViewModel
            {
                Id = ocjena.Id,
                PredmetId = ocjena.PredmetId ?? 0,
                PredmetNaziv = ocjena.Predmet?.Naziv,
                NastavnaAktivnostNaziv = ocjena.NastavnaAktivnost?.Naziv,
                Tip = ocjena.Tip.ToString(),
                StudentIme = ocjena.Student?.Ime,
                StudentPrezime = ocjena.Student?.Prezime,
                StudentBrojIndeksa = ocjena.Student?.BrojIndeksa,
                ProfesorIme = ocjena.Profesor?.Ime,
                ProfesorPrezime = ocjena.Profesor?.Prezime,
                ProfesorTitula = ocjena.Profesor?.ProfesorTitula,
                Vrijednost = ocjena.Vrijednost,
                StudentStudijskiProgramNaziv = ocjena.Predmet?.NastavniPlan?.StudijskiProgram?.Naziv ??
                    ocjena.Student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram.Naziv ?? "Nepoznato"
            };

            return View(ocjenaViewModel);
        }

        // GET: Ocjene/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Profesor,Student")]
        public IActionResult Create(string tip, long? predmetId, long? nastavnaAktivnostId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (tip == "Predmet" && User.IsInRole("Profesor"))
            {
                ViewBag.StudentId = new SelectList(_context.Studenti.Select(s => new
                {
                    s.Id,
                    FullName = s.Ime + " " + s.Prezime
                }), "Id", "FullName");

                ViewBag.PredmetId = new SelectList(_context.Predmeti
                    .Where(p => p.Profesor.AspNetUserId == userId || p.PredmetProfesori.Any(pp => pp.Profesor.AspNetUserId == userId))
                    .Select(p => new { p.Id, p.Naziv }), "Id", "Naziv", predmetId);

                ViewBag.ProfesorId = new SelectList(_context.Profesori.Select(p => new
                {
                    p.Id,
                    FullName = p.ProfesorTitula + " " + p.Ime + " " + p.Prezime
                }), "Id", "FullName");

                return View("CreatePredmetOcjena");
            }
            else if (tip == "NastavnaAktivnost" && User.IsInRole("Student"))
            {
                var aktivnost = _context.NastavneAktivnosti.Find(nastavnaAktivnostId);
                if (aktivnost == null)
                {
                    TempData["Error"] = "Nastavna aktivnost nije pronađena.";
                    return RedirectToAction("Index", "NastavneAktivnosti");
                }
                ViewBag.NastavnaAktivnostId = nastavnaAktivnostId;
                ViewBag.NastavnaAktivnostNaziv = aktivnost.Naziv;
                return View("CreateNastavnaAktivnostOcjena");
            }
            return Forbid();
        }

        // POST: Ocjene/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Student")]
        public async Task<IActionResult> Create([Bind("Tip,Vrijednost,PredmetId,StudentId,ProfesorId,NastavnaAktivnostId")] Ocjena ocjena)
        {
            if (!ocjena.IsValid())
            {
                ModelState.AddModelError("Vrijednost", "Ocjena nije u dozvoljenom rasponu.");
            }

            if (ModelState.IsValid)
            {
                if (ocjena.Tip == TipOcjene.NastavnaAktivnost)
                {
                    var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == korisnikId);
                    ocjena.StudentId = student.Id;
                    ocjena.ProfesorId = null; // Nije potreban za nastavne aktivnosti
                }

                _context.Add(ocjena);
                await _context.SaveChangesAsync();
                if (ocjena.PredmetId.HasValue)
                    await UpdateAverageGrade(ocjena.PredmetId.Value);
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ocjena.Tip == TipOcjene.Predmet)
            {
                ViewBag.StudentId = new SelectList(_context.Studenti.Select(s => new
                {
                    s.Id,
                    FullName = s.Ime + " " + s.Prezime
                }), "Id", "FullName", ocjena.StudentId);

                ViewBag.PredmetId = new SelectList(_context.Predmeti
                    .Where(p => p.Profesor.AspNetUserId == userId || p.PredmetProfesori.Any(pp => pp.Profesor.AspNetUserId == userId))
                    .Select(p => new { p.Id, p.Naziv }), "Id", "Naziv", ocjena.PredmetId);

                ViewBag.ProfesorId = new SelectList(_context.Profesori.Select(p => new
                {
                    p.Id,
                    FullName = p.ProfesorTitula + " " + p.Ime + " " + p.Prezime
                }), "Id", "FullName", ocjena.ProfesorId);

                return View("CreatePredmetOcjena", ocjena);
            }
            else if (ocjena.Tip == TipOcjene.NastavnaAktivnost)
            {
                ViewBag.NastavnaAktivnostId = ocjena.NastavnaAktivnostId;
                ViewBag.NastavnaAktivnostNaziv = _context.NastavneAktivnosti.Find(ocjena.NastavnaAktivnostId)?.Naziv;
                return View("CreateNastavnaAktivnostOcjena", ocjena);
            }

            return View(ocjena);
        }

        // GET: Ocjene/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjene.FindAsync(id);
            if (ocjena == null || ocjena.Tip != TipOcjene.Predmet)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewBag.StudentId = new SelectList(_context.Studenti.Select(s => new
            {
                s.Id,
                FullName = s.Ime + " " + s.Prezime
            }), "Id", "FullName", ocjena.StudentId);

            ViewBag.PredmetId = new SelectList(_context.Predmeti
                .Where(p => p.Profesor.AspNetUserId == userId || p.PredmetProfesori.Any(pp => pp.Profesor.AspNetUserId == userId))
                .Select(p => new { p.Id, p.Naziv }), "Id", "Naziv", ocjena.PredmetId);

            ViewBag.ProfesorId = new SelectList(_context.Profesori.Select(p => new
            {
                p.Id,
                FullName = p.ProfesorTitula + " " + p.Ime + " " + p.Prezime
            }), "Id", "FullName", ocjena.ProfesorId);

            return View(ocjena);
        }

        // POST: Ocjene/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Tip,Vrijednost,PredmetId,StudentId,ProfesorId,NastavnaAktivnostId")] Ocjena ocjena)
        {
            if (id != ocjena.Id || ocjena.Tip != TipOcjene.Predmet)
            {
                return NotFound();
            }

            if (!ocjena.IsValid())
            {
                ModelState.AddModelError("Vrijednost", "Ocjena nije u dozvoljenom rasponu.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ocjena);
                    await _context.SaveChangesAsync();
                    if (ocjena.PredmetId.HasValue)
                        await UpdateAverageGrade(ocjena.PredmetId.Value);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OcjenaExists(ocjena.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewBag.StudentId = new SelectList(_context.Studenti.Select(s => new
            {
                s.Id,
                FullName = s.Ime + " " + s.Prezime
            }), "Id", "FullName", ocjena.StudentId);

            ViewBag.PredmetId = new SelectList(_context.Predmeti
                .Where(p => p.Profesor.AspNetUserId == userId || p.PredmetProfesori.Any(pp => pp.Profesor.AspNetUserId == userId))
                .Select(p => new { p.Id, p.Naziv }), "Id", "Naziv", ocjena.PredmetId);

            ViewBag.ProfesorId = new SelectList(_context.Profesori.Select(p => new
            {
                p.Id,
                FullName = p.ProfesorTitula + " " + p.Ime + " " + p.Prezime
            }), "Id", "FullName", ocjena.ProfesorId);

            return View(ocjena);
        }

        // GET: Ocjene/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Student)
                .Include(o => o.Profesor)
                .Include(o => o.NastavnaAktivnost)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ocjena == null || ocjena.Tip != TipOcjene.Predmet)
            {
                return NotFound();
            }

            return View(ocjena);
        }

        // POST: Ocjene/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ocjena = await _context.Ocjene.FindAsync(id);
            if (ocjena == null || ocjena.Tip != TipOcjene.Predmet)
            {
                return NotFound();
            }

            _context.Ocjene.Remove(ocjena);
            await _context.SaveChangesAsync();
            if (ocjena.PredmetId.HasValue)
                await UpdateAverageGrade(ocjena.PredmetId.Value);
            return RedirectToAction(nameof(Index));
        }

        private bool OcjenaExists(long id)
        {
            return _context.Ocjene.Any(e => e.Id == id);
        }

        private async Task UpdateAverageGrade(long predmetId)
        {
            var ocjene = await _context.Ocjene.Where(o => o.PredmetId == predmetId).ToListAsync();
            var averageGrade = ocjene.Any() ? ocjene.Average(o => o.Vrijednost) : 0;
            await _hubContext.Clients.All.SendAsync("UpdateAverageGrade", predmetId, averageGrade);
        }
    }
}
