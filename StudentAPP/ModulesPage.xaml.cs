using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StudentAPP;

public partial class ModulesPage : ContentPage
{
    public ObservableCollection<ModuleItem> Modules { get; set; }
    public ObservableCollection<ModuleItem> FilteredModules { get; set; }
    public ICommand ModuleActionCommand { get; set; }

    private string currentFilter = "All";
    private List<ModuleItem> allModules;

    public ModulesPage()
    {
        InitializeComponent();

        Modules = new ObservableCollection<ModuleItem>();
        FilteredModules = new ObservableCollection<ModuleItem>();
        ModuleActionCommand = new Command<ModuleItem>(OnModuleActionClicked);

        BindingContext = this;
        LoadModules();
    }

    private void LoadModules()
    {
        // Mock module data - in real app, this would come from an API
        allModules = new List<ModuleItem>
        {
            new ModuleItem
            {
                ModuleName = "Introduction to Computer Science",
                ModuleCode = "CS101",
                Instructor = "Dr. Sarah Johnson",
                Description = "Learn the fundamentals of computer science including programming basics, algorithms, and problem-solving techniques.",
                Duration = "12 weeks",
                Capacity = "25/30",
                Credits = "3 credits",
                Category = "Computer Science",
                Icon = "neural.png",
                CategoryColor = Colors.Blue,
                Status = "Available",
                StatusColor = Colors.Green,
                ActionText = "Enroll Now",
                ActionColor = Colors.Blue,
                IsEnrolled = false
            },
            new ModuleItem
            {
                ModuleName = "Database Management Systems",
                ModuleCode = "CS301",
                Instructor = "Prof. Michael Chen",
                Description = "Comprehensive study of database design, SQL, normalization, and database administration concepts.",
                Duration = "16 weeks",
                Capacity = "20/25",
                Credits = "4 credits",
                Category = "Computer Science",
                Icon = "database_storage.png",
                CategoryColor = Colors.Purple,
                Status = "Enrolled",
                StatusColor = Colors.Orange,
                ActionText = "View Details",
                ActionColor = Colors.Gray,
                IsEnrolled = true
            },
            new ModuleItem
            {
                ModuleName = "Calculus I",
                ModuleCode = "MATH101",
                Instructor = "Dr. Emily Rodriguez",
                Description = "Introduction to differential and integral calculus with applications in science and engineering.",
                Duration = "14 weeks",
                Capacity = "30/35",
                Credits = "4 credits",
                Category = "Mathematics",
                Icon = "calculus.png",
                CategoryColor = Colors.Green,
                Status = "Available",
                StatusColor = Colors.Green,
                ActionText = "Enroll Now",
                ActionColor = Colors.Blue,
                IsEnrolled = false
            },
            new ModuleItem
            {
                ModuleName = "Linear Algebra",
                ModuleCode = "MATH201",
                Instructor = "Dr. Robert Kim",
                Description = "Vector spaces, matrices, determinants, eigenvalues, and linear transformations.",
                Duration = "12 weeks",
                Capacity = "22/25",
                Credits = "3 credits",
                Category = "Mathematics",
                Icon = "mathematic.png",
                CategoryColor = Colors.Teal,
                Status = "Available",
                StatusColor = Colors.Green,
                ActionText = "Enroll Now",
                ActionColor = Colors.Blue,
                IsEnrolled = false
            },
            new ModuleItem
            {
                ModuleName = "Engineering Mechanics",
                ModuleCode = "ENG101",
                Instructor = "Prof. David Wilson",
                Description = "Statics and dynamics of particles and rigid bodies, force analysis, and equilibrium.",
                Duration = "16 weeks",
                Capacity = "18/20",
                Credits = "4 credits",
                Category = "Engineering",
                Icon = "engineering.png",
                CategoryColor = Colors.Red,
                Status = "Available",
                StatusColor = Colors.Green,
                ActionText = "Enroll Now",
                ActionColor = Colors.Blue,
                IsEnrolled = false
            },
            new ModuleItem
            {
                ModuleName = "Thermodynamics",
                ModuleCode = "ENG301",
                Instructor = "Dr. Lisa Anderson",
                Description = "Laws of thermodynamics, heat engines, refrigeration cycles, and energy conversion.",
                Duration = "14 weeks",
                Capacity = "15/20",
                Credits = "3 credits",
                Category = "Engineering",
                Icon = "thermodynamics.png",
                CategoryColor = Colors.Orange,
                Status = "Available",
                StatusColor = Colors.Green,
                ActionText = "Enroll Now",
                ActionColor = Colors.Blue,
                IsEnrolled = false
            },
            new ModuleItem
            {
                ModuleName = "Business Analytics",
                ModuleCode = "BUS201",
                Instructor = "Prof. Amanda Taylor",
                Description = "Data-driven decision making, statistical analysis, and business intelligence tools.",
                Duration = "10 weeks",
                Capacity = "28/30",
                Credits = "3 credits",
                Category = "Business",
                Icon = "monitor.png",
                CategoryColor = Colors.Indigo,
                Status = "Enrolled",
                StatusColor = Colors.Orange,
                ActionText = "View Details",
                ActionColor = Colors.Gray,
                IsEnrolled = true
            },
            new ModuleItem
            {
                ModuleName = "Marketing Fundamentals",
                ModuleCode = "BUS101",
                Instructor = "Dr. James Brown",
                Description = "Principles of marketing, consumer behavior, market research, and promotional strategies.",
                Duration = "12 weeks",
                Capacity = "25/30",
                Credits = "3 credits",
                Category = "Business",
                Icon = "digital_marketing.png",
                CategoryColor = Colors.Pink,
                Status = "Waitlist",
                StatusColor = Colors.Yellow,
                ActionText = "Join Waitlist",
                ActionColor = Colors.Orange,
                IsEnrolled = false
            }
        };

        ApplyFilter("All");
        UpdateStats();
    }

