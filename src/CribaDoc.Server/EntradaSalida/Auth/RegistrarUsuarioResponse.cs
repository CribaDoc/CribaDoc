namespace CribaDoc.Server.EntradaSalida.Auth
{
    public class RegistrarUsuarioResponse
    {
        public long Id { get; set; }

        public string NombreUsuario { get; set; } = "";

        public string Token { get; set; } = "";
    }
}