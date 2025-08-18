using StudentAPP.Services;
using StudentAPP.Models;
using System.Collections.ObjectModel;

namespace StudentAPP;

public partial class ProfilePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    public ObservableCollection<RecentEnrollmentItem> RecentEnrollments { get; set; }

    public ProfilePage()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();
        RecentEnrollments = new ObservableCollection<RecentEnrollmentItem>();

        BindingContext = this;
        LoadProfileData();
    }

    private async void LoadProfileData()
    {
        try
        {
            // Get current student info from secure storage
            var studentId = await SecureStorage.GetAsync("student_id");
            if (string.IsNullOrEmpty(studentId) || studentId == "GUEST")
            {
                ShowGuestProfile();
                return;
            }

            // Initialize database and get student data
            await _databaseService.InitializeDatabaseAsync();
            var student = await _databaseService.GetStudentByIdAsync(studentId);

            if (student != null)
            {
                // Update profile header
                StudentNameLabel.Text = student.FullName;
                StudentIdLabel.Text = student.StudentId;
                EmailLabel.Text = student.Email;
                PhoneLabel.Text = !string.IsNullOrEmpty(student.Phone) ? student.Phone : "Not provided";
                MemberSinceLabel.Text = student.CreatedAt.ToString("MMMM yyyy");

                // Format last login
                var timeSinceLogin = DateTime.Now - student.LastLoginAt;
                if (timeSinceLogin.TotalDays < 1)
                    LastLoginLabel.Text = "Last login: Today";
                else if (timeSinceLogin.TotalDays < 2)
                    LastLoginLabel.Text = "Last login: Yesterday";
                else
                    LastLoginLabel.Text = $"Last login: {student.LastLoginAt:MMM dd, yyyy}";

                // Load academic stats
                await LoadAcademicStats(studentId);

                // Load recent enrollments
                await LoadRecentEnrollments(studentId);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
        }
    }

    private async Task LoadAcademicStats(string studentId)
    {
        try
        {
            var enrollments = await _databaseService.GetStudentEnrollmentsAsync(studentId);

            var enrolledCount = enrollments.Count(e => e.Status == "Enrolled");
            var completedCount = enrollments.Count(e => e.Status == "Completed");

            // Calculate total credits from completed modules
            var totalCredits = 0;
            foreach (var enrollment in enrollments.Where(e => e.Status == "Completed"))
            {
                var module = await _databaseService.GetModuleByCodeAsync(enrollment.ModuleCode);
                if (module != null)
                {
                    totalCredits += module.Credits;
                }
            }

            // Update UI
            EnrolledCountLabel.Text = enrolledCount.ToString();
            CompletedCountLabel.Text = completedCount.ToString();
            TotalCreditsLabel.Text = totalCredits.ToString();
        }
        catch (Exception ex)
        {
            // Handle error silently or log it
            EnrolledCountLabel.Text = "0";
            CompletedCountLabel.Text = "0";
            TotalCreditsLabel.Text = "0";
        }
    }

    private async Task LoadRecentEnrollments(string studentId)
    {
        try
        {
            var enrollments = await _databaseService.GetStudentEnrollmentsAsync(studentId);
            var recentEnrollments = enrollments
                .OrderByDescending(e => e.EnrollmentDate)
                .Take(5)
                .ToList();

            RecentEnrollments.Clear();

            foreach (var enrollment in recentEnrollments)
            {
                var module = await _databaseService.GetModuleByCodeAsync(enrollment.ModuleCode);
                if (module != null)
                {
                    var item = new RecentEnrollmentItem
                    {
                        ModuleName = $"{module.ModuleCode} - {module.ModuleName}",
                        EnrollmentDate = $"Enrolled: {enrollment.EnrollmentDate:MMM dd, yyyy}",
                        Status = enrollment.Status,
                        Icon = GetModuleIcon(module.Category)
                    };

                    RecentEnrollments.Add(item);
                }
            }

            RecentEnrollmentsCollection.ItemsSource = RecentEnrollments;
        }
        catch (Exception ex)
        {
            // Handle error silently
        }
    }

    private string GetModuleIcon(string category)
    {
        return category switch
        {
            "Computer Science" => "computer_science.png",
            "Mathematics" => "mathematics.png",
            "Engineering" => "engineering.png",
            "Business" => "portfolio.png",
            _ => "book.png"
        };
    }

    private void ShowGuestProfile()
    {
        StudentNameLabel.Text = "Guest User";
        StudentIdLabel.Text = "GUEST";
        EmailLabel.Text = "guest@portal.com";
        PhoneLabel.Text = "Not available";
        MemberSinceLabel.Text = "Today";
        LastLoginLabel.Text = "Current session";

        EnrolledCountLabel.Text = "0";
        CompletedCountLabel.Text = "0";
        TotalCreditsLabel.Text = "0";
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        RefreshView.IsRefreshing = true;
        await Task.Delay(1000); // Small delay for better UX
        LoadProfileData();
        RefreshView.IsRefreshing = false;
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "Edit Profile Information",
            "Cancel",
            null,
            "Update Phone Number",
            "Update Personal Details",
            "Change Profile Picture");

        switch (action)
        {
            case "Update Phone Number":
                await UpdatePhoneNumber();
                break;
            case "Update Personal Details":
                await DisplayAlert("Coming Soon", "Full profile editing will be available in the next update!", "OK");
                break;
            case "Change Profile Picture":
                await DisplayAlert("Coming Soon", "Profile picture upload will be available in the next update!", "OK");
                break;
        }
    }

    private async Task UpdatePhoneNumber()
    {
        var currentStudentId = await SecureStorage.GetAsync("student_id");
        if (string.IsNullOrEmpty(currentStudentId) || currentStudentId == "GUEST")
        {
            await DisplayAlert("Not Available", "Phone number update is not available for guest users.", "OK");
            return;
        }

        var newPhone = await DisplayPromptAsync(
            "Update Phone Number",
            "Enter your new phone number:",
            "Update",
            "Cancel",
            PhoneLabel.Text,
            keyboard: Keyboard.Telephone);

        if (!string.IsNullOrEmpty(newPhone))
        {
            try
            {
                var student = await _databaseService.GetStudentByIdAsync(currentStudentId);
                if (student != null)
                {
                    student.Phone = newPhone;
                    // In a real app, you'd update the database here
                    PhoneLabel.Text = newPhone;
                    await DisplayAlert("Success", "Phone number updated successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update phone number: {ex.Message}", "OK");
            }
        }
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        var currentStudentId = await SecureStorage.GetAsync("student_id");
        if (string.IsNullOrEmpty(currentStudentId) || currentStudentId == "GUEST")
        {
            await DisplayAlert("Not Available", "Password change is not available for guest users.", "OK");
            return;
        }

        var currentPassword = await DisplayPromptAsync(
            "Change Password",
            "Enter your current password:",
            "Verify",
            "Cancel",
            keyboard: Keyboard.Default);

        if (!string.IsNullOrEmpty(currentPassword))
        {
            var isValid = await _databaseService.ValidateStudentCredentialsAsync(currentStudentId, currentPassword);
            if (isValid)
            {
                var newPassword = await DisplayPromptAsync(
                    "New Password",
                    "Enter your new password:",
                    "Update",
                    "Cancel",
                    keyboard: Keyboard.Default);

                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length >= 6)
                {
                    await DisplayAlert("Success", "Password would be updated in a real implementation!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Password must be at least 6 characters long.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Current password is incorrect.", "OK");
            }
        }
    }

    private async void OnEmailPreferencesClicked(object sender, EventArgs e)
    {
        var preferences = await DisplayActionSheet(
            "Email Preferences",
            "Cancel",
            null,
            "Course Updates: ON",
            "Assignment Reminders: ON",
            "System Notifications: OFF",
            "Newsletter: ON");

        if (preferences != null && preferences != "Cancel")
        {
            await DisplayAlert("Email Preferences",
                $"'{preferences}' setting would be toggled in a real implementation!",
                "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirmLogout = await DisplayAlert(
            "Logout Confirmation",
            "Are you sure you want to logout from Student Portal?",
            "Yes, Logout",
            "Cancel");

        if (confirmLogout)
        {
            try
            {
                // Clear all stored credentials
                SecureStorage.RemoveAll();

                await DisplayAlert("Logged Out", "You have been successfully logged out. See you soon!", "OK");

                // Navigate back to login
                Application.Current!.MainPage = new NavigationPage(new StudentLoginPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Logout failed: {ex.Message}", "OK");
            }
        }
    }
}

// Data model for recent enrollments display
public class RecentEnrollmentItem
{
    public string ModuleName { get; set; } = string.Empty;
    public string EnrollmentDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}