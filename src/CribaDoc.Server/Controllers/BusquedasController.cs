using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using CribaDoc.Server.Auth;
using CribaDoc.Server.Negocio;
using CribaDoc.Server.EntradaSalida.Busquedas;
using CribaDoc.Server.EntradaSalida.Decisiones;
using CribaDoc.Server.Persistencia.Repositorios;

namespace CribaDoc.Server.Controllers
{
    [ApiController]
    [Authorize]
    public class BusquedasController : ControllerBase
    {
        private readonly BusquedaService _busquedaService;
        private readonly CribadoService _cribadoService;
        private readonly IAccesoProyectoRepositorio _accesoProyectoRepositorio;

        public BusquedasController(
            BusquedaService busquedaService,
            CribadoService cribadoService,
            IAccesoProyectoRepositorio accesoProyectoRepositorio)
        {
            _busquedaService = busquedaService;
            _cribadoService = cribadoService;
            _accesoProyectoRepositorio = accesoProyectoRepositorio;
        }

        [HttpPost("projects/{id:long}/busquedas")]
        public ActionResult<ImportarBusquedaResponse> ImportarBusqueda(long id, [FromBody] string risText)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || projectId.Value != id)
                return Forbid();

            var response = _busquedaService.Importar(id, risText);
            return Ok(response);
        }

        [HttpGet("busquedas/{id:long}/current")]
        public ActionResult<PaperActualResponse> ObtenerPaperActual(long id)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || !_accesoProyectoRepositorio.BusquedaPerteneceAProyecto(id, projectId.Value))
                return Forbid();

            var response = _busquedaService.ObtenerPaperActual(id);
            return Ok(response);
        }

        [HttpGet("busquedas/{id:long}/stats")]
        public ActionResult<EstadisticasBusquedaResponse> ObtenerEstadisticas(long id)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || !_accesoProyectoRepositorio.BusquedaPerteneceAProyecto(id, projectId.Value))
                return Forbid();

            var response = _busquedaService.ObtenerEstadisticas(id);
            return Ok(response);
        }

        [HttpPost("busquedas/{id:long}/decision")]
        public ActionResult<CrearDecisionResponse> CrearDecision(long id, [FromBody] CrearDecisionRequest request)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || !_accesoProyectoRepositorio.BusquedaPerteneceAProyecto(id, projectId.Value))
                return Forbid();

            var response = _cribadoService.CrearDecision(id, request);
            return Ok(response);
        }

        [HttpGet("busquedas/{id:long}/export/result.ris")]
        public IActionResult ExportarRisBusqueda(long id)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || !_accesoProyectoRepositorio.BusquedaPerteneceAProyecto(id, projectId.Value))
                return Forbid();

            var contenido = _busquedaService.ExportarRisBusqueda(id);
            var bytes = Encoding.UTF8.GetBytes(contenido);

            return File(
                bytes,
                "application/x-research-info-systems",
                $"busqueda-{id}-result.ris"
            );
        }
    }
}