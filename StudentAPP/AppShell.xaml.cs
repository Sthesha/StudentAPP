using Microsoft.Maui.Controls;

namespace StudentAPP;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        LoadUserInfo();
        RegisterRoutes();
    }

    private async void LoadUserInfo()
    {
        try
        {
            // Load user info from secure storage
            var studentName = await SecureStorage.GetAsync("student_name") ?? "Student";
            var studentId = await SecureStorage.GetAsync("student_id") ?? "Unknown";

            StudentNameLabel.Text = studentName;
            StudentIdLabel.Text = studentId;
        }
        catch (Exception ex)
        {
            // Handle error silently or log it
            StudentNameLabel.Text = "Student";
            StudentIdLabel.Text = "Unknown";
        }
    }

    private void RegisterRoutes()
    {
        // Register additional routes for navigation
        Routing.RegisterRoute("moduledetails", typeof(ModuleDetailsPage));
        Routing.RegisterRoute("enrollmentdetails", typeof(EnrollmentDetailsPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
        Routing.RegisterRoute("apidemo", typeof(ApiDemoPage));
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("settings");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirmLogout = await DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes",
            "Cancel");

        if (confirmLogout)
        {
            await LogoutAsync();
        }
    }

    private async Task LogoutAsync()
    {
        try
        {
            // Clear stored credentials
            SecureStorage.RemoveAll();

            // Navigate back to login
            Application.Current.MainPage = new NavigationPage(new StudentLoginPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Logout failed: {ex.Message}", "OK");
        }
    }
}