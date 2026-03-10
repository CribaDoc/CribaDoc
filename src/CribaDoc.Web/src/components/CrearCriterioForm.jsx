import { useState } from "react";
import { api } from "../api/client";

export default function CrearCriterioForm({ busquedaId, onCreado }) {
  const [tipo, setTipo] = useState("1");
  const [texto, setTexto] = useState("");
  const [mensaje, setMensaje] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setMensaje("");

    if (!texto.trim()) {
      setMensaje("El criterio no puede estar vacío.");
      return;
    }

    setLoading(true);

    try {
      await api.crearCriterio(busquedaId, {
        tipo: Number(tipo),
        texto: texto.trim()
      });

      setTexto("");
      setMensaje("Criterio creado.");

      if (onCreado) {
        await onCreado();
      }
    } catch (error) {
      setMensaje(error.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2>Añadir criterio</h2>

      <select value={tipo} onChange={(e) => setTipo(e.target.value)}>
        <option value="1">Inclusión</option>
        <option value="2">Exclusión</option>
      </select>

      <input
        type="text"
        placeholder="Texto del criterio"
        value={texto}
        onChange={(e) => setTexto(e.target.value)}
      />

      <button type="submit" disabled={loading}>
        {loading ? "Guardando..." : "Añadir criterio"}
      </button>

      {mensaje && <p>{mensaje}</p>}
    </form>
  );
}