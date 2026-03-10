using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CribaDoc.Server.Persistencia
{
    public class FabricaConexion
    {
        private readonly string _connectionString;

        public FabricaConexion(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public NpgsqlConnection Crear()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
