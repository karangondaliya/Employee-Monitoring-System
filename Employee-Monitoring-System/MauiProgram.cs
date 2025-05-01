using Employee_Monitoring_System.Services;
using Employee_Monitoring_System.ViewModels;
using Employee_Monitoring_System.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

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
            builder.ConfigureLifecycleEvents(events => {
            #if WINDOWS
                events.AddWindows(windows => windows.OnWindowCreated((window) => {
                    window.ExtendsContentIntoTitleBar = false;
                }));
            #endif
            });
            builder.Services.AddSingleton<HttpClient>(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7227/") });
            builder.Services.AddSingleton<HolidayService>();
            builder.Services.AddSingleton<ProjectService>();
            builder.Services.AddTransient<ProjectsViewModel>();
            builder.Services.AddTransient<ProjectsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
