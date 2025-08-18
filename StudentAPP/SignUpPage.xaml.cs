using Microsoft.Maui.Controls;

namespace StudentAPP;

public partial class SignUpPage : ContentPage
{
    public SignUpPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        // Validate all inputs
        if (!ValidateInputs())
        {
            return; // Validation failed, error already shown
        }

        // Disable button to prevent multiple submissions
        SignUpButton.IsEnabled = false;
        SignUpButton.Text = "Creating Account...";

        try
        {
            // Create user object with form data
            var newUser = new UserRegistrationData
            {
                FullName = FullNameEntry.Text.Trim(),
                Email = EmailEntry.Text.Trim().ToLower(),
                Phone = PhoneEntry.Text.Trim(),
                Password = PasswordEntry.Text
            };

            // Attempt to register user
            bool registrationSuccess = await RegisterUserAsync(newUser);

            if (registrationSuccess)
            {
                await DisplayAlert("Success",
                    "Account created successfully! You can now sign in with your credentials.",
                    "OK");

                // Clear the form
                ClearForm();

                // Navigate back to sign-in page
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error",
                    "Registration failed. This email might already be registered.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Registration failed: {ex.Message}", "OK");
        }
        finally
        {
            // Re-enable button
            SignUpButton.IsEnabled = true;
            SignUpButton.Text = "Create Account";
        }
    }

    private async void OnSignInTapped(object sender, EventArgs e)
    {
        // Navigate back to Sign In page
        await Navigation.PopAsync();
    }

    private async void OnTermsTapped(object sender, EventArgs e)
    {
        // Show terms and conditions
        string terms = @"Terms and Conditions

1. Account Usage
   - You must provide accurate information
   - You are responsible for your account security
   - One account per person

2. Privacy
   - We protect your personal information
   - Data is used only for app functionality
   - We don't share data with third parties

3. Acceptable Use
   - Use the app responsibly
   - No illegal activities
   - Respect other users

4. Changes
   - Terms may be updated periodically
   - Continued use implies acceptance

For full terms, visit our website.";

        await DisplayAlert("Terms and Conditions", terms, "I Understand");
    }

    // Comprehensive input validation
    private bool ValidateInputs()
    {
        // Check if all required fields are filled
        if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
        {
            ShowValidationError("Please enter your full name.");
            FullNameEntry.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            ShowValidationError("Please enter your email address.");
            EmailEntry.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            ShowValidationError("Please enter your phone number.");
            PhoneEntry.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowValidationError("Please enter a password.");
            PasswordEntry.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            ShowValidationError("Please confirm your password.");
            ConfirmPasswordEntry.Focus();
            return false;
        }

        // Validate full name (should contain at least first and last name)
        if (FullNameEntry.Text.Trim().Split(' ').Length < 2)
        {
            ShowValidationError("Please enter your full name (first and last name).");
            FullNameEntry.Focus();
            return false;
        }

        // Validate email format
        if (!IsValidEmail(EmailEntry.Text))
        {
            ShowValidationError("Please enter a valid email address.");
            EmailEntry.Focus();
            return false;
        }

        // Validate phone number
        if (!IsValidPhoneNumber(PhoneEntry.Text))
        {
            ShowValidationError("Please enter a valid phone number (10-15 digits).");
            PhoneEntry.Focus();
            return false;
        }

        // Check password requirements
        if (!IsValidPassword(PasswordEntry.Text))
        {
            ShowValidationError("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number.");
            PasswordEntry.Focus();
            return false;
        }

        // Check if passwords match
        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ShowValidationError("Passwords do not match.");
            ConfirmPasswordEntry.Focus();
            return false;
        }

        // Check if terms are accepted
        if (!TermsCheckBox.IsChecked)
        {
            ShowValidationError("Please accept the Terms and Conditions to continue.");
            return false;
        }

        return true; // All validation passed
    }

    // Helper method to show validation errors
    private async void ShowValidationError(string message)
    {
        await DisplayAlert("Validation Error", message, "OK");
    }

    // Email validation helper
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

    // Phone number validation
    private bool IsValidPhoneNumber(string phone)
    {
        // Remove common formatting characters
        string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

        // Check if it's all digits and has reasonable length
        return cleanPhone.All(char.IsDigit) && cleanPhone.Length >= 10 && cleanPhone.Length <= 15;
    }

    // Password validation with strength requirements
    private bool IsValidPassword(string password)
    {
        if (password.Length < 8)
            return false;

        bool hasUpper = password.Any(c => char.IsUpper(c));
        bool hasLower = password.Any(c => char.IsLower(c));
        bool hasDigit = password.Any(c => char.IsDigit(c));

        return hasUpper && hasLower && hasDigit;
    }

    // Clear all form fields
    private void ClearForm()
    {
        FullNameEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        ConfirmPasswordEntry.Text = string.Empty;
        TermsCheckBox.IsChecked = false;
    }

    // User registration method (replace with actual implementation)
    private async Task<bool> RegisterUserAsync(UserRegistrationData userData)
    {
        // Simulate API call delay
        await Task.Delay(2500);

        // TODO: Replace with actual registration logic
        // Example implementation:
        /*
        try
        {
            var registrationRequest = new RegistrationRequest
            {
                FullName = userData.FullName,
                Email = userData.Email,
                Phone = userData.Phone,
                Password = userData.Password // Note: Hash this before sending!
            };

            var result = await _userService.RegisterAsync(registrationRequest);
            
            if (result.IsSuccess)
            {
                // Optionally send verification email
                await _emailService.SendVerificationEmailAsync(userData.Email);
                return true;
            }
            
            return false;
        }
        catch (HttpRequestException)
        {
            throw new Exception("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Registration failed: {ex.Message}");
        }
        */

        // For demo purposes: simulate success for valid data
        // In real app, check if email already exists in database
        return !userData.Email.Contains("test@existing.com"); // Simulate existing email check
    }
}

// Data model for user registration
public class UserRegistrationData
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}