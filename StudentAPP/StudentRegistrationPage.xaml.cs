using StudentAPP.Services;
using StudentAPP.Models;

namespace StudentAPP;

public partial class StudentRegistrationPage : ContentPage
{
    private bool isPasswordVisible = false;
    private readonly DatabaseService _databaseService;

    public StudentRegistrationPage()
    {
        InitializeComponent();

        // Get database service from dependency injection
        _databaseService = Handler?.MauiContext?.Services.GetService<DatabaseService>()
                          ?? new DatabaseService();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            return; // Validation failed
        }

        RegisterButton.IsEnabled = false;
        RegisterButton.Text = "Creating Account...";

        try
        {
            // Initialize database
            await _databaseService.InitializeDatabaseAsync();

            // Create new student
            var newStudent = new Student
            {
                StudentId = StudentIdEntry.Text.Trim().ToUpper(),
                FullName = FullNameEntry.Text.Trim(),
                Email = EmailEntry.Text.Trim().ToLower(),
                Phone = PhoneEntry.Text.Trim(),
                PasswordHash = PasswordEntry.Text, // Will be hashed in DatabaseService
                CreatedAt = DateTime.Now,
                LastLoginAt = DateTime.Now,
                IsActive = true
            };

            // Try to create the student
            bool success = await _databaseService.CreateStudentAsync(newStudent);

            if (success)
            {
                await DisplayAlert("Success!",
                    $"🎉 Account created successfully!\n\n" +
                    $"Student ID: {newStudent.StudentId}\n" +
                    $"Email: {newStudent.Email}\n\n" +
                    $"You can now sign in with either your Student ID or email address.",
                    "Continue");

                // Navigate back to login
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Registration Failed",
                    "❌ This Student ID or Email is already registered.\n\nPlease try different credentials.",
                    "Try Again");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Registration failed: {ex.Message}", "OK");
        }
        finally
        {
            RegisterButton.IsEnabled = true;
            RegisterButton.Text = "Create Account";
        }
    }

    private bool ValidateInputs()
    {
        // Check all required fields
        if (string.IsNullOrWhiteSpace(FullNameEntry?.Text))
        {
            ShowError("Please enter your full name.");
            FullNameEntry?.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry?.Text))
        {
            ShowError("Please enter your email address.");
            EmailEntry?.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(PhoneEntry?.Text))
        {
            ShowError("Please enter your phone number.");
            PhoneEntry?.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(StudentIdEntry?.Text))
        {
            ShowError("Please enter a desired Student ID.");
            StudentIdEntry?.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry?.Text))
        {
            ShowError("Please enter a password.");
            PasswordEntry?.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(ConfirmPasswordEntry?.Text))
        {
            ShowError("Please confirm your password.");
            ConfirmPasswordEntry?.Focus();
            return false;
        }

        // Validate full name format
        if (FullNameEntry.Text.Trim().Split(' ').Length < 2)
        {
            ShowError("Please enter your first and last name.");
            FullNameEntry.Focus();
            return false;
        }

        // Validate email format
        if (!IsValidEmail(EmailEntry.Text))
        {
            ShowError("Please enter a valid email address.");
            EmailEntry.Focus();
            return false;
        }

        // Validate phone number
        if (!IsValidPhoneNumber(PhoneEntry.Text))
        {
            ShowError("Please enter a valid phone number (10-15 digits).");
            PhoneEntry.Focus();
            return false;
        }

        // Validate Student ID format
        if (StudentIdEntry.Text.Length < 4 || StudentIdEntry.Text.Length > 10)
        {
            ShowError("Student ID must be between 4-10 characters.");
            StudentIdEntry.Focus();
            return false;
        }

        // Validate password strength
        if (PasswordEntry.Text.Length < 6)
        {
            ShowError("Password must be at least 6 characters long.");
            PasswordEntry.Focus();
            return false;
        }

        // Check if passwords match
        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ShowError("Passwords do not match.");
            ConfirmPasswordEntry.Focus();
            return false;
        }

        // Check terms acceptance
        if (!TermsCheckBox.IsChecked)
        {
            ShowError("Please accept the Terms of Service and Privacy Policy.");
            return false;
        }

        return true;
    }

    private async void ShowError(string message)
    {
        await DisplayAlert("Registration Error", message, "OK");
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhoneNumber(string phone)
    {
        string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
        return cleanPhone.All(char.IsDigit) && cleanPhone.Length >= 10 && cleanPhone.Length <= 15;
    }

    private async Task<bool> RegisterStudentAsync(RegistrationData data)
    {
        // Simulate API call delay
        await Task.Delay(2500);

        // Mock registration logic - check for existing IDs
        var existingIds = new[] { "STU001", "STU002", "STU003", "ADMIN" };
        var existingEmails = new[] { "sthesham@gmail.com", "jane@student.com", "admin@school.com" };

        if (existingIds.Contains(data.StudentId))
        {
            return false; // Student ID already exists
        }

        if (existingEmails.Contains(data.Email))
        {
            return false; // Email already exists
        }

        // In a real app, you would:
        // 1. Hash the password
        // 2. Save to database
        // 3. Send verification email
        // 4. Return success/failure

        return true; // Registration successful
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        PasswordEntry.IsPassword = !isPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !isPasswordVisible;
        ShowPasswordBtn.Source = isPasswordVisible ? "hide.png" : "view.png";
    }

    private async void OnTermsTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Terms of Service",
            "By using this app, you agree to:\n\n• Use the platform responsibly\n• Provide accurate information\n• Respect other users\n• Follow academic integrity policies\n\nFull terms available at: www.studentportal.com/terms",
            "I Understand");
    }

    private async void OnPrivacyTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Privacy Policy",
            "We protect your privacy by:\n\n• Only collecting necessary information\n• Never sharing data with third parties\n• Securing all personal data\n• Allowing you to control your information\n\nFull policy available at: www.studentportal.com/privacy",
            "I Understand");
    }

    private async void OnSignInTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

// Data model for registration
public class RegistrationData
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}