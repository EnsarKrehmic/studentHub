using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace StudentHub.Controllers
{
    [Route("Studenti")]
    public class StudentiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PredmetiController> _logger;

        public StudentiController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<PredmetiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Studenti
        [HttpGet("")]
        public async Task<IActionResult> Index(string sortOrder, string searchString, long? studijskiProgramId)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SurnameSortParm"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["JMBGSortParm"] = sortOrder == "jmbg_asc" ? "jmbg_desc" : "jmbg_asc";
            ViewData["IndexSortParm"] = sortOrder == "index_asc" ? "index_desc" : "index_asc";
            ViewData["EmailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStudijskiProgramId"] = studijskiProgramId;

            var studentiQuery = _context.Studenti.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentiQuery = studentiQuery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                studentiQuery = studentiQuery.Where(p => _context.StudentStudijskiProgrami.Any(psp => psp.StudentId == p.Id && psp.StudijskiProgramId == studijskiProgramId.Value));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Ime);
                    break;
                case "surname_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.Prezime);
                    break;
                case "surname_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Prezime);
                    break;
                case "jmbg_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.JMBG);
                    break;
                case "jmbg_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.JMBG);
                    break;
                case "index_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.BrojIndeksa);
                    break;
                case "index_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.BrojIndeksa);
                    break;
                default:
                    studentiQuery = studentiQuery.OrderBy(s => s.Ime);
                    break;
            }

            var students = await studentiQuery.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(students);
        }

        // GET: Studenti/Details{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id, string sortOrder, string searchString, long? studijskiProgramId)
        {
            if (id == null)
                return NotFound();

            var student = await _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                    .ThenInclude(ssp => ssp.StudijskiProgram)
                .Include(s => s.StudentNaPredmetima)
                    .ThenInclude(snp => snp.Predmet)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new InvalidOperationException("Student nije pronađen.");

            // Provjera vlasništva ako je Student
            if (User.IsInRole("Student"))
            {
                var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (student.AspNetUserId != loggedInUserId)
                    return Forbid();
            }

            var studijskiProgram = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram;
            var studijskiProgramIdForLimit = studijskiProgram?.Id ?? 0;

            // Predmeti koje pohađa
            var predmeti = await _context.StudentiNaPredmetima
                .Include(snp => snp.Predmet)
                .Where(snp => snp.StudentId == id)
                .Select(snp => snp.Predmet)
                .ToListAsync();

            // Sve ocjene (predmetne)
            var sveOcjene = await _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Profesor)
                .Where(o => o.StudentId == student.Id && o.Tip == TipOcjene.Predmet)
                .ToListAsync();

            var glavne = sveOcjene.Where(o => o.ParentOcjenaId == null).ToList();
            var parcijalne = sveOcjene.Where(o => o.ParentOcjenaId != null).ToList();

            var grupisaneParcijalne = parcijalne
                .GroupBy(o => o.ParentOcjenaId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var ocjeneViewModels = glavne.Select(o =>
            {
                float? ponderisana = null;

                if (grupisaneParcijalne.TryGetValue(o.Id, out var parcijalneZaOvu) && parcijalneZaOvu.Any(p => p.TezinaProcentualno.HasValue))
                {
                    var suma = parcijalneZaOvu.Sum(p => p.Vrijednost * ((p.TezinaProcentualno ?? 0) / 100f));
                    ponderisana = suma;
                }

                return new OcjenaViewModel
                {
                    Id = o.Id,
                    Tip = o.Tip.ToString(),
                    PredmetId = o.PredmetId ?? 0,
                    PredmetNaziv = o.Predmet?.Naziv ?? "",
                    StudentIme = student.Ime,
                    StudentPrezime = student.Prezime,
                    StudentBrojIndeksa = student.BrojIndeksa,
                    StudentStudijskiProgramNaziv = studijskiProgram?.Naziv ?? "",
                    ProfesorIme = o.Profesor?.Ime ?? "",
                    ProfesorPrezime = o.Profesor?.Prezime ?? "",
                    ProfesorTitula = o.Profesor?.ProfesorTitula ?? "",
                    Vrijednost = o.Vrijednost,
                    TezinaProcentualno = o.TezinaProcentualno,
                    Komentar = o.Komentar,
                    DatumDodjele = o.DatumUnosa,
                    DjelimicneOcjene = grupisaneParcijalne.ContainsKey(o.Id)
                        ? grupisaneParcijalne[o.Id].Select(p => new OcjenaViewModel
                        {
                            Vrijednost = p.Vrijednost,
                            TezinaProcentualno = p.TezinaProcentualno,
                            Komentar = p.Komentar,
                            DatumDodjele = p.DatumUnosa
                        }).ToList()
                        : new List<OcjenaViewModel>(),
                    StudentId = student.Id,
                    ProfesorId = o.ProfesorId,
                    StudijskiProgramId = studijskiProgramIdForLimit,
                    ProsjekPrikaz = ponderisana.HasValue ? ponderisana.Value.ToString("0.00") : null
                };
            }).ToList();

            // Dohvati LIMIT ako postoji
            StudijskiProgramIzborniLimit? limit = null;
            if (student.GodinaStudija.HasValue && studijskiProgramIdForLimit > 0)
            {
                limit = await _context.StudijskiProgramIzborniLimiti
                    .FirstOrDefaultAsync(l =>
                        l.StudijskiProgramId == studijskiProgramIdForLimit &&
                        l.GodinaStudija == student.GodinaStudija.Value);
            }

            // Kreiraj view model
            var viewModel = new StudentDetailsViewModel
            {
                Student = student,
                CurrentSort = sortOrder,
                SearchString = searchString,
                StudijskiProgramId = studijskiProgramId,
                Predmeti = predmeti,
                OcjenePredmeta = ocjeneViewModels,
                StudijskiProgramIzborniLimit = limit
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);
            return View(viewModel);
        }

        // GET: Studenti/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.NastavniPlanovi = new SelectList(new List<SelectListItem>(), "Value", "Text");
            ViewBag.Predmeti = new SelectList(new List<SelectListItem>(), "Value", "Text");

            return View();
        }

        // POST: Studenti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create(StudentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"Field: {entry.Key}, Error: {error.ErrorMessage}");
                    }
                }

                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);

                ViewBag.NastavniPlanovi = model.StudijskiProgramId != 0
                    ? new SelectList(
                        _context.NastavniPlanovi
                            .Where(np => np.StudijskiProgramId == model.StudijskiProgramId)
                            .Select(np => new
                            {
                                Id = np.Id,
                                Naziv = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godinu"
                            }).ToList(),
                        "Id", "Naziv", model.NastavniPlanId)
                    : new SelectList(new List<SelectListItem>(), "Value", "Text");

                ViewBag.Predmeti = model.StudijskiProgramId != 0
                    ? new SelectList(
                        _context.Predmeti
                            .Where(p => p.NastavniPlan.StudijskiProgramId == model.StudijskiProgramId)
                            .Select(p => new
                            {
                                Id = p.Id,
                                Naziv = p.Naziv
                            }).ToList(),
                        "Id", "Naziv", model.PredmetIds)
                    : new SelectList(new List<SelectListItem>(), "Value", "Text");

                return View(model);
            }

            // 1. Kreiranje Identity User-a
            var identityUser = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true // ili false, po želji
            };

            var result = await _userManager.CreateAsync(identityUser, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);

                ViewBag.NastavniPlanovi = model.StudijskiProgramId != 0
                    ? new SelectList(
                        _context.NastavniPlanovi
                            .Where(np => np.StudijskiProgramId == model.StudijskiProgramId)
                            .Select(np => new
                            {
                                Id = np.Id,
                                Naziv = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godinu"
                            }).ToList(),
                        "Id", "Naziv", model.NastavniPlanId)
                    : new SelectList(new List<SelectListItem>(), "Value", "Text");

                ViewBag.Predmeti = model.StudijskiProgramId != 0
                    ? new SelectList(
                        _context.Predmeti
                            .Where(p => p.NastavniPlan.StudijskiProgramId == model.StudijskiProgramId)
                            .Select(p => new
                            {
                                Id = p.Id,
                                Naziv = p.Naziv
                            }).ToList(),
                        "Id", "Naziv", model.PredmetIds)
                    : new SelectList(new List<SelectListItem>(), "Value", "Text");

                return View(model);
            }

            // 2. Dodavanje u rolu "Student"
            try
            {
                await _userManager.AddToRoleAsync(identityUser, "Student");
                Console.WriteLine(">>> AddToRoleAsync prošao OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> ERROR u AddToRoleAsync: {ex.Message}");
            }

            // 3. Kreiranje Studenta u bazi
            try
            {
                var student = new Student
                {
                    AspNetUserId = identityUser.Id,
                    JMBG = model.JMBG,
                    Ime = model.Ime,
                    Prezime = model.Prezime,
                    Email = model.Email,
                    BrojIndeksa = model.BrojIndeksa,
                    PrethodnoObrazovanje = model.PrethodnoObrazovanje,
                    GodinaStudija = model.GodinaStudija,
                    Semestar = model.Semestar,
                    Uloga = Uloga.Student,
                    NastavniPlanId = model.NastavniPlanId
                };

                _context.Studenti.Add(student);
                await _context.SaveChangesAsync();
                Console.WriteLine(">>> SaveChangesAsync za Student prošao OK");

                // Upisivanje u pomoćne tabele
                if (model.StudijskiProgramId != 0)
                {
                    _context.StudentStudijskiProgrami.Add(new StudentStudijskiProgram
                    {
                        StudentId = student.Id,
                        StudijskiProgramId = model.StudijskiProgramId
                    });
                }

                if (model.PredmetIds != null && model.PredmetIds.Any())
                {
                    foreach (var predmetId in model.PredmetIds)
                    {
                        _context.StudentiNaPredmetima.Add(new StudentNaPredmetu
                        {
                            StudentId = student.Id,
                            PredmetId = predmetId,
                            AkademskaGodina = DateTime.Now.Year.ToString(),
                            AspNetUserId = identityUser.Id
                        });
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine(">>> SaveChangesAsync za pomoćne tabele prošao OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> ERROR u SaveChangesAsync: {ex.Message}");
            }

            // Gotovo → redirect na Index
            return RedirectToAction(nameof(Index));
        }

        // GET: Studenti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var studijskiProgramId = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgramId ?? 0;

            var model = new StudentEditViewModel
            {
                Id = student.Id,
                Ime = student.Ime,
                Prezime = student.Prezime,
                JMBG = student.JMBG,
                Email = student.Email,
                BrojIndeksa = student.BrojIndeksa,
                StudijskiProgramId = studijskiProgramId,
                GodinaStudija = student.GodinaStudija,
                Semestar = student.Semestar,
                PrethodnoObrazovanje = student.PrethodnoObrazovanje,
                Uloga = student.Uloga,
                IzborIzbornihPredmetaZakljucan = student.IzborIzbornihPredmetaZakljucan,
                PredmetIds = await _context.StudentiNaPredmetima
                    .Where(snp => snp.StudentId == student.Id)
                    .Select(snp => snp.PredmetId)
                    .ToListAsync()
            };

            ViewBag.Uloge = Enum.GetValues(typeof(Uloga))
                .Cast<Uloga>()
                .Select(u => new SelectListItem
                {
                    Value = ((int)u).ToString(),
                    Text = u.ToString(),
                    Selected = u == student.Uloga
                });

            ViewBag.StudijskiProgrami = new SelectList(
                _context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);
            ViewBag.NastavniPlanovi = new SelectList(
                _context.NastavniPlanovi, "Id", "GodinaStudija", student.NastavniPlanId);
            ViewBag.Predmeti = new SelectList(
                _context.Predmeti.Where(p => _context.StudentiNaPredmetima
                    .Any(snp => snp.StudentId == student.Id && snp.PredmetId == p.Id)),
                "Id", "Naziv");

            return View(model);
        }

        // POST: Studenti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, StudentEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.StudijskiProgrami = new SelectList(await _context.StudijskiProgrami.ToListAsync(), "Id", "Naziv");
                ViewBag.Predmeti = new SelectList(await _context.Predmeti.ToListAsync(), "Id", "Naziv");
                return View(model);
            }

            var existingStudent = await _context.Studenti.Include(s => s.StudentStudijskiProgrami)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStudent == null)
            {
                return NotFound();
            }

            existingStudent.Ime = model.Ime;
            existingStudent.Prezime = model.Prezime;
            existingStudent.Email = model.Email;
            existingStudent.BrojIndeksa = model.BrojIndeksa;
            existingStudent.PrethodnoObrazovanje = model.PrethodnoObrazovanje;
            existingStudent.GodinaStudija = model.GodinaStudija;
            existingStudent.Uloga = model.Uloga;
            existingStudent.Semestar = model.Semestar;

            existingStudent.IzborIzbornihPredmetaZakljucan = model.IzborIzbornihPredmetaZakljucan;

            var nastavniPlan = await _context.NastavniPlanovi
                .FirstOrDefaultAsync(np => np.StudijskiProgramId == model.StudijskiProgramId
                                           && np.GodinaStudija == model.GodinaStudija.ToString());

            if (nastavniPlan == null)
            {
                ModelState.AddModelError("NastavniPlanId", "Nastavni plan za odabranu godinu studija nije pronađen.");
                return View(model);
            }

            existingStudent.NastavniPlanId = nastavniPlan.Id;

            var stariPredmeti = await _context.StudentiNaPredmetima
                .Where(snp => snp.StudentId == existingStudent.Id)
                .ToListAsync();

            _context.StudentiNaPredmetima.RemoveRange(stariPredmeti);

            if (model.PredmetIds != null && model.PredmetIds.Any())
            {
                foreach (var predmetId in model.PredmetIds)
                {
                    _context.StudentiNaPredmetima.Add(new StudentNaPredmetu
                    {
                        StudentId = existingStudent.Id,
                        PredmetId = predmetId,
                        AkademskaGodina = DateTime.Now.Year.ToString(),
                        AspNetUserId = existingStudent.AspNetUserId
                    });
                }
            }

            // Ažuriranje pomoćne tabele StudentStudijskiProgram
            var existingEntries = _context.StudentStudijskiProgrami
                .Where(ssp => ssp.StudentId == existingStudent.Id)
                .ToList();

            _context.StudentStudijskiProgrami.RemoveRange(existingEntries);

            if (model.StudijskiProgramId != 0)
            {
                _context.StudentStudijskiProgrami.Add(new StudentStudijskiProgram
                {
                    StudentId = existingStudent.Id,
                    StudijskiProgramId = model.StudijskiProgramId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Studenti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Studenti/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student != null)
            {
                _context.Studenti.Remove(student);
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id)
        {
            return _context.Studenti.Any(e => e.Id == id);
        }

        [HttpGet("GroupedByProgram")]
        public async Task<IActionResult> GroupedByProgram(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SurnameSortParm"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["IndexSortParm"] = sortOrder == "index_asc" ? "index_desc" : "index_asc";
            ViewData["CurrentFilter"] = searchString;

            var studentiQuery = _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentiQuery = studentiQuery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Ime);
                    break;
                case "surname_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.Prezime);
                    break;
                case "surname_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Prezime);
                    break;
                case "index_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.BrojIndeksa);
                    break;
                case "index_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.BrojIndeksa);
                    break;
                default:
                    studentiQuery = studentiQuery.OrderBy(s => s.Ime);
                    break;
            }

            var groupedStudents = await studentiQuery
                .GroupBy(s => s.StudentStudijskiProgrami.FirstOrDefault().StudijskiProgram)
                .Select(g => new StudentGroupedByProgramViewModel
                {
                    StudijskiProgram = g.Key,
                    Studenti = g.ToList()
                })
                .ToListAsync();

            return View(groupedStudents);
        }

        [HttpGet("GetPredmetiByStudijskiProgram/{studijskiProgramId}")]
        public async Task<IActionResult> GetPredmetiByStudijskiProgram(long studijskiProgramId)
        {
            var predmeti = await _context.Predmeti
                .Where(p => p.NastavniPlan.StudijskiProgramId == studijskiProgramId)
                .Select(p => new
                {
                    id = p.Id,
                    naziv = p.Naziv
                })
                .ToListAsync();

            return Json(predmeti);
        }

        [HttpGet("GetNastavniPlanovi/{studijskiProgramId}")]
        public async Task<IActionResult> GetNastavniPlanovi(long studijskiProgramId)
        {
            var nastavaniPlanovi = await _context.NastavniPlanovi
                .Where(np => np.StudijskiProgramId == studijskiProgramId)
                .Select(np => new
                {
                    id = np.Id,
                    naziv = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godinu"
                })
                .ToListAsync();

            return Json(nastavaniPlanovi);
        }

        [HttpGet("BirajIzbornePredmete")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> BirajIzbornePredmete()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var student = await _context.Studenti
                .Include(s => s.StudentStudijskiProgrami).ThenInclude(ssp => ssp.StudijskiProgram)
                .FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            if (student == null)
            {
                return NotFound();
            }

            if (student.GodinaStudija == null || student.GodinaStudija <= 0)
            {
                TempData["ErrorMessage"] = "Vašoj studentskoj evidenciji nedostaje godina studija. Molimo kontaktirajte Studentsku službu.";
                return RedirectToAction("Details", "Studenti", new { id = student.Id });
            }

            var godinaStudija = student.GodinaStudija.Value;
            var studijskiProgramId = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgramId ?? 0;
            var studijskiProgram = await _context.StudijskiProgrami.FirstOrDefaultAsync(sp => sp.Id == studijskiProgramId);

            var limit = await _context.StudijskiProgramIzborniLimiti
                .FirstOrDefaultAsync(l => l.StudijskiProgramId == studijskiProgramId && l.GodinaStudija == godinaStudija);

            if (limit == null)
            {
                TempData["ErrorMessage"] = "Za Vaš studijski program i godinu nije postavljen limit za izborne predmete. Molimo kontaktirajte Studentsku službu.";
                return RedirectToAction("Details", "Studenti", new { id = student.Id });
            }

            var izborniPredmeti = await _context.Predmeti
                .Where(p => p.TipPredmeta == TipPredmeta.Izborni &&
                            p.NastavniPlan.StudijskiProgramId == studijskiProgramId &&
                            p.NastavniPlan.GodinaStudija == godinaStudija.ToString())
                .ToListAsync();

            if (!izborniPredmeti.Any())
            {
                TempData["ErrorMessage"] = "Trenutno nema dostupnih izbornih predmeta za Vaš studijski program i godinu.";
                return RedirectToAction("Details", "Studenti", new { id = student.Id });
            }

            var odabraniPredmetIds = await _context.StudentiNaPredmetima
                .Where(snp => snp.StudentId == student.Id && snp.Predmet.TipPredmeta == TipPredmeta.Izborni)
                .Select(snp => snp.PredmetId)
                .ToListAsync();

            var brojVecOdabranih = odabraniPredmetIds.Count;

            var model = new BirajIzbornePredmeteViewModel
            {
                StudentId = student.Id,
                ImePrezime = $"{student.Ime} {student.Prezime}",
                GodinaStudija = godinaStudija,
                StudijskiProgramId = studijskiProgramId,
                StudijskiProgramNaziv = studijskiProgram?.Naziv ?? "",
                MinIzborniPredmeti = limit.MinIzborniPredmeti,
                MaxIzborniPredmeti = limit.MaxIzborniPredmeti,
                BrojVecOdabranihPredmeta = brojVecOdabranih,
                IsLocked = student.IzborIzbornihPredmetaZakljucan || brojVecOdabranih >= limit.MaxIzborniPredmeti,
                Predmeti = izborniPredmeti.Select(p => new PredmetCheckboxViewModel
                {
                    PredmetId = p.Id,
                    Naziv = p.Naziv,
                    IsSelected = odabraniPredmetIds.Contains(p.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost("BirajIzbornePredmete")]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BirajIzbornePredmete(BirajIzbornePredmeteViewModel model)
        {
            _logger.LogInformation("POST BirajIzbornePredmete pozvan.");
            _logger.LogInformation("SelectedPredmetiIds count: {Count}", model.SelectedPredmetiIds?.Count ?? 0);

            if (model.SelectedPredmetiIds.Count < model.MinIzborniPredmeti)
            {
                TempData["ErrorMessage"] = $"Morate odabrati najmanje {model.MinIzborniPredmeti} izbornih predmeta.";
                return RedirectToAction("BirajIzbornePredmete");
            }

            if (model.SelectedPredmetiIds.Count > model.MaxIzborniPredmeti)
            {
                TempData["ErrorMessage"] = $"Možete odabrati najviše {model.MaxIzborniPredmeti} izbornih predmeta.";
                return RedirectToAction("BirajIzbornePredmete");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);
            if (student == null)
            {
                return NotFound();
            }

            if (student.IzborIzbornihPredmetaZakljucan)
            {
                TempData["ErrorMessage"] = "Vaš izbor izbornih predmeta je zaključan i ne može se mijenjati.";
                return RedirectToAction("BirajIzbornePredmete");
            }

            var stariIzborniPredmeti = await _context.StudentiNaPredmetima
                .Where(snp => snp.StudentId == student.Id && snp.Predmet.TipPredmeta == TipPredmeta.Izborni)
                .ToListAsync();

            _context.StudentiNaPredmetima.RemoveRange(stariIzborniPredmeti);

            if (model.SelectedPredmetiIds != null && model.SelectedPredmetiIds.Any())
            {
                foreach (var predmetId in model.SelectedPredmetiIds)
                {
                    _context.StudentiNaPredmetima.Add(new StudentNaPredmetu
                    {
                        StudentId = student.Id,
                        PredmetId = predmetId,
                        AkademskaGodina = DateTime.Now.Year.ToString(),
                        AspNetUserId = student.AspNetUserId
                    });
                }
            }

            // Zaključavamo SAMO AKO je odabrao tačno Max broj
            if (model.SelectedPredmetiIds.Count == model.MaxIzborniPredmeti)
            {
                student.IzborIzbornihPredmetaZakljucan = true;
                TempData["SuccessMessage"] = "Odabrali ste maksimalan broj izbornih predmeta. Vaš izbor je sada zaključan.";
            }
            else
            {
                TempData["SuccessMessage"] = "Vaš izbor je sačuvan. Možete naknadno dodati još izbornih predmeta do maksimalnog limita.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("BirajIzbornePredmete");
        }

        [HttpPost("ZakljucajIzborIzbornihPredmeta")]
        [Authorize(Roles = "Studentska služba")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ZakljucajIzborIzbornihPredmeta(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            student.IzborIzbornihPredmetaZakljucan = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Izbor izbornih predmeta je uspješno zaključan.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost("OtkjucajIzborIzbornihPredmeta")]
        [Authorize(Roles = "Studentska služba")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OtkjucajIzborIzbornihPredmeta(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            student.IzborIzbornihPredmetaZakljucan = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Izbor izbornih predmeta je uspješno otključan.";
            return RedirectToAction("Details", new { id });
        }
    }
}