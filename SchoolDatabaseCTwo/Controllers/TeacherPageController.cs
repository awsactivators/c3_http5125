using SchoolDatabaseCTwo.Models;
using Microsoft.AspNetCore.Mvc;

namespace SchoolDatabaseCTwo.Controllers
{
  public class TeacherPageController : Controller
  {
    private readonly TeacherAPIController _teacherApiController;
    private readonly CourseAPIController _courseApiController;

    public TeacherPageController(TeacherAPIController teacherApiController, CourseAPIController courseApiController)
    {
      _teacherApiController = teacherApiController;
      _courseApiController = courseApiController;
    }



    /// GET: /TeacherPage/List

    public IActionResult List(string searchKey = null, DateTime? startDate = null, DateTime? endDate = null)
    {
      var result = _teacherApiController.ListTeachers(searchKey, startDate, endDate);

      if (result is NotFoundObjectResult)
      {
        ViewBag.Message = "No teachers found.";
        return View(new List<Teacher>());
      }

      ViewBag.SuccessMessage = TempData["SuccessMessage"];

      var teachers = (result as OkObjectResult)?.Value as IEnumerable<Teacher> ?? new List<Teacher>();
      return View(teachers);
    }



    public IActionResult New() => View();


    /// GET: /TeacherPage/Ajax_New

    [HttpGet]
    public IActionResult Ajax_New()
    {
      return View();
    }




    /// GET: /TeacherPage/Show/{id}


    public IActionResult Show(int id)
    {
      var actionResult = _teacherApiController.FindTeacher(id);
      var okResult = actionResult as OkObjectResult;

      if (okResult == null || okResult.Value == null)
      {
        ViewBag.Message = "Teacher not found.";
        return View("NotFound");
      }

      var selectedTeacher = okResult.Value as Teacher;

      if (selectedTeacher != null)
      {
        selectedTeacher.Courses = _courseApiController.ListCoursesByTeacher(id);
      }

      ViewBag.SuccessMessage = TempData["SuccessMessage"];
      return View(selectedTeacher);
    }




    
    /// POST /TeacherPage/Create

    [HttpPost]
    public IActionResult Create(string teacherFname, string teacherLname, string employeeNumber, DateTime hireDate, decimal? salary, string teacherWorkPhone)
    {
    if (string.IsNullOrEmpty(teacherFname) || string.IsNullOrEmpty(teacherLname))
    {
      ViewBag.Message = "First and Last Name are required.";
      return View("New");
    }

    if (string.IsNullOrEmpty(employeeNumber) || !System.Text.RegularExpressions.Regex.IsMatch(employeeNumber, @"^T\d{3}$"))
    {
      ViewBag.Message = "Employee Number must start with 'T' followed by exactly 3 digits.";
      return View("New");
    }

    if (hireDate > DateTime.Now)
    {
      ViewBag.Message = "Hire Date cannot be in the future.";
      return View("New");
    }

    if (salary == null || salary < 0)
    {
        ViewBag.Message = "Salary must be a positive value.";
        return View("New");
    }

      var newTeacher = new Teacher
      {
        TeacherFname = teacherFname,
        TeacherLname = teacherLname,
        EmployeeNumber = employeeNumber,
        HireDate = hireDate,
        Salary = salary ?? 0,
        TeacherWorkPhone = teacherWorkPhone
      };

      var actionResult = _teacherApiController.AddTeacher(newTeacher);

      if (actionResult is BadRequestObjectResult)
      {
        ViewBag.Message = "Failed to create teacher.";
        return View("New");
      }


      TempData["SuccessMessage"] = "Teacher added successfully.";
      return RedirectToAction("List");
    }





    /// GET : /TeacherPage/DeleteConfirm/{id}

    public IActionResult DeleteConfirm(int id)
    {
      var result = _teacherApiController.FindTeacher(id);
      if (result is NotFoundObjectResult)
      {
        return View("NotFound");
      }

      var teacher = (result as OkObjectResult)?.Value as Teacher;
      TempData["SuccessMessage"] = "Teacher deleted successfully!";

      return View(teacher);
    }





    /// POST /TeacherPage/Delete/17

    [HttpPost]
    public IActionResult Delete(int id)
    {
      var result = _teacherApiController.DeleteTeacher(id);
      if (result is NotFoundObjectResult)
      {
        return View("NotFound");
      }

      TempData["SuccessMessage"] = "Teacher deleted successfully!";
      return RedirectToAction("List");
    }





    /// GET /TeacherPage/Update/1

    public IActionResult Update(int id)
    {
      var actionResult = _teacherApiController.FindTeacher(id);
      var selectedTeacher = (actionResult as OkObjectResult)?.Value as Teacher;

      if (selectedTeacher == null)
      {
        ViewBag.Message = "Teacher not found.";
        return View("NotFound");
      }

      TempData["SuccessMessage"] = "Teacher updated successfully!";
      return View(selectedTeacher);
    }



    
    /// POST /TeacherPage/Update/{id}

