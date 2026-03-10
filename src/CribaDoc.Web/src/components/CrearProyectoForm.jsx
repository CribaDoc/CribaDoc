import { useState } from "react";
import { api } from "../api/client";

export default function CrearProyectoForm() {
  const [nombre, setNombre] = useState("");
  const [password, setPassword] = useState("");
  const [mensaje, setMensaje] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setMensaje("");
    setLoading(true);

    try {
      const result = await api.crearProyecto({
        nombre,
        password
      });

      setMensaje(`Proyecto creado: ${result.nombre}`);
      setNombre("");
      setPassword("");
    } catch (error) {
      setMensaje(error.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2>Crear proyecto</h2>

      <div>
        <input
          type="text"
          placeholder="Nombre"
          value={nombre}
          onChange={(e) => setNombre(e.target.value)}
          required
        />
      </div>

      <div>
        <input
          type="password"
          placeholder="Contraseña"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
      </div>

      <button type="submit" disabled={loading}>
        {loading ? "Creando..." : "Crear"}
      </button>

      {mensaje && <p>{mensaje}</p>}
    </form>
  );
}