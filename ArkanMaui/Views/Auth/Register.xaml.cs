
using System.Net.Http.Headers;
using System.Text.Json;
using ArkanMaui.LoginPage;

namespace ArkanMaui.Views.Auth;

public partial class RegisterPage : ContentPage
{
    private static readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private FileResult? _selectedImage;

    private const string ApiUrl =
        "http://arkanteam.runasp.net/api/Auth/Register";

    public RegisterPage()
    {
        InitializeComponent();
    }

    // REGISTER BUTTON
    private async void RegisterBTN(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FirstName.Text) ||
            string.IsNullOrWhiteSpace(LastName.Text) ||
            string.IsNullOrWhiteSpace(Email.Text) ||
            string.IsNullOrWhiteSpace(Phone.Text) ||
            string.IsNullOrWhiteSpace(Password1.Text) ||
            string.IsNullOrWhiteSpace(Password2.Text))
        {
            await DisplayAlert("Required Fields",
                "Please fill in all required fields.", "OK");
            return;
        }

        string firstName = FirstName.Text.Trim();
        string lastName = LastName.Text.Trim();
        string email = Email.Text.Trim();
        string phone = Phone.Text.Trim();
        string password = Password1.Text;
        string confirmPassword = Password2.Text;

        if (password != confirmPassword)
        {
            await DisplayAlert("Password Error",
                "Passwords do not match.", "OK");
            return;
        }

        if (password.Length < 8)
        {
            await DisplayAlert("Password Error",
                "Password must be at least 8 characters.", "OK");
            return;
        }

        if (!email.Contains('@') || !email.Contains('.'))
        {
            await DisplayAlert("Email Error",
                "Please enter a valid email address.", "OK");
            return;
        }

        RegisterButtonLoading(true);

        try
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(firstName), "FirstName");
            content.Add(new StringContent(lastName), "LastName");
            content.Add(new StringContent(email), "Email");
            content.Add(new StringContent(phone), "Phone");
            content.Add(new StringContent(password), "Password");

            // OPTIONAL PROFILE IMAGE
            if (_selectedImage != null)
            {
                var stream = await _selectedImage.OpenReadAsync();

                var imageContent = new StreamContent(stream);

                imageContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        _selectedImage.ContentType ?? "application/octet-stream");

                content.Add(
                    imageContent,
                    "Image",
                    _selectedImage.FileName
                );
            }

            using var response = await _client.PostAsync(ApiUrl, content);

            string responseBody = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine(
                $"Final URL: {response.RequestMessage?.RequestUri}");

            System.Diagnostics.Debug.WriteLine(
                $"Status Code: {(int)response.StatusCode}");

            System.Diagnostics.Debug.WriteLine(responseBody);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Registration Successful",
                    "Your account has been created successfully.",
                    "Continue"
                );

                await Navigation.PushAsync(new Login());
            }
            else
            {
                string errorMessage = GetErrorMessage(responseBody);

                await DisplayAlert(
                    "Registration Failed",
                    errorMessage,
                    "Try Again"
                );
            }
        }
        catch (TaskCanceledException)
        {
            await DisplayAlert(
                "Connection Timeout",
                "The request took too long. Please try again.",
                "OK"
            );
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert(
                "Connection Error",
                $"Could not connect to the server.\n{ex.Message}",
                "OK"
            );
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Unexpected Error",
                ex.Message,
                "OK"
            );
        }
        finally
        {
            RegisterButtonLoading(false);
        }
    }

    // UPLOAD IMAGE BUTTON
    private async void OnUploadImageClicked(object sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Select Profile Image",
                FileTypes = FilePickerFileType.Images
            };

            var result = await FilePicker.Default.PickAsync(options);

            if (result == null)
                return;

            string extension = Path.GetExtension(result.FileName).ToLowerInvariant();

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
            {
                await DisplayAlert(
                    "Invalid Image",
                    "Only JPG, JPEG, and PNG images are allowed.",
                    "OK"
                );
                return;
            }

            using var stream = await result.OpenReadAsync();

            if (stream.Length > 5 * 1024 * 1024)
            {
                await DisplayAlert(
                    "Image Too Large",
                    "Image size must not exceed 5 MB.",
                    "OK"
                );
                return;
            }

            _selectedImage = result;

            using var previewStream = await result.OpenReadAsync();

            ProfileImagePreview.Source =
                ImageSource.FromStream(() => result.OpenReadAsync().Result);

            ProfileImagePreview.IsVisible = true;
            UploadIcon.IsVisible = false;

            UploadStatusLabel.Text = result.FileName;
            UploadHintLabel.Text = "Image selected successfully";
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Image Error",
                ex.Message,
                "OK"
            );
        }
    }

    // SIGN IN NAVIGATION
    private async void OnSignInTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new Login());
    }

    // REGISTER BUTTON LOADING STATE
    private void RegisterButtonLoading(bool isLoading)
    {
        if (this.FindByName<Button>("RegisterButton") is Button button)
        {
            button.IsEnabled = !isLoading;
            button.Text = isLoading ? "REGISTERING..." : "REGISTER";
        }
    }

    // READ API ERROR MESSAGE
    private string GetErrorMessage(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);

            var root = document.RootElement;

            if (root.TryGetProperty("message", out var message))
                return message.GetString() ?? "Registration failed.";

            if (root.TryGetProperty("title", out var title))
                return title.GetString() ?? "Registration failed.";

            if (root.TryGetProperty("errors", out var errors))
            {
                var errorMessages = new List<string>();

                foreach (var property in errors.EnumerateObject())
                {
                    foreach (var error in property.Value.EnumerateArray())
                    {
                        errorMessages.Add(error.GetString() ?? "");
                    }
                }

                if (errorMessages.Count > 0)
                    return string.Join("\n", errorMessages);
            }
        }
        catch
        {
            // Response is not JSON.
        }

        return string.IsNullOrWhiteSpace(responseBody)
            ? "Something went wrong. Please try again."
            : responseBody;
    }
}
