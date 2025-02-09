using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;

namespace StudentHub.Models
{
    public class StudijskiProgramiMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public StudijskiProgramiMenuViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var studijskiProgrami = await _context.StudijskiProgrami
                .OrderBy(sp => sp.Naziv)
                .ToListAsync();

            return View(studijskiProgrami);
        }
    }
}
