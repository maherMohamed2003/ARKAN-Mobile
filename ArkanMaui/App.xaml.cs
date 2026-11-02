using ArkanMaui.Views.Auth;

namespace ArkanMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        return new Window(new RegisterPage());
    }
}