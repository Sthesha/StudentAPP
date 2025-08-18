namespace StudentAPP;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Start with SignIn page instead of StudentLoginPage to avoid AppShell issues

        MainPage = new NavigationPage(new StudentLoginPage());
    }

}