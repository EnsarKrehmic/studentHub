using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Route("Pravilnici")]
    [Authorize(Roles = "Studentska služba")]
    public class PravilniciController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PravilniciController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pravilnici
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var pravilnici = await _context.Pravilnici
                .Include(p => p.Clanovi)
                .OrderByDescending(p => p.DatumKreiranja)
                .ToListAsync();

            // Mapiraj u ViewModel
            var viewModels = pravilnici.Select(p => new PravilnikViewModel
            {
                Id = p.Id,
                Naslov = p.Naslov,
                Opis = p.Opis,
                Clanovi = p.Clanovi
                    .OrderBy(c => c.RedniBroj)
                    .Select(c => new PravilnikClanakViewModel
                    {
                        Id = c.Id,
                        NaslovClanka = c.NaslovClanka,
                        Sadrzaj = c.Sadrzaj,
                        RedniBroj = c.RedniBroj
                    }).ToList()
            }).ToList();

            return View(viewModels);
        }

        // GET: Pravilnici/Details/{id}
        [HttpGet("Details/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var pravilnik = await _context.Pravilnici
                .Include(p => p.Clanovi)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pravilnik == null)
                return NotFound();

            var viewModel = new PravilnikViewModel
            {
                Id = pravilnik.Id,
                Naslov = pravilnik.Naslov,
                Opis = pravilnik.Opis,
                Clanovi = pravilnik.Clanovi?
                    .OrderBy(c => c.RedniBroj)
                    .Select(c => new PravilnikClanakViewModel
                    {
                        Id = c.Id,
                        NaslovClanka = c.NaslovClanka,
                        Sadrzaj = c.Sadrzaj,
                        RedniBroj = c.RedniBroj
                    }).ToList() ?? new List<PravilnikClanakViewModel>()
            };

            return View(viewModel);
        }

        // GET: Pravilnici/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            var viewModel = new PravilnikViewModel();
            return View(viewModel);
        }

        // POST: Pravilnici/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PravilnikViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Naslov))
                ModelState.AddModelError("Naslov", "Naziv pravilnika je obavezan.");

            if (string.IsNullOrWhiteSpace(model.Opis))
                ModelState.AddModelError("Opis", "Opis pravilnika je obavezan.");

            if (model.Clanovi != null)
            {
                foreach (var cl in model.Clanovi)
                {
                    if (string.IsNullOrWhiteSpace(cl.NaslovClanka) || string.IsNullOrWhiteSpace(cl.Sadrzaj))
                    {
                        ModelState.AddModelError("", "Svi članci moraju imati naslov i sadržaj.");
                        break;
                    }
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            var pravilnik = new Pravilnik
            {
                Naslov = model.Naslov.Trim(),
                Opis = model.Opis?.Trim(),
                DatumKreiranja = DateTime.Now,
                Clanovi = model.Clanovi?.Select(cl => new PravilnikClanak
                {
                    NaslovClanka = cl.NaslovClanka.Trim(),
                    Sadrzaj = cl.Sadrzaj.Trim(),
                    RedniBroj = cl.RedniBroj
                }).ToList() ?? new List<PravilnikClanak>()
            };

            _context.Pravilnici.Add(pravilnik);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Pravilnik je uspješno kreiran!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Pravilnici/Edit/{id}
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var pravilnik = await _context.Pravilnici
                .Include(p => p.Clanovi)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pravilnik == null)
                return NotFound();

            var model = new PravilnikViewModel
            {
                Id = pravilnik.Id,
                Naslov = pravilnik.Naslov,
                Opis = pravilnik.Opis,
                Clanovi = pravilnik.Clanovi
                    .OrderBy(c => c.RedniBroj)
                    .Select(cl => new PravilnikClanakViewModel
                    {
                        Id = cl.Id,
                        NaslovClanka = cl.NaslovClanka,
                        Sadrzaj = cl.Sadrzaj,
                        RedniBroj = cl.RedniBroj
                    }).ToList()
            };

            return View(model);
        }

        // POST: Pravilnici/Edit/{id}
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PravilnikViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(model.Naslov))
                ModelState.AddModelError("Naslov", "Naziv pravilnika je obavezan.");

            if (string.IsNullOrWhiteSpace(model.Opis))
                ModelState.AddModelError("Opis", "Opis pravilnika je obavezan.");

            if (model.Clanovi != null)
            {
                foreach (var cl in model.Clanovi)
                {
                    if (string.IsNullOrWhiteSpace(cl.NaslovClanka) || string.IsNullOrWhiteSpace(cl.Sadrzaj))
                    {
                        ModelState.AddModelError("", "Svi članci moraju imati naslov i sadržaj.");
                        break;
                    }
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            var pravilnik = await _context.Pravilnici
                .Include(p => p.Clanovi)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pravilnik == null)
                return NotFound();

            pravilnik.Naslov = model.Naslov.Trim();
            pravilnik.Opis = model.Opis?.Trim();

            // Zamijeni sve članove za jednostavnost
            _context.PravilnikClanovi.RemoveRange(pravilnik.Clanovi);
            pravilnik.Clanovi = model.Clanovi?.Select(cl => new PravilnikClanak
            {
                NaslovClanka = cl.NaslovClanka.Trim(),
                Sadrzaj = cl.Sadrzaj.Trim(),
                RedniBroj = cl.RedniBroj,
                PravilnikId = pravilnik.Id
            }).ToList() ?? new List<PravilnikClanak>();

            await _context.SaveChangesAsync();
            TempData["Success"] = "Pravilnik je uspješno izmijenjen!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Pravilnici/Delete/{id}
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pravilnik = await _context.Pravilnici
                .Include(p => p.Clanovi)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pravilnik == null)
                return NotFound();

            var model = new PravilnikViewModel
            {
                Id = pravilnik.Id,
                Naslov = pravilnik.Naslov,
                Opis = pravilnik.Opis,
                Clanovi = pravilnik.Clanovi
                    .OrderBy(c => c.RedniBroj)
                    .Select(cl => new PravilnikClanakViewModel
                    {
                        Id = cl.Id,
                        NaslovClanka = cl.NaslovClanka,
                        Sadrzaj = cl.Sadrzaj,
                        RedniBroj = cl.RedniBroj
                    }).ToList()
            };

            return View(model);
        }

        // POST: Pravilnici/Delete/{id}
        [HttpPost("Delete/{id:int}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pravilnik = await _context.Pravilnici
                .Include(p => p.Clanovi)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pravilnik == null)
                return NotFound();

            _context.PravilnikClanovi.RemoveRange(pravilnik.Clanovi);
            _context.Pravilnici.Remove(pravilnik);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Pravilnik je uspješno obrisan!";
            return RedirectToAction(nameof(Index));
        }
    }
}
