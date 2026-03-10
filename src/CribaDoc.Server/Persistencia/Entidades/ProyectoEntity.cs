namespace CribaDoc.Server.Persistencia.Entidades
{
    public class ProyectoEntity
    {
        public long Id { get; set; }

        public long UsuarioId { get; set; }

        public string Nombre { get; set; } = "";

        public string PasswordHash { get; set; } = "";
    }
}