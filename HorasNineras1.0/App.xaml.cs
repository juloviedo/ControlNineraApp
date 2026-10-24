using HorasNiñeras1._0;

namespace HorasNineras1._0;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        int temaGuardado = Preferences.Get(ConfiguracionPage.KEY_TEMA, 0);
        ConfiguracionPage.AplicarTema(temaGuardado);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new SplashPage());
    }
}