using ArkanMaui.LoginPage;
using ArkanMaui.ViewModels.DTOs.RestoreAccountDTOs;

namespace ArkanMaui.Views.Auth;

public partial class ResetPassword : ContentPage
{
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(20) };
    private const string ResetPasswordUrl = "http://arkanteam.runasp.net/api/Auth/Reset-Password";

    private static readonly Color Filled = Color.FromArgb("#C5A15B");
    private static readonly Color Empty = Color.FromArgb("#4A4A4A");

    private readonly int _userId;

    public ResetPassword(int userId)
    {
        InitializeComponent();
        _userId = userId;
    }

    private void OnToggleNewTapped(object sender, TappedEventArgs e)
    {
        NewPassword.IsPassword = !NewPassword.IsPassword;
        EyeNew.Source = NewPassword.IsPassword ? "lockedeye.png" : "eye.png";
    }

    private void OnToggleConfirmTapped(object sender, TappedEventArgs e)
    {
        ConfirmPassword.IsPassword = !ConfirmPassword.IsPassword;
        EyeConfirm.Source = ConfirmPassword.IsPassword ? "lockedeye.png" : "eye.png";
    }

    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        string p = e.NewTextValue ?? "";
        int score = 0;

        if (p.Length >= 8) score++;
        if (p.Any(char.IsUpper) && p.Any(char.IsLower)) score++;
        if (p.Any(char.IsDigit)) score++;
        if (p.Any(c => !char.IsLetterOrDigit(c))) score++;

        if (p.Length > 0 && score == 0) score = 1;

        var bars = new[] { Bar1, Bar2, Bar3, Bar4 };
        for (int i = 0; i < bars.Length; i++)
             bars[i].Background = i < score ? (Brush)Resources["FilledBrush"] : (Brush)Resources["EmptyBrush"];
            //bars[i].Color = i < score ? Color.FromArgb("#C5A15B") : Color.FromArgb("#4A4A4A");

        StrengthLabel.Text = score switch
        {
            0 => "",
            1 => "Weak",
            2 => "Medium",
            3 => "Strong",
            _ => "Very Strong"
        };
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewPassword.Text) ||
            string.IsNullOrWhiteSpace(ConfirmPassword.Text))
        {
            await DisplayAlertAsync("Error", "Please fill both fields", "OK");
            return;
        }

        if (NewPassword.Text != ConfirmPassword.Text)
        {
            await DisplayAlertAsync("Error", "Passwords do not match", "OK");
            return;
        }

        try
        {
            var dto = new ResetPasswordDTO
            {
                newPassword = NewPassword.Text,
                userId = _userId
            };

            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(ResetPasswordUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync("Error", "Reset failed. Please try again.", "OK");
                return;
            }

            await DisplayAlertAsync("Success", "Your password has been changed.", "OK");
            await Navigation.PushAsync(new Login());
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Error", "Something went wrong. Please try again.", "OK");
        }
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new Login());
    }
}