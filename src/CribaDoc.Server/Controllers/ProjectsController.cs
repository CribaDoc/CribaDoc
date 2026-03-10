using CribaDoc.Server.Auth;
using CribaDoc.Server.EntradaSalida.Proyectos;
using CribaDoc.Server.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CribaDoc.Server.Controllers
{
    [ApiController]
    [Route("projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly ProyectoService _proyectoService;

        public ProjectsController(ProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }

        [Authorize]
        [HttpPost("load")]
        public ActionResult<CargarProyectoResponse> Load([FromBody] CargarProyectoRequest request)
        {
            if (User.GetScope() != "user")
                return Forbid();

            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var response = _proyectoService.CargarAjeno(userId.Value, request);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("{id:long}/summary")]
        public ActionResult<ResumenProyectoResponse> GetResumen(long id)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || projectId.Value != id)
                return Forbid();

            var response = _proyectoService.ObtenerResumen(id);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("{id:long}/export/result.ris")]
        public IActionResult ExportarRisGlobal(long id)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || projectId.Value != id)
                return Forbid();

            var contenido = _proyectoService.ExportarRisGlobal(id);
            var bytes = System.Text.Encoding.UTF8.GetBytes(contenido);

            return File(
                bytes,
                "application/x-research-info-systems",
                $"proyecto-{id}-result.ris"
            );
        }

        [Authorize]
        [HttpGet("{id:long}/export/executive.xlsx")]
        public IActionResult ExportarExcelEjecutivo(long id)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || projectId.Value != id)
                return Forbid();

            var bytes = _proyectoService.ExportarExcelEjecutivo(id);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"proyecto-{id}-executive.xlsx"
            );
        }
    }
}