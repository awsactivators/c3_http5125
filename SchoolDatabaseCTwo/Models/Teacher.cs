namespace SchoolDatabaseCTwo.Models
{
  /// <summary>
  /// Represents a teacher in the school system.
  /// </summary>
  public class Teacher
  {
    // Properties of the Teacher entity
    public int TeacherId { get; set; }
    public string? TeacherFname { get; set; }
    public string? TeacherLname { get; set; }
    public string? EmployeeNumber { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public string? TeacherWorkPhone { get; set; }

    // List of courses taught by the teacher
    public List<Course> Courses { get; set; } = new List<Course>();
  }
}

