using System;
using System.Collections.Generic;
using System.Linq;
using CribaDoc.Core.Extractor;
using CribaDoc.Core.ExportRis;
using CribaDoc.Core.Ris;
using CribaDoc.Server.EntradaSalida.Busquedas;
using CribaDoc.Server.Persistencia;
using Npgsql;

namespace CribaDoc.Server.Negocio
{
    public class BusquedaService
    {
        private readonly FabricaConexion _fabricaConexion;
        private readonly IConversorRis _conversorRis;
        private readonly IExportadorRis _exportadorRis;

        public BusquedaService(
            FabricaConexion fabricaConexion,
            IConversorRis conversorRis,
            IExportadorRis exportadorRis)
        {
            _fabricaConexion = fabricaConexion;
            _conversorRis = conversorRis;
            _exportadorRis = exportadorRis;
        }

        public ImportarBusquedaResponse Importar(long proyectoId, string risText)
        {
            if (proyectoId <= 0)
                throw new ArgumentException("El proyecto es obligatorio.");

            if (string.IsNullOrWhiteSpace(risText))
                throw new ArgumentException("El texto RIS no puede estar vacío.");

            var ris = _conversorRis.Parse(risText);
            var papersRis = ris?.Papers ?? new List<RisPaper>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var tx = conn.BeginTransaction();

            if (!ExisteProyecto(conn, tx, proyectoId))
                throw new InvalidOperationException("El proyecto no existe.");

            var orden = ObtenerSiguienteOrden(conn, tx, proyectoId);
            var busquedaId = InsertarBusqueda(conn, tx, proyectoId, orden, risText);

            for (var i = 0; i < papersRis.Count; i++)
            {
                var vista = papersRis[i].ToVista();
                InsertarPaper(conn, tx, busquedaId, vista, papersRis[i].TxtOr, i);
            }

            tx.Commit();

            return new ImportarBusquedaResponse
            {
                BusquedaId = busquedaId
            };
        }

        public PaperActualResponse ObtenerPaperActual(long busquedaId)
        {
            if (busquedaId <= 0)
                throw new ArgumentException("La búsqueda es obligatoria.");

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            var marcador = ObtenerMarcador(conn, null, busquedaId);
            var total = ContarPapers(conn, null, busquedaId);

            if (total == 0 || marcador >= total)
            {
                return new PaperActualResponse
                {
                    Status = "finished",
                    Marcador = marcador,
                    Total = total,
                    Paper = null
                };
            }

            var paper = ObtenerPaperPorOffset(conn, null, busquedaId, marcador);

            if (paper == null)
            {
                return new PaperActualResponse
                {
                    Status = "finished",
                    Marcador = marcador,
                    Total = total,
                    Paper = null
                };
            }

            return new PaperActualResponse
            {
                Status = "ok",
                Marcador = marcador,
                Total = total,
                Paper = paper
            };
        }

        public EstadisticasBusquedaResponse ObtenerEstadisticas(long busquedaId)
        {
            if (busquedaId <= 0)
                throw new ArgumentException("La búsqueda es obligatoria.");

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            return ObtenerEstadisticasInternas(conn, null, busquedaId);
        }

        public string ExportarRisBusqueda(long busquedaId)
        {
            if (busquedaId <= 0)
                throw new ArgumentException("La búsqueda es obligatoria.");

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            var bloques = new List<string>();

            const string sql = @"
                SELECT p.ris_bloque
                FROM paper p
                INNER JOIN decision d
                    ON d.paper_id = p.id
                   AND d.busqueda_id = p.busqueda_id
                WHERE p.busqueda_id = @busquedaId
                  AND d.tipo = 1
                ORDER BY p.orden_original;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bloques.Add(reader.IsDBNull(0) ? string.Empty : reader.GetString(0));
            }

            return _exportadorRis.Exportar(bloques);
        }

        private static bool ExisteProyecto(NpgsqlConnection conn, NpgsqlTransaction? tx, long proyectoId)
        {
            const string sql = @"SELECT 1 FROM proyecto WHERE id = @id LIMIT 1;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("id", proyectoId);

            var result = cmd.ExecuteScalar();
            return result != null;
        }

        private static int ObtenerSiguienteOrden(NpgsqlConnection conn, NpgsqlTransaction tx, long proyectoId)
        {
            const string sql = @"
                SELECT COALESCE(MAX(orden), 0) + 1
                FROM busqueda
                WHERE proyecto_id = @proyectoId;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("proyectoId", proyectoId);

            return Convert.ToInt32(cmd.ExecuteScalar() ?? 1);
        }