    private void ApplyFilter(string filter)
    {
        currentFilter = filter;

        var filtered = filter == "All"
            ? allModules
            : allModules.Where(m => m.Category == filter).ToList();

        if (!string.IsNullOrWhiteSpace(SearchEntry.Text))
        {
            var searchTerm = SearchEntry.Text.ToLower();
            filtered = filtered.Where(m =>
                m.ModuleName.ToLower().Contains(searchTerm) ||
                m.ModuleCode.ToLower().Contains(searchTerm) ||
                m.Instructor.ToLower().Contains(searchTerm) ||
                m.Description.ToLower().Contains(searchTerm)
            ).ToList();
        }

        Modules.Clear();
        foreach (var module in filtered)
        {
            Modules.Add(module);
        }

        ModulesCollection.ItemsSource = Modules;
        UpdateFilterButtons();
    }

    private void UpdateFilterButtons()
    {
        // Reset all buttons
        AllFilterBtn.BackgroundColor = Colors.LightGray;
        AllFilterBtn.TextColor = Colors.Gray;
        ComputerScienceFilterBtn.BackgroundColor = Colors.LightGray;
        ComputerScienceFilterBtn.TextColor = Colors.Gray;
        MathematicsFilterBtn.BackgroundColor = Colors.LightGray;
        MathematicsFilterBtn.TextColor = Colors.Gray;
        EngineeringFilterBtn.BackgroundColor = Colors.LightGray;
        EngineeringFilterBtn.TextColor = Colors.Gray;
        BusinessFilterBtn.BackgroundColor = Colors.LightGray;
        BusinessFilterBtn.TextColor = Colors.Gray;

        // Highlight active filter
        switch (currentFilter)
        {
            case "All":
                AllFilterBtn.BackgroundColor = Colors.Blue;
                AllFilterBtn.TextColor = Colors.White;
                break;
            case "Computer Science":
                ComputerScienceFilterBtn.BackgroundColor = Colors.Blue;
                ComputerScienceFilterBtn.TextColor = Colors.White;
                break;
            case "Mathematics":
                MathematicsFilterBtn.BackgroundColor = Colors.Blue;
                MathematicsFilterBtn.TextColor = Colors.White;
                break;
            case "Engineering":
                EngineeringFilterBtn.BackgroundColor = Colors.Blue;
                EngineeringFilterBtn.TextColor = Colors.White;
                break;
            case "Business":
                BusinessFilterBtn.BackgroundColor = Colors.Blue;
                BusinessFilterBtn.TextColor = Colors.White;
                break;
        }
    }

