namespace SchoolDatabaseCTwo.Models
{
  /// <summary>
  /// Represents a course in the school system.
  /// </summary>
  public class Course
  {
    // Properties of the Course entity
    public int CourseId { get; set; }
    public string? CourseCode { get; set; }
    public int TeacherId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public string? CourseName { get; set; }

    // List of students enrolled in the courses
    public List<Student> Students { get; set; } = new List<Student>();
  }
}