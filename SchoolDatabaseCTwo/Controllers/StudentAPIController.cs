using SchoolDatabaseCTwo.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;



namespace SchoolDatabaseCTwo.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class StudentAPIController : ControllerBase
  {
    // Database context class for accessing the MySQL Database.
    private readonly SchoolDbContext _context;
    // dependency injection of database context
    public StudentAPIController(SchoolDbContext context)
    {
      _context = context;
    }

    /// <summary>
    /// Retrieves a list of all students from the database.
    /// </summary>
    /// <returns>
    /// A list containing all students in the system.
    /// </returns>
    /// <example>
    /// Example of a GET request to retrieve all students:
    /// curl -X GET "http://localhost:5254/api/StudentAPI/ListStudents
    /// Output: List o all Students
    /// </example>
    
    [HttpGet]
    [Route(template: "ListStudents")]
    public IEnumerable<Student> ListStudents()
    {
      // Create a list to store student objects
      List<Student> Students = new List<Student>();

      // Establish a connection to the database
      MySqlConnection Connection = _context.AccessDatabase();
      Connection.Open();

      // Prepare SQL query
      MySqlCommand Command = Connection.CreateCommand();
      Command.CommandText = "SELECT * FROM students";

      // Execute the query
      MySqlDataReader ResultSet = Command.ExecuteReader();
      

      // Iterate through each row in the result set
      while (ResultSet.Read())
      {
        Student student = new Student
        {
          StudentId = Convert.ToInt32(ResultSet["StudentId"]),
          StudentFname = ResultSet["StudentFname"].ToString(),
          StudentLname = ResultSet["StudentLname"].ToString(),
          StudentNumber = ResultSet["StudentNumber"].ToString(),
          EnrolDate = Convert.ToDateTime(ResultSet["EnrolDate"])
        };
        Students.Add(student);
      }
      // Close the database connection
      Connection.Close();

      // Return the list of students
      return Students;
    }


    /// <summary>
    /// Retrieves the details of a specific student identified by their unique student ID.
    /// </summary>
    /// <param name="id">The unique identifier of the student to retrieve.</param>
    /// <returns>
    /// A student object containing the details of the specified student.
    /// </returns>
    /// <example>
    /// Example of a GET request to retrieve details of a student with ID 5:
    /// curl -X GET "http://localhost:5254/api/StudentAPI/FindStudent/5
    ///
    /// Output: {"studentId":5,"studentFname":"Elizabeth","studentLname":"Murray","studentNumber":"N1690",
    ///         "enrolDate":"2018-07-12T00:00:00","courses":[]}
    /// </example>

    [HttpGet]
    [Route(template: "FindStudent/{id}")]
    public Student FindStudent(int id)
    {
      MySqlConnection connection = _context.AccessDatabase();
      connection.Open();

      MySqlCommand command = connection.CreateCommand();
      command.CommandText = "SELECT * FROM Students WHERE StudentId = @id";
      command.Parameters.AddWithValue("@id", id);

      MySqlDataReader resultSet = command.ExecuteReader();
      Student student = null;

      if (resultSet.Read())
      {
        student = new Student
        {
          StudentId = Convert.ToInt32(resultSet["StudentId"]),
          StudentFname = resultSet["StudentFname"].ToString(),
          StudentLname = resultSet["StudentLname"].ToString(),
          StudentNumber = resultSet["StudentNumber"].ToString(),
          EnrolDate = Convert.ToDateTime(resultSet["EnrolDate"])
        };
      }

      connection.Close();
      return student;
    }



    /// <summary>
    /// Adds a new student to the database.
    /// </summary>
    /// <param name="student">The student object to be added.</param>
    /// <returns>
    /// A success message if the student is added successfully; otherwise, a bad request error.
    /// </returns>
    /// <example>
    /// curl -X POST http://localhost:5254/api/StudentAPI/AddStudent \
    /// -H "Content-Type: application/json" \
    /// -d '{
    ///   "StudentFname": "Folake",
    ///   "StudentLname": "Bamidele",
    ///   "StudentNumber": "N1234",
    ///   "EnrolDate": "2023-09-01"
    /// }'
    ///
    /// Output: Student added successfully
    /// </example>

    [HttpPost]
    [Route("AddStudent")]
    public IActionResult AddStudent([FromBody] Student student)
    {
      if (student == null) return BadRequest("Student data is required.");

      var errors = new List<string>();

      // Validate fields
      if (string.IsNullOrEmpty(student.StudentFname)) errors.Add("First name is required.");
      if (string.IsNullOrEmpty(student.StudentLname)) errors.Add("Last name is required.");
      if (string.IsNullOrEmpty(student.StudentNumber)) errors.Add("Student number is required.");
      if (!Regex.IsMatch(student.StudentNumber, @"^N\d{4}$"))
          errors.Add("Student number must start with 'N' followed by 4 digits.");
      if (student.EnrolDate == null) 
        errors.Add("Enrollment date is required.");
      else if (student.EnrolDate > DateTime.Now) 
        errors.Add("Enrollment date cannot be in the future.");

      // Return validation errors if any
      if (errors.Any()) return BadRequest(new { success = false, errors });

      using (var connection = _context.AccessDatabase())
      {
        connection.Open();

        // Check if Student Number is unique
        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Students WHERE LOWER(StudentNumber) = LOWER(@StudentNumber)";
        checkCmd.Parameters.AddWithValue("@StudentNumber", student.StudentNumber);
        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (count > 0)
        {
          return BadRequest(new { success = false, message = "Student number must be unique." });
        }

        // Insert student data into the Students table
        var studentCmd = connection.CreateCommand();
        studentCmd.CommandText = @"
            INSERT INTO Students (StudentFname, StudentLname, StudentNumber, EnrolDate) 
            VALUES (@StudentFname, @StudentLname, @StudentNumber, @EnrolDate)";
        studentCmd.Parameters.AddWithValue("@StudentFname", student.StudentFname);
        studentCmd.Parameters.AddWithValue("@StudentLname", student.StudentLname);
        studentCmd.Parameters.AddWithValue("@StudentNumber", student.StudentNumber);
        studentCmd.Parameters.AddWithValue("@EnrolDate", student.EnrolDate);
        studentCmd.ExecuteNonQuery();

        // Get the newly created StudentId
        var studentId = Convert.ToInt32(studentCmd.LastInsertedId);

        // Insert into the studentsxcourses bridge table for each course
        if (student.Courses != null && student.Courses.Any())
        {
          foreach (var course in student.Courses)
          {
            var bridgeCmd = connection.CreateCommand();
            bridgeCmd.CommandText = @"
                INSERT INTO studentsxcourses (StudentId, CourseId) 
                VALUES (@StudentId, @CourseId)";
            bridgeCmd.Parameters.AddWithValue("@StudentId", studentId);
            bridgeCmd.Parameters.AddWithValue("@CourseId", course.CourseId);
            bridgeCmd.ExecuteNonQuery();
          }
        }
      }

      return Ok(new { success = true, message = "Student added successfully." });
    }




    /// <summary>
    /// Updates an existing student in the database.
    /// </summary>
    /// <param name="student">The student object with updated data.</param>
    /// <returns>
    /// A success message if the student is updated successfully; otherwise, a bad request error.
    /// </returns>
    /// <example>
    /// curl -X POST http://localhost:5254/api/StudentAPI/UpdateStudent \
    /// -H "Content-Type: application/json" \
    /// -d '{
    ///   "StudentId": 38,
    ///   "StudentFname": "Folake",
    ///   "StudentLname": "Dele",
    ///   "StudentNumber": "N1234",
    ///   "EnrolDate": "2023-09-01"
    /// }'
    ///
    /// Output: Student updated successfully
    /// </example>

    [HttpPut]
    [Route("UpdateStudent")]
    public IActionResult UpdateStudent([FromBody] Student student)
    {
      if (student == null) return BadRequest("Student data is required");

      using (var connection = _context.AccessDatabase())
      {
        connection.Open();

        // Update the student information
        var studentCmd = connection.CreateCommand();
        studentCmd.CommandText = @"
            UPDATE Students
            SET StudentFname = @StudentFname, StudentLname = @StudentLname, 
                StudentNumber = @StudentNumber, EnrolDate = @EnrolDate
            WHERE StudentId = @StudentId";
        studentCmd.Parameters.AddWithValue("@StudentFname", student.StudentFname);
        studentCmd.Parameters.AddWithValue("@StudentLname", student.StudentLname);
        studentCmd.Parameters.AddWithValue("@StudentNumber", student.StudentNumber);
        studentCmd.Parameters.AddWithValue("@EnrolDate", student.EnrolDate);
        studentCmd.Parameters.AddWithValue("@StudentId", student.StudentId);
        studentCmd.ExecuteNonQuery();

        connection.Close();
      }

      return Ok("Student updated successfully");
    }




    /// <summary>
    /// Deletes a student identified by their ID.
    /// </summary>
    /// <param name="id">The unique identifier of the student to delete.</param>
    /// <returns>
    /// A success message if the student is deleted; otherwise, a not found or error response.
    /// </returns>
    /// <example>
    /// curl -X POST http://localhost:5254/api/StudentAPI/DeleteStudent/38 \
    ///             -H "Content-Type: application/json"
    ///
    /// Output: {"success":true,"message":"Student deleted successfully."}
    /// </example>

    [HttpPost("DeleteStudent/{id}")]
    public IActionResult DeleteStudent(int id)
    {
      try
      {
        using (var connection = _context.AccessDatabase())
        {
          connection.Open();

          // Check if the student exists
          var checkCmd = connection.CreateCommand();
          checkCmd.CommandText = "SELECT COUNT(*) FROM Students WHERE StudentId = @id";
          checkCmd.Parameters.AddWithValue("@id", id);
          int studentExists = Convert.ToInt32(checkCmd.ExecuteScalar());
          if (studentExists == 0)
          {
            return NotFound(new { success = false, message = "Student not found." });
          }

          // Proceed to delete the student
          var deleteCmd = connection.CreateCommand();
          deleteCmd.CommandText = "DELETE FROM Students WHERE StudentId = @id";
          deleteCmd.Parameters.AddWithValue("@id", id);
          deleteCmd.ExecuteNonQuery();
        }

        return Ok(new { success = true, message = "Student deleted successfully." });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { success = false, message = "An error occurred.", details = ex.Message });
      }
    }




    /// <summary>
    /// Retrieves all courses a student is enrolled in.
    /// </summary>
    /// <param name="studentId">The unique identifier of the student.</param>
    /// <returns>A list of courses the student is enrolled in.</returns>
    /// <example>
    /// curl -X GET http://localhost:5254/api/StudentAPI/GetCoursesByStudent/36
    ///
    /// Output: [{"courseId":3,"courseCode":"http5103","teacherId":5,"startDate":"2018-09-04T00:00:00",
    ///         "finishDate":"2018-12-14T00:00:00","courseName":"Web Programming","students":[]}]
    /// </example>

    [HttpGet]
    [Route("GetCoursesByStudent/{studentId}")]
    public IActionResult GetCoursesByStudent(int studentId)
    {
      using (var connection = _context.AccessDatabase())
      {
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT c.* FROM studentsxcourses sc
            JOIN courses c ON sc.courseId = c.Courseid
            WHERE sc.studentid = @StudentId";
        cmd.Parameters.AddWithValue("@StudentId", studentId);

        var courses = new List<Course>();
        using (var reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            courses.Add(new Course
            {
              CourseId = Convert.ToInt32(reader["CourseId"]),
              CourseCode = reader["CourseCode"].ToString(),
              CourseName = reader["CourseName"].ToString(),
              StartDate = Convert.ToDateTime(reader["StartDate"]),
              FinishDate = Convert.ToDateTime(reader["FinishDate"]),
              TeacherId = Convert.ToInt32(reader["TeacherId"])
            });
          }
        }

        return Ok(courses);
      }
    }

  }
}