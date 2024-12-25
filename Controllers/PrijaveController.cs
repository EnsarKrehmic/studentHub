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
    [Route("Prijave")]
    public class PrijaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrijaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Prijave
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Prijave.Include(p => p.Ispit).Include(p => p.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Prijave/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prijava = await _context.Prijave
                .Include(p => p.Ispit)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prijava == null)
            {
                return NotFound();
            }

            return View(prijava);
        }

        // GET: Prijave/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Id");
            ViewData["StudentId"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa");
            return View();
        }

        // POST: Prijave/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,datumPrijave,IspitId,StudentId")] Prijava prijava)
        {
            if (ModelState.IsValid)
            {
                _context.Add(prijava);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Id", prijava.IspitId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa", prijava.StudentId);
            return View(prijava);
        }

        // GET: Prijave/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prijava = await _context.Prijave.FindAsync(id);
            if (prijava == null)
            {
                return NotFound();
            }
            ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Id", prijava.IspitId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa", prijava.StudentId);
            return View(prijava);
        }

        // POST: Prijave/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,datumPrijave,IspitId,StudentId")] Prijava prijava)
        {
            if (id != prijava.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prijava);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrijavaExists(prijava.Id))
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
            ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Id", prijava.IspitId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa", prijava.StudentId);
            return View(prijava);
        }

        // GET: Prijave/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prijava = await _context.Prijave
                .Include(p => p.Ispit)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prijava == null)
            {
                return NotFound();
            }

            return View(prijava);
        }

        // POST: Prijave/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var prijava = await _context.Prijave.FindAsync(id);
            if (prijava != null)
            {
                _context.Prijave.Remove(prijava);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrijavaExists(long id)
        {
            return _context.Prijave.Any(e => e.Id == id);
        }
    }
}
