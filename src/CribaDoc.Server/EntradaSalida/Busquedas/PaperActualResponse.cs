using CribaDoc.Core.Extractor;

namespace CribaDoc.Server.EntradaSalida.Busquedas
{
    public class PaperActualResponse
    {
        public string? Status { get; set; }  // ok / finished

        public int Marcador { get; set; }

        public int Total { get; set; }

        public PaperVista? Paper { get; set; }
    }
}
