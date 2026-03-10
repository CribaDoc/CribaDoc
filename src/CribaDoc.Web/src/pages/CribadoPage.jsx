import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { api } from "../api/client";
import { obtenerProyectoId } from "../session";
import StatsBar from "../components/StatsBar";
import PaperViewer from "../components/PaperViewer";
import ListaCriterios from "../components/ListaCriterios";
import DecisionForm from "../components/DecisionForm";

function normalizarCriterios(data) {
  const inclusion =
    data?.inclusion ??
    data?.Inclusion ??
    [];

  const exclusion =
    data?.exclusion ??
    data?.Exclusion ??
    [];

  const inclusionArray = Array.isArray(inclusion) ? inclusion : [];
  const exclusionArray = Array.isArray(exclusion) ? exclusion : [];

  return {
    inclusion: inclusionArray,
    exclusion: exclusionArray,
    plano: [
      ...inclusionArray.map((c) => ({
        id: c.id ?? c.Id,
        tipo: 1,
        texto: c.texto ?? c.Texto
      })),
      ...exclusionArray.map((c) => ({
        id: c.id ?? c.Id,
        tipo: 2,
        texto: c.texto ?? c.Texto
      }))
    ]
  };
}

function normalizarStats(data) {
  const total = data?.total ?? data?.Total ?? 0;
  const result = data?.result ?? data?.Result ?? 0;
  const deleted = data?.deleted ?? data?.Deleted ?? 0;
  const marcador = data?.marcador ?? data?.Marcador ?? 0;

  return {
    total,
    result,
    deleted,
    marcador,
    remaining: Math.max(0, total - result - deleted)
  };
}

function normalizarPaperActual(data) {
  if (!data) {
    return {
      status: "finished",
      marcador: 0,
      total: 0,
      paper: null
    };
  }

  const status = data?.status ?? data?.Status ?? "finished";
  const marcador = data?.marcador ?? data?.Marcador ?? 0;
  const total = data?.total ?? data?.Total ?? 0;
  const src = data?.paper ?? data?.Paper ?? null;

  if (status === "finished" || !src) {
    return {
      status,
      marcador,
      total,
      paper: null
    };
  }

  const titulos = src?.titulos ?? src?.Titulos ?? [];
  const autores = src?.autores ?? src?.Autores ?? [];
  const keywords = src?.keywords ?? src?.Keywords ?? [];

  return {
    status,
    marcador,
    total,
    paper: {
      titulo: Array.isArray(titulos) ? (titulos[0] ?? "") : "",
      autores: Array.isArray(autores) ? autores : [],
      keywords: Array.isArray(keywords) ? keywords : [],
      resumen: src?.resumen ?? src?.Resumen ?? "",
      url: src?.url ?? src?.Url ?? "",
      doi: src?.doi ?? src?.Doi ?? "",
      anioPublicacion:
        src?.anioPublicacion ??
        src?.AnioPublicacion ??
        null
    }
  };
}

export default function CribadoPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [paperActual, setPaperActual] = useState({
    status: "finished",
    marcador: 0,
    total: 0,
    paper: null
  });
  const [stats, setStats] = useState(null);
  const [criterios, setCriterios] = useState({
    inclusion: [],
    exclusion: [],
    plano: []
  });
  const [loading, setLoading] = useState(true);
  const [decisionLoading, setDecisionLoading] = useState(false);
  const [error, setError] = useState("");
  const [mensaje, setMensaje] = useState("");

  const cargarTodo = useCallback(async () => {
    setError("");

    try {
      const [paperResponse, statsResponse, criteriosResponse] = await Promise.all([
        api.obtenerPaperActual(id),
        api.obtenerEstadisticasBusqueda(id),
        api.obtenerCriterios(id)
      ]);

      const criteriosNorm = normalizarCriterios(criteriosResponse);
      const totalCriterios =
        criteriosNorm.inclusion.length + criteriosNorm.exclusion.length;

      if (totalCriterios === 0) {
        navigate(`/busqueda/${id}/configurar`);
        return;
      }

      setCriterios(criteriosNorm);
      setPaperActual(normalizarPaperActual(paperResponse));
      setStats(normalizarStats(statsResponse));
    } catch (err) {
      setError(err.message);
    }
  }, [id, navigate]);

  useEffect(() => {
    async function init() {
      setLoading(true);
      await cargarTodo();
      setLoading(false);
    }

    init();
  }, [cargarTodo]);

  async function handleCrearDecision(data) {
    setMensaje("");
    setError("");
    setDecisionLoading(true);

    try {
      const response = await api.crearDecision(id, data);

      if (response?.stats ?? response?.Stats) {
        setStats(normalizarStats(response.stats ?? response.Stats));
      }

      const nuevoPaperResponse = await api.obtenerPaperActual(id);
      setPaperActual(normalizarPaperActual(nuevoPaperResponse));

      setMensaje("Decisión guardada.");
    } catch (err) {
      setError(err.message);
    } finally {
      setDecisionLoading(false);
    }
  }

  function volverAlProyecto() {
    const proyectoId = obtenerProyectoId();

    if (proyectoId) {
      navigate(`/proyecto/${proyectoId}`);
      return;
    }

    navigate(-1);
  }

  async function handleExportarRis() {
    try {
      setError("");
      await api.descargarRisBusqueda(id);
    } catch (err) {
      setError(err.message);
    }
  }

  if (loading) {
    return <h1>Cargando búsqueda...</h1>;
  }

  const indiceVisible =
    paperActual.paper && paperActual.status === "ok"
      ? paperActual.marcador + 1
      : paperActual.marcador;

  return (
    <div>
      <button onClick={volverAlProyecto}>Volver al proyecto</button>

      <h1>Búsqueda {id}</h1>

      {error && <p>{error}</p>}
      {mensaje && <p>{mensaje}</p>}

      <div style={{ marginBottom: "1rem" }}>
        <button onClick={handleExportarRis}>Exportar RIS de esta búsqueda</button>
      </div>

      <StatsBar stats={stats} />

      <div style={{ marginBottom: "1rem" }}>
        <p>
          Paper actual: {indiceVisible} / {paperActual.total}
        </p>
        <p>Estado: {paperActual.status}</p>
      </div>

      <ListaCriterios criterios={criterios} />

      {paperActual.status !== "finished" && !paperActual.paper && (
        <p>No se ha podido cargar el paper actual. Revisa el endpoint current.</p>
      )}

      <PaperViewer paper={paperActual.paper} />

      {paperActual.paper ? (
        <DecisionForm
          criterios={criterios.plano}
          onSubmit={handleCrearDecision}
          loading={decisionLoading}
        />
      ) : paperActual.status === "finished" ? (
        <p>Cribado completado para esta búsqueda.</p>
      ) : null}
    </div>
  );
}