import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { api } from "../api/client";
import { obtenerProyectoId } from "../session";
import CrearCriterioForm from "../components/CrearCriterioForm";
import ListaCriterios from "../components/ListaCriterios";

function normalizarCriterios(data) {
  return {
    inclusion: Array.isArray(data?.inclusion ?? data?.Inclusion)
      ? (data.inclusion ?? data.Inclusion)
      : [],
    exclusion: Array.isArray(data?.exclusion ?? data?.Exclusion)
      ? (data.exclusion ?? data.Exclusion)
      : []
  };
}

export default function ConfigurarBusquedaPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [criterios, setCriterios] = useState({
    inclusion: [],
    exclusion: []
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const cargarCriterios = useCallback(async () => {
    setError("");

    try {
      const data = await api.obtenerCriterios(id);
      setCriterios(normalizarCriterios(data));
    } catch (err) {
      setError(err.message);
    }
  }, [id]);

  useEffect(() => {
    async function init() {
      setLoading(true);
      await cargarCriterios();
      setLoading(false);
    }

    init();
  }, [cargarCriterios]);

  const totalCriterios = useMemo(() => {
    return criterios.inclusion.length + criterios.exclusion.length;
  }, [criterios]);

  function volverAlProyecto() {
    const proyectoId = obtenerProyectoId();

    if (proyectoId) {
      navigate(`/proyecto/${proyectoId}`);
      return;
    }

    navigate(-1);
  }

  function empezarCribado() {
    if (totalCriterios === 0) {
      return;
    }

    navigate(`/busqueda/${id}`);
  }

  if (loading) {
    return <h1>Cargando configuración de búsqueda...</h1>;
  }

  return (
    <div>
      <h1>Configurar búsqueda {id}</h1>

      {error && <p>{error}</p>}

      <CrearCriterioForm busquedaId={id} onCreado={cargarCriterios} />

      <ListaCriterios criterios={criterios} />

      {totalCriterios === 0 && (
        <p>Debes añadir al menos un criterio antes de empezar el cribado.</p>
      )}

      <div style={{ marginTop: "1rem" }}>
        <button onClick={volverAlProyecto}>Terminar y volver al proyecto</button>

        <button
          onClick={empezarCribado}
          disabled={totalCriterios === 0}
          style={{ marginLeft: "0.75rem" }}
        >
          Empezar cribado
        </button>
      </div>
    </div>
  );
}