namespace CribaDoc.Server.Persistencia.Entidades
{
    public class BusquedaEntity
{
    public long Id { get; set; }

    public long ProyectoId { get; set; }

    public int Orden { get; set; }

    public string RisTextoOriginal { get; set; } = "";

    public int Marcador { get; set; }
}
}