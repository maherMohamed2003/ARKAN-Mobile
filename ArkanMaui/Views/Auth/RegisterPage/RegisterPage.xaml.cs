namespace ArkanMaui.Views.Auth;

public partial class RegisterPage : ContentPage
{
    double fullHeight;

    Entry? current;
    bool hasFocus;
    DateTime focusedAt;

    IDispatcherTimer? keyboardWatcher;

    static readonly Thickness NormalPadding = new(0, 0, 0, 12);

    public RegisterPage()
    {
        InitializeComponent();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0)
            return;

        if (height > fullHeight)
        {
            fullHeight = height;
            BackgroundImage.HeightRequest = fullHeight;
            TopSpacer.HeightRequest = fullHeight * 0.41;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        keyboardWatcher?.Stop();
    }

    async void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        hasFocus = true;
        current = sender as Entry;
        focusedAt = DateTime.UtcNow;
        FormStack.Padding = new Thickness(0, 0, 0, fullHeight * 0.34);

        StartKeyboardWatcher();

        if (sender is VisualElement entry)
        {
            await Task.Delay(300); 
            if (hasFocus)
                await PageScroll.ScrollToAsync(entry, ScrollToPosition.Start, true);
        }
    }

    
    async void OnEntryUnfocused(object? sender, FocusEventArgs e)
    {
        hasFocus = false;
        await Task.Delay(250); 

        if (!hasFocus)
            await ResetFormAsync();
    }

    
    void StartKeyboardWatcher()
    {
        if (keyboardWatcher == null)
        {
            keyboardWatcher = Dispatcher.CreateTimer();
            keyboardWatcher.Interval = TimeSpan.FromMilliseconds(250);
            keyboardWatcher.Tick += async (s, e) =>
            {
                try
                {
                    if ((DateTime.UtcNow - focusedAt).TotalMilliseconds < 1000)
                        return;

                    if (current != null && !IsKeyboardVisible())
                        await ResetFormAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"KeyboardWatcher error: {ex}");
                }
            };
        }

        keyboardWatcher.Start();
    }

    bool IsKeyboardVisible()
    {
#if ANDROID
        var decor = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView;
        if (decor != null)
        {
            var insets = AndroidX.Core.View.ViewCompat.GetRootWindowInsets(decor);
            if (insets != null)
                return insets.IsVisible(AndroidX.Core.View.WindowInsetsCompat.Type.Ime());
        }
#endif
        return current?.IsSoftInputShowing() ?? false;
    }

    async Task ResetFormAsync()
    {
        keyboardWatcher?.Stop();
        hasFocus = false;

        var entry = current;
        current = null;
        entry?.Unfocus();

        FormStack.Padding = NormalPadding;
        await Task.Delay(50);
        await PageScroll.ScrollToAsync(0, 0, true);
    }
}
