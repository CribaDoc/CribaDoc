import { useMemo, useState } from "react";

export default function DecisionForm({ criterios, onSubmit, loading }) {
  const [tipo, setTipo] = useState(1);
  const [nota, setNota] = useState("");
  const [seleccionados, setSeleccionados] = useState([]);
  const [mensaje, setMensaje] = useState("");

  const criteriosFiltrados = useMemo(() => {
    return criterios.filter((c) => c.tipo === tipo);
  }, [criterios, tipo]);

  function cambiarTipo(nuevoTipo) {
    setTipo(nuevoTipo);
    setSeleccionados([]);
    setMensaje("");
  }

  function toggleCriterio(id) {
    setSeleccionados((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setMensaje("");

    if (criteriosFiltrados.length === 0) {
      setMensaje("No hay criterios disponibles para este tipo de decisión.");
      return;
    }

    if (seleccionados.length === 0) {
      setMensaje("Selecciona al menos un criterio aplicado.");
      return;
    }

    await onSubmit({
      tipo,
      nota,
      criterioIds: seleccionados
    });

    setNota("");
    setSeleccionados([]);
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2>Decisión</h2>

      <div>
        <button type="button" onClick={() => cambiarTipo(1)}>
          Incluir
        </button>

        <button
          type="button"
          onClick={() => cambiarTipo(2)}
          style={{ marginLeft: "0.5rem" }}
        >
          Excluir
        </button>
      </div>

      <div style={{ marginTop: "1rem" }}>
        <h3>
          {tipo === 1 ? "Criterios de inclusión" : "Criterios de exclusión"}
        </h3>

        {!criteriosFiltrados.length ? (
          <p>No hay criterios de este tipo.</p>
        ) : (
          criteriosFiltrados.map((c) => (
            <label
              key={c.id}
              style={{ display: "block", marginBottom: "0.35rem" }}
            >
              <input
                type="checkbox"
                checked={seleccionados.includes(c.id)}
                onChange={() => toggleCriterio(c.id)}
              />{" "}
              {c.texto ?? c.Texto}
            </label>
          ))
        )}
      </div>

      <div style={{ marginTop: "1rem" }}>
        <textarea
          placeholder="Nota"
          value={nota}
          onChange={(e) => setNota(e.target.value)}
          rows={5}
          style={{ width: "100%" }}
        />
      </div>

      <button type="submit" disabled={loading} style={{ marginTop: "1rem" }}>
        {loading
          ? "Guardando..."
          : tipo === 1
            ? "Confirmar inclusión"
            : "Confirmar exclusión"}
      </button>

      {mensaje && <p>{mensaje}</p>}
    </form>
  );
}