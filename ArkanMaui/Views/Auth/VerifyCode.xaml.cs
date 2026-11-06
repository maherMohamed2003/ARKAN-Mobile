namespace ArkanMaui.Views.Auth;

public partial class VerifyCode : ContentPage
{
    private static readonly Color EmptyFill = Color.FromArgb("#2B2D2E");
    private static readonly Color EmptyStroke = Color.FromArgb("#D9D2C5");
    private static readonly Color FilledFill = Color.FromArgb("#6B1F37");   
    private static readonly Color FilledStroke = Color.FromArgb("#C9A55C"); 
    private const int TotalSeconds = 180; // 03:00
    private readonly Border[] _boxes;
    private readonly Label[] _digits;
    private IDispatcherTimer? _timer;
    private int _secondsLeft;

    public VerifyCode()
	{
		InitializeComponent();

        _boxes = new[] { Box1, Box2, Box3, Box4, Box5, Box6 };
        _digits = new[] { Digit1, Digit2, Digit3, Digit4, Digit5, Digit6 };

        StartTimer();
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
            _boxes[i].Stroke = filled ? FilledStroke : EmptyStroke;
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

    private async void OnResendTapped(object sender, TappedEventArgs e)
    {
        if (_secondsLeft > 0)
            return; 

        CodeEntry.Text = string.Empty;

        //  حط ال API بتاع إرسال الكود 


        StartTimer();
        CodeEntry.Focus();
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        var code = CodeEntry.Text ?? string.Empty;

        if (code.Length != 6)
        {
            await DisplayAlert("Error", "Please enter the 6-digit code.", "OK");
            return;
        }

        // حط ال API بتاع التحقق هنا   

    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}