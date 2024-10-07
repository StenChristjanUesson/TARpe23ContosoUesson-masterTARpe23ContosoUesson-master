using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : Controller
    {
        private readonly SchoolContext _context;
        public CoursesController(SchoolContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var schoolContext = _context.Courses.Include(d => d.CourseID);
            return View(await schoolContext.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> DetailsDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.CourseID == id);
            
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }
        [HttpPost, ActionName("DetailsDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Course = await _context.Courses.FindAsync(id);

            _context.Courses.Remove(Course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Clone(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var clonedCourse = await _context.Courses
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (clonedCourse == null)
            {
                return NotFound();
            }
            int lastID = _context.Courses.OrderBy(u => u.CourseID).Last().CourseID;
            lastID++;
            var selectedCourse = new Course();
            selectedCourse.Title = clonedCourse.Title;
            selectedCourse.Credits = clonedCourse.Credits;
            selectedCourse.Enrollments = clonedCourse.Enrollments;
            _context.Courses.Add(selectedCourse);
            await _context.SaveChangesAsync(true);
            return RedirectToAction(nameof(Index));
        }
    }
}
