namespace StudentAPP;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await PerformLoginAsync();
    }

    private async Task PerformLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(EmailEntry?.Text) || string.IsNullOrWhiteSpace(PasswordEntry?.Text))
        {
            await DisplayAlert("Error", "Please fill in all fields", "OK");
            return;
        }

        LoginButton.IsEnabled = false;
        LoginButton.Text = "Logging in...";

        // Simple validation - any email with password123
        if (PasswordEntry.Text == "password123")
        {
            await DisplayAlert("Success", "Login successful!", "OK");

            // Navigate to modules page
            Application.Current.MainPage = new NavigationPage(new ModulesPage());
        }
        else
        {
            await DisplayAlert("Error", "Invalid credentials. Use any email with 'password123'", "OK");
        }

        LoginButton.IsEnabled = true;
        LoginButton.Text = "Login";
    }

    private async void OnDemoClicked(object sender, EventArgs e)
    {
        if (EmailEntry != null && PasswordEntry != null)
        {
            EmailEntry.Text = "demo@student.com";
            PasswordEntry.Text = "password123";
            await PerformLoginAsync();
        }
    }

    private async void OnSignUpTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}