namespace CribaDoc.Server.Persistencia.Entidades
{
    public class CriterioEntity
    {
        public long Id { get; set; }

        public long BusquedaId { get; set; }

        public int Tipo { get; set; }

        public string Texto { get; set; } = "";
    }
}