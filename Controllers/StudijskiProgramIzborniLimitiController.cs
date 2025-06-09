using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Authorize(Roles = "Studentska služba")]
    public class StudijskiProgramIzborniLimitiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudijskiProgramIzborniLimitiController> _logger;

        public StudijskiProgramIzborniLimitiController(ApplicationDbContext context, ILogger<StudijskiProgramIzborniLimitiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: StudijskiProgramIzborniLimiti
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Prikaz svih limita izbornih predmeta.");
            var applicationDbContext = _context.StudijskiProgramIzborniLimiti.Include(s => s.StudijskiProgram);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: StudijskiProgramIzborniLimiti/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studijskiProgramIzborniLimit = await _context.StudijskiProgramIzborniLimiti
                .Include(s => s.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studijskiProgramIzborniLimit == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Prikaz detalja limita ID={Id}", id);

            return View(studijskiProgramIzborniLimit);
        }

        // GET: StudijskiProgramIzborniLimiti/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Otvaranje forme za dodavanje novog limita.");
            ViewData["StudijskiProgramId"] = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            return View();
        }

        // POST: StudijskiProgramIzborniLimiti/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudijskiProgramId,GodinaStudija,MinIzborniPredmeti,MaxIzborniPredmeti")] StudijskiProgramIzborniLimit studijskiProgramIzborniLimit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(studijskiProgramIzborniLimit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Uspješno kreiran novi limit za StudijskiProgramId={ProgramId}, GodinaStudija={Godina}",
                    studijskiProgramIzborniLimit.StudijskiProgramId, studijskiProgramIzborniLimit.GodinaStudija);

                return RedirectToAction(nameof(Index));
            }

            // Ako ModelState nije validan → ispiši greške
            foreach (var modelStateKey in ModelState.Keys)
            {
                var value = ModelState[modelStateKey];
                if (value.Errors.Count > 0)
                {
                    _logger.LogWarning("Greška u polju: {Field}", modelStateKey);
                    foreach (var error in value.Errors)
                    {
                        _logger.LogWarning(" - Error: {ErrorMessage}", error.ErrorMessage);
                    }
                }
            }

            ViewData["StudijskiProgramId"] = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramIzborniLimit.StudijskiProgramId);
            return View(studijskiProgramIzborniLimit);
        }

        // GET: StudijskiProgramIzborniLimiti/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studijskiProgramIzborniLimit = await _context.StudijskiProgramIzborniLimiti.FindAsync(id);
            if (studijskiProgramIzborniLimit == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Otvaranje forme za uređivanje limita ID={Id}", id);

            ViewData["StudijskiProgramId"] = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramIzborniLimit.StudijskiProgramId);
            return View(studijskiProgramIzborniLimit);
        }

        // POST: StudijskiProgramIzborniLimiti/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,StudijskiProgramId,GodinaStudija,MinIzborniPredmeti,MaxIzborniPredmeti")] StudijskiProgramIzborniLimit studijskiProgramIzborniLimit)
        {
            if (id != studijskiProgramIzborniLimit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(studijskiProgramIzborniLimit);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Uspješno ažuriran limit ID={Id}", id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudijskiProgramIzborniLimitExists(studijskiProgramIzborniLimit.Id))
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

            ViewData["StudijskiProgramId"] = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramIzborniLimit.StudijskiProgramId);
            return View(studijskiProgramIzborniLimit);
        }

        // GET: StudijskiProgramIzborniLimiti/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studijskiProgramIzborniLimit = await _context.StudijskiProgramIzborniLimiti
                .Include(s => s.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studijskiProgramIzborniLimit == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Otvaranje forme za brisanje limita ID={Id}", id);

            return View(studijskiProgramIzborniLimit);
        }

        // POST: StudijskiProgramIzborniLimiti/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var studijskiProgramIzborniLimit = await _context.StudijskiProgramIzborniLimiti.FindAsync(id);
            if (studijskiProgramIzborniLimit != null)
            {
                _context.StudijskiProgramIzborniLimiti.Remove(studijskiProgramIzborniLimit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Limit ID={Id} uspješno obrisan.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudijskiProgramIzborniLimitExists(long id)
        {
            return _context.StudijskiProgramIzborniLimiti.Any(e => e.Id == id);
        }
    }
}
