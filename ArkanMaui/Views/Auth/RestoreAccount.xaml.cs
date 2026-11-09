using ArkanMaui.ViewModels.DTOs.RestoreAccountDTOs;
using System.Text;
using System.Text.Json;

namespace ArkanMaui.Views.Auth;

public partial class RestoreAccount : ContentPage
{
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(20) };

    
    private const string RestoreUrl = "http://arkanteam.runasp.net/api/Auth/GetRestoreMethods";

    public RestoreAccount()
	{
        InitializeComponent();
    }
    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            await DisplayAlert("Error", "Please enter a valid email.", "OK");
            return;
        }

        var button = (Button)sender;
        button.IsEnabled = false;

        try
        {
            var request = new RestoreAccountRequestDTO { Email = email };
            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _client.PostAsync(RestoreUrl, content);


            var result = await response.Content.ReadAsStringAsync();

            var restoreResponse = JsonSerializer.Deserialize<RestoreAccountResponseDTO>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response.IsSuccessStatusCode && restoreResponse?.isSuccess == true && restoreResponse.value != null)
            {
                var data = restoreResponse.value;

                await Navigation.PushAsync(new RestoreMethods(data.UserId, email, data.RestoreMethodResponseDTOs));

            }
            else
            {
                await DisplayAlert("Error", "We couldn't find an account with this email.", "OK");
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
        catch (JsonException)
        {
            await DisplayAlert("Error", "Unexpected response from the server.", "OK");
        }
        finally
        {
            button.IsEnabled = true;
        }
    }
}