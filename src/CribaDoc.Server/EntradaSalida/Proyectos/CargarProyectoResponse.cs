namespace CribaDoc.Server.EntradaSalida.Proyectos
{
    public class CargarProyectoResponse
    {
        public long Id { get; set; }

        public string Nombre { get; set; } = "";

        public string NombreUsuarioPropietario { get; set; } = "";

        public string Token { get; set; } = "";
    }
}
