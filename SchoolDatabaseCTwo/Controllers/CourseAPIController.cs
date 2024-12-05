using SchoolDatabaseCTwo.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;


namespace SchoolDatabaseCTwo.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class CourseAPIController : ControllerBase
  {
    // Database context class for accessing the MySQL Database.
    private readonly SchoolDbContext _context;
    // dependency injection of database context
    public CourseAPIController(SchoolDbContext context)
    {
      _context = context;
    }

    /// <summary>
    /// Retrieves a list of all courses from the database.
    /// </summary>
    /// <returns>
    /// A list containing all courses in the system.
    /// </returns>
    /// <example>
    /// Example of a GET request to retrieve all courses:
    /// curl -X GET http://localhost:5254/api/CourseAPI/ListCourses
    /// Output: All Courses
    /// </example>



    [HttpGet]
    [Route(template:"ListCourses")]
    public IEnumerable<Course> ListCourses()
    {
      // Create a list to store course objects
      List<Course> Courses = new List<Course>();

      // Establish a connection to the database
      MySqlConnection Connection = _context.AccessDatabase();
      Connection.Open();

      // Prepare SQL query
      MySqlCommand Command = Connection.CreateCommand();
      Command.CommandText = "SELECT * FROM courses";

      // Execute the query
      MySqlDataReader ResultSet = Command.ExecuteReader();
      

      // Iterate through each row in the result set
      while (ResultSet.Read())
      {
        Course course = new Course
        {
          CourseId = Convert.ToInt32(ResultSet["CourseId"]),
          CourseCode = ResultSet["CourseCode"].ToString(),
          TeacherId = Convert.ToInt32(ResultSet["TeacherId"]),
          StartDate = Convert.ToDateTime(ResultSet["StartDate"]),
          FinishDate = Convert.ToDateTime(ResultSet["FinishDate"]),
          CourseName = ResultSet["CourseName"].ToString()
        };
        Courses.Add(course);
      }

      // Close the database connection
      Connection.Close();

      // Return the list of Course
      return Courses;
    }



    /// <summary>
    /// Retrieves a list of courses taught by a specific teacher, identified by their unique teacher ID.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher whose courses are to be listed.</param>
    /// <returns>A list of course objects associated with the specified teacher. 
    /// </returns>
    /// <example>
    /// curl -X GET http://localhost:5254/api/CourseAPI/ListCoursesByTeacher/4
    ///
    /// Output: [{"courseId":8,"courseCode":"http5203","teacherId":4,"startDate":"2019-01-08T00:00:00",
    ///         "finishDate":"2019-04-27T00:00:00","courseName":"XML and Web Services","students":[]}]
    /// </example>

    [HttpGet]
    [Route(template: "ListCoursesByTeacher/{teacherId}")]
    public List<Course> ListCoursesByTeacher(int teacherId)
    {
      List<Course> courses = new List<Course>();

      MySqlConnection Connection = _context.AccessDatabase();
      Connection.Open();

      MySqlCommand Command = Connection.CreateCommand();
      Command.CommandText = "SELECT * FROM Courses WHERE TeacherId = @teacherId";
      Command.Parameters.AddWithValue("@teacherId", teacherId);
      Command.Prepare();

      MySqlDataReader ResultSet = Command.ExecuteReader();

      while (ResultSet.Read())
      {
        Course course = new Course
        {
          CourseId = Convert.ToInt32(ResultSet["CourseId"]),
          CourseCode = ResultSet["CourseCode"].ToString(),
          TeacherId = teacherId,
          StartDate = Convert.ToDateTime(ResultSet["StartDate"]),
          FinishDate = Convert.ToDateTime(ResultSet["FinishDate"]),
          CourseName = ResultSet["CourseName"].ToString()
        };

        courses.Add(course);
      }

      Connection.Close();
      return courses;
    }



    /// <summary>
    /// Retrieves the details of a specific course identified by its unique course ID.
    /// </summary>
    /// <param name="id">The unique identifier of the course to retrieve.</param>
    /// <returns>A course object containing the details of the specified course. 
    /// </returns>
    /// <example>
    /// curl -X GET http://localhost:5254/api/CourseAPI/FindCourse/10 
    /// 
    /// Output: {"courseId":10,"courseCode":"http5205","teacherId":6,"startDate":"2019-01-08T00:00:00",
    ///         "finishDate":"2019-04-27T00:00:00","courseName":"Career Connections","students":[]}
    /// </example>

    [HttpGet]
    [Route(template: "FindCourse/{id}")]
    public Course FindCourse(int id)
    {
      MySqlConnection connection = _context.AccessDatabase();
      connection.Open();

      MySqlCommand command = connection.CreateCommand();
      command.CommandText = "SELECT * FROM Courses WHERE CourseId = @id";
      command.Parameters.AddWithValue("@id", id);

      MySqlDataReader resultSet = command.ExecuteReader();
      Course course = null;

      if (resultSet.Read())
      {
        course = new Course
        {
          CourseId = Convert.ToInt32(resultSet["CourseId"]),
          CourseCode = resultSet["CourseCode"].ToString(),
          TeacherId = Convert.ToInt32(resultSet["TeacherId"]),
          StartDate = Convert.ToDateTime(resultSet["StartDate"]),
          FinishDate = Convert.ToDateTime(resultSet["FinishDate"]),
          CourseName = resultSet["CourseName"].ToString()
        };
      }

      connection.Close();
      return course;
    }



    /// <summary>
    /// Adds a new course to the database.
    /// </summary>
    /// <param name="course">The course object to be added.</param>
    /// <returns>A success message if the course is added successfully, otherwise an error message.</returns>
    /// <example>
    /// curl -X POST http://localhost:5254/api/CourseAPI/AddCourse \
    /// -H "Content-Type: application/json" \
    /// -d '{
    ///   "CourseCode": "http1001",
    ///   "CourseName": "Introduction to Programming",
    ///   "StartDate": "2024-01-01",
    ///   "FinishDate": "2024-06-01",
    ///   "TeacherId": 3
    /// }'
    ///
    /// Output: Course added successfully
    /// </example>

    [HttpPost]
    [Route("AddCourse")]
    public IActionResult AddCourse([FromBody] Course course)
    {
      if (course == null) return BadRequest("Course data is required.");

      var errors = new List<string>();

      if (string.IsNullOrEmpty(course.CourseCode)) errors.Add("Course code is required.");
      if (string.IsNullOrEmpty(course.CourseName)) errors.Add("Course name is required.");
      if (course.StartDate == null) errors.Add("Start date is required.");
      if (course.FinishDate == null) errors.Add("Finish date is required.");
      if (course.TeacherId <= 0) errors.Add("Teacher ID is required.");

    // Return validation errors if any
    if (errors.Any()) return BadRequest(new { success = false, errors });

      using (var connection = _context.AccessDatabase())
    {
        connection.Open();

        // Check if Course Code is unique
        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Courses WHERE LOWER(CourseCode) = LOWER(@CourseCode)";
        checkCmd.Parameters.AddWithValue("@CourseCode", course.CourseCode);
        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (count > 0)
        {
          return BadRequest(new { success = false, message = "Course code must be unique." });
        }

        // Add the course to Course Table
        var courseCmd = connection.CreateCommand();
        courseCmd.CommandText = @"
            INSERT INTO Courses (CourseCode, CourseName, StartDate, FinishDate, TeacherId) 
            VALUES (@CourseCode, @CourseName, @StartDate, @FinishDate, @TeacherId)";
        courseCmd.Parameters.AddWithValue("@CourseCode", course.CourseCode);
        courseCmd.Parameters.AddWithValue("@CourseName", course.CourseName);
        courseCmd.Parameters.AddWithValue("@StartDate", course.StartDate);
        courseCmd.Parameters.AddWithValue("@FinishDate", course.FinishDate);
        courseCmd.Parameters.AddWithValue("@TeacherId", course.TeacherId);
        courseCmd.ExecuteNonQuery();
        connection.Close();
    }

    return Ok(new { success = true, message = "Course added successfully." });

    }



    /// <summary>
    /// Updates an existing course in the database.
    /// </summary>
    /// <param name="course">The course object with updated details.</param>
    /// <returns>A success message if the course is updated successfully, otherwise an error message.</returns>
    /// <example>
    /// curl -X POST http://localhost:5254/api/CourseAPI/UpdateCourse \
    /// -H "Content-Type: application/json" \
    /// -d '{
    ///   "CourseId": 17,
    ///   "CourseCode": "http1001",
    ///   "CourseName": "Introduction to Programming",
    ///   "StartDate": "2024-01-01",
    ///   "FinishDate": "2024-06-01",
    ///   "TeacherId": 6
    /// }'
    ///
    /// Output: Course added successfully
    /// </example>

    [HttpPut]
    [Route("UpdateCourse")]
    public IActionResult UpdateCourse([FromBody] Course course)
    {
      if (course == null) return BadRequest("Course data is required");

      MySqlConnection connection = _context.AccessDatabase();
      connection.Open();

      MySqlCommand command = connection.CreateCommand();
      command.CommandText = "UPDATE Courses SET CourseCode = @CourseCode, CourseName = @CourseName, StartDate = @StartDate, FinishDate = @FinishDate, TeacherId = @TeacherId WHERE CourseId = @CourseId";
      command.Parameters.AddWithValue("@CourseCode", course.CourseCode);
      command.Parameters.AddWithValue("@CourseName", course.CourseName);
      command.Parameters.AddWithValue("@StartDate", course.StartDate);
      command.Parameters.AddWithValue("@FinishDate", course.FinishDate);
      command.Parameters.AddWithValue("@TeacherId", course.TeacherId);
      command.Parameters.AddWithValue("@CourseId", course.CourseId);

      command.ExecuteNonQuery();
      connection.Close();

      return Ok("Course updated successfully");
    }



    /// <summary>
    /// Deletes a course identified by its unique ID.
    /// </summary>
    /// <param name="id">The unique ID of the course to delete.</param>
    /// <returns>A success message if the course is deleted successfully, otherwise an error message.</returns>
    /// <example>
    /// curl -X POST http://localhost:5254/api/CourseAPI/DeleteCourse/17 \
    ///             -H "Content-Type: application/json"  
    ///
    /// Output: {"success":true,"message":"Course deleted successfully."}     
    /// </example>

    [HttpPost("DeleteCourse/{id}")]
    public IActionResult DeleteCourse(int id)
    {
      try
      {
        using (var connection = _context.AccessDatabase())
        {
          connection.Open();

          // Check if the course exists
          var checkCmd = connection.CreateCommand();
          checkCmd.CommandText = "SELECT COUNT(*) FROM Courses WHERE CourseId = @id";
          checkCmd.Parameters.AddWithValue("@id", id);
          int courseExists = Convert.ToInt32(checkCmd.ExecuteScalar());
          if (courseExists == 0)
          {
            return NotFound(new { success = false, message = "Course not found." });
          }

          // Proceed to delete the course
          var deleteCmd = connection.CreateCommand();
          deleteCmd.CommandText = "DELETE FROM Courses WHERE CourseId = @id";
          deleteCmd.Parameters.AddWithValue("@id", id);
          deleteCmd.ExecuteNonQuery();
        }

        return Ok(new { success = true, message = "Course deleted successfully." });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { success = false, message = "An error occurred.", details = ex.Message });
      }
    }




    /// <summary>
    /// Retrieves a list of students enrolled in a specific course.
    /// </summary>
    /// <param name="courseId">The unique ID of the course.</param>
    /// <returns>A list of students enrolled in the course.</returns>
    /// <example>
    /// curl -X GET http://localhost:5254/api/CourseAPI/GetStudentsByCourse/5
    ///
    /// Output: [{"studentId":1,"studentFname":"Sarah","studentLname":"Valdez","studentNumber":"N1678",
    ///         "enrolDate":"2018-06-18T00:00:00","courses":[]},
    /// </example>

    [HttpGet]
    [Route("GetStudentsByCourse/{courseId}")]
    public IActionResult GetStudentsByCourse(int courseId)
    {
      using (var connection = _context.AccessDatabase())
      {
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT s.* FROM studentsxcourses sc
            JOIN students s ON sc.studentid = s.studentid
            WHERE sc.courseid = @CourseId";
        cmd.Parameters.AddWithValue("@CourseId", courseId);

        var students = new List<Student>();
        using (var reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            students.Add(new Student
            {
              StudentId = Convert.ToInt32(reader["StudentId"]),
              StudentFname = reader["StudentFname"].ToString(),
              StudentLname = reader["StudentLname"].ToString(),
              StudentNumber = reader["StudentNumber"].ToString(),
              EnrolDate = Convert.ToDateTime(reader["EnrolDate"])
            });
          }
        }

        return Ok(students);
      }
    }

  }
}