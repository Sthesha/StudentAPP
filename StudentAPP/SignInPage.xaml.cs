using Microsoft.Maui.Controls;

namespace StudentAPP;

public partial class SignInPage : ContentPage
{
    public SignInPage()
    {
        InitializeComponent();
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in all fields", "OK");
            return;
        }

        // Validate email format
        if (!IsValidEmail(EmailEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a valid email address", "OK");
            return;
        }

        // Disable button to prevent multiple taps
        SignInButton.IsEnabled = false;
        SignInButton.Text = "Signing In...";

        try
        {
            // Simulate authentication process
            bool authenticationSuccess = await AuthenticateUserAsync(EmailEntry.Text, PasswordEntry.Text);

            if (authenticationSuccess)
            {
                await DisplayAlert("Success", "Sign in successful!", "OK");

                // Clear the form
                ClearForm();

                Application.Current.MainPage = new NavigationPage(new ModulesPage());

            }
            else
            {
                await DisplayAlert("Error", "Invalid email or password. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Sign in failed: {ex.Message}", "OK");
        }
        finally
        {
            // Re-enable button
            SignInButton.IsEnabled = true;
            SignInButton.Text = "Sign In";
        }
    }

    private async void OnSignUpTapped(object sender, EventArgs e)
    {
        // Navigate to Sign Up page
        await Navigation.PushAsync(new SignUpPage());
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        // Prompt for email to reset password
        string email = await DisplayPromptAsync("Forgot Password",
            "Enter your email address to reset your password:",
            "Send Reset Link",
            "Cancel",
            "Email Address",
            keyboard: Keyboard.Email);

        if (!string.IsNullOrWhiteSpace(email))
        {
            if (IsValidEmail(email))
            {
                try
                {
                    // TODO: Implement actual password reset logic
                    await SimulatePasswordResetAsync(email);
                    await DisplayAlert("Password Reset",
                        $"A password reset link has been sent to {email}",
                        "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to send reset email: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Please enter a valid email address", "OK");
            }
        }
    }

    // Helper method to validate email format
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

    // Clear form fields
    private void ClearForm()
    {
        EmailEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
    }

    // Simulate user authentication (replace with actual authentication logic)
    private async Task<bool> AuthenticateUserAsync(string email, string password)
    {
        // Simulate API call delay
        await Task.Delay(2000);

        // TODO: Replace with actual authentication logic
        // Example implementation:
        /*
        var authRequest = new AuthenticationRequest
        {
            Email = email,
            Password = password
        };

        var result = await _authService.AuthenticateAsync(authRequest);
        
        if (result.IsSuccess)
        {
            // Store authentication token
            await SecureStorage.SetAsync("auth_token", result.Token);
            await SecureStorage.SetAsync("user_email", email);
            return true;
        }
        
        return false;
        */

        // For demo purposes: accept any email with password "password123"
        return password == "password123";
    }

    // Simulate password reset (replace with actual logic)
    private async Task SimulatePasswordResetAsync(string email)
    {
        // Simulate API call delay
        await Task.Delay(1500);

        // TODO: Replace with actual password reset logic
        // Example: await _authService.SendPasswordResetAsync(email);

        // For demo purposes, just simulate success
    }
}