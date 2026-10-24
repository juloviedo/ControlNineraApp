using Plugin.LocalNotification;

using Plugin.LocalNotification.Core.Models;

namespace HorasNiñeras1._0;

public partial class ConfiguracionPage : ContentPage
{
    public const string KEY_TEMA = "pref_tema_app";
    public const string KEY_ALERTA_ACTIVA = "pref_alerta_activa";
    public const string KEY_ALERTA_HORA = "pref_alerta_hora";
    public const string KEY_ALERTA_DIAS = "pref_alerta_dias";

    private bool _cargando = true;

    public ConfiguracionPage()
    {
        InitializeComponent();
        CargarPreferencias();
        _cargando = false;
    }

    private void CargarPreferencias()
    {
        int temaGuardado = Preferences.Get(KEY_TEMA, 0);
        PickerTema.SelectedIndex = temaGuardado;

        bool alertaActiva = Preferences.Get(KEY_ALERTA_ACTIVA, false);
        SwitchAlerta.IsToggled = alertaActiva;
        PanelAlertaDetalle.IsVisible = alertaActiva;

        string horaStr = Preferences.Get(KEY_ALERTA_HORA, "20:00");
        if (TimeSpan.TryParse(horaStr, out TimeSpan horaGuardada))
        {
            TpHoraAlerta.Time = horaGuardada;
        }

        string diasGuardados = Preferences.Get(KEY_ALERTA_DIAS, "1,2,3,4,5");
        var listaDias = diasGuardados.Split(',').ToList();

        ChkLunes.IsChecked = listaDias.Contains("1");
        ChkMartes.IsChecked = listaDias.Contains("2");
        ChkMiercoles.IsChecked = listaDias.Contains("3");
        ChkJueves.IsChecked = listaDias.Contains("4");
        ChkViernes.IsChecked = listaDias.Contains("5");
        ChkSabado.IsChecked = listaDias.Contains("6");
        ChkDomingo.IsChecked = listaDias.Contains("0");
    }

    private void OnTemaChanged(object sender, EventArgs e)
    {
        int seleccion = PickerTema.SelectedIndex;
        Preferences.Set(KEY_TEMA, seleccion);
        AplicarTema(seleccion);
    }

    public static void AplicarTema(int seleccion)
    {
        switch (seleccion)
        {
            case 1: Application.Current.UserAppTheme = AppTheme.Light; break;
            case 2: Application.Current.UserAppTheme = AppTheme.Dark; break;
            default: Application.Current.UserAppTheme = AppTheme.Unspecified; break;
        }
    }

    private async void OnAlertaToggled(object sender, ToggledEventArgs e)
    {
        PanelAlertaDetalle.IsVisible = e.Value;
        Preferences.Set(KEY_ALERTA_ACTIVA, e.Value);

        if (!_cargando)
        {
            if (e.Value)
            {
                if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
                {
                    await LocalNotificationCenter.Current.RequestNotificationPermission();
                }
            }
            ActualizarProgramacionNotificacion();
        }
    }

    private void OnHoraAlertaChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time) && !_cargando)
        {
            TimeSpan horaSeleccionada = TpHoraAlerta.Time ?? new TimeSpan(20, 0, 0);
            Preferences.Set(KEY_ALERTA_HORA, horaSeleccionada.ToString(@"hh\:mm"));
            ActualizarProgramacionNotificacion();
        }
    }

    private void OnDiaCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_cargando) return;

        List<string> dias = new();
        if (ChkLunes.IsChecked) dias.Add("1");
        if (ChkMartes.IsChecked) dias.Add("2");
        if (ChkMiercoles.IsChecked) dias.Add("3");
        if (ChkJueves.IsChecked) dias.Add("4");
        if (ChkViernes.IsChecked) dias.Add("5");
        if (ChkSabado.IsChecked) dias.Add("6");
        if (ChkDomingo.IsChecked) dias.Add("0");

        Preferences.Set(KEY_ALERTA_DIAS, string.Join(",", dias));
        ActualizarProgramacionNotificacion();
    }

    private void ActualizarProgramacionNotificacion()
    {
        LocalNotificationCenter.Current.CancelAll();

        bool activa = Preferences.Get(KEY_ALERTA_ACTIVA, false);
        if (!activa) return;

        // Asignar valor predeterminado si Time es null
        TimeSpan hora = TpHoraAlerta.Time ?? new TimeSpan(20, 0, 0);
        DateTime fechaProxima = DateTime.Today.Add(hora);

        if (fechaProxima < DateTime.Now)
        {
            fechaProxima = fechaProxima.AddDays(1);
        }

        var request = new NotificationRequest
        {
            NotificationId = 1001,
            Title = "Recordatorio Horas Niñeras 👶",
            Description = "¿Registraste las horas de hoy? Presiona para ingresar.",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = fechaProxima,
                RepeatType = NotificationRepeat.Daily
            }
        };

        LocalNotificationCenter.Current.Show(request);
    }
}