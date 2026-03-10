import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import { guardarSesion } from "../session";

export default function AbrirProyectoForm() {
  const [nombre, setNombre] = useState("");
  const [password, setPassword] = useState("");
  const [mensaje, setMensaje] = useState("");
  const [loading, setLoading] = useState(false);

  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setMensaje("");
    setLoading(true);

    try {
      const result = await api.abrirProyecto({
        nombre,
        password
      });

      guardarSesion({
        token: result.token,
        proyectoId: result.id,
        nombre: result.nombre
      });

      setMensaje("Proyecto abierto correctamente");
      navigate(`/proyecto/${result.id}`);
    } catch (error) {
      setMensaje(error.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2>Abrir proyecto</h2>

      <input
        type="text"
        placeholder="Nombre"
        value={nombre}
        onChange={(e) => setNombre(e.target.value)}
        required
      />

      <input
        type="password"
        placeholder="Contraseña"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        required
      />

      <button type="submit" disabled={loading}>
        {loading ? "Abriendo..." : "Abrir"}
      </button>

      {mensaje && <p>{mensaje}</p>}
    </form>
  );
}