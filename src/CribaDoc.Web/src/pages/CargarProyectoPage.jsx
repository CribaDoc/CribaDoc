import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import { guardarSesionProyecto } from "../session";

export default function CargarProyectoPage() {
  const navigate = useNavigate();

  const [nombreUsuarioPropietario, setNombreUsuarioPropietario] = useState("");
  const [nombreProyecto, setNombreProyecto] = useState("");
  const [passwordProyecto, setPasswordProyecto] = useState("");
  const [mensaje, setMensaje] = useState("");

  async function handleSubmit(e) {
    e.preventDefault();
    setMensaje("");

    try {
      const response = await api.cargarProyecto({
        nombreUsuarioPropietario,
        nombreProyecto,
        passwordProyecto
      });

      guardarSesionProyecto({
        token: response.token,
        proyectoId: response.id,
        nombre: response.nombre
      });

      navigate(`/proyecto/${response.id}`);
    } catch (error) {
      setMensaje(error.message);
    }
  }

  return (
    <div>
      <h1>Cargar proyecto ajeno</h1>

      <button onClick={() => navigate("/dashboard")}>Volver</button>

      <form onSubmit={handleSubmit} style={{ marginTop: "1rem" }}>
        <input
          type="text"
          placeholder="Usuario propietario"
          value={nombreUsuarioPropietario}
          onChange={(e) => setNombreUsuarioPropietario(e.target.value)}
          required
        />

        <input
          type="text"
          placeholder="Nombre del proyecto"
          value={nombreProyecto}
          onChange={(e) => setNombreProyecto(e.target.value)}
          required
          style={{ marginLeft: "0.5rem" }}
        />

        <input
          type="password"
          placeholder="Contraseña del proyecto"
          value={passwordProyecto}
          onChange={(e) => setPasswordProyecto(e.target.value)}
          required
          style={{ marginLeft: "0.5rem" }}
        />

        <button type="submit" style={{ marginLeft: "0.5rem" }}>
          Cargar
        </button>
      </form>

      {mensaje && <p>{mensaje}</p>}
    </div>
  );
}
