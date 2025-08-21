using System.Net.Http.Json;
using System.Text.Json;

namespace StudentAPP.Services;

public class MockApiService
{
    private readonly HttpClient _httpClient;
    private readonly List<MockStudent> _mockStudents;
    private readonly List<MockModule> _mockModules;

    public MockApiService()
    {
        _httpClient = new HttpClient();
        _mockStudents = InitializeMockStudents();
        _mockModules = InitializeMockModules();
    }

    // GET Methods
    public async Task<List<MockStudent>> GetStudentsAsync()
    {
        // Simulate API delay
        await Task.Delay(1000);

        // Return mock data
        return _mockStudents;
    }

    public async Task<MockStudent> GetStudentByIdAsync(string studentId)
    {
        await Task.Delay(500);

        var student = _mockStudents.FirstOrDefault(s => s.StudentId == studentId);
        if (student == null)
            throw new Exception($"Student with ID {studentId} not found");

        return student;
    }

    public async Task<List<MockModule>> GetModulesAsync()
    {
        await Task.Delay(800);
        return _mockModules;
    }

    public async Task<List<MockEnrollment>> GetStudentEnrollmentsAsync(string studentId)
    {
        await Task.Delay(600);

        // Mock enrollments for the student
        return new List<MockEnrollment>
        {
            new MockEnrollment
            {
                Id = 1,
                StudentId = studentId,
                ModuleCode = "CS101",
                Status = "Enrolled",
                EnrollmentDate = DateTime.Now.AddDays(-30),
                Grade = null
            },
            new MockEnrollment
            {
                Id = 2,
                StudentId = studentId,
                ModuleCode = "MATH201",
                Status = "Completed",
                EnrollmentDate = DateTime.Now.AddDays(-90),
                Grade = "A-"
            }
        };
    }

