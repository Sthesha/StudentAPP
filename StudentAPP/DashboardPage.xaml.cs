using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace StudentAPP;

public partial class DashboardPage : ContentPage
{
    public ObservableCollection<EnrollmentItem> RecentEnrollments { get; set; }

    public DashboardPage()
    {
        InitializeComponent();
        LoadStudentData();
        LoadRecentEnrollments();
    }

    private async void LoadStudentData()
    {
        try
        {
            // Get student info from secure storage
            var studentName = await SecureStorage.GetAsync("student_name") ?? "Student";
            var studentId = await SecureStorage.GetAsync("student_id") ?? "Unknown";

            WelcomeLabel.Text = $"Welcome back, {studentName}!";
            StudentIdLabel.Text = $"Student ID: {studentId}";

            // Load stats (this would typically come from an API)
            await LoadStudentStats();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load student data: {ex.Message}", "OK");
        }
    }

    private async Task LoadStudentStats()
    {
        // Simulate API call to get student statistics
        await Task.Delay(1000);

        // Mock data - in real app, this would come from your API
        EnrolledCountLabel.Text = "5";
        AvailableCountLabel.Text = "12";
        CompletedCountLabel.Text = "3";
    }

    private void LoadRecentEnrollments()
    {
        // Mock recent enrollments data
        RecentEnrollments = new ObservableCollection<EnrollmentItem>
        {
            new EnrollmentItem
            {
                ModuleName = "Introduction to Computer Science",
                ModuleCode = "CS101",
                Status = "Active",
                Icon = "neural.png",
                CategoryColor = Colors.Blue
            },
            new EnrollmentItem
            {
                ModuleName = "Mathematics for Engineers",
                ModuleCode = "MATH201",
                Status = "Active",
                Icon = "mathematics.png",
                CategoryColor = Colors.Green
            },
            new EnrollmentItem
            {
                ModuleName = "Database Management Systems",
                ModuleCode = "CS301",
                Status = "In Progress",
                Icon = "database_storage.png",
                CategoryColor = Colors.Purple
            }
        };

        RecentEnrollmentsCollection.ItemsSource = RecentEnrollments;
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        RefreshView.IsRefreshing = true;

        await LoadStudentStats();
        LoadRecentEnrollments();

        RefreshView.IsRefreshing = false;
    }

    private async void OnBrowseModulesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//modules");
    }

    private async void OnMyEnrollmentsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//enrollments");
    }

    private async void OnMyProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//profile");
    }

    private async void OnSearchModulesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//modules?search=true");
    }

    private async void OnViewAllEnrollmentsTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//enrollments");
    }
}

// Data models
public class EnrollmentItem
{
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Color CategoryColor { get; set; }
}