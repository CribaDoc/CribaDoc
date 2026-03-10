namespace CribaDoc.Server.Persistencia.Entidades
{
    public class PaperEntity
    {
        public long Id { get; set; }

        public long BusquedaId { get; set; }

        public string Url { get; set; } = "";

        public string? Doi { get; set; }

        public string[] Titulos { get; set; } = [];

        public string[] Autores { get; set; } = [];

        public string[] Keywords { get; set; } = [];

        public string? Resumen { get; set; }

        public int? AnioPublicacion { get; set; }

        public string RisBloque { get; set; } = "";

        public int OrdenOriginal { get; set; }
    }
}