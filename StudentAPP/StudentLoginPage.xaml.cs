using Microsoft.Maui.Controls;
using StudentAPP.Services;

namespace StudentAPP;

public partial class StudentLoginPage : ContentPage
{
    private bool isPasswordVisible = false;
    private readonly DatabaseService _databaseService;

    public StudentLoginPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await PerformLoginAsync();
    }

    private async Task PerformLoginAsync()
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(StudentIdEntry?.Text) || string.IsNullOrWhiteSpace(PasswordEntry?.Text))
        {
            await DisplayAlert("Login Error", "Please enter both Student ID and Password", "OK");
            return;
        }

        // Show loading state
        LoginButton.IsEnabled = false;
        LoginButton.Text = "Logging in...";

        try
        {
            // Initialize database
            await _databaseService.InitializeDatabaseAsync();

            // Authenticate using database
            var loginResult = await AuthenticateStudentAsync(StudentIdEntry.Text, PasswordEntry.Text);

            if (loginResult.IsSuccess)
            {
                // Update last login time in database
                await _databaseService.UpdateLastLoginAsync(loginResult.StudentId);

                // Store login session
                await SecureStorage.SetAsync("student_id", loginResult.StudentId);
                await SecureStorage.SetAsync("student_name", loginResult.StudentName);
                await SecureStorage.SetAsync("student_email", loginResult.Email);
                await SecureStorage.SetAsync("is_logged_in", "true");

                await DisplayAlert("Success", $"Welcome back, {loginResult.StudentName}!", "Continue");

                Application.Current!.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Login Failed", $"❌ {loginResult.ErrorMessage}\n\nTry these demo accounts:\n• STU001 / demo123\n• STU002 / password\n• STU003 / student123", "Try Again");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoginButton.Text = "Login to Portal";
        }
    }

    private async void OnDemoStudentClicked(object sender, EventArgs e)
    {
        // Fill in demo credentials
        if (StudentIdEntry != null && PasswordEntry != null)
        {
            StudentIdEntry.Text = "STU001";
            PasswordEntry.Text = "demo123";

            // Auto login
            await PerformLoginAsync();
        }
    }

    private async void OnGuestAccessClicked(object sender, EventArgs e)
    {
        // Provide limited guest access
        await SecureStorage.SetAsync("student_id", "GUEST");
        await SecureStorage.SetAsync("student_name", "Guest User");
        await SecureStorage.SetAsync("student_email", "guest@portal.com");
        await SecureStorage.SetAsync("is_logged_in", "guest");

        await DisplayAlert("Guest Access", "Welcome! You have limited access to browse modules.", "Continue");

        // ✅ FIXED: Use Shell navigation for guest too
        Application.Current!.MainPage = new AppShell();
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        PasswordEntry.IsPassword = !isPasswordVisible;

        // Change from Text to Source for ImageButton
        ShowPasswordBtn.Source = isPasswordVisible ? "hide.png" : "view.png";
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        string? studentId = await DisplayPromptAsync(
            "Reset Password",
            "Enter your Student ID to reset password:",
            "Send Reset Link",
            "Cancel",
            "Student ID");

        if (!string.IsNullOrWhiteSpace(studentId))
        {
            // Check if student exists in database
            await _databaseService.InitializeDatabaseAsync();
            var student = await _databaseService.GetStudentByIdAsync(studentId.ToUpper());

            if (student != null)
            {
                await DisplayAlert("Password Reset",
                    $"✅ A password reset link has been sent to {student.Email}",
                    "OK");
            }
            else
            {
                await DisplayAlert("Student Not Found",
                    $"❌ No student found with ID: {studentId.ToUpper()}\n\nPlease check your Student ID and try again.",
                    "OK");
            }
        }
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        // ✅ FIXED: Use proper navigation to registration
        await Navigation.PushAsync(new StudentRegistrationPage());
    }

    // Database-backed authentication service
    private async Task<LoginResult> AuthenticateStudentAsync(string studentId, string password)
    {
        try
        {
            // Get student from database
            var student = await _databaseService.GetStudentByIdAsync(studentId.ToUpper());

            if (student == null)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Student ID not found"
                };
            }

            // Validate password using database service
            bool isValidPassword = await _databaseService.ValidateStudentCredentialsAsync(student.StudentId, password);

            if (isValidPassword)
            {
                return new LoginResult
                {
                    IsSuccess = true,
                    StudentName = student.FullName,
                    StudentId = student.StudentId,
                    Email = student.Email
                };
            }
            else
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid password"
                };
            }
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                IsSuccess = false,
                ErrorMessage = $"Authentication error: {ex.Message}"
            };
        }
    }
}

// Helper classes
public class LoginResult
{
    public bool IsSuccess { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}