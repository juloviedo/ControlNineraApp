namespace HorasNiñeras1._0;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Animación suave del logo (Fade In + Zoom liviano)
        await Task.WhenAll(
            ImgLogo.FadeTo(1, 1000, Easing.CubicOut),
            ImgLogo.ScaleTo(1.0, 1000, Easing.CubicOut)
        );

        // Breve pausa para mostrar la carga
        await Task.Delay(800);

        // Cambiar la pantalla principal a AppShell
        Application.Current.MainPage = new AppShell();
    }
}