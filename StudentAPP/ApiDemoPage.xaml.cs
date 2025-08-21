using StudentAPP.Services;
using System.Text.Json;

namespace StudentAPP;

public partial class ApiDemoPage : ContentPage
{
    private readonly MockApiService _apiService;

    public ApiDemoPage()
    {
        InitializeComponent();
        _apiService = new MockApiService();
    }

    // GET Operations
    private async void OnGetStudentsClicked(object sender, EventArgs e)
    {
        await ExecuteApiCall(async () =>
        {
            var students = await _apiService.GetStudentsAsync();
            return new
            {
                endpoint = "GET /api/students",
                status = "200 OK",
                count = students.Count,
                data = students.Select(s => new { s.StudentId, s.FullName, s.Email }).ToList()
            };
        });
    }

    private async void OnGetStudentByIdClicked(object sender, EventArgs e)
    {
        await ExecuteApiCall(async () =>
        {
            var student = await _apiService.GetStudentByIdAsync("STU001");
            return new
            {
                endpoint = "GET /api/students/STU001",
                status = "200 OK",
                data = new { student.StudentId, student.FullName, student.Email, student.Phone, student.LastLoginAt }
            };
        });
    }

    private async void OnGetModulesClicked(object sender, EventArgs e)
    {
        await ExecuteApiCall(async () =>
        {
            var modules = await _apiService.GetModulesAsync();
            return new
            {
                endpoint = "GET /api/modules",
                status = "200 OK",
                count = modules.Count,
                data = modules.Select(m => new { m.ModuleCode, m.ModuleName, m.Instructor, m.Credits }).ToList()
            };
        });
    }

    private async void OnGetEnrollmentsClicked(object sender, EventArgs e)
    {
        await ExecuteApiCall(async () =>
        {
            var enrollments = await _apiService.GetStudentEnrollmentsAsync("STU001");
            return new
            {
                endpoint = "GET /api/enrollments/STU001",
                status = "200 OK",
                count = enrollments.Count,
                data = enrollments.Select(e => new { e.ModuleCode, e.Status, e.EnrollmentDate, e.Grade }).ToList()
            };
        });
    }

    // POST Operations
    private async void OnCreateStudentClicked(object sender, EventArgs e)
    {
        await ExecuteApiCall(async () =>
        {
            var request = new CreateStudentRequest
            {
                FullName = "Alice Cooper",
                Email = "alice.cooper@portal.com",
                Phone = "+1-555-0199"
            };

            var response = await _apiService.CreateStudentAsync(request);
            return new
            {
                endpoint = "POST /api/students",
                status = response.Success ? "201 Created" : "400 Bad Request",
                request = request,
                response = new { response.Success, response.Message, response.Data }
            };
        });
    }

    private async void OnEnrollStudentClicked(object sender, EventArgs e)
    {
        await ExecuteApiCall(async () =>
        {
            var request = new EnrollmentRequest
            {
                StudentId = "STU001",
                ModuleCode = "BUS301"
            };

            var response = await _apiService.EnrollStudentAsync(request);
            return new
            {
                endpoint = "POST /api/enrollments",
                status = response.Success ? "201 Created" : "400 Bad Request",
                request = request,
                response = new { response.Success, response.Message, response.Data }
            };
        });
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await ExecuteApiCall(async () =>
        {
            var request = new LoginRequest
            {
                StudentId = "STU001",
                Password = "demo123"
            };

            var response = await _apiService.LoginAsync(request);
            return new
            {
                endpoint = "POST /api/auth/login",
                status = response.Success ? "200 OK" : "401 Unauthorized",
                request = new { request.StudentId, Password = "***" }, // Hide password in response
                response = new
                {
                    response.Success,
                    response.Message,
                    Token = response.Data?.Substring(0, Math.Min(20, response.Data?.Length ?? 0)) + "..."
                }
            };
        });
    }

    // Other Operations
    private async void OnUpdateStudentClicked(object sender, EventArgs e)
    {
        await ExecuteApiCall(async () =>
        {
            var request = new UpdateStudentRequest
            {
                Phone = "+1-555-0999"
            };

            var response = await _apiService.UpdateStudentAsync("STU001", request);
            return new
            {
                endpoint = "PUT /api/students/STU001",
                status = response.Success ? "200 OK" : "404 Not Found",
                request = request,
                response = new { response.Success, response.Message, response.Data }
            };
        });
    }

    private async void OnDeleteStudentClicked(object sender, EventArgs e)
    {
        await ExecuteApiCall(async () =>
        {
            var response = await _apiService.DeleteStudentAsync("STU003");
            return new
            {
                endpoint = "DELETE /api/students/STU003",
                status = response.Success ? "200 OK" : "404 Not Found",
                response = new { response.Success, response.Message, Deleted = response.Data }
            };
        });
    }

    private void OnClearResponseClicked(object sender, EventArgs e)
    {
        ResponseLabel.Text = "Response will appear here...";
    }

    // Helper method to execute API calls with loading indicator
    private async Task ExecuteApiCall(Func<Task<object>> apiCall)
    {
        try
        {
            // Show loading indicator
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            ResponseLabel.Text = "Loading...";

            // Execute the API call
            var result = await apiCall();

            // Format and display the response
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonResponse = JsonSerializer.Serialize(result, jsonOptions);
            ResponseLabel.Text = jsonResponse;
        }
        catch (Exception ex)
        {
            // Display error response
            var errorResponse = new
            {
                error = true,
                message = ex.Message,
                timestamp = DateTime.Now
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            ResponseLabel.Text = JsonSerializer.Serialize(errorResponse, jsonOptions);
        }
        finally
        {
            // Hide loading indicator
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
}