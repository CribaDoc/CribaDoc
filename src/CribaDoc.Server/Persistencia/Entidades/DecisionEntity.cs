namespace CribaDoc.Server.Persistencia.Entidades
{
    public class DecisionEntity
    {
        public long Id { get; set; }

        public long BusquedaId { get; set; }

        public long PaperId { get; set; }

        public int Tipo { get; set; }

        public string? Nota { get; set; }
    }
}
