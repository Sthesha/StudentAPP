using Microsoft.Maui.Controls;
using StudentAPP.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StudentAPP;

public partial class EnrollmentsPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    public ObservableCollection<EnrollmentDisplayItem> Enrollments { get; set; }
    public ObservableCollection<EnrollmentDisplayItem> FilteredEnrollments { get; set; }

    public ICommand PrimaryActionCommand { get; set; }
    public ICommand SecondaryActionCommand { get; set; }
    public ICommand MoreActionsCommand { get; set; }

    private string currentFilter = "All";
    private List<EnrollmentDisplayItem> allEnrollments = new();
    private string currentStudentId = string.Empty;

    public EnrollmentsPage()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();
        Enrollments = new ObservableCollection<EnrollmentDisplayItem>();
        FilteredEnrollments = new ObservableCollection<EnrollmentDisplayItem>();

        PrimaryActionCommand = new Command<EnrollmentDisplayItem>(OnPrimaryActionClicked);
        SecondaryActionCommand = new Command<EnrollmentDisplayItem>(OnSecondaryActionClicked);
        MoreActionsCommand = new Command<EnrollmentDisplayItem>(OnMoreActionsClicked);

        BindingContext = this;
        LoadEnrollments();
    }

    private async void LoadEnrollments()
    {
        try
        {
            // Get current student ID
            currentStudentId = await SecureStorage.GetAsync("student_id") ?? "GUEST";

            if (currentStudentId == "GUEST")
            {
                ShowGuestMessage();
                return;
            }

            // Initialize database and load data
            await _databaseService.InitializeDatabaseAsync();
            await LoadStudentEnrollments();
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Failed to load enrollments. Please try again later.", "OK");
        }
    }

    private async Task LoadStudentEnrollments()
    {
        try
        {
            // Get enrollments from database
            var dbEnrollments = await _databaseService.GetStudentEnrollmentsAsync(currentStudentId);
            var displayItems = new List<EnrollmentDisplayItem>();

            foreach (var enrollment in dbEnrollments)
            {
                // Get module details
                var module = await _databaseService.GetModuleByCodeAsync(enrollment.ModuleCode);
                if (module != null)
                {
                    var displayItem = CreateEnrollmentDisplayItem(enrollment, module);
                    displayItems.Add(displayItem);
                }
            }

            // Sort by enrollment date (newest first)
            allEnrollments = displayItems.OrderByDescending(e => e.EnrollmentDateRaw).ToList();

            // Apply current filter
            ApplyFilter(currentFilter);
            UpdateStats();
            UpdateSummaryLabel();
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Failed to load enrollment data. Please try again later.", "OK");
        }
    }

    private EnrollmentDisplayItem CreateEnrollmentDisplayItem(dynamic enrollment, dynamic module)
    {
        // Create a random progress value for demo purposes (in real app, this would come from enrollment data)
        var random = new Random();
        var progress = enrollment.Status == "Completed" ? 100 :
                      enrollment.Status == "Dropped" ? 0 :
                      random.Next(20, 95);

        var item = new EnrollmentDisplayItem
        {
            EnrollmentId = enrollment.Id,
            ModuleName = module.ModuleName,
            ModuleCode = module.ModuleCode,
            Instructor = module.Instructor,
            Category = module.Category,
            Icon = GetModuleIcon(module.Category),
            CategoryColor = GetCategoryColor(module.Category),
            Status = enrollment.Status,
            StatusColor = GetStatusColor(enrollment.Status),
            EnrollmentDate = $"Enrolled: {enrollment.EnrollmentDate:MMM dd, yyyy}",
            EnrollmentDateRaw = enrollment.EnrollmentDate,
            Credits = $"{module.Credits} credits",
            Grade = enrollment.Grade ?? "Not graded",
            Progress = progress,
            ProgressText = $"{progress}% complete",
            ProgressPercentage = $"{progress}%",
            ShowProgress = enrollment.Status == "Enrolled" || enrollment.Status == "In Progress"
        };

        // Set progress bar properties
        item.ProgressColor = progress >= 80 ? Colors.Green :
                           progress >= 50 ? Colors.Orange : Colors.Red;
        item.ProgressWidth = (progress / 100.0) * 200; // Assuming 200 is the max width

        // Set action buttons based on status
        SetActionButtons(item, enrollment.Status);

        return item;
    }

    private void SetActionButtons(EnrollmentDisplayItem item, string status)
    {
        switch (status)
        {
            case "Enrolled":
            case "In Progress":
                item.PrimaryActionText = "Continue";
                item.PrimaryActionColor = Colors.Blue;
                item.PrimaryActionIcon = "play.png";
                item.SecondaryActionText = "Progress";
                item.SecondaryActionColor = Colors.Green;
                item.SecondaryActionIcon = "perfomance_review.png";
                break;

            case "Completed":
                item.PrimaryActionText = "Certificate";
                item.PrimaryActionColor = Color.FromArgb("#F59E0B");
                item.PrimaryActionIcon = "certificate.png"; 
                item.SecondaryActionText = "Transcript";
                item.SecondaryActionColor = Colors.Purple;
                item.SecondaryActionIcon = "document.png";
                break;

            case "Dropped":
                item.PrimaryActionText = "Re-enroll";
                item.PrimaryActionColor = Colors.Orange;
                item.PrimaryActionIcon = "refresh.png"; 
                item.SecondaryActionText = "Details";
                item.SecondaryActionColor = Colors.Gray;
                item.SecondaryActionIcon = "info.png";  
                break;

            default:
                item.PrimaryActionText = "View";
                item.PrimaryActionColor = Colors.Gray;
                item.PrimaryActionIcon = "totally.png";  
                item.SecondaryActionText = "Options";
                item.SecondaryActionColor = Colors.Gray;
                item.SecondaryActionIcon = "settings.png";
                break;
        }
    }

    private string GetModuleIcon(string category)
    {
        return category switch
        {
            "Computer Science" => "computer_science.png",
            "Mathematics" => "calculator.png",
            "Engineering" => "engineering.png",
            "Business" => "portfolio.png",
            _ => "book.png"
        };
    }

    private Color GetCategoryColor(string category)
    {
        return category switch
        {
            "Computer Science" => Colors.Blue,
            "Mathematics" => Colors.Green,
            "Engineering" => Colors.Red,
            "Business" => Colors.Purple,
            "Science" => Colors.Teal,
            "Arts" => Colors.Pink,
            _ => Colors.Gray
        };
    }

    private Color GetStatusColor(string status)
    {
        return status switch
        {
            "Enrolled" => Colors.Blue,
            "In Progress" => Colors.Orange,
            "Completed" => Colors.Green,
            "Dropped" => Colors.Red,
            _ => Colors.Gray
        };
    }

    private void ApplyFilter(string filter)
    {
        currentFilter = filter;

        var filtered = filter == "All" ? allEnrollments :
                      filter == "Enrolled" ? allEnrollments.Where(e => e.Status == "Enrolled").ToList() :
                      filter == "In Progress" ? allEnrollments.Where(e => e.Status == "In Progress").ToList() :
                      filter == "Completed" ? allEnrollments.Where(e => e.Status == "Completed").ToList() :
                      filter == "Dropped" ? allEnrollments.Where(e => e.Status == "Dropped").ToList() :
                      allEnrollments;

        // Apply search filter if search text exists
        if (!string.IsNullOrWhiteSpace(SearchEntry?.Text))
        {
            var searchTerm = SearchEntry.Text.ToLower();
            filtered = filtered.Where(e =>
                e.ModuleName.ToLower().Contains(searchTerm) ||
                e.ModuleCode.ToLower().Contains(searchTerm) ||
                e.Instructor.ToLower().Contains(searchTerm) ||
                e.Category.ToLower().Contains(searchTerm)
            ).ToList();
        }

        Enrollments.Clear();
        foreach (var enrollment in filtered)
        {
            Enrollments.Add(enrollment);
        }

        EnrollmentsCollection.ItemsSource = Enrollments;
        UpdateFilterButtons();
    }

    private void UpdateFilterButtons()
    {
        // Reset all buttons to inactive state
        var inactiveColor = Color.FromArgb("#E2E8F0");
        var inactiveTextColor = Color.FromArgb("#64748B");
        var activeColor = Color.FromArgb("#3B82F6");
        var activeTextColor = Colors.White;

        AllFilterBtn.BackgroundColor = inactiveColor;
        AllFilterBtn.TextColor = inactiveTextColor;
        ActiveFilterBtn.BackgroundColor = inactiveColor;
        ActiveFilterBtn.TextColor = inactiveTextColor;
        InProgressFilterBtn.BackgroundColor = inactiveColor;
        InProgressFilterBtn.TextColor = inactiveTextColor;
        CompletedFilterBtn.BackgroundColor = inactiveColor;
        CompletedFilterBtn.TextColor = inactiveTextColor;
        DroppedFilterBtn.BackgroundColor = inactiveColor;
        DroppedFilterBtn.TextColor = inactiveTextColor;

        // Highlight active filter
        switch (currentFilter)
        {
            case "All":
                AllFilterBtn.BackgroundColor = activeColor;
                AllFilterBtn.TextColor = activeTextColor;
                break;
            case "Enrolled":
                ActiveFilterBtn.BackgroundColor = activeColor;
                ActiveFilterBtn.TextColor = activeTextColor;
                break;
            case "In Progress":
                InProgressFilterBtn.BackgroundColor = activeColor;
                InProgressFilterBtn.TextColor = activeTextColor;
                break;
            case "Completed":
                CompletedFilterBtn.BackgroundColor = activeColor;
                CompletedFilterBtn.TextColor = activeTextColor;
                break;
            case "Dropped":
                DroppedFilterBtn.BackgroundColor = activeColor;
                DroppedFilterBtn.TextColor = activeTextColor;
                break;
        }
    }

    private void UpdateStats()
    {
        var activeCount = allEnrollments.Count(e => e.Status == "Enrolled");
        var completedCount = allEnrollments.Count(e => e.Status == "Completed");
        var inProgressCount = allEnrollments.Count(e => e.Status == "In Progress");
        var totalCredits = allEnrollments.Where(e => e.Status == "Completed")
                                       .Sum(e => int.Parse(e.Credits.Split(' ')[0]));

        ActiveCountLabel.Text = activeCount.ToString();
        CompletedCountLabel.Text = completedCount.ToString();
        InProgressCountLabel.Text = inProgressCount.ToString();
        TotalCreditsLabel.Text = totalCredits.ToString();
    }

    private void UpdateSummaryLabel()
    {
        var totalEnrollments = allEnrollments.Count;
        var activeEnrollments = allEnrollments.Count(e => e.Status == "Enrolled" || e.Status == "In Progress");

        if (totalEnrollments == 0)
        {
            EnrollmentSummaryLabel.Text = "No enrollments found. Start by browsing available modules.";
        }
        else
        {
            EnrollmentSummaryLabel.Text = $"You have {totalEnrollments} total enrollment{(totalEnrollments != 1 ? "s" : "")} with {activeEnrollments} currently active";
        }
    }

    private void ShowGuestMessage()
    {
        EnrollmentSummaryLabel.Text = "Guest users cannot view enrollments. Please log in to access this feature.";

        // Hide stats for guest users
        ActiveCountLabel.Text = "—";
        CompletedCountLabel.Text = "—";
        InProgressCountLabel.Text = "—";
        TotalCreditsLabel.Text = "—";
    }

    // Event Handlers
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter(currentFilter);
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var filter = button.Text switch
            {
                "All" => "All",
                "Enrolled" => "Enrolled",
                "In Progress" => "In Progress",
                "Completed" => "Completed",
                "Dropped" => "Dropped",
                _ => "All"
            };
            ApplyFilter(filter);
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        EnrollmentsRefreshView.IsRefreshing = true;

        try
        {
            await Task.Delay(1000); // Small delay for better UX
            await LoadStudentEnrollments();
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Failed to refresh. Please try again later.", "OK");
        }
        finally
        {
            EnrollmentsRefreshView.IsRefreshing = false;
        }
    }

    private async void OnCalendarViewClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Calendar View", "Calendar view will show your enrollment schedule and upcoming deadlines in a future update!", "OK");
    }

    private async void OnReportsClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "Academic Reports",
            "Cancel",
            null,
            "📊 Academic Transcript",
            "📈 Progress Report",
            "📋 Enrollment History",
            "🎯 Credit Summary");

        switch (action)
        {
            case "📊 Academic Transcript":
                await ShowAcademicTranscript();
                break;
            case "📈 Progress Report":
                await ShowProgressReport();
                break;
            case "📋 Enrollment History":
                await ShowEnrollmentHistory();
                break;
            case "🎯 Credit Summary":
                await ShowCreditSummary();
                break;
        }
    }

    private async void OnBrowseModulesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//modules");
    }

    // Action Commands
    private async void OnPrimaryActionClicked(EnrollmentDisplayItem enrollment)
    {
        switch (enrollment.Status)
        {
            case "Enrolled":
            case "In Progress":
                await ContinueLearning(enrollment);
                break;
            case "Completed":
                await ViewCertificate(enrollment);
                break;
            case "Dropped":
                await ReEnroll(enrollment);
                break;
            default:
                await ViewEnrollmentDetails(enrollment);
                break;
        }
    }

    private async void OnSecondaryActionClicked(EnrollmentDisplayItem enrollment)
    {
        switch (enrollment.Status)
        {
            case "Enrolled":
            case "In Progress":
                await ViewProgress(enrollment);
                break;
            case "Completed":
                await ViewTranscript(enrollment);
                break;
            case "Dropped":
                await ViewEnrollmentDetails(enrollment);
                break;
            default:
                await ShowEnrollmentOptions(enrollment);
                break;
        }
    }

    private async void OnMoreActionsClicked(EnrollmentDisplayItem enrollment)
    {
        var actions = new List<string> { "📋 View Details", "📧 Contact Instructor" };

        if (enrollment.Status == "Enrolled" || enrollment.Status == "In Progress")
        {
            actions.AddRange(new[] { "⏸️ Pause Enrollment", "❌ Drop Course" });
        }
        else if (enrollment.Status == "Completed")
        {
            actions.AddRange(new[] { "📜 Download Certificate", "⭐ Rate Course" });
        }

        actions.Add("🔄 Refresh Status");

        var action = await DisplayActionSheet(
            $"Actions for {enrollment.ModuleCode}",
            "Cancel",
            null,
            actions.ToArray());

        await HandleMoreAction(enrollment, action);
    }

    // Helper Methods for Actions
    private async Task ContinueLearning(EnrollmentDisplayItem enrollment)
    {
        await DisplayAlert("Continue Learning",
            $"Opening {enrollment.ModuleName}...\n\nThis would navigate to the course content, assignments, and learning materials.",
            "OK");
    }

    private async Task ViewProgress(EnrollmentDisplayItem enrollment)
    {
        var progressInfo = $"""
            Course Progress for {enrollment.ModuleName}
            
            Overall Progress: {enrollment.ProgressPercentage}
            
            📚 Completed Modules: {enrollment.Progress / 10}/10
            📝 Assignments: 5/8 submitted
            🎯 Quizzes: 7/10 completed
            📊 Current Grade: {enrollment.Grade}
            
            Next deadline: Assignment 6 - Due in 3 days
            """;

        await DisplayAlert("Progress Report", progressInfo, "OK");
    }

    private async Task ViewCertificate(EnrollmentDisplayItem enrollment)
    {
        await DisplayAlert("Certificate",
            $"🎉 Congratulations!\n\nYou have successfully completed {enrollment.ModuleName}.\n\nYour certificate is ready for download.",
            "Download");
    }

    private async Task ViewTranscript(EnrollmentDisplayItem enrollment)
    {
        var transcriptInfo = $"""
            Academic Transcript - {enrollment.ModuleName}
            
            Student: {await SecureStorage.GetAsync("student_name")}
            Module Code: {enrollment.ModuleCode}
            Credits: {enrollment.Credits}
            Final Grade: {enrollment.Grade}
            Completion Date: {enrollment.EnrollmentDateRaw.AddDays(90):MMM dd, yyyy}
            
            This transcript can be shared with employers or other institutions.
            """;

        await DisplayAlert("Academic Transcript", transcriptInfo, "OK");
    }

    private async Task ReEnroll(EnrollmentDisplayItem enrollment)
    {
        bool confirm = await DisplayAlert("Re-enroll",
            $"Would you like to re-enroll in {enrollment.ModuleName}?\n\nNote: Previous progress may not be restored.",
            "Re-enroll", "Cancel");

        if (confirm)
        {
            // Update enrollment status in database
            try
            {
                // In a real app, you'd get the actual enrollment from database
                // For now, we'll simulate updating the enrollment
                await DisplayAlert("Success", $"Successfully re-enrolled in {enrollment.ModuleName}!", "OK");
                await LoadStudentEnrollments(); // Refresh the list
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "Failed to re-enroll. Please try again later.", "OK");
            }
        }
    }

    private async Task ViewEnrollmentDetails(EnrollmentDisplayItem enrollment)
    {
        var details = $"""
            Enrollment Details
            
            📚 Module: {enrollment.ModuleName}
            🔤 Code: {enrollment.ModuleCode}
            👨‍🏫 Instructor: {enrollment.Instructor}
            📅 Enrolled: {enrollment.EnrollmentDate}
            🎯 Credits: {enrollment.Credits}
            📊 Status: {enrollment.Status}
            ⭐ Grade: {enrollment.Grade}
            📈 Progress: {enrollment.ProgressPercentage}
            """;

        await DisplayAlert("Enrollment Details", details, "OK");
    }

    private async Task HandleMoreAction(EnrollmentDisplayItem enrollment, string action)
    {
        switch (action)
        {
            case "📋 View Details":
                await ViewEnrollmentDetails(enrollment);
                break;
            case "📧 Contact Instructor":
                await ContactInstructor(enrollment);
                break;
            case "⏸️ Pause Enrollment":
                await PauseEnrollment(enrollment);
                break;
            case "❌ Drop Course":
                await DropCourse(enrollment);
                break;
            case "📜 Download Certificate":
                await ViewCertificate(enrollment);
                break;
            case "⭐ Rate Course":
                await RateCourse(enrollment);
                break;
            case "🔄 Refresh Status":
                await RefreshEnrollmentStatus(enrollment);
                break;
        }
    }

    private async Task ContactInstructor(EnrollmentDisplayItem enrollment)
    {
        await DisplayAlert("Contact Instructor",
            $"📧 {enrollment.Instructor}\n\nThis would open your email client or internal messaging system to contact the instructor.",
            "OK");
    }

    private async Task PauseEnrollment(EnrollmentDisplayItem enrollment)
    {
        bool confirm = await DisplayAlert("Pause Enrollment",
            $"Are you sure you want to pause your enrollment in {enrollment.ModuleName}?\n\nYou can resume later without losing progress.",
            "Pause", "Cancel");

        if (confirm)
        {
            await DisplayAlert("Enrollment Paused",
                $"Your enrollment in {enrollment.ModuleName} has been paused. You can resume anytime from your enrollments.",
                "OK");
        }
    }

    private async Task DropCourse(EnrollmentDisplayItem enrollment)
    {
        bool confirm = await DisplayAlert("Drop Course",
            $"⚠️ Are you sure you want to drop {enrollment.ModuleName}?\n\nThis action cannot be undone and you may lose all progress.",
            "Drop Course", "Cancel");

        if (confirm)
        {
            try
            {
                // In a real app, you'd update the database here
                await DisplayAlert("Course Dropped",
                    $"You have successfully dropped {enrollment.ModuleName}.",
                    "OK");

                await LoadStudentEnrollments(); // Refresh the list
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "Failed to drop course. Please try again later.", "OK");
            }
        }
    }

    private async Task RateCourse(EnrollmentDisplayItem enrollment)
    {
        var rating = await DisplayActionSheet(
            $"Rate {enrollment.ModuleName}",
            "Cancel",
            null,
            "⭐⭐⭐⭐⭐ Excellent (5/5)",
            "⭐⭐⭐⭐ Good (4/5)",
            "⭐⭐⭐ Average (3/5)",
            "⭐⭐ Poor (2/5)",
            "⭐ Very Poor (1/5)");

        if (rating != null && rating != "Cancel")
        {
            await DisplayAlert("Thank You!",
                $"Your rating has been submitted for {enrollment.ModuleName}.\n\nYour feedback helps improve the course for future students.",
                "OK");
        }
    }

    private async Task RefreshEnrollmentStatus(EnrollmentDisplayItem enrollment)
    {
        await DisplayAlert("Status Refreshed",
            $"Enrollment status for {enrollment.ModuleName} has been refreshed.\n\nAll information is up to date.",
            "OK");

        await LoadStudentEnrollments(); // Refresh the entire list
    }

    // Report Methods
    private async Task ShowAcademicTranscript()
    {
        var completedCourses = allEnrollments.Where(e => e.Status == "Completed").ToList();
        var totalCredits = completedCourses.Sum(e => int.Parse(e.Credits.Split(' ')[0]));
        var gpa = "3.75"; // Calculate based on grades

        var transcript = $"""
            🎓 ACADEMIC TRANSCRIPT
            
            Student: {await SecureStorage.GetAsync("student_name")}
            Student ID: {currentStudentId}
            
            COMPLETED COURSES ({completedCourses.Count}):
            {string.Join("\n", completedCourses.Select(c => $"• {c.ModuleCode} - {c.ModuleName} ({c.Credits}) - Grade: {c.Grade}"))}
            
            📊 SUMMARY:
            Total Credits: {totalCredits}
            GPA: {gpa}
            """;

        await DisplayAlert("Academic Transcript", transcript, "OK");
    }

    private async Task ShowProgressReport()
    {
        var activeCourses = allEnrollments.Where(e => e.Status == "Enrolled" || e.Status == "In Progress").ToList();
        var avgProgress = activeCourses.Any() ? activeCourses.Average(c => c.Progress) : 0;

        var report = $"""
            📈 PROGRESS REPORT
            
            ACTIVE ENROLLMENTS ({activeCourses.Count}):
            {string.Join("\n", activeCourses.Select(c => $"• {c.ModuleCode} - {c.ProgressPercentage} complete"))}
            
            📊 OVERALL PROGRESS:
            Average Progress: {avgProgress:F1}%
            On Track: {activeCourses.Count(c => c.Progress >= 50)}/{activeCourses.Count} courses
            """;

        await DisplayAlert("Progress Report", report, "OK");
    }

    private async Task ShowEnrollmentHistory()
    {
        var history = $"""
            📋 ENROLLMENT HISTORY
            
            CHRONOLOGICAL ORDER:
            {string.Join("\n", allEnrollments.OrderBy(e => e.EnrollmentDateRaw).Select(e => $"• {e.EnrollmentDateRaw:MMM yyyy} - {e.ModuleCode} ({e.Status})"))}
            
            📊 SUMMARY:
            Total Enrollments: {allEnrollments.Count}
            Success Rate: {(allEnrollments.Count > 0 ? (double)allEnrollments.Count(e => e.Status == "Completed") / allEnrollments.Count * 100 : 0):F1}%
            """;

        await DisplayAlert("Enrollment History", history, "OK");
    }

    private async Task ShowCreditSummary()
    {
        var completedCredits = allEnrollments.Where(e => e.Status == "Completed")
                                           .Sum(e => int.Parse(e.Credits.Split(' ')[0]));
        var enrolledCredits = allEnrollments.Where(e => e.Status == "Enrolled" || e.Status == "In Progress")
                                          .Sum(e => int.Parse(e.Credits.Split(' ')[0]));

        var summary = $"""
            🎯 CREDIT SUMMARY
            
            ✅ COMPLETED CREDITS: {completedCredits}
            📚 IN PROGRESS CREDITS: {enrolledCredits}
            📊 TOTAL CREDITS: {completedCredits + enrolledCredits}
            
            BREAKDOWN BY CATEGORY:
            {string.Join("\n", allEnrollments.Where(e => e.Status == "Completed")
                .GroupBy(e => e.Category)
                .Select(g => $"• {g.Key}: {g.Sum(x => int.Parse(x.Credits.Split(' ')[0]))} credits"))}
            """;

        await DisplayAlert("Credit Summary", summary, "OK");
    }

    private async Task ShowEnrollmentOptions(EnrollmentDisplayItem enrollment)
    {
        await DisplayAlert("Enrollment Options",
            $"Additional options for {enrollment.ModuleName} will be available in future updates.",
            "OK");
    }
}

// Data Model for Display
public class EnrollmentDisplayItem
{
    public int EnrollmentId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string PrimaryActionIcon { get; set; } = string.Empty;

    public string SecondaryActionIcon { get; set; } = string.Empty;
    public Color CategoryColor { get; set; } = Colors.Gray;
    public string Status { get; set; } = string.Empty;
    public Color StatusColor { get; set; } = Colors.Gray;
    public string EnrollmentDate { get; set; } = string.Empty;
    public DateTime EnrollmentDateRaw { get; set; }
    public string Credits { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string ProgressText { get; set; } = string.Empty;
    public string ProgressPercentage { get; set; } = string.Empty;
    public bool ShowProgress { get; set; }
    public Color ProgressColor { get; set; } = Colors.Blue;
    public double ProgressWidth { get; set; }
    public string PrimaryActionText { get; set; } = string.Empty;
    public Color PrimaryActionColor { get; set; } = Colors.Blue;
    public string SecondaryActionText { get; set; } = string.Empty;
    public Color SecondaryActionColor { get; set; } = Colors.Gray;
}