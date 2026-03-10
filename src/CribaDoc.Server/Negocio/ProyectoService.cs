using CribaDoc.Core.ExporExcel;
using CribaDoc.Core.ExportRis;
using CribaDoc.Server.Auth;
using CribaDoc.Server.EntradaSalida.Busquedas;
using CribaDoc.Server.EntradaSalida.Proyectos;
using CribaDoc.Server.Persistencia.Repositorios;
using System.Collections.Generic;
using System.Linq;

namespace CribaDoc.Server.Negocio
{
    public class ProyectoService
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IProyectoRepositorio _proyectoRepositorio;
        private readonly IBusquedaRepositorio _busquedaRepositorio;
        private readonly IPaperRepositorio _paperRepositorio;
        private readonly IDecisionRepositorio _decisionRepositorio;
        private readonly IDecisionCriterioRepositorio _decisionCriterioRepositorio;
        private readonly ICriterioRepositorio _criterioRepositorio;
        private readonly PasswordHasher _passwordHasher;
        private readonly AppTokenService _tokenService;
        private readonly IExportadorRis _exportadorRis;
        private readonly IExportadorExcel _exportadorExcel;

        public ProyectoService(
            IUsuarioRepositorio usuarioRepositorio,
            IProyectoRepositorio proyectoRepositorio,
            IBusquedaRepositorio busquedaRepositorio,
            IPaperRepositorio paperRepositorio,
            IDecisionRepositorio decisionRepositorio,
            IDecisionCriterioRepositorio decisionCriterioRepositorio,
            ICriterioRepositorio criterioRepositorio,
            PasswordHasher passwordHasher,
            AppTokenService tokenService,
            IExportadorRis exportadorRis,
            IExportadorExcel exportadorExcel)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _proyectoRepositorio = proyectoRepositorio;
            _busquedaRepositorio = busquedaRepositorio;
            _paperRepositorio = paperRepositorio;
            _decisionRepositorio = decisionRepositorio;
            _decisionCriterioRepositorio = decisionCriterioRepositorio;
            _criterioRepositorio = criterioRepositorio;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _exportadorRis = exportadorRis;
            _exportadorExcel = exportadorExcel;
        }

        public CrearProyectoResponse CrearParaUsuario(long usuarioId, CrearProyectoRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del proyecto es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("La contraseña del proyecto es obligatoria.");

            var existente = _proyectoRepositorio.ObtenerPorUsuarioYNombre(usuarioId, request.Nombre);
            if (existente != null)
                throw new InvalidOperationException("Ya existe un proyecto con ese nombre para este usuario.");

            var passwordHash = _passwordHasher.Hash(request.Password);
            var id = _proyectoRepositorio.Insertar(usuarioId, request.Nombre, passwordHash);

            return new CrearProyectoResponse
            {
                Id = id,
                Nombre = request.Nombre
            };
        }

        public ListaMisProyectosResponse ListarPorUsuario(long usuarioId)
        {
            var proyectos = _proyectoRepositorio.ListarPorUsuario(usuarioId);

            return new ListaMisProyectosResponse
            {
                Proyectos = proyectos
                    .Select(p => new MiProyectoItem
                    {
                        Id = p.Id,
                        Nombre = p.Nombre
                    })
                    .ToList()
            };
        }

        public AbrirProyectoResponse AbrirPropio(long usuarioId, long proyectoId)
        {
            var proyecto = _proyectoRepositorio.ObtenerPorId(proyectoId);
            if (proyecto == null)
                throw new InvalidOperationException("El proyecto no existe.");

            if (proyecto.UsuarioId != usuarioId)
                throw new InvalidOperationException("Ese proyecto no pertenece al usuario actual.");

            var token = _tokenService.CreateProjectToken(usuarioId, proyecto.Id);

            return new AbrirProyectoResponse
            {
                Id = proyecto.Id,
                Nombre = proyecto.Nombre,
                Token = token
            };
        }

        public CargarProyectoResponse CargarAjeno(long usuarioActualId, CargarProyectoRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.NombreUsuarioPropietario))
                throw new ArgumentException("El usuario propietario es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.NombreProyecto))
                throw new ArgumentException("El nombre del proyecto es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.PasswordProyecto))
                throw new ArgumentException("La contraseña del proyecto es obligatoria.");

            var usuarioPropietario = _usuarioRepositorio.ObtenerPorNombreUsuario(request.NombreUsuarioPropietario);
            if (usuarioPropietario == null)
                throw new InvalidOperationException("El usuario propietario no existe.");

            var proyecto = _proyectoRepositorio.ObtenerPorUsuarioYNombre(usuarioPropietario.Id, request.NombreProyecto);
            if (proyecto == null)
                throw new InvalidOperationException("El proyecto no existe.");

            var ok = _passwordHasher.Verify(request.PasswordProyecto, proyecto.PasswordHash);
            if (!ok)
                throw new InvalidOperationException("La contraseña del proyecto es incorrecta.");

            var token = _tokenService.CreateProjectToken(usuarioActualId, proyecto.Id);

            return new CargarProyectoResponse
            {
                Id = proyecto.Id,
                Nombre = proyecto.Nombre,
                NombreUsuarioPropietario = usuarioPropietario.NombreUsuario,
                Token = token
            };
        }

        public ResumenProyectoResponse ObtenerResumen(long proyectoId)
        {
            var proyecto = _proyectoRepositorio.ObtenerPorId(proyectoId);
            if (proyecto == null)
                throw new InvalidOperationException("El proyecto no existe.");

            var busquedas = _busquedaRepositorio.ListarPorProyecto(proyectoId);

            var response = new ResumenProyectoResponse
            {
                Nombre = proyecto.Nombre
            };

            foreach (var busqueda in busquedas)
            {
                response.Busquedas.Add(new BusquedaResumenItem
                {
                    BusquedaId = busqueda.Id,
                    Orden = busqueda.Orden,
                    Total = _paperRepositorio.ContarPorBusqueda(busqueda.Id),
                    Result = _decisionRepositorio.ContarPorBusquedaYTipo(busqueda.Id, 1),
                    Deleted = _decisionRepositorio.ContarPorBusquedaYTipo(busqueda.Id, 2),
                    Marcador = busqueda.Marcador
                });
            }

            return response;
        }

        public string ExportarRisGlobal(long proyectoId)
        {
            var proyecto = _proyectoRepositorio.ObtenerPorId(proyectoId);
            if (proyecto == null)
                throw new InvalidOperationException("El proyecto no existe.");

            var bloques = _paperRepositorio.ObtenerBloquesRisIncluidosPorProyecto(proyectoId);
            return _exportadorRis.Exportar(bloques);
        }

        public byte[] ExportarExcelEjecutivo(long proyectoId)
        {
            var proyecto = _proyectoRepositorio.ObtenerPorId(proyectoId);
            if (proyecto == null)
                throw new InvalidOperationException("El proyecto no existe.");

            var busquedas = _busquedaRepositorio.ListarPorProyecto(proyectoId);
            var hojas = new Dictionary<string, List<FilaExcel>>();

            foreach (var busqueda in busquedas)
            {
                var nombreHoja = $"Busqueda {busqueda.Orden}";
                var filas = new List<FilaExcel>();

                var papers = _paperRepositorio.ListarPorBusqueda(busqueda.Id);
                var decisiones = _decisionRepositorio.ListarPorBusqueda(busqueda.Id);
                var criteriosBusqueda = _criterioRepositorio.ListarPorBusqueda(busqueda.Id);

                foreach (var decision in decisiones)
                {
                    var paper = papers.FirstOrDefault(p => p.Id == decision.PaperId);
                    if (paper == null)
                        continue;

                    var relaciones = _decisionCriterioRepositorio.ListarPorDecision(decision.Id);
                    var criteriosAplicados = relaciones
                        .Select(r => criteriosBusqueda.FirstOrDefault(c => c.Id == r.CriterioId)?.Texto)
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Cast<string>()
                        .ToList();

                    filas.Add(new FilaExcel
                    {
                        BusquedaOrden = busqueda.Orden,
                        BusquedaNombre = nombreHoja,
                        DecisionTipo = decision.Tipo == 1 ? "Result" : "Deleted",
                        Nota = decision.Nota,
                        CriteriosAplicados = criteriosAplicados,
                        Titulo = paper.Titulos.FirstOrDefault(),
                        Anio = paper.AnioPublicacion,
                        Doi = paper.Doi,
                        Url = paper.Url
                    });
                }

                hojas[nombreHoja] = filas;
            }

            return _exportadorExcel.Exportar(hojas);
        }
    }
}