using SchoolDatabaseCTwo.Models;
using Microsoft.AspNetCore.Mvc;


namespace SchoolDatabaseCTwo.Controllers
{

  public class CoursePageController : Controller
  {
    private readonly CourseAPIController _courseApiController;

    public CoursePageController(CourseAPIController courseApiController)
    {
      _courseApiController = courseApiController;
    }


    // GET: /CoursePage/List

    public IActionResult List()
    {
      IEnumerable<Course> courses = _courseApiController.ListCourses();
      return View(courses);
    }


    
    /// GET /CoursePage/Show/7

    public IActionResult Show(int id)
    {
      var course = _courseApiController.FindCourse(id);
      if (course == null)
      {
        ViewBag.Message = "Course not found.";
        return View("NotFound");
      }

      // Fetch students for the course
      var studentsResult = _courseApiController.GetStudentsByCourse(id);
      var students = (studentsResult as OkObjectResult)?.Value as List<Student>;

      ViewBag.Students = students ?? new List<Student>();
      return View(course);

    }




    /// GET: /CoursePage/Add

    [HttpGet]
    public IActionResult Add()
    {
      return View();
    }



    /// POST: /CoursePage/Add

    [HttpPost]
    public IActionResult Add(Course course)
    {
      // Validate required fields
      if (string.IsNullOrWhiteSpace(course.CourseCode) || string.IsNullOrWhiteSpace(course.CourseName) || course.StartDate == default || course.FinishDate == default || course.TeacherId <= 0)
      {
        ModelState.AddModelError("", "All fields are required.");
      }

      // Check if Course Code is unique
      var existingCourse = _courseApiController.ListCourses().FirstOrDefault(c => c.CourseCode == course.CourseCode);
      if (existingCourse != null)
      {
        ModelState.AddModelError("", "Course Code already exists.");
      }

      if (!ModelState.IsValid)
      {
        return View(course);
      }

      _courseApiController.AddCourse(course);
      TempData["SuccessMessage"] = "Course added successfully!";

      return RedirectToAction("List");

    }



    /// GET: /CoursePage/Update/10

    [HttpGet]
    public IActionResult Update(int id)
    {
      Course course = _courseApiController.FindCourse(id);
      if (course == null) return NotFound();
      return View(course);
    }



    /// POST: /CoursePage/Update

    [HttpPost]
    public IActionResult Update(Course course)
    {
      // Validate required fields
      if (string.IsNullOrWhiteSpace(course.CourseCode) || string.IsNullOrWhiteSpace(course.CourseName) || course.StartDate == default || course.FinishDate == default || course.TeacherId <= 0)
      {
        ModelState.AddModelError("", "All fields are required.");
      }

      // Check for unique Course Code (excluding the current course being updated)
      var existingCourse = _courseApiController.ListCourses().FirstOrDefault(c => c.CourseCode == course.CourseCode && c.CourseId != course.CourseId);
      if (existingCourse != null)
      {
        ModelState.AddModelError("", "Course Code already exists.");
      }

      if (!ModelState.IsValid)
      {
        return View(course);
      }

      _courseApiController.UpdateCourse(course);
      TempData["SuccessMessage"] = "Course updated successfully!";

      return RedirectToAction("List");
    }


    /// GET: /CoursePage/Delete/10

    [HttpGet]
    public IActionResult Delete(int id)
    {
      Course course = _courseApiController.FindCourse(id);
      if (course == null) return NotFound();
      return View(course);
    }


  
    /// POST: /CoursePage/DeleteConfirmed/10

    [HttpPost]
    public IActionResult DeleteConfirmed(int id)
    {
      _courseApiController.DeleteCourse(id);
      TempData["SuccessMessage"] = "Course deleted successfully!";

      return RedirectToAction("List");
    }

  }
}