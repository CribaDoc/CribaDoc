namespace CribaDoc.Server.Persistencia.Entidades
{
    public class UsuarioEntity
    {
        public long Id { get; set; }

        public string NombreUsuario { get; set; } = "";

        public string PasswordHash { get; set; } = "";
    }
}
