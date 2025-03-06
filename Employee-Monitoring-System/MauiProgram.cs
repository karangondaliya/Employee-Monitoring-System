using Microsoft.Extensions.Logging;

namespace Employee_Monitoring_System
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("SegoeUIVariable.ttf", "SegoeUI");
                });
            builder.Services.AddSingleton<HttpClient>(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7227/") });


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
