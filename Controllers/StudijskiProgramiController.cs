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
    [Route("StudijskiProgrami")]
    public class StudijskiProgramiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudijskiProgramiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudijskiProgrami
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.StudijskiProgrami.ToListAsync());
        }

        // GET: StudijskiProgrami/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studijskiProgram = await _context.StudijskiProgrami
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studijskiProgram == null)
            {
                return NotFound();
            }

            return View(studijskiProgram);
        }

        // GET: StudijskiProgrami/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: StudijskiProgrami/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Naziv,Opis,trajanjeUGodinama")] StudijskiProgram studijskiProgram)
        {
            if (ModelState.IsValid)
            {
                _context.Add(studijskiProgram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(studijskiProgram);
        }

        // GET: StudijskiProgrami/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studijskiProgram = await _context.StudijskiProgrami.FindAsync(id);
            if (studijskiProgram == null)
            {
                return NotFound();
            }
            return View(studijskiProgram);
        }

        // POST: StudijskiProgrami/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Opis,trajanjeUGodinama")] StudijskiProgram studijskiProgram)
        {
            if (id != studijskiProgram.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(studijskiProgram);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudijskiProgramExists(studijskiProgram.Id))
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
            return View(studijskiProgram);
        }

        // GET: StudijskiProgrami/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studijskiProgram = await _context.StudijskiProgrami
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studijskiProgram == null)
            {
                return NotFound();
            }

            return View(studijskiProgram);
        }

        // POST: StudijskiProgrami/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var studijskiProgram = await _context.StudijskiProgrami.FindAsync(id);
            if (studijskiProgram != null)
            {
                _context.StudijskiProgrami.Remove(studijskiProgram);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudijskiProgramExists(long id)
        {
            return _context.StudijskiProgrami.Any(e => e.Id == id);
        }
    }
}
