using ArkanMaui.LoginPage;
using System.Net.Http.Headers;

namespace ArkanMaui.Views.Auth;

public partial class RegisterPage : ContentPage
{
    double fullHeight;

    Entry? current;
    bool hasFocus;
    DateTime focusedAt;

    IDispatcherTimer? keyboardWatcher;
    private const int MaxProfileImageSize = 5 * 1024 * 1024;
    public byte[]? SelectedProfileImageBytes { get; private set; }
    public string? SelectedProfileImageFileName { get; private set; }
    static readonly Thickness NormalPadding = new(0, 0, 0, 12);
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(20) };
    public RegisterPage()
    {
        InitializeComponent();
    }

    

    async void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        hasFocus = true;
        current = sender as Entry;
        focusedAt = DateTime.UtcNow;
        FormStack.Padding = new Thickness(0, 0, 0, fullHeight * 0.34);

        StartKeyboardWatcher();
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
    }

    private async void OnSignInTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new Login());
    }

    private async void RegisterBTN(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (!button.IsEnabled)
            return;

        var fname = FirstName.Text?.Trim() ?? "";
        var lname = LastName.Text?.Trim() ?? "";
        var email = Email.Text?.Trim() ?? "";
        var phone = Phone.Text?.Trim() ?? "";
        var pass1 = Password1.Text ?? "";
        var pass2 = Password2.Text ?? "";

        if (pass1 != pass2)
        {
            await DisplayAlert("Error", "Passwords do not match.", "OK");
            return;
        }

        button.IsEnabled = false;
        button.Text = "Registering...";

        await Task.Yield();

        try
        {
            var api = "http://arkanteam.runasp.net/api/Auth/Register";

            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(fname), "FirstName");
            form.Add(new StringContent(lname), "LastName");
            form.Add(new StringContent(email), "Email");
            form.Add(new StringContent(phone), "Phone");
            form.Add(new StringContent(pass1), "Password");

            if (SelectedProfileImageBytes != null)
            {
                var imageContent =
                    new ByteArrayContent(SelectedProfileImageBytes);

                var extension = Path.GetExtension(
                    SelectedProfileImageFileName ?? "").ToLowerInvariant();

                imageContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        extension == ".png" ? "image/png" : "image/jpeg");

                form.Add(
                    imageContent,
                    "Image",
                    SelectedProfileImageFileName ?? "profile.jpg");
            }

            

            var response = await _client.PostAsync(api, form);

            var result = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine(
                $"Status Code: {(int)response.StatusCode}");

            System.Diagnostics.Debug.WriteLine($"Response: {result}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Success",
                    "Registered Successfully!",
                    "OK");

                await Navigation.PushAsync(new Login());
            }
            else
            {
                await DisplayAlert(
                    "Registration Failed",
                    result,
                    "OK");
            }
        }
        catch (TaskCanceledException)
        {
            await DisplayAlert(
                "Timeout",
                "The server took too long to respond.",
                "OK");
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert(
                "Connection Error",
                ex.Message,
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.ToString(), "OK");
        }
        finally
        {
            button.IsEnabled = true;
            button.Text = "Register";
        }
    }
    private async void OnUploadImageClicked(object sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Choose your profile image",
                FileTypes = FilePickerFileType.Images
            };

            var file = await FilePicker.Default.PickAsync(options);

            // The user cancelled the picker.
            if (file is null)
                return;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".jpg" &&
                extension != ".jpeg" &&
                extension != ".png")
            {
                await DisplayAlert(
                    "Invalid image",
                    "Please select a JPG, JPEG, or PNG image.",
                    "OK");
                return;
            }

            await using var input = await file.OpenReadAsync();
            using var output = new MemoryStream();

            // Read at most 5 MB + 1 byte, so oversized files are rejected.
            var buffer = new byte[81920];
            int totalBytes = 0;
            int bytesRead;

            while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                totalBytes += bytesRead;

                if (totalBytes > MaxProfileImageSize)
                {
                    await DisplayAlert(
                        "Image too large",
                        "The maximum image size is 5 MB.",
                        "OK");
                    return;
                }

                await output.WriteAsync(buffer, 0, bytesRead);
            }

            if (totalBytes == 0)
            {
                await DisplayAlert(
                    "Invalid image",
                    "The selected image is empty.",
                    "OK");
                return;
            }

            SelectedProfileImageBytes = output.ToArray();
            SelectedProfileImageFileName = file.FileName;

            // Show the selected image in the preview.
            ProfileImagePreview.Source = ImageSource.FromStream(
                () => new MemoryStream(SelectedProfileImageBytes));

            ProfileImagePreview.IsVisible = true;
            UploadIcon.IsVisible = false;

            UploadStatusLabel.Text = file.FileName;
            UploadHintLabel.Text = "Image selected successfully";

            // Change the button caption after selecting an image.
            if (sender is Button uploadButton)
                uploadButton.Text = "Change";
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Image selection error",
                ex.Message,
                "OK");
        }
    }

}
