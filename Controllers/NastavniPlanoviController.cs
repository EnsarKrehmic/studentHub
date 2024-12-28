using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("NastavniPlanovi")]
    public class NastavniPlanoviController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NastavniPlanoviController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NastavniPlanovi
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.NastavniPlanovi
                .Include(n => n.StudijskiProgram);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: NastavniPlanovi/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound();

            var nastavniPlan = await _context.NastavniPlanovi
                .Include(n => n.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nastavniPlan == null)
                return NotFound();

            return View(nastavniPlan);
        }

        // GET: NastavniPlanovi/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["StudijskiProgramId"] = new SelectList(_context.StudijskiProgrami, "Id", "Id");
            return View();
        }

        // POST: NastavniPlanovi/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,godinaStudija,StudijskiProgramId")] NastavniPlan nastavniPlan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nastavniPlan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudijskiProgramId"] = new SelectList(_context.StudijskiProgrami, "Id", "Id", nastavniPlan.StudijskiProgramId);
            return View(nastavniPlan);
        }

        // GET: NastavniPlanovi/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();

            var nastavniPlan = await _context.NastavniPlanovi.FindAsync(id);
            if (nastavniPlan == null)
                return NotFound();

            ViewData["StudijskiProgramId"] = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", nastavniPlan.StudijskiProgramId);
            return View(nastavniPlan);
        }

        // POST: NastavniPlanovi/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,godinaStudija,StudijskiProgramId")] NastavniPlan nastavniPlan)
        {
            if (id != nastavniPlan.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nastavniPlan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NastavniPlanExists(nastavniPlan.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudijskiProgramId"] = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", nastavniPlan.StudijskiProgramId);
            return View(nastavniPlan);
        }

        // GET: NastavniPlanovi/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            var nastavniPlan = await _context.NastavniPlanovi
                .Include(n => n.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nastavniPlan == null)
                return NotFound();

            return View(nastavniPlan);
        }

        // POST: NastavniPlanovi/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var nastavniPlan = await _context.NastavniPlanovi.FindAsync(id);
            if (nastavniPlan != null)
            {
                _context.NastavniPlanovi.Remove(nastavniPlan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NastavniPlanExists(long id)
        {
            return _context.NastavniPlanovi.Any(e => e.Id == id);
        }
    }
}