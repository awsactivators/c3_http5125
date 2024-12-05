namespace SchoolDatabaseCTwo.Models
{
  /// <summary>
  /// Represents a student in the school system.
  /// </summary>
  public class Student
  {
    // Properties of the Student entity
    public int StudentId { get; set; }
    public string? StudentFname { get; set; }
    public string? StudentLname { get; set; }
    public string? StudentNumber { get; set; }
    public DateTime EnrolDate { get; set; }
    
    // List of courses enrolled by the student
    public List<Course> Courses { get; set; } = new List<Course>();
  }
}