using Microsoft.AspNetCore.Mvc;
using SchoolDatabaseCTwo.Models;
using Microsoft.AspNetCore.Cors;


namespace SchoolDatabaseCTwo.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [EnableCors("AllowAll")]
  public class TeacherAPIController : ControllerBase
  {
    private readonly SchoolDbContext _context;
    // dependency injection of database context
    public TeacherAPIController(SchoolDbContext context)
    {
      _context = context;
    }


    /// <summary>
    /// Retrieves a list of teachers from the database, optionally filtered by a search key.
    /// The search key can match the teacher's first name, last name, full name, or hire date.
    /// </summary>
    /// <param name="SearchKey">An optional parameter to filter teachers by first name, last name, full name, or hire date.</param>
    /// <returns>
    /// - A list of <see cref="Teacher"/> objects that match the search criteria, returned with an HTTP 200 status code if found.
    /// - An HTTP 404 status code with a custom message if no teachers match the search criteria.
    /// </returns>
    /// <example>
    /// curl -X GET "http://localhost:5254/api/TeacherAPI/ListTeachers/John" \
    ///             -H "Content-Type: application/json
    /// 
    /// Output: [{"teacherId":10,"teacherFname":"John","teacherLname":"Taram","employeeNumber":"T505",
    ///         "hireDate":"2015-10-23T00:00:00","salary":79.63,"teacherWorkPhone":null,"courses":[]},    
    ///         {"teacherId":15,"teacherFname":"John","teacherLname":"Doe","employeeNumber":"T123",
    ///         "hireDate":"2023-01-01T00:00:00","salary":120.00,"teacherWorkPhone":null,"courses":[]}]
    ///
    /// curl -X GET "http://localhost:5254/api/TeacherAPI/ListTeachers?startDate=2023-01-01&endDate=2023-01-01" \
    ///             -H "Content-Type: application/json"
    ///
    /// Output: [{"teacherId":15,"teacherFname":"John","teacherLname":"Doe","employeeNumber":"T123",
    ///         "hireDate":"2023-01-01T00:00:00","salary":120.00,"teacherWorkPhone":null,"courses":[]},
    ///         {"teacherId":16,"teacherFname":"Janet","teacherLname":"Jackson","employeeNumber":"T444",
    ///         "hireDate":"2023-01-01T00:00:00","salary":130.00,"teacherWorkPhone":null,"courses":[]}]
    /// </example>


    [HttpGet("ListTeachers/{searchKey?}")]
    public IActionResult ListTeachers(string searchKey = null, DateTime? startDate = null, DateTime? endDate = null)
    {
      try
      {
        var teachers = new List<Teacher>();

        using (var connection = _context.AccessDatabase())
        {
          connection.Open();
          var cmd = connection.CreateCommand();

          var query = @"
              SELECT * FROM Teachers 
              WHERE 
                (@Key IS NULL OR LOWER(teacherfname) LIKE LOWER(@Key) 
                  OR LOWER(teacherlname) LIKE LOWER(@Key) 
                  OR LOWER(CONCAT(teacherfname, ' ', teacherlname)) LIKE LOWER(@Key))";

          // Add date range filter
          if (startDate.HasValue && endDate.HasValue)
          {
            query += " AND hiredate BETWEEN @StartDate AND @EndDate";
            cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
            cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
          }

          cmd.CommandText = query;
          cmd.Parameters.AddWithValue("@Key", string.IsNullOrEmpty(searchKey) ? DBNull.Value : $"%{searchKey}%");

          using (var reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              teachers.Add(new Teacher
              {
                TeacherId = Convert.ToInt32(reader["teacherid"]),
                TeacherFname = reader["teacherfname"].ToString(),
                TeacherLname = reader["teacherlname"].ToString(),
                EmployeeNumber = reader["employeenumber"].ToString(),
                HireDate = Convert.ToDateTime(reader["hiredate"]),
                Salary = Convert.ToDecimal(reader["salary"])
              });
            }
          }
        }

        if (teachers.Count == 0)
        {
          return NotFound("No teachers found.");
        }

        return Ok(teachers);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }





    /// <summary>
    /// Retrieves the details of a specific teacher identified by their unique teacher ID.
    /// </summary>
    /// <param name="id">The unique identifier of the teacher to retrieve.</param>
    /// <returns>
    /// - A <see cref="Teacher"/> object with the details of the specified teacher, returned with an HTTP 200 status code if found.
    /// - An HTTP 404 status code with a "Teacher not found." message if no teacher with the specified ID exists.
    /// </returns>
    /// <example>
    /// curl -X GET http://localhost:5254/api/TeacherAPI/FindTeacher/1
    /// 
    /// Output: {"teacherId":1,"teacherFname":"Alexander","teacherLname":"Bennet","employeeNumber":"T378",
    ///          "hireDate":"2016-08-05T00:00:00","salary":98.75,"teacherWorkPhone":"555-987-6543","courses":[]}
    /// </example>

    [HttpGet]
    [Route(template: "FindTeacher/{id}")]
    public IActionResult FindTeacher(int id)
    {
      // Create a new Teacher object
      Teacher SelectedTeacher = new Teacher();

      // Create a connection to the database
      using (var connection = _context.AccessDatabase())
      {
      connection.Open();

      // Prepare SQL query to retrieve teacher information
      var Command = connection.CreateCommand();
      Command.CommandText = "SELECT * FROM Teachers WHERE teacherid = @id";
      Command.Parameters.AddWithValue("@id", id);
      Command.Prepare();

      // Execute the query
      using (var ResultSet = Command.ExecuteReader())
      {

      if (!ResultSet.HasRows) 
      {
        connection.Close();
        return NotFound("Teacher not found.");
      }

      // Populate the teacher object with information from the result set
      while (ResultSet.Read())
      {
        SelectedTeacher.TeacherId = Convert.ToInt32(ResultSet["teacherId"]);
        SelectedTeacher.TeacherFname = ResultSet["teacherFname"].ToString();
        SelectedTeacher.TeacherLname = ResultSet["teacherLname"].ToString();
        SelectedTeacher.EmployeeNumber = ResultSet["employeenumber"].ToString();
        SelectedTeacher.HireDate = Convert.ToDateTime(ResultSet["hiredate"]);
        SelectedTeacher.Salary = Convert.ToDecimal(ResultSet["salary"]);
        SelectedTeacher.TeacherWorkPhone = ResultSet["teacherworkphone"].ToString();
      }
    

      // Close the result set
      ResultSet.Close(); 
      }

      // Close the database connection
      connection.Close();

      // Return the teacher object
      return Ok(SelectedTeacher);;
      }
    }




    /// <summary>
    /// Adds a teacher to the Database.
    /// </summary>
    /// <param name="NewTeacher">An object with fields that map to the columns of the teacher's table.</param>
    /// <returns>
    /// A response indicating the success or failure of the operation.
    /// Returns a 400 Bad Request response if the provided information is missing or incorrect.
    /// Returns a 200 OK response if the teacher is added successfully.
    /// </returns>
    /// <example>
    /// curl -X POST http://localhost:5254/api/TeacherAPI/AddTeacher \
    /// -H "Content-Type: application/json" \
    /// -d '{
    ///  "TeacherFname": "Bassey",
    ///  "TeacherLname": "Ekom",
    ///  "EmployeeNumber": "T779",
    ///  "HireDate": "2023-08-08",
    ///  "Salary": 75.50,
    ///  "TeacherWorkPhone": "555-123-4567"
    /// }'
    /// 
    /// Output: {"success":true,"message":"Teacher added successfully."}
    ///
    /// curl -X POST http://localhost:5254/api/TeacherAPI/AddTeacher \
    /// -H "Content-Type: application/json" \
    /// -d '{
    ///  "TeacherFname": "",
    ///  "TeacherLname": "",
    ///  "EmployeeNumber": "T799",
    ///  "HireDate": "2023-08-08",
    ///  "Salary": 75.50,
    ///  "TeacherWorkPhone": "555-123-4567"
    /// }'
    /// 
    /// Output: {"success":false,"message":"Invalid input data.","errors":["First name is required.","Last name is required."]}
    /// </example>

    [HttpPost("AddTeacher")]
    public IActionResult AddTeacher([FromBody] Teacher newTeacher)
    {
      var errors = new List<string>();


      if (string.IsNullOrEmpty(newTeacher.TeacherFname))
        errors.Add("First name is required.");
      if (string.IsNullOrEmpty(newTeacher.TeacherLname))
        errors.Add("Last name is required.");


      if (string.IsNullOrEmpty(newTeacher.EmployeeNumber) || !System.Text.RegularExpressions.Regex.IsMatch(newTeacher.EmployeeNumber, @"^T\d{3}$"))
        errors.Add("Employee number must start with 'T' followed by exactly three digits.");


      if (newTeacher.HireDate == null || newTeacher.HireDate > DateTime.Now)
        errors.Add("Hire date is invalid or in the future.");


      if (newTeacher.Salary < 0)
        errors.Add("Salary must be a positive number.");


      if (string.IsNullOrEmpty(newTeacher.TeacherWorkPhone))
        errors.Add("Work phone is required.");


      if (errors.Any())
        return BadRequest(new { success = false, message = "Invalid input data.", errors });


      try
      {
        // Check if EmployeeNumber already exists
        using (var connection = _context.AccessDatabase())
        {
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Teachers WHERE LOWER(employeenumber) = LOWER(@EmployeeNumber)";
        cmd.Parameters.AddWithValue("@EmployeeNumber", newTeacher.EmployeeNumber);
        int count = Convert.ToInt32(cmd.ExecuteScalar());

        if (count > 0)
        {
          return BadRequest(new { success = false, message = "Error: Employee number is already taken by another teacher." });
        }

        // Add the teacher if validation passes
        cmd.CommandText = @"
            INSERT INTO teachers (teacherfname, teacherlname, employeenumber, hiredate, salary, teacherworkphone) 
            VALUES (@TeacherFname, @TeacherLname, @EmployeeNumber, @HireDate, @Salary, @TeacherWorkPhone)";
        cmd.Parameters.AddWithValue("@TeacherFname", newTeacher.TeacherFname);
        cmd.Parameters.AddWithValue("@TeacherLname", newTeacher.TeacherLname);
        cmd.Parameters.AddWithValue("@HireDate", newTeacher.HireDate);
        cmd.Parameters.AddWithValue("@Salary", newTeacher.Salary);
        cmd.Parameters.AddWithValue("@TeacherWorkPhone", newTeacher.TeacherWorkPhone ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
      }

      return Ok(new { success = true, message = "Teacher added successfully." });
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }



    /// <summary>
    /// Deletes a teacher from the connected Database if the ID of that teacher exists.
    /// </summary>
    /// <param name="id">The unique ID of the teacher.</param>
    /// <returns>
    /// A response indicating the success of the operation..
    /// Returns a 200 OK response if the teacher is updated successfully.
    /// </returns>
    /// <example>
    /// curl -X POST http://localhost:5254/api/TeacherAPI/DeleteTeacher/30
    ///
    /// Output: Teacher deleted successfully. 
    ///
    /// curl -X POST http://localhost:5254/api/TeacherAPI/DeleteTeacher/30
    ///
    /// Output: Teacher not found.      
    ///</example>

    [HttpPost("DeleteTeacher/{id}")]
    public IActionResult DeleteTeacher(int id)
    {
      try
      {
        using (var connection = _context.AccessDatabase())
        {
          connection.Open();
          var cmd = connection.CreateCommand();

          cmd.CommandText = "SELECT COUNT(*) FROM Teachers WHERE teacherid = @id";
          cmd.Parameters.AddWithValue("@id", id);
          int teacherExists = Convert.ToInt32(cmd.ExecuteScalar());
          if (teacherExists == 0)
          {
            return NotFound("Teacher not found.");
          }

          cmd.CommandText = "DELETE FROM Teachers WHERE teacherid = @id";
          cmd.ExecuteNonQuery();
        }

        return Ok("Teacher deleted successfully.");
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }




    /// <summary>
    /// Updates the information of a specific teacher in the Database.
    /// </summary>
    /// <param name="id">The unique ID of the teacher to update.</param>
    /// <param name="TeacherInfo">An object containing the updated information of the teacher.</param>
    /// <returns>
    /// A response indicating the success or failure of the operation.
    /// Returns a 400 Bad Request response if the provided information is missing or incorrect.
    /// Returns a 200 OK response if the teacher is updated successfully.
    /// </returns>
    /// <example>
    /// curl -X POST http://localhost:5254/api/TeacherAPI/1 \
    ///  -H "Content-Type: application/json" \
    ///  -d '{
    ///    "TeacherFname": "Alexander",
    ///    "TeacherLname": "Bennet",
    ///    "EmployeeNumber": "T378",
    ///    "HireDate": "2016-08-05",
    ///    "Salary": 56.75,
    ///    "TeacherWorkPhone": "555-987-6543"
    ///  }'
    ///
    /// Output: {"success":true,"message":"Teacher updated successfully."}
    /// </example>
    
    //[HttpPost("{id}")]
    [HttpPut("{id}")]
    [EnableCors("AllowAll")]
    public IActionResult UpdateTeacher(int id, [FromBody] Teacher updatedTeacher)
    {
      Console.WriteLine($"UpdateTeacher API called for TeacherId: {id}");

      // if (string.IsNullOrEmpty(updatedTeacher.TeacherFname) ||
      //     string.IsNullOrEmpty(updatedTeacher.TeacherLname) ||
      //     string.IsNullOrEmpty(updatedTeacher.EmployeeNumber) ||
      //     updatedTeacher.HireDate == null ||
      //     updatedTeacher.HireDate > DateTime.Now ||
      //     !updatedTeacher.EmployeeNumber.StartsWith("T") ||
      //     updatedTeacher.Salary < 0)
      // {
      //   return BadRequest(new { success = false, message = "Invalid input data." });
      // }

      // Collect specific validation errors
      var errors = new List<string>();

      if (string.IsNullOrEmpty(updatedTeacher.TeacherFname))
          errors.Add("Teacher first name is required.");
      if (string.IsNullOrEmpty(updatedTeacher.TeacherLname))
          errors.Add("Teacher last name is required.");
      if (string.IsNullOrEmpty(updatedTeacher.EmployeeNumber) || !updatedTeacher.EmployeeNumber.StartsWith("T"))
          errors.Add("Employee number must start with 'T'.");
      if (updatedTeacher.HireDate == null || updatedTeacher.HireDate > DateTime.Now)
          errors.Add("Hire date cannot be in the future.");
      if (updatedTeacher.Salary < 0)
          errors.Add("Salary must be a positive number.");

      // If there are validation errors, return them
      if (errors.Any())
      {
          return BadRequest(new { success = false, message = "Invalid input data.", errors });
      }

      try
      {
        using (var connection = _context.AccessDatabase())
        {
          connection.Open();

          // Check if the teacher exists
          var checkCmd = connection.CreateCommand();
          checkCmd.CommandText = "SELECT COUNT(*) FROM Teachers WHERE teacherid = @id";
          checkCmd.Parameters.AddWithValue("@id", id);
          int teacherExists = Convert.ToInt32(checkCmd.ExecuteScalar());
          if (teacherExists == 0)
          {
            return NotFound(new { success = false, message = "Teacher not found." });
          }

          // Check for duplicate EmployeeNumber in other teachers
          var duplicateCheckCmd = connection.CreateCommand();
          duplicateCheckCmd.CommandText = @"
            SELECT COUNT(*) 
            FROM Teachers 
            WHERE LOWER(employeenumber) = LOWER(@EmployeeNumber) AND teacherid != @id";
          duplicateCheckCmd.Parameters.AddWithValue("@EmployeeNumber", updatedTeacher.EmployeeNumber);
          duplicateCheckCmd.Parameters.AddWithValue("@id", id);
          int duplicateCount = Convert.ToInt32(duplicateCheckCmd.ExecuteScalar());

          if (duplicateCount > 0)
          {
            return BadRequest(new { success = false, message = $"Employee number '{updatedTeacher.EmployeeNumber}' already exists for another teacher." });
          }


          Console.WriteLine($"Updating Teacher: ID={id}, Fname={updatedTeacher.TeacherFname}, Lname={updatedTeacher.TeacherLname}, Salary={updatedTeacher.Salary}");

          // Update the teacher record
          var updateCmd = connection.CreateCommand();
          updateCmd.CommandText = @"
              UPDATE Teachers 
              SET teacherfname = @TeacherFname, 
                  teacherlname = @TeacherLname, 
                  employeenumber = @EmployeeNumber, 
                  hiredate = @HireDate, 
                  salary = @Salary,
                  teacherworkphone = @TeacherWorkPhone 
              WHERE teacherid = @id";
          updateCmd.Parameters.AddWithValue("@TeacherFname", updatedTeacher.TeacherFname);
          updateCmd.Parameters.AddWithValue("@TeacherLname", updatedTeacher.TeacherLname);
          updateCmd.Parameters.AddWithValue("@EmployeeNumber", updatedTeacher.EmployeeNumber);
          updateCmd.Parameters.AddWithValue("@HireDate", updatedTeacher.HireDate);
          updateCmd.Parameters.AddWithValue("@Salary", updatedTeacher.Salary);
          updateCmd.Parameters.AddWithValue("@TeacherWorkPhone", updatedTeacher.TeacherWorkPhone ?? (object)DBNull.Value);
          updateCmd.Parameters.AddWithValue("@id", id);
          updateCmd.ExecuteNonQuery();
        }

      return Ok(new { success = true, message = "Teacher updated successfully." });
    }

    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
      Console.WriteLine($"Stack Trace: {ex.StackTrace}");
      return StatusCode(500, new { success = false, message = "An error occurred while updating the teacher.", details = ex.Message });
    }

    }

  }
}
