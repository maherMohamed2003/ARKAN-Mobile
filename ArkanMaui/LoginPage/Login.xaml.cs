using ArkanMaui.DTOs;
using System.Text;
using System.Text.Json;

namespace ArkanMaui.LoginPage;

public partial class Login : ContentPage
{
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(20) };
    private const string LoginUrl = "http://arkanteam.runasp.net/api/Auth/Login";
    private int passcheck = 0;
    public Login()
    {
        InitializeComponent();
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        TogglePasswordButton.Source = PasswordEntry.IsPassword ? "lockedeye.png" : "eye.png";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter your email and password.", "OK");
            return;
        }

        var button = (Button)sender;
        button.IsEnabled = false;

        try
        {
            var json = JsonSerializer.Serialize(new { email, password });
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _client.PostAsync(LoginUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponseDTO>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                 await SecureStorage.SetAsync("token", loginResponse.value.accessToken);
                // لو RememberCheck.IsChecked احفظ الإيميل برضو
                await DisplayAlert("Success", "Welcome Back! \n"+loginResponse.value.fullName, "OK");
            }
            else
            {
                await DisplayAlert("Error", result, "OK");
            }
        }
        catch (TaskCanceledException)
        {
            await DisplayAlert("Error", "The request timed out.", "OK");
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Network error", ex.Message, "OK");
        }
        finally
        {
            button.IsEnabled = true;
        }
    }

    private async void OnGoogleTapped(object sender, TappedEventArgs e) { }
    private async void OnGuestTapped(object sender, TappedEventArgs e) { }
    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e) { }
    private async void OnSignUpTapped(object sender, TappedEventArgs e) { }
}