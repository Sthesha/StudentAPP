using Microsoft.Extensions.Logging;
using StudentAPP.Services;

namespace StudentAPP;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Services.AddLogging(logging =>
        {
            logging.AddDebug();
        });
#endif

        // Register DatabaseService as Singleton
        builder.Services.AddSingleton<DatabaseService>();

        builder.Services.AddSingleton<MockApiService>();

        return builder.Build();
    }
}