using ControlNineras.Services;
using HorasNiñeras1._0.Models;


namespace HorasNiñeras1._0;

public partial class PagosPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private readonly Ninera _ninera;

    public PagosPage(Ninera ninera)
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        _ninera = ninera;
        LblTituloNinera.Text = $"Pagos: {_ninera.Nombre}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPagos();
    }

    private async Task CargarPagos()
    {
        var pagos = await _dbService.ObtenerPagosPorNineraAsync(_ninera.Id);
        ContainerPagos.Children.Clear();

        if (pagos == null || pagos.Count == 0)
        {
            ContainerPagos.Children.Add(new Label
            {
                Text = "No hay registros de liquidaciones o pagos para esta niñera.",
                TextColor = Colors.Gray,
                FontAttributes = FontAttributes.Italic,
                Margin = new Thickness(0, 10)
            });
            return;
        }

        foreach (var pago in pagos)
        {
            Color colorEstado = pago.EstadoPago ? Color.FromArgb("#2E7D32") : Color.FromArgb("#F57F17");
            string textoEstado = pago.EstadoPago
                ? $"PAGADO ({pago.FechaPago:dd/MM/yyyy})"
                : "PENDIENTE DE PAGO";

            var card = new Frame
            {
                Padding = new Thickness(12),
                CornerRadius = 8,
                HasShadow = false,
                BorderColor = colorEstado,
                BackgroundColor = Color.FromArgb("#1E1E1E")
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 8
            };

            var infoStack = new VerticalStackLayout();
            infoStack.Children.Add(new Label { Text = $"Período: {pago.PeriodoFormateado}", FontAttributes = FontAttributes.Bold, FontSize = 15, TextColor = Colors.White });
            infoStack.Children.Add(new Label { Text = $"Horas: {pago.TotalHoras:F2} hs | Total: ${pago.TotalAbonar:N2}", FontSize = 13, TextColor = Colors.LightGray });
            infoStack.Children.Add(new Label { Text = textoEstado, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = colorEstado, Margin = new Thickness(0, 4, 0, 0) });

            var btnEstado = new Button
            {
                Text = pago.EstadoPago ? "✏️ Opciones" : "💵 Pagar",
                BackgroundColor = pago.EstadoPago ? Colors.DarkSlateGray : Colors.DarkGreen,
                TextColor = Colors.White,
                FontSize = 12,
                HeightRequest = 36,
                CommandParameter = pago
            };
            btnEstado.Clicked += OnGestionarEstadoClicked;

            var btnBorrar = new Button
            {
                Text = "🗑️",
                BackgroundColor = Colors.DarkRed,
                TextColor = Colors.White,
                FontSize = 12,
                HeightRequest = 36,
                CommandParameter = pago
            };
            btnBorrar.Clicked += OnEliminarPagoClicked;

            Grid.SetColumn(infoStack, 0);
            Grid.SetColumn(btnEstado, 1);
            Grid.SetColumn(btnBorrar, 2);

            grid.Children.Add(infoStack);
            grid.Children.Add(btnEstado);
            grid.Children.Add(btnBorrar);

            card.Content = grid;
            ContainerPagos.Children.Add(card);
        }
    }

    private async void OnGestionarEstadoClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var pago = button?.CommandParameter as Pago;
        if (pago == null) return;

        if (!pago.EstadoPago)
        {
            bool confirmar = await DisplayAlert("Pagar Período", $"¿Marcar el período {pago.PeriodoFormateado} por ${pago.TotalAbonar:N2} como PAGADO HOY?", "Sí, abonado hoy", "Cancelar");
            if (confirmar)
            {
                pago.EstadoPago = true;
                pago.FechaPago = DateTime.Now;
                await _dbService.GuardarPagoAsync(pago);
                await CargarPagos();
            }
        }
        else
        {
            string opcion = await DisplayActionSheet($"Opciones {pago.PeriodoFormateado}", "Cancelar", null, "✏️ Modificar Fecha de Pago", "🔄 Revertir a PENDIENTE");

            if (opcion == "✏️ Modificar Fecha de Pago")
            {
                string fechaStr = await DisplayPromptAsync("Modificar Fecha", "Ingrese fecha (dd/MM/yyyy):", initialValue: pago.FechaPago?.ToString("dd/MM/yyyy"));
                if (DateTime.TryParseExact(fechaStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime nuevaFecha))
                {
                    pago.FechaPago = nuevaFecha;
                    await _dbService.GuardarPagoAsync(pago);
                    await CargarPagos();
                }
            }
            else if (opcion == "🔄 Revertir a PENDIENTE")
            {
                pago.EstadoPago = false;
                pago.FechaPago = null;
                await _dbService.GuardarPagoAsync(pago);
                await CargarPagos();
            }
        }
    }

    private async void OnEliminarPagoClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var pago = button?.CommandParameter as Pago;
        if (pago == null) return;

        bool confirmar = await DisplayAlert("Confirmar", $"¿Desea eliminar la liquidación {pago.PeriodoFormateado}?", "Sí, eliminar", "No");
        if (confirmar)
        {
            await _dbService.EliminarPagoAsync(pago);
            await CargarPagos();
        }
    }

    private async void OnExportarPdfPagosClicked(object sender, EventArgs e)
    {
        string anioStr = await DisplayPromptAsync("Exportar Histórico", "Ingrese el año a consultar:", initialValue: DateTime.Today.Year.ToString(), keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(anioStr) || !int.TryParse(anioStr, out int anio)) return;

        var pagos = await _dbService.ObtenerPagosPorNineraAsync(_ninera.Id);
        var pagosAnio = pagos?.Where(p => p.Anio == anio).OrderBy(p => p.Mes).ToList();

        if (pagosAnio == null || pagosAnio.Count == 0)
        {
            await DisplayAlert("Aviso", $"No hay pagos registrados para el año {anio}.", "OK");
            return;
        }

        // 1. Generar el informe de pagos (HTML) de forma asíncrona
        string rutaPdf = await PdfReportService.GenerarInformePagosPDF(_ninera, pagosAnio, anio);

        // 2. Abrir el menú para compartir o ver el archivo en el celular
        await PdfReportService.CompartirArchivoAsync(rutaPdf, "Historial de Pagos");
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}