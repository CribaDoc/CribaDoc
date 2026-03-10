import { useState } from "react";
import { api } from "../api/client";

export default function SubirBusquedaForm({ proyectoId, onImportada }) {
  const [archivo, setArchivo] = useState(null);
  const [mensaje, setMensaje] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setMensaje("");

    if (!archivo) {
      setMensaje("Selecciona un archivo RIS.");
      return;
    }

    setLoading(true);

    try {
      const risText = await archivo.text();
      const response = await api.importarBusqueda(proyectoId, risText);

      setMensaje("Búsqueda importada correctamente.");
      setArchivo(null);
      e.target.reset();

      if (onImportada) {
        await onImportada(response);
      }
    } catch (error) {
      setMensaje(error.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2>Añadir búsqueda</h2>

      <input
        type="file"
        accept=".ris,text/plain"
        onChange={(e) => setArchivo(e.target.files?.[0] ?? null)}
      />

      <button type="submit" disabled={loading}>
        {loading ? "Importando..." : "Importar RIS"}
      </button>

      {mensaje && <p>{mensaje}</p>}
    </form>
  );
}