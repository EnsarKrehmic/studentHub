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
        private readonly ILogger<OcjeneController> _logger;

        public OcjeneController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, ILogger<OcjeneController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

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
                        .ThenInclude(ssp => ssp.StudijskiProgram)
                .Include(o => o.Profesor)
                .Include(o => o.NastavnaAktivnost)
                .Include(o => o.DjelimicneOcjene);

            List<Ocjena> ocjene;

            if (User.IsInRole("Student"))
            {
                ocjene = await ocjeneQuery.Where(o => o.Student.AspNetUserId == userId && o.ParentOcjenaId == null).ToListAsync();

                double prosjekOcjena = ocjene.Any() ? ocjene.Average(o => o.Vrijednost) : 0;

                var ocjeneViewModel = ocjene.Select(o => new OcjenaViewModel
                {
                    Id = o.Id,
                    Tip = o.Tip.ToString(),
                    Vrijednost = o.Vrijednost,
                    DatumDodjele = o.DatumUnosa,
                    Komentar = o.Komentar,
                    TezinaProcentualno = o.TezinaProcentualno,
                    PredmetId = o.PredmetId ?? 0,
                    PredmetNaziv = o.Predmet?.Naziv,
                    NastavnaAktivnostNaziv = o.NastavnaAktivnost?.Naziv,
                    StudentIme = o.Student?.Ime,
                    StudentPrezime = o.Student?.Prezime,
                    StudentBrojIndeksa = o.Student?.BrojIndeksa,
                    ProfesorIme = o.Profesor?.Ime,
                    ProfesorPrezime = o.Profesor?.Prezime,
                    ProfesorTitula = o.Profesor?.ProfesorTitula,
                    ProsjekOcjena = prosjekOcjena,
                    StudentStudijskiProgramNaziv = GetStudijskiProgram(o)?.Naziv ?? "Nepoznato",
                    StudijskiProgramId = GetStudijskiProgram(o)?.Id ?? 0,
                    DjelimicneOcjene = o.DjelimicneOcjene.Select(d => new OcjenaViewModel
                    {
                        Id = d.Id,
                        Vrijednost = d.Vrijednost,
                        Tip = d.Tip.ToString(),
                        Komentar = d.Komentar,
                        TezinaProcentualno = d.TezinaProcentualno,
                        DatumDodjele = d.DatumUnosa,
                        NastavnaAktivnostNaziv = d.NastavnaAktivnost?.Naziv,
                        ProfesorIme = d.Profesor?.Ime,
                        ProfesorPrezime = d.Profesor?.Prezime
                    }).ToList()
                }).ToList();

                return View(ocjeneViewModel);
            }
            else if (User.IsInRole("Profesor"))
            {
                ocjene = await ocjeneQuery
                    .Where(o => o.Profesor.AspNetUserId == userId && o.Tip == TipOcjene.Predmet && o.ParentOcjenaId == null)
                    .ToListAsync();

                var prosjekPoPredmetu = ocjene
                    .GroupBy(o => o.PredmetId)
                    .ToDictionary(g => g.Key, g => g.Average(x => x.Vrijednost));

                var ocjeneViewModel = ocjene.Select(o => new OcjenaViewModel
                {
                    Id = o.Id,
                    Tip = o.Tip.ToString(),
                    Vrijednost = o.Vrijednost,
                    DatumDodjele = o.DatumUnosa,
                    Komentar = o.Komentar,
                    TezinaProcentualno = o.TezinaProcentualno,
                    PredmetId = o.PredmetId ?? 0,
                    PredmetNaziv = o.Predmet?.Naziv,
                    StudentIme = o.Student?.Ime,
                    StudentPrezime = o.Student?.Prezime,
                    StudentBrojIndeksa = o.Student?.BrojIndeksa,
                    ProfesorIme = o.Profesor?.Ime,
                    ProfesorPrezime = o.Profesor?.Prezime,
                    ProfesorTitula = o.Profesor?.ProfesorTitula,
                    ProsjekPoPredmetu = prosjekPoPredmetu.ContainsKey(o.PredmetId ?? 0) ? prosjekPoPredmetu[o.PredmetId ?? 0] : 0,
                    StudentStudijskiProgramNaziv = GetStudijskiProgram(o)?.Naziv ?? "Nepoznato",
                    StudijskiProgramId = GetStudijskiProgram(o)?.Id ?? 0,
                    DjelimicneOcjene = o.DjelimicneOcjene.Select(d => new OcjenaViewModel
                    {
                        Id = d.Id,
                        Vrijednost = d.Vrijednost,
                        Tip = d.Tip.ToString(),
                        Komentar = d.Komentar,
                        TezinaProcentualno = d.TezinaProcentualno,
                        DatumDodjele = d.DatumUnosa,
                        NastavnaAktivnostNaziv = d.NastavnaAktivnost?.Naziv,
                        ProfesorIme = d.Profesor?.Ime,
                        ProfesorPrezime = d.Profesor?.Prezime
                    }).ToList()
                }).ToList();

                return View(ocjeneViewModel);
            }
            else if (User.IsInRole("Studentska služba"))
            {
                ocjene = await ocjeneQuery
                    .Where(o => o.ParentOcjenaId == null)
                    .ToListAsync();

                var prosjekPoPredmetu = ocjene
                    .Where(o => o.Tip == TipOcjene.Predmet && o.Predmet != null)
                    .GroupBy(o => o.Predmet.Naziv)
                    .ToDictionary(g => g.Key, g => g.Average(x => x.Vrijednost));

                var prosjekPoStudijskomProgramu = ocjene
                    .Where(o => o.Tip == TipOcjene.Predmet && GetStudijskiProgram(o) != null)
                    .GroupBy(o => GetStudijskiProgram(o).Naziv)
                    .ToDictionary(g => g.Key, g => g.Average(x => x.Vrijednost));

                var ocjeneViewModel = ocjene.Select(o =>
                {
                    var studijskiProgram = GetStudijskiProgram(o);
                    var studijskiProgramNaziv = studijskiProgram?.Naziv ?? "Nepoznato";

                    return new OcjenaViewModel
                    {
                        Id = o.Id,
                        Tip = o.Tip.ToString(),
                        Vrijednost = o.Vrijednost,
                        DatumDodjele = o.DatumUnosa,
                        Komentar = o.Komentar,
                        TezinaProcentualno = o.TezinaProcentualno,
                        PredmetId = o.PredmetId ?? 0,
                        PredmetNaziv = o.Predmet?.Naziv,
                        NastavnaAktivnostNaziv = o.NastavnaAktivnost?.Naziv,
                        StudentId = o.StudentId,
                        StudentIme = o.Student?.Ime,
                        StudentPrezime = o.Student?.Prezime,
                        StudentBrojIndeksa = o.Student?.BrojIndeksa,
                        ProfesorIme = o.Profesor?.Ime,
                        ProfesorPrezime = o.Profesor?.Prezime,
                        ProfesorTitula = o.Profesor?.ProfesorTitula,
                        ProsjekPoPredmetu = o.Predmet != null && prosjekPoPredmetu.ContainsKey(o.Predmet.Naziv) ? prosjekPoPredmetu[o.Predmet.Naziv] : 0,
                        ProsjekPoStudijskomProgramu = studijskiProgram != null && prosjekPoStudijskomProgramu.ContainsKey(studijskiProgramNaziv) ? prosjekPoStudijskomProgramu[studijskiProgramNaziv] : 0,
                        ProsjekOcjena = ((o.Predmet != null && prosjekPoPredmetu.ContainsKey(o.Predmet.Naziv) ? prosjekPoPredmetu[o.Predmet.Naziv] : 0) +
                                        (studijskiProgram != null && prosjekPoStudijskomProgramu.ContainsKey(studijskiProgramNaziv) ? prosjekPoStudijskomProgramu[studijskiProgramNaziv] : 0)) / 2,
                        ProsjekPrikaz = $"Predmet '{(o.Predmet?.Naziv ?? "Nepoznat predmet")}': {(o.Predmet != null && prosjekPoPredmetu.ContainsKey(o.Predmet.Naziv) ? prosjekPoPredmetu[o.Predmet.Naziv] : 0):0.00}, " +
                                        (studijskiProgram != null ? $"Studijski program '{studijskiProgramNaziv}': {(prosjekPoStudijskomProgramu.ContainsKey(studijskiProgramNaziv) ? prosjekPoStudijskomProgramu[studijskiProgramNaziv] : 0):0.00}, " : "") +
                                        $"Ukupno: {(((o.Predmet != null && prosjekPoPredmetu.ContainsKey(o.Predmet.Naziv) ? prosjekPoPredmetu[o.Predmet.Naziv] : 0) + (studijskiProgram != null && prosjekPoStudijskomProgramu.ContainsKey(studijskiProgramNaziv) ? prosjekPoStudijskomProgramu[studijskiProgramNaziv] : 0)) / 2):0.00}",
                        StudentStudijskiProgramNaziv = studijskiProgramNaziv,
                        StudijskiProgramId = studijskiProgram?.Id ?? 0,
                        DjelimicneOcjene = o.DjelimicneOcjene.Select(d => new OcjenaViewModel
                        {
                            Id = d.Id,
                            Vrijednost = d.Vrijednost,
                            Tip = d.Tip.ToString(),
                            Komentar = d.Komentar,
                            TezinaProcentualno = d.TezinaProcentualno,
                            DatumDodjele = d.DatumUnosa,
                            NastavnaAktivnostNaziv = d.NastavnaAktivnost?.Naziv,
                            ProfesorIme = d.Profesor?.Ime,
                            ProfesorPrezime = d.Profesor?.Prezime
                        }).ToList()
                    };
                }).ToList();

                return View(ocjeneViewModel);
            }

            return Forbid();
        }

        // Helper metoda za pronalaženje studijskog programa
        private StudijskiProgram GetStudijskiProgram(Ocjena o)
        {
            return o.Predmet?.NastavniPlan?.StudijskiProgram
                ?? o.Student?.StudentStudijskiProgrami?.OrderByDescending(s => s.Id).FirstOrDefault()?.StudijskiProgram;
        }

        // GET: Ocjene/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound();

            var ocjena = await _context.Ocjene
                .Include(o => o.Predmet)
                    .ThenInclude(p => p.NastavniPlan)
                        .ThenInclude(np => np.StudijskiProgram)
                .Include(o => o.Student)
                    .ThenInclude(s => s.StudentStudijskiProgrami)
                        .ThenInclude(ssp => ssp.StudijskiProgram)
                .Include(o => o.Profesor)
                .Include(o => o.NastavnaAktivnost)
                .Include(o => o.DjelimicneOcjene)
                    .ThenInclude(p => p.Profesor)
                .Include(o => o.DjelimicneOcjene)
                    .ThenInclude(p => p.NastavnaAktivnost)
                .Include(o => o.ParentOcjena)
                    .ThenInclude(p => p.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ocjena == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Student") && ocjena.Student.AspNetUserId != userId)
                return Forbid();
            else if (User.IsInRole("Profesor") && ocjena.Profesor.AspNetUserId != userId)
                return Forbid();

            var studijskiProgram = ocjena.Predmet?.NastavniPlan?.StudijskiProgram
                ?? ocjena.Student.StudentStudijskiProgrami
                    .OrderByDescending(s => s.Id)
                    .FirstOrDefault()?.StudijskiProgram;

            // Ispiti i prijave
            var ispiti = await _context.Ispiti
                .Where(i => i.PredmetId == ocjena.PredmetId)
                .OrderBy(i => i.DatumOdrzavanja)
                .ToListAsync();

            var prijave = await _context.Prijave
                .Where(p => p.StudentId == ocjena.StudentId && ispiti.Select(i => i.Id).Contains(p.IspitId))
                .ToListAsync();

            var bodoviSaIspita = prijave
                .Join(ispiti, p => p.IspitId, i => i.Id, (p, i) => (
                    IspitId: i.Id,
                    IspitNaziv: $"Ispit ({i.DatumOdrzavanja:dd.MM.yyyy})",
                    Bodovi: p.Bodovi ?? 0,
                    Datum: i.DatumOdrzavanja
                ))
                .ToList();

            // Prisustvo
            var aktivnosti = await _context.NastavneAktivnosti
                .Where(a => a.PredmetId == ocjena.PredmetId)
                .ToListAsync();

            var prisustva = await _context.PrisustvaNaAktivnostima
                .Where(p => p.StudentId == ocjena.StudentId && aktivnosti.Select(a => a.Id).Contains(p.NastavnaAktivnostId))
                .ToListAsync();

            float IzracunajProcenat(TipNastavneAktivnosti tip)
            {
                var aktivnostiTipa = aktivnosti.Where(a => a.Tip == tip).ToList();
                if (!aktivnostiTipa.Any()) return 0;
                var prisutan = prisustva.Count(p => aktivnostiTipa.Any(a => a.Id == p.NastavnaAktivnostId));
                return (float)prisutan / aktivnostiTipa.Count * 100;
            }

            var ocjenaViewModel = new OcjenaViewModel
            {
                Id = ocjena.Id,
                PredmetId = ocjena.PredmetId ?? 0,
                PredmetNaziv = ocjena.Predmet?.Naziv,
                NastavnaAktivnostNaziv = ocjena.NastavnaAktivnost?.Naziv,
                Tip = ocjena.Tip.ToString(),
                StudentId = ocjena.StudentId,
                StudentIme = ocjena.Student?.Ime,
                StudentPrezime = ocjena.Student?.Prezime,
                StudentBrojIndeksa = ocjena.Student?.BrojIndeksa,
                ProfesorIme = ocjena.Profesor?.Ime,
                ProfesorPrezime = ocjena.Profesor?.Prezime,
                ProfesorTitula = ocjena.Profesor?.ProfesorTitula,
                Vrijednost = ocjena.Vrijednost,
                DatumDodjele = ocjena.DatumUnosa,
                Komentar = ocjena.Komentar,
                TezinaProcentualno = ocjena.TezinaProcentualno,
                ParentOcjenaId = ocjena.ParentOcjenaId,
                ParentOcjena = ocjena.ParentOcjena != null ? new OcjenaViewModel
                {
                    Id = ocjena.ParentOcjena.Id,
                    Vrijednost = ocjena.ParentOcjena.Vrijednost,
                    Komentar = ocjena.ParentOcjena.Komentar,
                    TezinaProcentualno = ocjena.ParentOcjena.TezinaProcentualno
                } : null,
                ParentNaziv = ocjena.ParentOcjena != null
                    ? ocjena.ParentOcjena.Predmet?.Naziv ?? "Ocjena"
                    : null,
                StudentStudijskiProgramNaziv = studijskiProgram?.Naziv ?? "Nepoznato",
                StudijskiProgramId = studijskiProgram?.Id ?? 0,
                BodoviSaIspita = bodoviSaIspita,
                ProcenatPrisustvaUkupno = aktivnosti.Any() ? (float)prisustva.Count / aktivnosti.Count * 100 : 0,
                ProcenatPrisustvaPredavanja = IzracunajProcenat(TipNastavneAktivnosti.Predavanje),
                ProcenatPrisustvaVjezbi = IzracunajProcenat(TipNastavneAktivnosti.Vjezba),
                DjelimicneOcjene = ocjena.DjelimicneOcjene?.Select(d => new OcjenaViewModel
                {
                    Id = d.Id,
                    Vrijednost = d.Vrijednost,
                    Komentar = d.Komentar,
                    Tip = d.Tip.ToString(),
                    TezinaProcentualno = d.TezinaProcentualno,
                    DatumDodjele = d.DatumUnosa,
                    ProfesorIme = d.Profesor?.Ime,
                    ProfesorPrezime = d.Profesor?.Prezime,
                    NastavnaAktivnostNaziv = d.NastavnaAktivnost?.Naziv
                }).ToList()
            };

            return View(ocjenaViewModel);
        }

        // GET: Ocjene/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Profesor,Student")]
        public IActionResult Create(string tip, long? predmetId, long? nastavnaAktivnostId, long? studentId, long? parentOcjenaId)
        {
            if (tip == "Predmet" && predmetId.HasValue && studentId.HasValue)
            {
                return RedirectToAction(nameof(CreatePredmetOcjena), new { predmetId, studentId, parentOcjenaId });
            }

            if (tip == "NastavnaAktivnost" && nastavnaAktivnostId.HasValue)
            {
                return RedirectToAction(nameof(CreateNastavnaAktivnostOcjena), new { nastavnaAktivnostId });
            }

            TempData["Error"] = "Nedostaju potrebni parametri za unos ocjene.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Ocjene/CreatePredmetOcjena
        [HttpGet("CreatePredmetOcjena")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> CreatePredmetOcjena(long predmetId, long studentId, long? parentOcjenaId)
        {
            var predmet = await _context.Predmeti.FindAsync(predmetId);
            var student = await _context.Studenti.FindAsync(studentId);
            var profesor = await _context.Profesori.FirstOrDefaultAsync(p => p.AspNetUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (predmet == null || student == null || profesor == null)
            {
                TempData["Error"] = "Podaci o predmetu, studentu ili profesoru nisu pronađeni.";
                return RedirectToAction("Index");
            }

            ViewBag.PredmetNaziv = predmet.Naziv;
            ViewBag.StudentImePrezime = $"{student.Ime} {student.Prezime}";
            if (parentOcjenaId.HasValue)
                ViewBag.ParentOcjenaId = parentOcjenaId;

            var model = new Ocjena
            {
                Tip = TipOcjene.Predmet,
                PredmetId = predmetId,
                StudentId = studentId,
                ProfesorId = profesor.Id,
                DatumUnosa = DateTime.Today,
                ParentOcjenaId = parentOcjenaId
            };

            return View("CreatePredmetOcjena", model);
        }

        // POST: Ocjene/CreatePredmetOcjena
        [HttpPost("CreatePredmetOcjena")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> CreatePredmetOcjena(Ocjena ocjena)
        {
            var profesor = await _context.Profesori.FirstOrDefaultAsync(p => p.AspNetUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (profesor != null)
                ocjena.ProfesorId = profesor.Id;

            if (!ocjena.IsValid())
                ModelState.AddModelError("Vrijednost", "Ocjena nije u dozvoljenom rasponu.");

            if (ocjena.TezinaProcentualno is < 0 or > 100)
                ModelState.AddModelError("TezinaProcentualno", "Težina mora biti između 0 i 100.");

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning($"Greška u polju '{entry.Key}': {error.ErrorMessage}");
                    }
                }

                TempData["Error"] = "Unos ocjene nije uspio. Provjerite greške u formi.";

                var predmet = await _context.Predmeti.FindAsync(ocjena.PredmetId);
                var student = await _context.Studenti.FindAsync(ocjena.StudentId);
                ViewBag.PredmetNaziv = predmet?.Naziv;
                ViewBag.StudentImePrezime = student != null ? $"{student.Ime} {student.Prezime}" : "";
                if (ocjena.ParentOcjenaId.HasValue)
                    ViewBag.ParentOcjenaId = ocjena.ParentOcjenaId;
                return View("CreatePredmetOcjena", ocjena);
            }

            _context.Add(ocjena);
            await _context.SaveChangesAsync();

            if (ocjena.PredmetId.HasValue)
                await UpdateAverageGrade(ocjena.PredmetId.Value);

            TempData["Success"] = "Ocjena je uspješno dodana.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Ocjene/CreateNastavnaAktivnostOcjena
        [HttpGet("CreateNastavnaAktivnostOcjena")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateNastavnaAktivnostOcjena(long nastavnaAktivnostId)
        {
            var aktivnost = await _context.NastavneAktivnosti.FindAsync(nastavnaAktivnostId);
            if (aktivnost == null)
            {
                TempData["Error"] = "Nastavna aktivnost nije pronađena.";
                return RedirectToAction("Index");
            }

            ViewBag.NastavnaAktivnostId = nastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = aktivnost.Naziv;

            var model = new Ocjena
            {
                Tip = TipOcjene.NastavnaAktivnost,
                NastavnaAktivnostId = nastavnaAktivnostId,
                DatumUnosa = DateTime.Today
            };

            return View("CreateNastavnaAktivnostOcjena", model);
        }

        // POST: Ocjene/CreateNastavnaAktivnostOcjena
        [HttpPost("CreateNastavnaAktivnostOcjena")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateNastavnaAktivnostOcjena(Ocjena ocjena)
        {
            if (!ocjena.IsValid())
                ModelState.AddModelError(nameof(ocjena.Vrijednost), "Ocjena mora biti u dozvoljenom rasponu.");

            if (ocjena.TezinaProcentualno is < 0 or > 100)
                ModelState.AddModelError(nameof(ocjena.TezinaProcentualno), "Težina mora biti između 0 i 100.");

            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (student != null)
                ocjena.StudentId = student.Id;

            ocjena.ProfesorId = null;

            if (!ModelState.IsValid)
            {
                var aktivnost = await _context.NastavneAktivnosti.FindAsync(ocjena.NastavnaAktivnostId);
                ViewBag.NastavnaAktivnostId = ocjena.NastavnaAktivnostId;
                ViewBag.NastavnaAktivnostNaziv = aktivnost?.Naziv ?? "Nepoznata aktivnost";
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning($"Greška u polju '{entry.Key}': {error.ErrorMessage}");
                    }
                }
                TempData["Error"] = "Unos ocjene nije uspio. Provjerite greške u formi."; return View("CreateNastavnaAktivnostOcjena", ocjena);
            }

            _context.Add(ocjena);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Vaša ocjena je uspješno zabilježena.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Ocjene/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Edit(long id)
        {
            var ocjena = await _context.Ocjene.FirstOrDefaultAsync(o => o.Id == id);
            if (ocjena == null)
                return NotFound();

            return ocjena.Tip switch
            {
                TipOcjene.Predmet => RedirectToAction(nameof(EditPredmetOcjena), new { id }),
                TipOcjene.NastavnaAktivnost => RedirectToAction(nameof(EditNastavnaAktivnostOcjena), new { id }),
                _ => NotFound()
            };
        }

        // GET: Ocjene/EditPredmetOcjena/{id}
        [HttpGet("EditPredmetOcjena/{id:long}")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> EditPredmetOcjena(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ocjena = await _context.Ocjene
                .Include(o => o.Student)
                .Include(o => o.Predmet)
                .Include(o => o.Profesor)
                .FirstOrDefaultAsync(o => o.Id == id && o.Tip == TipOcjene.Predmet);

            if (ocjena == null)
            {
                TempData["Error"] = "Ocjena nije pronađena.";
                return RedirectToAction("Index");
            }

            ViewBag.StudentId = new SelectList(_context.Studenti.Select(s => new
            {
                s.Id,
                FullName = s.Ime + " " + s.Prezime
            }), "Id", "FullName", ocjena.StudentId);

            ViewBag.PredmetId = new SelectList(_context.Predmeti
                .Where(p => p.Profesor.AspNetUserId == userId || p.PredmetProfesori.Any(pp => pp.Profesor.AspNetUserId == userId))
                .Select(p => new { p.Id, p.Naziv }), "Id", "Naziv", ocjena.PredmetId);

            var profesor = await _context.Profesori.FirstOrDefaultAsync(p => p.AspNetUserId == userId);
            if (profesor != null)
            {
                ViewBag.ProfesorId = new SelectList(new[] {
            new { profesor.Id, FullName = profesor.ProfesorTitula + " " + profesor.Ime + " " + profesor.Prezime }
        }, "Id", "FullName", profesor.Id);
            }
            else
            {
                ViewBag.ProfesorId = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            return View("EditPredmetOcjena", ocjena);
        }

        // POST: Ocjene/EditPredmetOcjena/{id}
        [HttpPost("EditPredmetOcjena/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> EditPredmetOcjena(long id, Ocjena ocjena)
        {
            if (id != ocjena.Id || ocjena.Tip != TipOcjene.Predmet)
                return BadRequest();

            if (!ocjena.IsValid())
                ModelState.AddModelError("Vrijednost", "Ocjena nije validna.");

            if (ocjena.TezinaProcentualno is < 0 or > 100)
                ModelState.AddModelError("TezinaProcentualno", "Težina mora biti između 0 i 100.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profesor = await _context.Profesori.FirstOrDefaultAsync(p => p.AspNetUserId == userId);
            if (profesor != null)
                ocjena.ProfesorId = profesor.Id;

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning($"Greška u polju '{entry.Key}': {error.ErrorMessage}");
                    }
                }

                TempData["Error"] = "Izmjena ocjene nije uspjela. Provjerite greške u formi.";

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

                return View("EditPredmetOcjena", ocjena);
            }

            _context.Update(ocjena);
            await _context.SaveChangesAsync();

            if (ocjena.PredmetId.HasValue)
                await UpdateAverageGrade(ocjena.PredmetId.Value);

            TempData["Success"] = "Ocjena je uspješno ažurirana.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Ocjene/EditNastavnaAktivnostOcjena/{id}
        [HttpGet("EditNastavnaAktivnostOcjena/{id:long}")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> EditNastavnaAktivnostOcjena(long id)
        {
            var ocjena = await _context.Ocjene
                .Include(o => o.Student)
                .Include(o => o.NastavnaAktivnost)
                .Include(o => o.Profesor)
                .FirstOrDefaultAsync(o => o.Id == id && o.Tip == TipOcjene.NastavnaAktivnost);

            if (ocjena == null)
            {
                TempData["Error"] = "Ocjena nije pronađena.";
                return RedirectToAction("Index");
            }

            ViewBag.NastavnaAktivnostId = ocjena.NastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = ocjena.NastavnaAktivnost?.Naziv;

            return View("EditNastavnaAktivnostOcjena", ocjena);
        }

        // POST: Ocjene/EditNastavnaAktivnostOcjena/{id}
        [HttpPost("EditNastavnaAktivnostOcjena/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> EditNastavnaAktivnostOcjena(long id, Ocjena ocjena)
        {
            if (id != ocjena.Id || ocjena.Tip != TipOcjene.NastavnaAktivnost)
                return BadRequest();

            if (!ocjena.IsValid())
                ModelState.AddModelError("Vrijednost", "Ocjena nije validna.");

            if (ocjena.TezinaProcentualno is < 0 or > 100)
                ModelState.AddModelError("TezinaProcentualno", "Težina mora biti između 0 i 100.");

            if (!ModelState.IsValid)
            {
                ViewBag.NastavnaAktivnostId = ocjena.NastavnaAktivnostId;
                ViewBag.NastavnaAktivnostNaziv = (await _context.NastavneAktivnosti
                    .FirstOrDefaultAsync(n => n.Id == ocjena.NastavnaAktivnostId))?.Naziv ?? "Nepoznata aktivnost";

                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning($"Greška u polju '{entry.Key}': {error.ErrorMessage}");
                    }
                }

                TempData["Error"] = "Izmjena ocjene nije uspjela. Provjerite greške u formi.";
                return View("EditNastavnaAktivnostOcjena", ocjena);
            }

            _context.Update(ocjena);
            await _context.SaveChangesAsync();

            if (ocjena.PredmetId.HasValue)
                await UpdateAverageGrade(ocjena.PredmetId.Value);

            TempData["Success"] = "Ocjena je uspješno ažurirana.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Ocjene/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            var ocjena = await _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Student)
                .Include(o => o.Profesor)
                .Include(o => o.NastavnaAktivnost)
                .Include(o => o.DjelimicneOcjene)
                .Include(o => o.ParentOcjena)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ocjena == null)
                return NotFound();

            return View(ocjena);
        }

        // POST: Ocjene/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ocjena = await _context.Ocjene
                .Include(o => o.DjelimicneOcjene)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ocjena == null)
                return NotFound();

            // Briši parcijalne ocjene (ako ih ima)
            if (ocjena.DjelimicneOcjene?.Any() == true)
            {
                _context.Ocjene.RemoveRange(ocjena.DjelimicneOcjene);
            }

            _context.Ocjene.Remove(ocjena);
            await _context.SaveChangesAsync();

            if (ocjena.PredmetId.HasValue)
                await UpdateAverageGrade(ocjena.PredmetId.Value);

            TempData["SuccessMessage"] = "Ocjena je uspješno obrisana.";

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
