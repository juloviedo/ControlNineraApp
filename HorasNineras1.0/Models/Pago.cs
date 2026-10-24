using SQLite;

namespace HorasNiñeras1._0.Models;

[Table("Pagos")]
public class Pago
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int NineraId { get; set; }

    public int Mes { get; set; }
    public int Anio { get; set; }

    public double TotalHoras { get; set; }
    public decimal TotalAbonar { get; set; }

    public bool EstadoPago { get; set; } // false = Pendiente, true = Pagado
    public DateTime? FechaPago { get; set; } // Fecha en la que se efectúa el pago
    // Propiedad calculada para mostrar en pantalla
    [Ignore]
    public string PeriodoFormateado => $"{Mes:D2}/{Anio}";
}