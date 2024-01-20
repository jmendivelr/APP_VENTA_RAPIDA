using LoginApp.Maui.ViewModels;
using LoginApp.Maui.Views;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui;

namespace LoginApp.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<LoginPage>();

            builder.Services.AddSingleton<VentaRapidaPage>();
            builder.Services.AddSingleton<AboutPage>();
            builder.Services.AddSingleton<LoginPageViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}