        private static long InsertarBusqueda(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            long proyectoId,
            int orden,
            string risText)
        {
            const string sql = @"
                INSERT INTO busqueda (proyecto_id, orden, ris_texto_original, marcador)
                VALUES (@proyectoId, @orden, @risTextoOriginal, 0)
                RETURNING id;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("proyectoId", proyectoId);
            cmd.Parameters.AddWithValue("orden", orden);
            cmd.Parameters.AddWithValue("risTextoOriginal", risText);

            return Convert.ToInt64(cmd.ExecuteScalar()!);
        }

        private static void InsertarPaper(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            long busquedaId,
            PaperVista vista,
            string risBloque,
            int ordenOriginal)
        {
            const string sql = @"
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
                );";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("url", string.IsNullOrWhiteSpace(vista.Url) ? DBNull.Value : vista.Url);
            cmd.Parameters.AddWithValue("doi", string.IsNullOrWhiteSpace(vista.Doi) ? DBNull.Value : vista.Doi);
            cmd.Parameters.AddWithValue("titulos", (vista.Titulos ?? new List<string>()).ToArray());
            cmd.Parameters.AddWithValue("autores", (vista.Autores ?? new List<string>()).ToArray());
            cmd.Parameters.AddWithValue("keywords", (vista.Keywords ?? new List<string>()).ToArray());
            cmd.Parameters.AddWithValue("resumen", string.IsNullOrWhiteSpace(vista.Resumen) ? DBNull.Value : vista.Resumen);
            cmd.Parameters.AddWithValue("anioPublicacion", vista.AnioPublicacion.HasValue ? vista.AnioPublicacion.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("risBloque", risBloque ?? string.Empty);
            cmd.Parameters.AddWithValue("ordenOriginal", ordenOriginal);

            cmd.ExecuteNonQuery();
        }

        private static int ObtenerMarcador(NpgsqlConnection conn, NpgsqlTransaction? tx, long busquedaId)
        {
            const string sql = @"SELECT marcador FROM busqueda WHERE id = @id;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("id", busquedaId);

            var result = cmd.ExecuteScalar();
            if (result == null)
                throw new InvalidOperationException("La búsqueda no existe.");

            return Convert.ToInt32(result);
        }

        private static int ContarPapers(NpgsqlConnection conn, NpgsqlTransaction? tx, long busquedaId)
        {
            const string sql = @"SELECT COUNT(*) FROM paper WHERE busqueda_id = @busquedaId;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        private static PaperVista? ObtenerPaperPorOffset(
            NpgsqlConnection conn,
            NpgsqlTransaction? tx,
            long busquedaId,
            int offset)
        {
            const string sql = @"
                SELECT
                    url,
                    doi,
                    titulos,
                    autores,
                    keywords,
                    resumen,
                    anio_publicacion
                FROM paper
                WHERE busqueda_id = @busquedaId
                ORDER BY orden_original
                OFFSET @offset
                LIMIT 1;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("offset", offset);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            var vista = new PaperVista
            {
                Url = reader.IsDBNull(0) ? null : reader.GetString(0),
                Doi = reader.IsDBNull(1) ? null : reader.GetString(1),
                Titulos = reader.IsDBNull(2)
                    ? new List<string>()
                    : reader.GetFieldValue<string[]>(2).ToList(),
                Autores = reader.IsDBNull(3)
                    ? new List<string>()
                    : reader.GetFieldValue<string[]>(3).ToList(),
                Keywords = reader.IsDBNull(4)
                    ? new List<string>()
                    : reader.GetFieldValue<string[]>(4).ToList(),
                Resumen = reader.IsDBNull(5) ? null : reader.GetString(5),
                AnioPublicacion = reader.IsDBNull(6) ? null : reader.GetInt32(6)
            };

            return vista;
        }

        private static EstadisticasBusquedaResponse ObtenerEstadisticasInternas(
            NpgsqlConnection conn,
            NpgsqlTransaction? tx,
            long busquedaId)
        {
            var total = ContarPapers(conn, tx, busquedaId);
            var result = ContarDecisiones(conn, tx, busquedaId, 1);
            var deleted = ContarDecisiones(conn, tx, busquedaId, 2);
            var marcador = ObtenerMarcador(conn, tx, busquedaId);

            return new EstadisticasBusquedaResponse
            {
                Total = total,
                Result = result,
                Deleted = deleted,
                Marcador = marcador
            };
        }

        private static int ContarDecisiones(
            NpgsqlConnection conn,
            NpgsqlTransaction? tx,
            long busquedaId,
            int tipo)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM decision
                WHERE busqueda_id = @busquedaId
                  AND tipo = @tipo;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("tipo", tipo);

            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }
    }
}