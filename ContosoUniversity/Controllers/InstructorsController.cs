using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly SchoolContext _context;

        public InstructorsController(SchoolContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int? id, int? courseId)
        {
            var vm = new InstructorIndexData();
            vm.Instructors = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                .ThenInclude(i => i.Course)
                .ThenInclude(i => i.Enrollments)
                .ThenInclude(i => i.Student)
                .Include(i => i.CourseAssignments)
                .ThenInclude(i => i.Course)
                .AsNoTracking()
                .OrderBy(i => i.LastName)
                .ToListAsync();

            if (id != null)
            {
                ViewData["InstructorID"] = id.Value;
                Instructor instructor = vm.Instructors
                    .Where(i => i.ID == id.Value).Single();
                vm.Courses = instructor.CourseAssignments
                    .Select(i => i.Course);
            }
            if (courseId != null)
            {
                ViewData["CourseID"] = courseId.Value;
                vm.Enrollments = vm.Courses
                    .Where(x => x.CourseID == courseId)
                    .Single()
                    .Enrollments;
            }

            return View(vm);

        }

        [HttpGet]
        public IActionResult Create()
        {
            var instructor = new Instructor();
            instructor.CourseAssignments = new List<CourseAssignment>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instructor instructor /*string? selectedCourses*/)
        {
            //if (selectedCourses == null)
            //{
            //    instructor.CourseAssignments = new List<CourseAssignment>();
            //    foreach (var course in selectedCourses)
            //    {
            //        var courseToAdd = new CourseAssignment
            //        {
            //            InstructorID = instructor.ID,
            //            CourseID = course
            //        };
            //        instructor.CourseAssignments.Add(courseToAdd);
            //    }
            //}
            //ModelState.Remove();
            //ModelState.Remove(selectedCourses);
            if (ModelState.IsValid) 
            {
                _context.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            //PopulateAssignedCourseData(instructor); //uuendab instructori juures olevaid kursuseid
            return View(instructor);
        }

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = _context.Courses; //leiame kõik kursused
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments.Select(c => c.CourseID));
            //valime kursused kus courseid on õpetajal olemas
            var vm = new List<AssignedCourseData>(); //teeme viewmodeli jaoks uue nimekirja
            foreach (var course in allCourses) 
            {
                vm.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewData["Courses"] = vm;
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) //kui id on tühi/null, siis Instructor ei leita
            {
                return NotFound();
            }

            var instructor = await _context.Instructors // tehakse Instructori objekt andmebaasis oleva id järgi
                .FirstOrDefaultAsync(m => m.ID == id);

            if (instructor == null) //kui Instructior objekt on tühi/null, siis ka Instructor ei leita
            {
                return NotFound();
            }

            return View(instructor);
        }
        /// <summary>
        /// Asünkroonne DeleteConfirmed meetod.
        /// Kustutab kaasaantud ID alusel ära Instructori andmebaasist ning tagastab kasutaja Index vaatesse.
        /// </summary>
        /// <param name="id">Kustutatava Instructor ID</param>
        /// <returns>Kustutab Instructor andmed andmebaasist ära ning tagastab kasutajale Index vaate</returns>
        //Delete POST meetod, teostab andmebaasis vajaliku muudatuse. ehk kustutab andme ära
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id); //otsime andmebaasist Instructor id järgi ja paneme ta "Instructor" nimelisse muutujasse.

            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Clone(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //var existingInstructor = Details(id);
            //return View(existingStudent);
            var clonedInstuctors = await _context.Instructors // tehakse Instructori objekt andmebaasis oleva id järgi
                .FirstOrDefaultAsync(m => m.ID == id);
            if (clonedInstuctors == null)
            {
                return NotFound();
            }
            int lastID = _context.Instructors.OrderBy(u => u.ID).Last().ID;
            lastID++;
            var selectedInstuctor = new Instructor();
            selectedInstuctor.FirstMidName = clonedInstuctors.FirstMidName;
            selectedInstuctor.LastName = clonedInstuctors.LastName;
            //selectedInstuctor.OfficeAssignment.Location = clonedInstuctors.OfficeAssignment.Location; See ei tööta kuna database intructoris pole database OfficeAssignmentsi Datad
            selectedInstuctor.HireDate = clonedInstuctors.HireDate;
            _context.Instructors.Add(selectedInstuctor);
            await _context.SaveChangesAsync(true);
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Asünkronne Edit GET meetod.
        /// Leiab andmebaasist päringus oleva id järgi õpilase
        /// ning tagastab vaate koos selle õpilase infoga
        /// kus selle õpilase infot muuta ja üle salvestada saab.
        /// </summary>
        /// <param name="id">Otsitava õpilase ID</param>
        /// <returns>Tagastab kasutajale vaate, koos õpilase muudetavate andmetega.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var InstructorToEdit = await _context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (InstructorToEdit == null)
            {
                return NotFound();
            }
            return View(InstructorToEdit);
        }
        /// <summary>
        /// Asünkroonne POST meetod, mis uuendab andmebaasis oleva Instructori, võttes selleks
        /// andmed vaatest "modifiedInstructor" nimelise objekti seest. Päringule on juurde binditud
        /// andmebaasi jaoks vajalikud andmeväljad.
        /// </summary>
        /// <param name="modifiedInstructor"></param>
        /// <returns>Tagastab kasutaja "Index" vaatesse koos nüüd muudetud Instructoriga</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,LastName,FirstMidName,Hiredate,Instructor")] Instructor modifiedInstructor)
        {
            if (ModelState.IsValid)
            {
                if (modifiedInstructor.ID == null)
                {
                    return BadRequest();
                }
                _context.Instructors.Update(modifiedInstructor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(modifiedInstructor);
        }
    }
}
