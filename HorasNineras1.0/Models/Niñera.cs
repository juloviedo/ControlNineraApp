using SQLite;

namespace HorasNiñeras1._0.Models;

[Table("Nineras")]
public class Ninera
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Nombre { get; set; }

    // Valor actual de la hora para esta niñera
    public decimal ValorHora { get; set; }

    public bool Activa { get; set; } = true;
}