    // POST Methods
    public async Task<ApiResponse<MockStudent>> CreateStudentAsync(CreateStudentRequest request)
    {
        await Task.Delay(1200);

        // Validate request
        if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email))
        {
            return new ApiResponse<MockStudent>
            {
                Success = false,
                Message = "Name and email are required",
                Data = null
            };
        }

        // Check if email already exists
        if (_mockStudents.Any(s => s.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return new ApiResponse<MockStudent>
            {
                Success = false,
                Message = "Email already exists",
                Data = null
            };
        }

        // Create new student
        var newStudent = new MockStudent
        {
            StudentId = $"STU{_mockStudents.Count + 100:000}",
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.Now,
            LastLoginAt = DateTime.Now
        };

        _mockStudents.Add(newStudent);

        return new ApiResponse<MockStudent>
        {
            Success = true,
            Message = "Student created successfully",
            Data = newStudent
        };
    }

    public async Task<ApiResponse<MockEnrollment>> EnrollStudentAsync(EnrollmentRequest request)
    {
        await Task.Delay(1000);

        // Validate student exists
        var student = _mockStudents.FirstOrDefault(s => s.StudentId == request.StudentId);
        if (student == null)
        {
            return new ApiResponse<MockEnrollment>
            {
                Success = false,
                Message = "Student not found",
                Data = null
            };
        }

        // Validate module exists
        var module = _mockModules.FirstOrDefault(m => m.ModuleCode == request.ModuleCode);
        if (module == null)
        {
            return new ApiResponse<MockEnrollment>
            {
                Success = false,
                Message = "Module not found",
                Data = null
            };
        }

        // Create enrollment
        var enrollment = new MockEnrollment
        {
            Id = Random.Shared.Next(1000, 9999),
            StudentId = request.StudentId,
            ModuleCode = request.ModuleCode,
            Status = "Enrolled",
            EnrollmentDate = DateTime.Now,
            Grade = null
        };

        return new ApiResponse<MockEnrollment>
        {
            Success = true,
            Message = $"Successfully enrolled in {module.ModuleName}",
            Data = enrollment
        };
    }

    public async Task<ApiResponse<string>> LoginAsync(LoginRequest request)
    {
        await Task.Delay(800);

        // Validate credentials
        var student = _mockStudents.FirstOrDefault(s =>
            s.StudentId.Equals(request.StudentId, StringComparison.OrdinalIgnoreCase));

        if (student == null || request.Password != "demo123")
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid credentials",
                Data = null
            };
        }

        // Update last login
        student.LastLoginAt = DateTime.Now;

        // Return mock JWT token
        var token = $"mock_jwt_token_{student.StudentId}_{DateTime.Now.Ticks}";

        return new ApiResponse<string>
        {
            Success = true,
            Message = "Login successful",
            Data = token
        };
    }

    // PUT Method
    public async Task<ApiResponse<MockStudent>> UpdateStudentAsync(string studentId, UpdateStudentRequest request)
    {
        await Task.Delay(900);

        var student = _mockStudents.FirstOrDefault(s => s.StudentId == studentId);
        if (student == null)
        {
            return new ApiResponse<MockStudent>
            {
                Success = false,
                Message = "Student not found",
                Data = null
            };
        }

        // Update fields
        if (!string.IsNullOrEmpty(request.FullName))
            student.FullName = request.FullName;
        if (!string.IsNullOrEmpty(request.Phone))
            student.Phone = request.Phone;
        if (!string.IsNullOrEmpty(request.Email))
            student.Email = request.Email;

        return new ApiResponse<MockStudent>
        {
            Success = true,
            Message = "Student updated successfully",
            Data = student
        };
    }

    // DELETE Method
    public async Task<ApiResponse<bool>> DeleteStudentAsync(string studentId)
    {
        await Task.Delay(600);

        var student = _mockStudents.FirstOrDefault(s => s.StudentId == studentId);
        if (student == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Student not found",
                Data = false
            };
        }

        _mockStudents.Remove(student);

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Student deleted successfully",
            Data = true
        };
    }

    private List<MockStudent> InitializeMockStudents()
    {
        return new List<MockStudent>
        {
            new MockStudent
            {
                StudentId = "STU001",
                FullName = "John Doe",
                Email = "john.doe@portal.com",
                Phone = "+1-555-0123",
                CreatedAt = DateTime.Now.AddMonths(-6),
                LastLoginAt = DateTime.Now.AddHours(-2)
            },
            new MockStudent
            {
                StudentId = "STU002",
                FullName = "Jane Smith",
                Email = "jane.smith@portal.com",
                Phone = "+1-555-0124",
                CreatedAt = DateTime.Now.AddMonths(-4),
                LastLoginAt = DateTime.Now.AddDays(-1)
            },
            new MockStudent
            {
                StudentId = "STU00",
                FullName = "Mike Johnson",
                Email = "mike.johnson@portal.com",
                Phone = "+1-555-0125",
                CreatedAt = DateTime.Now.AddMonths(-3),
                LastLoginAt = DateTime.Now.AddHours(-5)
            }
        };
    }

    private List<MockModule> InitializeMockModules()
    {
        return new List<MockModule>
        {
            new MockModule
            {
                ModuleCode = "CS101",
                ModuleName = "Introduction to Computer Science",
                Instructor = "Dr. Sarah Johnson",
                Credits = 3,
                Category = "Computer Science"
            },
            new MockModule
            {
                ModuleCode = "MATH201",
                ModuleName = "Mathematics for Engineers",
                Instructor = "Dr. Emily Rodriguez",
                Credits = 4,
                Category = "Mathematics"
            },
            new MockModule
            {
                ModuleCode = "BUS301",
                ModuleName = "Business Analytics",
                Instructor = "Prof. Amanda Taylor",
                Credits = 3,
                Category = "Business"
            }
        };
    }
}

// Mock Data Models (separate from your existing ones)
public class MockStudent
{
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
}

public class MockModule
{
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class MockEnrollment
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public string? Grade { get; set; }
}

// Request Models
public class CreateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class UpdateStudentRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class EnrollmentRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Response Model
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}