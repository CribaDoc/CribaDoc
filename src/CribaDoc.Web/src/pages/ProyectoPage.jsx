import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { api } from "../api/client";
import { limpiarSesion, obtenerProyectoId } from "../session";
import SubirBusquedaForm from "../components/SubirBusquedaForm";
import BusquedaTable from "../components/BusquedaTable";

function extraerBusquedaIdImportada(response) {
  return (
    response?.busquedaId ??
    response?.id ??
    response?.BusquedaId ??
    response?.Id ??
    null
  );
}

function normalizarCriterios(data) {
  const inclusion =
    data?.inclusion ??
    data?.Inclusion ??
    [];

  const exclusion =
    data?.exclusion ??
    data?.Exclusion ??
    [];

  return {
    inclusion: Array.isArray(inclusion) ? inclusion : [],
    exclusion: Array.isArray(exclusion) ? exclusion : []
  };
}

export default function ProyectoPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [resumen, setResumen] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [descargandoRis, setDescargandoRis] = useState(false);
  const [descargandoExcel, setDescargandoExcel] = useState(false);

  const cargarResumen = useCallback(async () => {
    setError("");

    try {
      const proyectoIdGuardado = obtenerProyectoId();

      if (!proyectoIdGuardado) {
        throw new Error("No hay sesión de proyecto.");
      }

      if (String(proyectoIdGuardado) !== String(id)) {
        throw new Error("La sesión no corresponde con este proyecto.");
      }

      const data = await api.obtenerResumenProyecto(id);
      setResumen(data);
    } catch (err) {
      setError(err.message);
    }
  }, [id]);

  useEffect(() => {
    async function init() {
      setLoading(true);
      await cargarResumen();
      setLoading(false);
    }

    init();
  }, [cargarResumen]);

  function cerrarProyecto() {
    limpiarSesion();
    navigate("/");
  }

  async function handleBusquedaImportada(response) {
    await cargarResumen();

    const nuevaBusquedaId = extraerBusquedaIdImportada(response);

    if (nuevaBusquedaId) {
      navigate(`/busqueda/${nuevaBusquedaId}/configurar`);
    }
  }

  async function handleDescargarRisGlobal() {
    try {
      setDescargandoRis(true);
      setError("");
      await api.descargarRisGlobal(id);
    } catch (err) {
      setError(err.message);
    } finally {
      setDescargandoRis(false);
    }
  }

  async function handleDescargarExcel() {
    try {
      setDescargandoExcel(true);
      setError("");
      await api.descargarExcelProyecto(id);
    } catch (err) {
      setError(err.message);
    } finally {
      setDescargandoExcel(false);
    }
  }

  async function handleExportarRisBusqueda(busquedaId) {
    try {
      setError("");
      await api.descargarRisBusqueda(busquedaId);
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleEntrarBusqueda(busquedaId) {
    try {
      setError("");

      const criteriosResponse = await api.obtenerCriterios(busquedaId);
      const criterios = normalizarCriterios(criteriosResponse);
      const totalCriterios =
        criterios.inclusion.length + criterios.exclusion.length;

      if (totalCriterios === 0) {
        navigate(`/busqueda/${busquedaId}/configurar`);
        return;
      }

      navigate(`/busqueda/${busquedaId}`);
    } catch (err) {
      setError(err.message);
    }
  }

  if (loading) {
    return <h1>Cargando proyecto...</h1>;
  }

  if (error && !resumen) {
    return (
      <div>
        <h1>Error</h1>
        <p>{error}</p>
        <button onClick={() => navigate("/")}>Volver</button>
      </div>
    );
  }

  return (
    <div>
      <h1>{resumen?.nombre ?? "Proyecto"}</h1>

      <button onClick={cerrarProyecto}>Cerrar proyecto</button>

      <SubirBusquedaForm proyectoId={id} onImportada={handleBusquedaImportada} />

      <div style={{ marginTop: "1rem", marginBottom: "1.5rem" }}>
        <button onClick={handleDescargarRisGlobal} disabled={descargandoRis}>
          {descargandoRis ? "Descargando RIS..." : "Exportar RIS global"}
        </button>

        <button
          onClick={handleDescargarExcel}
          disabled={descargandoExcel}
          style={{ marginLeft: "0.75rem" }}
        >
          {descargandoExcel ? "Descargando Excel..." : "Exportar Excel"}
        </button>
      </div>

      {error && <p>{error}</p>}

      <h2>Búsquedas</h2>

      <BusquedaTable
        busquedas={resumen?.busquedas ?? []}
        onEntrar={handleEntrarBusqueda}
        onExportarRis={handleExportarRisBusqueda}
      />
    </div>
  );
}