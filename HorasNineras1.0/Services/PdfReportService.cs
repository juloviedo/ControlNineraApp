using HorasNiñeras1._0.Models;
using Microsoft.Maui.ApplicationModel;

namespace ControlNineras.Services
{
    public class PdfReportService
    {
        // 1. GENERAR INFORME DE HORAS
        public static async Task<string> GenerarInformePDF(Ninera ninera, List<RegistroHorario> registros, int mes, int anio)
        {
            string fileName = $"Informe_{ninera.Nombre}_{anio}_{mes:D2}.html";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            double totalHoras = 0;
            decimal totalAbonar = registros.Sum(r => (decimal)r.HorasTrabajadas * (r.ValorHoraAplicado > 0 ? r.ValorHoraAplicado : ninera.ValorHora));

            var filasTabla = new System.Text.StringBuilder();

            foreach (var reg in registros)
            {
                string fechaStr = reg.Fecha.ToString("dd/MM/yyyy");
                string ingresoStr = "-";
                string egresoStr = "-";
                string horasStr = "-";

                if (!reg.EsSoloComentario && reg.HoraIngreso.HasValue)
                {
                    ingresoStr = reg.HoraIngreso.Value.ToString(@"hh\:mm");
                    egresoStr = reg.HoraEgreso.HasValue ? reg.HoraEgreso.Value.ToString(@"hh\:mm") : "-";
                    horasStr = $"{reg.HorasTrabajadas:F2} hs";
                    totalHoras += reg.HorasTrabajadas;
                }

                filasTabla.AppendLine($@"
                    <tr>
                        <td>{fechaStr}</td>
                        <td>{ingresoStr}</td>
                        <td>{egresoStr}</td>
                        <td>{horasStr}</td>
                        <td>{reg.Comentario ?? ""}</td>
                    </tr>");
            }

            string htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 15px; color: #333; }}
                    h2 {{ color: #1f4e79; margin-bottom: 5px; }}
                    .sub-header {{ font-size: 13px; color: #666; margin-bottom: 15px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 10px; font-size: 13px; }}
                    th {{ background-color: #f2f2f2; text-align: left; padding: 8px; border-bottom: 2px solid #ddd; }}
                    td {{ padding: 8px; border-bottom: 1px solid #eee; }}
                    .totales {{ margin-top: 20px; text-align: right; font-size: 15px; }}
                    .monto {{ font-size: 18px; color: #2e7d32; font-weight: bold; }}
                </style>
            </head>
            <body>
                <h2>Informe de Horarios - {ninera.Nombre}</h2>
                <div class='sub-header'>Período: {mes:D2}/{anio} | Tarifa por hora: ${ninera.ValorHora:N2}</div>
                
                <table>
                    <thead>
                        <tr>
                            <th>Fecha</th>
                            <th>Ingreso</th>
                            <th>Egreso</th>
                            <th>Total Hs</th>
                            <th>Comentario</th>
                        </tr>
                    </thead>
                    <tbody>
                        {filasTabla}
                    </tbody>
                </table>

                <div class='totales'>
                    <p><strong>Total Horas Trabajadas:</strong> {totalHoras:F2} hs</p>
                    <p class='monto'>Total a Abonar: ${totalAbonar:N2}</p>
                </div>
            </body>
            </html>";

            await File.WriteAllTextAsync(filePath, htmlContent);
            return filePath;
        }

        // 2. GENERAR HISTORIAL DE PAGOS
        public static async Task<string> GenerarInformePagosPDF(Ninera ninera, List<Pago> pagos, int anio)
        {
            string fileName = $"Historial_Pagos_{ninera.Nombre}_{anio}.html";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var filasTabla = new System.Text.StringBuilder();

            foreach (var p in pagos)
            {
                string estado = p.EstadoPago ? "<span style='color:green; font-weight:bold;'>PAGADO</span>" : "<span style='color:orange; font-weight:bold;'>PENDIENTE</span>";
                string fechaPago = p.FechaPago.HasValue ? p.FechaPago.Value.ToString("dd/MM/yyyy") : "-";

                filasTabla.AppendLine($@"
                    <tr>
                        <td>{p.PeriodoFormateado}</td>
                        <td>{p.TotalHoras:F2} hs</td>
                        <td>${p.TotalAbonar:N2}</td>
                        <td>{estado}</td>
                        <td>{fechaPago}</td>
                    </tr>");
            }

            decimal totalPagado = pagos.Where(p => p.EstadoPago).Sum(p => p.TotalAbonar);
            decimal totalPendiente = pagos.Where(p => !p.EstadoPago).Sum(p => p.TotalAbonar);

            string htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 15px; color: #333; }}
                    h2 {{ color: #1f4e79; margin-bottom: 5px; }}
                    .sub-header {{ font-size: 13px; color: #666; margin-bottom: 15px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 10px; font-size: 13px; }}
                    th {{ background-color: #f2f2f2; text-align: left; padding: 8px; border-bottom: 2px solid #ddd; }}
                    td {{ padding: 8px; border-bottom: 1px solid #eee; }}
                    .totales {{ margin-top: 20px; text-align: right; font-size: 14px; }}
                </style>
            </head>
            <body>
                <h2>Resumen de Pagos - {ninera.Nombre}</h2>
                <div class='sub-header'>Histórico de Liquidaciones Año {anio}</div>
                
                <table>
                    <thead>
                        <tr>
                            <th>Período</th>
                            <th>Horas</th>
                            <th>Importe</th>
                            <th>Estado</th>
                            <th>Fecha Pago</th>
                        </tr>
                    </thead>
                    <tbody>
                        {filasTabla}
                    </tbody>
                </table>

                <div class='totales'>
                    <p style='color:green;'><strong>Total Abonado:</strong> ${totalPagado:N2}</p>
                    <p style='color:orange;'><strong>Total Pendiente:</strong> ${totalPendiente:N2}</p>
                </div>
            </body>
            </html>";

            await File.WriteAllTextAsync(filePath, htmlContent);
            return filePath;
        }

        // 3. COMPARTIR EL ARCHIVO GENERADO
        public static async Task CompartirArchivoAsync(string rutaArchivo, string titulo)
        {
            if (File.Exists(rutaArchivo))
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = titulo,
                    File = new ShareFile(rutaArchivo)
                });
            }
        }
    }
}