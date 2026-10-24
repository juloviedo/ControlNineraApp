using SQLite;

namespace HorasNiñeras1._0.Models;

[Table("RegistrosHorarios")]
public class RegistroHorario
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int NineraId { get; set; }

    public DateTime Fecha { get; set; }

    public TimeSpan? HoraIngreso { get; set; }

    public TimeSpan? HoraEgreso { get; set; }

    public bool EsSoloComentario { get; set; }

    public string Comentario { get; set; }

    // --- NUEVO CAMPO HISTÓRICO DE TARIFA ---
    public decimal ValorHoraAplicado { get; set; }

    [Ignore]
    public double HorasTrabajadas
    {
        get
        {
            if (HoraIngreso.HasValue && HoraEgreso.HasValue)
            {
                var diferencia = HoraEgreso.Value - HoraIngreso.Value;
                if (diferencia.TotalMinutes < 0)
                {
                    diferencia = diferencia.Add(TimeSpan.FromHours(24));
                }
                return diferencia.TotalHours;
            }
            return 0;
        }
    }
}