using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;
using Npgsql;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public class PaperRepositorio : IPaperRepositorio
    {
        private readonly FabricaConexion _fabricaConexion;

        public PaperRepositorio(FabricaConexion fabricaConexion)
        {
            _fabricaConexion = fabricaConexion;
        }

        public void InsertarVarios(List<PaperEntity> papers)
        {
            if (papers == null || papers.Count == 0)
                return;

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            foreach (var paper in papers)
            {
                using var cmd = new NpgsqlCommand(
                    """
                    INSERT INTO paper
                    (
                        busqueda_id,
                        url,
                        doi,
                        titulos,
                        autores,
                        keywords,
                        resumen,
                        anio_publicacion,
                        ris_bloque,
                        orden_original
                    )
                    VALUES
                    (
                        @busquedaId,
                        @url,
                        @doi,
                        @titulos,
                        @autores,
                        @keywords,
                        @resumen,
                        @anioPublicacion,
                        @risBloque,
                        @ordenOriginal
                    );
                    """,
                    conn);

                cmd.Parameters.AddWithValue("busquedaId", paper.BusquedaId);
                cmd.Parameters.AddWithValue("url", paper.Url);
                cmd.Parameters.AddWithValue("doi", (object?)paper.Doi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("titulos", paper.Titulos);
                cmd.Parameters.AddWithValue("autores", paper.Autores);
                cmd.Parameters.AddWithValue("keywords", paper.Keywords);
                cmd.Parameters.AddWithValue("resumen", (object?)paper.Resumen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("anioPublicacion", (object?)paper.AnioPublicacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("risBloque", paper.RisBloque);
                cmd.Parameters.AddWithValue("ordenOriginal", paper.OrdenOriginal);

                cmd.ExecuteNonQuery();
            }
        }

        public PaperEntity? ObtenerActual(long busquedaId, int ordenOriginal)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT
                    id,
                    busqueda_id,
                    url,
                    doi,
                    titulos,
                    autores,
                    keywords,
                    resumen,
                    anio_publicacion,
                    ris_bloque,
                    orden_original
                FROM paper
                WHERE busqueda_id = @busquedaId
                  AND orden_original = @ordenOriginal;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("ordenOriginal", ordenOriginal);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return LeerPaper(reader);
        }

        public int ContarPorBusqueda(long busquedaId)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT COUNT(*)
                FROM paper
                WHERE busqueda_id = @busquedaId;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            return System.Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<PaperEntity> ListarPorBusqueda(long busquedaId)
        {
            var papers = new List<PaperEntity>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT
                    id,
                    busqueda_id,
                    url,
                    doi,
                    titulos,
                    autores,
                    keywords,
                    resumen,
                    anio_publicacion,
                    ris_bloque,
                    orden_original
                FROM paper
                WHERE busqueda_id = @busquedaId
                ORDER BY orden_original;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                papers.Add(LeerPaper(reader));
            }

            return papers;
        }

        public List<string> ObtenerBloquesRisIncluidosPorBusqueda(long busquedaId)
        {
            var bloques = new List<string>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT p.ris_bloque
                FROM paper p
                INNER JOIN decision d ON d.paper_id = p.id
                WHERE d.busqueda_id = @busquedaId
                  AND d.tipo = 1
                ORDER BY p.orden_original;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                bloques.Add(reader.GetString(0));
            }

            return bloques;
        }

        public List<string> ObtenerBloquesRisIncluidosPorProyecto(long proyectoId)
        {
            var bloques = new List<string>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT p.ris_bloque
                FROM paper p
                INNER JOIN decision d ON d.paper_id = p.id
                INNER JOIN busqueda b ON b.id = p.busqueda_id
                WHERE b.proyecto_id = @proyectoId
                  AND d.tipo = 1
                ORDER BY b.orden, p.orden_original;
                """,
                conn);

            cmd.Parameters.AddWithValue("proyectoId", proyectoId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                bloques.Add(reader.GetString(0));
            }

            return bloques;
        }

        private static PaperEntity LeerPaper(NpgsqlDataReader reader)
        {
            return new PaperEntity
            {
                Id = reader.GetInt64(0),
                BusquedaId = reader.GetInt64(1),
                Url = reader.GetString(2),
                Doi = reader.IsDBNull(3) ? null : reader.GetString(3),
                Titulos = reader.GetFieldValue<string[]>(4),
                Autores = reader.GetFieldValue<string[]>(5),
                Keywords = reader.GetFieldValue<string[]>(6),
                Resumen = reader.IsDBNull(7) ? null : reader.GetString(7),
                AnioPublicacion = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                RisBloque = reader.GetString(9),
                OrdenOriginal = reader.GetInt32(10)
            };
        }
    }
}
