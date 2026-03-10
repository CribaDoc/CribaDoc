using System.Collections.Generic;

namespace CribaDoc.Server.EntradaSalida.Proyectos
{
    public class ListaMisProyectosResponse
    {
        public List<MiProyectoItem> Proyectos { get; set; } = new();
    }
}
