using SQLite;

namespace StudentAPP.Models;

[Table("Students")]
public class Student
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string StudentId { get; set; } = string.Empty;

    [NotNull]
    public string FullName { get; set; } = string.Empty;

    [Unique, NotNull]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    [NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property - not stored in DB
    [Ignore]
    public List<Enrollment> Enrollments { get; set; } = new();
}

[Table("Modules")]
public class Module
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string ModuleCode { get; set; } = string.Empty;

    [NotNull]
    public string ModuleName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Instructor { get; set; } = string.Empty;

    public int Credits { get; set; }

    public int Duration { get; set; } // Duration in weeks

    public int MaxCapacity { get; set; }

    public int CurrentEnrollment { get; set; }

    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("Enrollments")]
public class Enrollment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string StudentId { get; set; } = string.Empty;

    [NotNull]
    public string ModuleCode { get; set; } = string.Empty;

    public DateTime EnrollmentDate { get; set; } = DateTime.Now;

    public string Status { get; set; } = "Enrolled"; // Enrolled, Completed, Dropped, Waitlist

    public double? Grade { get; set; }

    public DateTime? CompletionDate { get; set; }

    // Navigation properties - not stored in DB
    [Ignore]
    public Student? Student { get; set; }

    [Ignore]
    public Module? Module { get; set; }
}