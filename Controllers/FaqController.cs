using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Route("FAQ")]
    public class FaqController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<FaqController> _logger;

        private static List<string> DefaultKategorije = new List<string>
        {
            "Upis/Prijava",
            "Ispiti",
            "Nastava i predmeti",
            "Tehnička podrška",
            "Sistem i nalog",
            "Studentska služba",
            "Raspored i kalendar",
            "Plaćanje i školarine",
            "Prava i obaveze studenata",
            "Ostalo"
        };

        public FaqController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<FaqController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: FAQ
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var pitanja = await _context.FaqPitanja
                .OrderBy(p => p.Kategorija)
                .ThenBy(p => p.Pitanje)
                .ToListAsync();
            return View(pitanja);
        }

        private List<string> GetKategorije()
        {
            var izBaze = _context.FaqPitanja
                .Select(f => f.Kategorija)
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .ToList();

            return DefaultKategorije
                .Union(izBaze)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        // GET: FAQ/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            var viewModel = new FaqPitanjeViewModel
            {
                SveKategorije = GetKategorije()
            };
            return View(viewModel);
        }

        // POST: FAQ/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create(FaqPitanjeViewModel model, string NovaKategorija)
        {
            // Očisti helper polja iz ModelState, da ne smetaju validaciji
            ModelState.Remove("SveKategorije");
            ModelState.Remove("NovaKategorija");

            // Trimuj sve tekstualne unose radi sigurnosti
            model.Kategorija = (model.Kategorija ?? string.Empty).Trim();
            model.Pitanje = (model.Pitanje ?? string.Empty).Trim();
            model.Odgovor = (model.Odgovor ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(NovaKategorija))
                model.Kategorija = NovaKategorija.Trim();

            // Server-side validacija kategorije
            if (string.IsNullOrWhiteSpace(model.Kategorija))
            {
                ModelState.AddModelError("Kategorija", "Kategorija je obavezna.");
            }
            else if (model.Kategorija.Length < 3)
            {
                ModelState.AddModelError("Kategorija", "Kategorija mora imati najmanje 3 znaka.");
            }
            else if (model.Kategorija.Length > 100)
            {
                ModelState.AddModelError("Kategorija", "Kategorija ne može imati više od 100 znakova.");
            }

            // Validacija pitanja
            if (string.IsNullOrWhiteSpace(model.Pitanje))
            {
                ModelState.AddModelError("Pitanje", "Pitanje je obavezno.");
            }
            else if (model.Pitanje.Length < 3)
            {
                ModelState.AddModelError("Pitanje", "Pitanje mora imati najmanje 3 znaka.");
            }
            else if (model.Pitanje.Length > 300)
            {
                ModelState.AddModelError("Pitanje", "Pitanje ne može imati više od 300 znakova.");
            }

            // Validacija odgovora
            if (string.IsNullOrWhiteSpace(model.Odgovor))
            {
                ModelState.AddModelError("Odgovor", "Odgovor je obavezan.");
            }
            else if (model.Odgovor.Length < 5)
            {
                ModelState.AddModelError("Odgovor", "Odgovor mora imati najmanje 5 znakova.");
            }
            else if (model.Odgovor.Length > 2000)
            {
                ModelState.AddModelError("Odgovor", "Odgovor ne može imati više od 2000 znakova.");
            }

            // Provjera duplikata (pitanje + kategorija)
            bool duplikat = await _context.FaqPitanja
                .AnyAsync(f =>
                    f.Kategorija.ToLower() == model.Kategorija.ToLower() &&
                    f.Pitanje.ToLower() == model.Pitanje.ToLower());

            if (duplikat)
            {
                ModelState.AddModelError("", "Pitanje sa istom kategorijom već postoji u bazi.");
                _logger.LogWarning("Duplikat pri kreiranju FAQ: {Kategorija} | {Pitanje}", model.Kategorija, model.Pitanje);
            }

            // VALIDACIJA: Najviše 3 preporučena pitanja
            if (model.Preporuceno)
            {
                int brojPreporucenih = await _context.FaqPitanja.CountAsync(f => f.Preporuceno);
                if (brojPreporucenih >= 3)
                {
                    ModelState.AddModelError("Preporuceno", "Moguće je preporučiti najviše 3 pitanja.");
                }
            }

            // Loguj i vrati greške ako ih ima
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key]?.Errors;
                    if (errors != null && errors.Count > 0)
                        foreach (var error in errors)
                            _logger.LogWarning("ModelState error for key '{Key}': {ErrorMessage}", key, error.ErrorMessage);
                }
                model.SveKategorije = GetKategorije();
                return View(model);
            }

            // Dodaj novo pitanje
            var faq = new FaqPitanje
            {
                Kategorija = model.Kategorija,
                Pitanje = model.Pitanje,
                Odgovor = model.Odgovor,
                Preporuceno = model.Preporuceno
            };
            _context.FaqPitanja.Add(faq);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Pitanje je uspješno dodano!";
            return RedirectToAction(nameof(Index));
        }

        // GET: FAQ/Edit/{id}
        [HttpGet("Edit/{id:int}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(int id)
        {
            var pitanje = await _context.FaqPitanja.FindAsync(id);
            if (pitanje == null)
                return NotFound();

            var model = new FaqPitanjeViewModel
            {
                Id = pitanje.Id,
                Kategorija = pitanje.Kategorija,
                Pitanje = pitanje.Pitanje,
                Odgovor = pitanje.Odgovor,
                Preporuceno = pitanje.Preporuceno,
                SveKategorije = GetKategorije()
            };
            return View(model);
        }

        // POST: FAQ/Edit/{id}
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(int id, FaqPitanjeViewModel model, string NovaKategorija)
        {
            // Očisti helper polja iz ModelState, da ne smetaju validaciji
            ModelState.Remove("SveKategorije");
            ModelState.Remove("NovaKategorija");

            if (id != model.Id)
                return BadRequest();

            // Trimuj sve tekstualne unose radi sigurnosti
            model.Kategorija = (model.Kategorija ?? string.Empty).Trim();
            model.Pitanje = (model.Pitanje ?? string.Empty).Trim();
            model.Odgovor = (model.Odgovor ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(NovaKategorija))
                model.Kategorija = NovaKategorija.Trim();

            // Server-side validacija kategorije
            if (string.IsNullOrWhiteSpace(model.Kategorija))
            {
                ModelState.AddModelError("Kategorija", "Kategorija je obavezna.");
            }
            else if (model.Kategorija.Length < 3)
            {
                ModelState.AddModelError("Kategorija", "Kategorija mora imati najmanje 3 znaka.");
            }
            else if (model.Kategorija.Length > 100)
            {
                ModelState.AddModelError("Kategorija", "Kategorija ne može imati više od 100 znakova.");
            }

            // Validacija pitanja
            if (string.IsNullOrWhiteSpace(model.Pitanje))
            {
                ModelState.AddModelError("Pitanje", "Pitanje je obavezno.");
            }
            else if (model.Pitanje.Length < 3)
            {
                ModelState.AddModelError("Pitanje", "Pitanje mora imati najmanje 3 znaka.");
            }
            else if (model.Pitanje.Length > 300)
            {
                ModelState.AddModelError("Pitanje", "Pitanje ne može imati više od 300 znakova.");
            }

            // Validacija odgovora
            if (string.IsNullOrWhiteSpace(model.Odgovor))
            {
                ModelState.AddModelError("Odgovor", "Odgovor je obavezan.");
            }
            else if (model.Odgovor.Length < 5)
            {
                ModelState.AddModelError("Odgovor", "Odgovor mora imati najmanje 5 znakova.");
            }
            else if (model.Odgovor.Length > 2000)
            {
                ModelState.AddModelError("Odgovor", "Odgovor ne može imati više od 2000 znakova.");
            }

            // Provjera duplikata (pitanje + kategorija, osim ovog koji editiraš)
            bool duplikat = await _context.FaqPitanja
                .AnyAsync(f =>
                    f.Id != id &&
                    f.Kategorija.ToLower() == model.Kategorija.ToLower() &&
                    f.Pitanje.ToLower() == model.Pitanje.ToLower());

            if (duplikat)
            {
                ModelState.AddModelError("", "Pitanje sa istom kategorijom već postoji u bazi.");
                _logger.LogWarning("Duplikat pri editovanju FAQ: {Kategorija} | {Pitanje}", model.Kategorija, model.Pitanje);
            }

            // Validacija: najviše 3 preporučena pitanja
            if (model.Preporuceno)
            {
                int brojPreporucenih = await _context.FaqPitanja.CountAsync(f => f.Preporuceno && f.Id != id);
                if (brojPreporucenih >= 3)
                {
                    ModelState.AddModelError("Preporuceno", "Moguće je preporučiti najviše 3 pitanja.");
                }
            }

            // Loguj i vrati greške ako ih ima
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key]?.Errors;
                    if (errors != null && errors.Count > 0)
                        foreach (var error in errors)
                            _logger.LogWarning("ModelState error for key '{Key}': {ErrorMessage}", key, error.ErrorMessage);
                }
                model.SveKategorije = GetKategorije();
                return View(model);
            }

            // Sačuvaj izmjene
            var pitanje = await _context.FaqPitanja.FindAsync(id);
            if (pitanje == null)
                return NotFound();

            pitanje.Kategorija = model.Kategorija;
            pitanje.Pitanje = model.Pitanje;
            pitanje.Odgovor = model.Odgovor;
            pitanje.Preporuceno = model.Preporuceno;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Pitanje je uspješno izmijenjeno!";
            return RedirectToAction(nameof(Index));
        }

        // GET: FAQ/Delete/{id}
        [HttpGet("Delete/{id:int}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(int id)
        {
            var pitanje = await _context.FaqPitanja.FindAsync(id);
            if (pitanje == null)
                return NotFound();
            return View(pitanje);
        }

        // POST: FAQ/Delete/{id}
        [HttpPost("Delete/{id:int}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pitanje = await _context.FaqPitanja.FindAsync(id);
            if (pitanje == null)
                return NotFound();

            _context.FaqPitanja.Remove(pitanje);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Pitanje je uspješno obrisano!";
            return RedirectToAction(nameof(Index));
        }
    }
}