    [HttpPost]
    public IActionResult Update(int id, string teacherFname, string teacherLname, string employeeNumber, DateTime hireDate, decimal? salary, string teacherWorkPhone)
    {
    if (string.IsNullOrEmpty(teacherFname) || string.IsNullOrEmpty(teacherLname))
    {
      ViewBag.Message = "First and Last Name are required.";
      return View("Update", new Teacher { TeacherId = id, TeacherFname = teacherFname, TeacherLname = teacherLname });
    }

    if (string.IsNullOrEmpty(employeeNumber) || !System.Text.RegularExpressions.Regex.IsMatch(employeeNumber, @"^T\d{3}$"))
    {
      ViewBag.Message = "Employee Number must start with 'T' followed by exactly 3 digits.";
      return View("Update", new Teacher { TeacherId = id, TeacherFname = teacherFname, TeacherLname = teacherLname, EmployeeNumber = employeeNumber });
    }

    if (hireDate > DateTime.Now)
    {
      ViewBag.Message = "Hire Date cannot be in the future.";
      return View("Update", new Teacher { TeacherId = id, HireDate = hireDate });
    }

    if (salary == null || salary < 0)
    {
      ViewBag.Message = "Salary must be a positive value.";
      return View("Update", new Teacher { TeacherId = id, Salary = salary ?? 0 });
    }

      var teacherInfo = new Teacher
      {
        TeacherFname = teacherFname,
        TeacherLname = teacherLname,
        EmployeeNumber = employeeNumber,
        HireDate = hireDate,
        Salary = salary ?? 0,
        TeacherWorkPhone = teacherWorkPhone
      };

      var result = _teacherApiController.UpdateTeacher(id, teacherInfo);

      if (result is BadRequestObjectResult badRequest)
      {
        var errorResponse = badRequest.Value as dynamic;
        var errorMessage = errorResponse?.message ?? "Failed to update teacher.";
        ViewBag.Message = errorMessage;
        return View("Update", teacherInfo);
      }

      if (result is NotFoundObjectResult)
      {
        ViewBag.Message = "Teacher not found.";
        return View("Update", teacherInfo);
      }

      TempData["SuccessMessage"] = "Teacher updated successfully!";

      return RedirectToAction("Show", new { id });
    }





    
    /// GET /TeacherPage/Ajax_Update/{id}

    public IActionResult Ajax_Update(int id)
    {
      // Fetch the teacher details from the API
      var result = _teacherApiController.FindTeacher(id);
      if (result is NotFoundObjectResult)
      {
        return View("NotFound");
      }

      var teacher = (result as OkObjectResult)?.Value as Teacher;

      TempData["SuccessMessage"] = "Teacher updated successfully!";
      // Pass the teacher details to the view
      return View(teacher);
    }



    
     /// POST /TeacherPage/Ajax_Update/{id}

    [HttpPost]
    public JsonResult Ajax_Update([FromBody] Teacher updatedTeacher)
    {
      if (string.IsNullOrEmpty(updatedTeacher.TeacherFname) || 
          string.IsNullOrEmpty(updatedTeacher.TeacherLname) || 
          string.IsNullOrEmpty(updatedTeacher.EmployeeNumber) || 
          updatedTeacher.HireDate == null || 
          updatedTeacher.HireDate > DateTime.Now || 
          updatedTeacher.Salary < 0)
      {
        return Json(new { success = false, message = "Invalid input data." });
      }

      var result = _teacherApiController.UpdateTeacher(updatedTeacher.TeacherId, updatedTeacher);
      if (result is NotFoundObjectResult)
      {
        return Json(new { success = false, message = "Teacher not found." });
      }

      TempData["SuccessMessage"] = "Teacher updated successfully!";
      return Json(new { success = true, message = "Teacher updated successfully!" });
    }


    

    /// GET : /TeacherPage/Ajax_New

    [HttpPost]
    public JsonResult Ajax_Create(string teacherFname, string teacherLname, string employeeNumber, DateTime hireDate, decimal? salary)
    {
      if (string.IsNullOrEmpty(teacherFname) || string.IsNullOrEmpty(teacherLname) ||
          string.IsNullOrEmpty(employeeNumber) || hireDate == null || hireDate > DateTime.Now || salary == null || salary < 0)
      {
        return Json(new { success = false, message = "Invalid input. Please provide valid details." });
      }

      var newTeacher = new Teacher
      {
        TeacherFname = teacherFname,
        TeacherLname = teacherLname,
        EmployeeNumber = employeeNumber,
        HireDate = hireDate,
        Salary = salary ?? 0
      };

      var actionResult = _teacherApiController.AddTeacher(newTeacher);
      if (actionResult is BadRequestResult)
      {
        return Json(new { success = false, message = "Failed to add teacher. Please try again." });
      }

      TempData["SuccessMessage"] = "Teacher created successfully!";

      return Json(new { success = true, message = "Teacher added successfully!" });
    }



    

    /// POST /TeacherPage/Delete/17

    [HttpPost]
    public JsonResult Ajax_Delete(int id)
    {
      var result = _teacherApiController.DeleteTeacher(id);
      if (result is NotFoundObjectResult)
      {
        return Json(new { success = false, message = "Teacher not found." });
      }

      TempData["SuccessMessage"] = "Teacher deleted successfully!";

      return Json(new { success = true, message = "Teacher deleted successfully!" });
    }


    
    

    /// GET : /TeacherPage/Ajax_DeleteConfirm/{id}

    public IActionResult Ajax_DeleteConfirm(int id)
    {
      var result = _teacherApiController.FindTeacher(id);
      if (result is NotFoundObjectResult)
      {
        return View("NotFound");
      }

      var teacher = (result as OkObjectResult)?.Value as Teacher;

      TempData["SuccessMessage"] = "Teacher deleted successfully!";

      // Pass the teacher details to the view
      return View(teacher);
    }

  }
}
