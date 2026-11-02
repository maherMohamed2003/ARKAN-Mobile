using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

namespace ArkanMaui
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
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("PlusJakartaSans-Regular.ttf", "JakartaRegular");
                    fonts.AddFont("PlusJakartaSans-Medium.ttf", "JakartaMedium");
                    fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "JakartaSemiBold");
                    fonts.AddFont("ZenAntiqueSoft-Regular.ttf", "ZenAntique");
                    fonts.AddFont("Cinzel-Regular.ttf", "CinzelRegular");
                });  
            EntryHandler.Mapper.AppendToMapping("MyHandler", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.Background = null;
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.SetPadding(0, 0, 0, 0);
#elif WINDOWS
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
#endif
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
