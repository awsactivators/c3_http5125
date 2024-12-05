
using SchoolDatabaseCTwo.Models;
using Microsoft.AspNetCore.Mvc;

namespace SchoolDatabaseCTwo.Controllers
{
  public class StudentPageController : Controller
  {
    private readonly StudentAPIController _studentApiController;
    private readonly CourseAPIController _courseApiController;

    public StudentPageController(StudentAPIController studentApiController, CourseAPIController courseApiController)
    {
      _studentApiController = studentApiController;
      _courseApiController = courseApiController;
    }


    // GET: /StudentPage/List
    
    public IActionResult List()
    {
      IEnumerable<Student> students = _studentApiController.ListStudents();
      return View(students);
    }

   
    /// GET /StudentPage/Show/15

    public IActionResult Show(int id)
    {
      var student = _studentApiController.FindStudent(id);
      if (student == null)
      {
        ViewBag.Message = "Student not found.";
        return View("NotFound");
      }

      // Fetch courses for the student
      var coursesResult = _studentApiController.GetCoursesByStudent(id);
      var courses = (coursesResult as OkObjectResult)?.Value as List<Course>;

      ViewBag.Courses = courses ?? new List<Course>();
      return View(student);


    }


    /// GET: /StudentPage/Add

    [HttpGet]
    public IActionResult Add()
    {
      // Fetch the list of available courses
      var courses = _courseApiController.ListCourses();
      ViewBag.Courses = courses;

      return View();
    }




    /// POST: /StudentPage/Add

    [HttpPost]
    public IActionResult Add(Student student, List<int> selectedCourseIds)
    {
      if (string.IsNullOrWhiteSpace(student.StudentFname) || string.IsNullOrWhiteSpace(student.StudentLname))
      {
        ModelState.AddModelError("", "First and Last Name are required.");
      }


      if (string.IsNullOrWhiteSpace(student.StudentNumber) || !System.Text.RegularExpressions.Regex.IsMatch(student.StudentNumber, @"^N\d{4}$"))
      {
        ModelState.AddModelError("", "Student Number must follow the pattern N followed by 4 digits.");
      }


      if (student.EnrolDate == default || student.EnrolDate > DateTime.Now)
      {
        ModelState.AddModelError("", "Enrollment Date cannot be empty or in the future.");
      }


      var existingStudent = _studentApiController.ListStudents().FirstOrDefault(s => s.StudentNumber == student.StudentNumber);
      if (existingStudent != null)
      {
        ModelState.AddModelError("", "Student Number already exists.");
      }

      if (!ModelState.IsValid)
      {
        ViewBag.Courses = _courseApiController.ListCourses();
        return View(student);
      }


      if (selectedCourseIds != null && selectedCourseIds.Any())
      {
        student.Courses = selectedCourseIds
          .Select(courseId => _courseApiController.FindCourse(courseId))
          .Where(course => course != null)
          .ToList();
      }

      _studentApiController.AddStudent(student);
      TempData["SuccessMessage"] = "Student added successfully!";

      return RedirectToAction("List");

    }


    /// GET: /StudentPage/Update

    [HttpGet]
    public IActionResult Update(int id)
    {
      Student student = _studentApiController.FindStudent(id);
      if (student == null) return NotFound();
      return View(student);
    }


    /// POST: /StudentPage/Update

    [HttpPost]
    public IActionResult Update(Student student)
    {

      if (string.IsNullOrWhiteSpace(student.StudentFname) || string.IsNullOrWhiteSpace(student.StudentLname))
      {
        ModelState.AddModelError("", "First and Last Name are required.");
      }

      if (string.IsNullOrWhiteSpace(student.StudentNumber) || !System.Text.RegularExpressions.Regex.IsMatch(student.StudentNumber, @"^N\d{4}$"))
      {
        ModelState.AddModelError("", "Student Number must follow the pattern N followed by 4 digits.");
      }

      if (student.EnrolDate == default || student.EnrolDate > DateTime.Now)
      {
        ModelState.AddModelError("", "Enrollment Date cannot be empty or in the future.");
      }

      // Check for unique Student Number 
      var existingStudent = _studentApiController.ListStudents().FirstOrDefault(s => s.StudentNumber == student.StudentNumber && s.StudentId != student.StudentId);
      if (existingStudent != null)
      {
        ModelState.AddModelError("", "Student Number already exists.");
      }

      if (!ModelState.IsValid)
      {
        return View(student);
      }

      _studentApiController.UpdateStudent(student);
      TempData["SuccessMessage"] = "Student updated successfully!";

      return RedirectToAction("List");
    }



    /// GET: /StudentPage/Delete

    [HttpGet]
    public IActionResult Delete(int id)
    {
      Student student = _studentApiController.FindStudent(id);
      if (student == null) return NotFound();
      return View(student);
    }



    /// POST: /StudentPage/DeleteConfirmed

    [HttpPost]
    public IActionResult DeleteConfirmed(int id)
    {
      _studentApiController.DeleteStudent(id);
      TempData["SuccessMessage"] = "Student deleted successfully!";

      return RedirectToAction("List");
    }

  }
}
