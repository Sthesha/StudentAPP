using SQLite;
using StudentAPP.Models;
using System.Security.Cryptography;
using System.Text;

namespace StudentAPP.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    public async Task InitializeDatabaseAsync()
    {
        if (_database != null)
            return;

        // Get database path
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "StudentPortal.db");

        // Create connection
        _database = new SQLiteAsyncConnection(databasePath);

        // Create tables
        await _database.CreateTableAsync<Student>();
        await _database.CreateTableAsync<Module>();
        await _database.CreateTableAsync<Enrollment>();

        // Seed initial data
        await SeedInitialDataAsync();
    }

    private async Task SeedInitialDataAsync()
    {
        // Check if data already exists
        var studentCount = await _database!.Table<Student>().CountAsync();
        if (studentCount > 0)
            return; // Data already seeded

        // Seed some initial students
        var students = new List<Student>
        {
            new Student
            {
                StudentId = "STU001",
                FullName = "Sthembiso Mkhwanazi",
                Email = "stheshaam@gmail.com",
                Phone = "+1234567890",
                PasswordHash = HashPassword("demo123"),
                LastLoginAt = DateTime.Now.AddDays(-1)
            },
            new Student
            {
                StudentId = "STU002",
                FullName = "Sipho Zungu",
                Email = "sipho@student.com",
                Phone = "+1234567891",
                PasswordHash = HashPassword("password"),
                LastLoginAt = DateTime.Now.AddDays(-2)
            },
            new Student
            {
                StudentId = "STU003",
                FullName = "Mike Johnson",
                Email = "mike@student.com",
                Phone = "+1234567892",
                PasswordHash = HashPassword("student123"),
                LastLoginAt = DateTime.Now.AddDays(-3)
            }
        };

        await _database.InsertAllAsync(students);

        // Seed some initial modules
        var modules = new List<Module>
        {
            new Module
            {
                ModuleCode = "CS101",
                ModuleName = "Introduction to Computer Science",
                Description = "Learn the fundamentals of computer science including programming basics, algorithms, and problem-solving techniques.",
                Instructor = "Dr. Sarah Dlamini",
                Credits = 3,
                Duration = 12,
                MaxCapacity = 30,
                CurrentEnrollment = 25,
                Category = "Computer Science"
            },
            new Module
            {
                ModuleCode = "CS301",
                ModuleName = "Database Management Systems",
                Description = "Comprehensive study of database design, SQL, normalization, and database administration concepts.",
                Instructor = "Prof. Sandile Zulu",
                Credits = 4,
                Duration = 16,
                MaxCapacity = 25,
                CurrentEnrollment = 20,
                Category = "Computer Science"
            },
            new Module
            {
                ModuleCode = "MATH101",
                ModuleName = "Calculus I",
                Description = "Introduction to differential and integral calculus with applications in science and engineering.",
                Instructor = "Dr. Emily Rodriguez",
                Credits = 4,
                Duration = 14,
                MaxCapacity = 35,
                CurrentEnrollment = 30,
                Category = "Mathematics"
            },
            new Module
            {
                ModuleCode = "MATH201",
                ModuleName = "Linear Algebra",
                Description = "Vector spaces, matrices, determinants, eigenvalues, and linear transformations.",
                Instructor = "Dr. Robert Kim",
                Credits = 3,
                Duration = 12,
                MaxCapacity = 25,
                CurrentEnrollment = 22,
                Category = "Mathematics"
            },
            new Module
            {
                ModuleCode = "ENG101",
                ModuleName = "Engineering Mechanics",
                Description = "Statics and dynamics of particles and rigid bodies, force analysis, and equilibrium.",
                Instructor = "Prof. David Wilson",
                Credits = 4,
                Duration = 16,
                MaxCapacity = 20,
                CurrentEnrollment = 18,
                Category = "Engineering"
            },
            new Module
            {
                ModuleCode = "BUS201",
                ModuleName = "Business Analytics",
                Description = "Data-driven decision making, statistical analysis, and business intelligence tools.",
                Instructor = "Prof. Amanda Taylor",
                Credits = 3,
                Duration = 10,
                MaxCapacity = 30,
                CurrentEnrollment = 28,
                Category = "Business"
            }
        };

        await _database.InsertAllAsync(modules);

        // Seed some initial enrollments
        var enrollments = new List<Enrollment>
        {
            new Enrollment { StudentId = "STU001", ModuleCode = "CS301", Status = "Enrolled" },
            new Enrollment { StudentId = "STU001", ModuleCode = "BUS201", Status = "Enrolled" },
            new Enrollment { StudentId = "STU002", ModuleCode = "MATH101", Status = "Enrolled" },
            new Enrollment { StudentId = "STU002", ModuleCode = "CS101", Status = "Completed", Grade = 85.5, CompletionDate = DateTime.Now.AddMonths(-1) },
            new Enrollment { StudentId = "STU003", ModuleCode = "ENG101", Status = "Enrolled" }
        };

        await _database.InsertAllAsync(enrollments);
    }

    // Student operations
    public async Task<Student?> GetStudentByIdAsync(string studentId)
    {
        await InitializeDatabaseAsync();
        return await _database!.Table<Student>()
            .Where(s => s.StudentId == studentId && s.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<Student?> GetStudentByEmailAsync(string email)
    {
        await InitializeDatabaseAsync();
        return await _database!.Table<Student>()
            .Where(s => s.Email == email.ToLower() && s.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CreateStudentAsync(Student student)
    {
        await InitializeDatabaseAsync();

        try
        {
            // Check if student ID or email already exists
            var existingStudent = await GetStudentByIdAsync(student.StudentId);
            if (existingStudent != null)
                return false;

            var existingEmail = await GetStudentByEmailAsync(student.Email);
            if (existingEmail != null)
                return false;

            // Hash the password
            student.PasswordHash = HashPassword(student.PasswordHash);
            student.Email = student.Email.ToLower();

            await _database!.InsertAsync(student);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateStudentCredentialsAsync(string studentId, string password)
    {
        await InitializeDatabaseAsync();

        var student = await GetStudentByIdAsync(studentId);
        if (student == null)
            return false;

        var hashedPassword = HashPassword(password);
        return student.PasswordHash == hashedPassword;
    }

    public async Task UpdateLastLoginAsync(string studentId)
    {
        await InitializeDatabaseAsync();

        var student = await GetStudentByIdAsync(studentId);
        if (student != null)
        {
            student.LastLoginAt = DateTime.Now;
            await _database!.UpdateAsync(student);
        }
    }

    // Module operations
    public async Task<List<Module>> GetAllModulesAsync()
    {
        await InitializeDatabaseAsync();
        return await _database!.Table<Module>()
            .Where(m => m.IsActive)
            .ToListAsync();
    }

    public async Task<List<Module>> GetModulesByCategoryAsync(string category)
    {
        await InitializeDatabaseAsync();
        return await _database!.Table<Module>()
            .Where(m => m.Category == category && m.IsActive)
            .ToListAsync();
    }

    public async Task<Module?> GetModuleByCodeAsync(string moduleCode)
    {
        await InitializeDatabaseAsync();
        return await _database!.Table<Module>()
            .Where(m => m.ModuleCode == moduleCode && m.IsActive)
            .FirstOrDefaultAsync();
    }

    // Enrollment operations
    public async Task<List<Enrollment>> GetStudentEnrollmentsAsync(string studentId)
    {
        await InitializeDatabaseAsync();
        return await _database!.Table<Enrollment>()
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<bool> EnrollStudentAsync(string studentId, string moduleCode)
    {
        await InitializeDatabaseAsync();

        try
        {
            // Check if already enrolled
            var existingEnrollment = await _database!.Table<Enrollment>()
                .Where(e => e.StudentId == studentId && e.ModuleCode == moduleCode)
                .FirstOrDefaultAsync();

            if (existingEnrollment != null)
                return false;

            // Check module capacity
            var module = await GetModuleByCodeAsync(moduleCode);
            if (module == null || module.CurrentEnrollment >= module.MaxCapacity)
                return false;

            // Create enrollment
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                ModuleCode = moduleCode,
                Status = "Enrolled"
            };

            await _database!.InsertAsync(enrollment);

            // Update module enrollment count
            module.CurrentEnrollment++;
            await _database!.UpdateAsync(module);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsStudentEnrolledAsync(string studentId, string moduleCode)
    {
        await InitializeDatabaseAsync();

        var enrollment = await _database!.Table<Enrollment>()
            .Where(e => e.StudentId == studentId && e.ModuleCode == moduleCode)
            .FirstOrDefaultAsync();

        return enrollment != null;
    }

    // Utility methods
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "StudentPortalSalt"));
        return Convert.ToBase64String(hashedBytes);
    }

    public async Task<int> GetTotalStudentsAsync()
    {
        await InitializeDatabaseAsync();
        return await _database!.Table<Student>().Where(s => s.IsActive).CountAsync();
    }

    public async Task<int> GetTotalModulesAsync()
    {
        await InitializeDatabaseAsync();
        return await _database!.Table<Module>().Where(m => m.IsActive).CountAsync();
    }

    public async Task<int> GetTotalEnrollmentsAsync()
    {
        await InitializeDatabaseAsync();
        return await _database!.Table<Enrollment>().CountAsync();
    }
}