    private void UpdateStats()
    {
        TotalModulesLabel.Text = allModules.Count.ToString();
        AvailableModulesLabel.Text = allModules.Count(m => m.Status == "Available").ToString();
        EnrolledModulesLabel.Text = allModules.Count(m => m.IsEnrolled).ToString();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter(currentFilter);
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var filter = button.Text;
            ApplyFilter(filter);
        }
    }

    private async void OnAdvancedSearchClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Advanced Search", "Advanced search filters coming soon!", "OK");
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        ModulesRefreshView.IsRefreshing = true;

        // Simulate refreshing data
        await Task.Delay(1000);
        LoadModules();

        ModulesRefreshView.IsRefreshing = false;
    }

    private async void OnModuleActionClicked(ModuleItem module)
    {
        if (module.IsEnrolled)
        {
            // Navigate to module details
            await DisplayAlert("Module Details",
                $"You are enrolled in {module.ModuleName}\n\nInstructor: {module.Instructor}\nCredits: {module.Credits}\nDuration: {module.Duration}",
                "OK");
        }
        else if (module.Status == "Available")
        {
            // Show enrollment confirmation
            bool enroll = await DisplayAlert("Enroll in Module",
                $"Do you want to enroll in {module.ModuleName}?\n\nInstructor: {module.Instructor}\nCredits: {module.Credits}\nDuration: {module.Duration}",
                "Enroll", "Cancel");

            if (enroll)
            {
                await EnrollInModuleAsync(module);
            }
        }
        else if (module.Status == "Waitlist")
        {
            // Join waitlist
            bool joinWaitlist = await DisplayAlert("Join Waitlist",
                $"This module is currently full. Would you like to join the waitlist for {module.ModuleName}?",
                "Join Waitlist", "Cancel");

            if (joinWaitlist)
            {
                await JoinWaitlistAsync(module);
            }
        }
    }

    private async Task EnrollInModuleAsync(ModuleItem module)
    {
        try
        {
            // Simulate enrollment API call
            await Task.Delay(1500);

            // Update module status
            module.IsEnrolled = true;
            module.Status = "Enrolled";
            module.StatusColor = Colors.Orange;
            module.ActionText = "View Details";
            module.ActionColor = Colors.Gray;

            // Refresh the collection
            var index = Modules.IndexOf(module);
            if (index >= 0)
            {
                Modules[index] = module;
            }

            // Update stats
            UpdateStats();

            await DisplayAlert("Enrollment Successful",
                $"You have successfully enrolled in {module.ModuleName}!",
                "Great!");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Enrollment Failed",
                $"Failed to enroll in {module.ModuleName}: {ex.Message}",
                "OK");
        }
    }

    private async Task JoinWaitlistAsync(ModuleItem module)
    {
        try
        {
            // Simulate waitlist API call
            await Task.Delay(1000);

            await DisplayAlert("Waitlist Joined",
                $"You have been added to the waitlist for {module.ModuleName}. You will be notified if a spot becomes available.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Waitlist Failed",
                $"Failed to join waitlist for {module.ModuleName}: {ex.Message}",
                "OK");
        }
    }
}

// Data model for modules
public class ModuleItem
{
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Capacity { get; set; } = string.Empty;
    public string Credits { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Color CategoryColor { get; set; }
    public string Status { get; set; } = string.Empty;
    public Color StatusColor { get; set; }
    public string ActionText { get; set; } = string.Empty;
    public Color ActionColor { get; set; }
    public bool IsEnrolled { get; set; }
}