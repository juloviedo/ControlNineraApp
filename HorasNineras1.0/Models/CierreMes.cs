using SQLite;

namespace HorasNiñeras1._0.Models;

[Table("CierresMes")]
public class CierreMes
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int NineraId { get; set; }

    public int Mes { get; set; }
    public int Anio { get; set; }

    public double TotalHoras { get; set; }
    public decimal ValorHoraAplicado { get; set; } // Tarifa al momento del cierre
    public decimal TotalAbonar { get; set; }

    public DateTime FechaPago { get; set; }
    public bool Abonado { get; set; }
    public string ObservacionPago { get; set; }
}