using ArkanMaui.ViewModels.DTOs.RestoreAccountDTOs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using static ArkanMaui.ViewModels.DTOs.RestoreAccountDTOs.RestoreAccountResponseDTO;

namespace ArkanMaui.Views.Auth;

public partial class RestoreMethods : ContentPage
{

	private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(20) };
	private const string RequestCodeUrl = "http://arkanteam.runasp.net/api/Auth/RequestCode";
	
	private readonly int _userId;
	private readonly string _email;
	public ObservableCollection<RestoreMethodItem> Methods { get; set; } = new ();
    public RestoreMethods(int userId, string email, List<RestoreMethodDTO> methods)
	{
		InitializeComponent();
        _userId = userId;
        _email = email;
        SetupCard(GmailCard, "Gmail", methods, alwaysShow: true);
        SetupCard(WhatsappCard, "Whatsapp", methods, alwaysShow: true);

		if (Methods.Count > 0)
			Methods[0].IsSelected = true;
    }

	private void SetupCard(Border card, string methodName,
        List<RestoreMethodDTO> methods, bool alwaysShow = false)
    {
        var dto = methods.FirstOrDefault(m =>
            m.MethodName.Equals(methodName, StringComparison.OrdinalIgnoreCase));

        if (dto == null && !alwaysShow)
            return; 

        var item = new RestoreMethodItem(methodName, dto?.MethodTarget ?? string.Empty);
        card.BindingContext = item;
        card.IsVisible = true;
        Methods.Add(item);
    }

    
    private void OnMethodTapped(object sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not RestoreMethodItem tapped)
            return;

        foreach (var m in Methods)
            m.IsSelected = m == tapped;
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        var selected = Methods.FirstOrDefault(m => m.IsSelected);
        if (selected == null)
        {
            await DisplayAlert("Error", "Please choose a method.", "OK");
            return;
        }

        var button = (Button)sender;
        button.IsEnabled = false;

        try
        {
            
            var request = new RequestCodeRequestDTO { Email = _email };
            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _client.PostAsync(RequestCodeUrl, content);

            var result = await response.Content.ReadAsStringAsync();
            var codeResponse = JsonSerializer.Deserialize<RequestCodeResponseDTO>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response.IsSuccessStatusCode && codeResponse?.IsSuccess == true)
            {
                await Navigation.PushAsync(new VerifyCode(_userId, _email, selected.DisplayName, selected.Target));
            }
            else
            {
                await DisplayAlert("Error", "Couldn't send the code. Please try again.", "OK");
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

    // يرجع لصفحة الـ Login
    private async void OnSignInTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}

// عنصر واحد في القايمة
public class RestoreMethodItem : INotifyPropertyChanged
{
    public string MethodName { get; }
    public string Target { get; }
    public string DisplayName { get; }
    public string Icon { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public RestoreMethodItem(string methodName, string target)
    {
        MethodName = methodName;
        Target = FormatTarget(methodName, target);

        
        (DisplayName, Icon) = methodName.ToLowerInvariant() switch
        {
            "gmail" => ("Email", "icon_email.png"),
            "whatsapp" => ("WhatsApp", "icon_whatsapp.png"),
            _ => (methodName, "icon_email.png")
        };
    }


    private static string FormatTarget(string methodName, string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return string.Empty;

        var t = target.Trim();
        var key = methodName.ToLowerInvariant();

        if (key == "whatsapp")
        {
            if (t.StartsWith("0"))
                t = t[1..];

            if (t.StartsWith("+20"))
                t = t[3..];

            if (t.Length > 4)
                t = t[..2] + new string('*', t.Length - 4) + t[^2..];

            return "+20 " + t;
        }


        if (!t.Contains('*'))
        {
            var at = t.IndexOf('@');

            if (at > 2)
                t = t[..2] + new string('*', at - 2) + t[at..];
        }

        return t;
    }


    public event PropertyChangedEventHandler? PropertyChanged;
}