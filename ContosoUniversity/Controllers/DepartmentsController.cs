using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly SchoolContext _context;
        public DepartmentsController(SchoolContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schoolContext = _context.Departments.Include(d => d.Administrator);
            return View(await schoolContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string query = "SELECT * FROM Departments WHERE DepartmentID = {0}";
            var department = await _context.Departments
                .FromSqlRaw(query, id)
                .Include(d => d.Administrator)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (department == null) 
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "FullName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Budget,StartTime,RowVersion,Instructor,Personality")] Department department)
        {
            if (!ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "Fullname", department.InstructorID);
            return View(department);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var DepartmentToEdit = await _context.Departments
                .FirstOrDefaultAsync(m => m.DepartmentID == id);
            if (DepartmentToEdit == null)
            {
                return NotFound();
            }
            return View(DepartmentToEdit);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Name,Budget,StartTime,RowVersion,Instructor,Personality")] Department modifiedDepartment)
        {
            if (ModelState.IsValid)
            {
                if (modifiedDepartment.DepartmentID == null)
                {
                    return BadRequest();
                }
                _context.Departments.Update(modifiedDepartment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "Fullname", modifiedDepartment.InstructorID);
            return View(modifiedDepartment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.DepartmentID == id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id); 
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult>BaseOn(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var DepartmentToBaseOn = await _context.Departments
                .FirstOrDefaultAsync(m => m.DepartmentID == id);
            if (DepartmentToBaseOn == null)
            {
                return NotFound();
            }
            return View(DepartmentToBaseOn);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BaseOn([Bind("Name,Budget,StartTime,RowVersion,Instructor,Personality")] Department BasedOnDepartment)
        {
            if (ModelState.IsValid)
            {
                if (BasedOnDepartment.DepartmentID == null)
                {
                    return BadRequest();
                }
                int lastID = _context.Departments.OrderBy(u => u.DepartmentID).Last().DepartmentID;
                lastID++;
                var selectedDepartment = new Department();
                selectedDepartment.Name = BasedOnDepartment.Name;
                selectedDepartment.Budget = BasedOnDepartment.Budget;
                selectedDepartment.StartTime = BasedOnDepartment.StartTime;
                selectedDepartment.InstructorID = BasedOnDepartment.InstructorID;
                selectedDepartment.Personality = BasedOnDepartment.Personality;
                _context.Departments.Add(selectedDepartment);
                await _context.SaveChangesAsync(true);
                ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "Fullname", BasedOnDepartment.InstructorID);
                return RedirectToAction("Index");
            }
            return View(BasedOnDepartment);
        }
        //public async Task<IActionResult> Clone(int? id)
        //{
        //    int lastID = _context.Students.OrderBy(u => u.ID).Last().ID;
        //    lastID++;
        //    var selectedStudent = new Student();
        //    selectedStudent.FirstMidName = clonedStudent.FirstMidName;
        //    selectedStudent.LastName = clonedStudent.LastName;
        //    selectedStudent.EnrollmentDate = clonedStudent.EnrollmentDate;
        //    _context.Students.Add(selectedStudent);
        //    await _context.SaveChangesAsync(true);
        //    return RedirectToAction("Index");
        //}
    }
}
