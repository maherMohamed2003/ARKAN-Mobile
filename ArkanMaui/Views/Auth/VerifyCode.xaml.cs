using ArkanMaui.ViewModels.DTOs.RestoreAccountDTOs;
using System.Text;
using System.Text.Json;

namespace ArkanMaui.Views.Auth;

public partial class VerifyCode : ContentPage
{
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(20) };
    private const string RequestCodeUrl = "http://arkanteam.runasp.net/api/Auth/RequestCode";
    private static readonly Color EmptyFill = Color.FromArgb("#2B2D2E");
    private static readonly Color FilledFill = Color.FromArgb("#6B1F37");
    private const int TotalSeconds = 180; // 03:00
    private readonly Border[] _boxes;
    private readonly Label[] _digits;
    private IDispatcherTimer? _timer;
    private int _secondsLeft;

    private int _userId;
    private string _email = string.Empty;
    private string _methodName = string.Empty;


    public VerifyCode()
    {
        InitializeComponent();

        _boxes = new[] { Box1, Box2, Box3, Box4, Box5, Box6 };
        _digits = new[] { Digit1, Digit2, Digit3, Digit4, Digit5, Digit6 };

        StartTimer();
    }


    // بتتنادي من صفحة RestoreMethods بعد ما تختاري الطريقة وتدوسي RESET
    public VerifyCode(int userId, string email, string methodName, string methodTarget) : this()
    {
        _userId = userId;
        _email = email;
        _methodName = methodName;
        DescriptionLabel.Text = $"Enter the digit code sent to your.";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CodeEntry.Focus();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
    }


    private void OnCodeTextChanged(object sender, TextChangedEventArgs e)
    {
        var raw = e.NewTextValue ?? string.Empty;
        var text = new string(raw.Where(char.IsDigit).ToArray());

        // لو دخل حرف مش رقم نشيله
        if (text != raw)
        {
            CodeEntry.Text = text;
            return;
        }

        for (int i = 0; i < _boxes.Length; i++)
        {
            bool filled = i < text.Length;
            _digits[i].Text = filled ? text[i].ToString() : string.Empty;
            _boxes[i].BackgroundColor = filled ? FilledFill : EmptyFill;
        }
    }

    private void StartTimer()
    {
        _secondsLeft = TotalSeconds;
        UpdateTimerLabel();
        ResendLabel.Opacity = 0.5;

        _timer?.Stop();
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) =>
        {
            _secondsLeft--;
            UpdateTimerLabel();

            if (_secondsLeft <= 0)
            {
                _timer?.Stop();
                ResendLabel.Opacity = 1;
            }
        };
        _timer.Start();
    }

    private void UpdateTimerLabel()
    {
        var t = TimeSpan.FromSeconds(Math.Max(_secondsLeft, 0));
        TimerLabel.Text = $"{(int)t.TotalMinutes:00}:{t.Seconds:00}";
    }

    // Resend code: نفس endpoint الـ RequestCode
    private async void OnResendTapped(object sender, TappedEventArgs e)
    {
        if (_secondsLeft > 0 || string.IsNullOrEmpty(_email))
            return; // لسه التايمر شغال

        ResendLabel.Opacity = 0.5;

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
                CodeEntry.Text = string.Empty;
                StartTimer();
                CodeEntry.Focus();
            }
            else
            {
                ResendLabel.Opacity = 1;
                await DisplayAlert("Error", "Couldn't resend the code. Please try again.", "OK");
            }
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException or JsonException)
        {
            ResendLabel.Opacity = 1;
            await DisplayAlert("Error", "Something went wrong. Please try again.", "OK");
        }
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        var code = CodeEntry.Text ?? string.Empty;

        if (code.Length != 6)
        {
            await DisplayAlert("Error", "Please enter the 6-digit code.", "OK");
            return;
        }

        // هنا نادي endpoint الـ Check-Code بالإيميل والكود (_email و code)
        // وبعدها افتحي صفحة الباسورد الجديد (Reset-Password)
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}