using Maui.Rtsp;
using Maui.Rtsp.Controls;
#if ANDROID
using Maui.Rtsp.Handlers;
#endif
using Microsoft.Extensions.Logging;
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
#if ANDROID
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<CameraView, CameraViewHandler>();
            });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
