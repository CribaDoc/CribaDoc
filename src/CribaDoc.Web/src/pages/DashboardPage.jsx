import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import { guardarSesionProyecto, limpiarSesionProyecto, limpiarSesionUsuario } from "../session";

export default function DashboardPage() {
  const navigate = useNavigate();

  const [proyectos, setProyectos] = useState([]);
  const [nombre, setNombre] = useState("");
  const [password, setPassword] = useState("");
  const [mensaje, setMensaje] = useState("");

  async function cargar() {
    try {
      const data = await api.listarMisProyectos();
      setProyectos(data.proyectos ?? []);
    } catch (error) {
      setMensaje(error.message);
    }
  }

  useEffect(() => {
    cargar();
  }, []);

  async function handleCrear(e) {
    e.preventDefault();
    setMensaje("");

    try {
      const response = await api.crearMiProyecto({ nombre, password });
      setNombre("");
      setPassword("");
      await cargar();

      const openResponse = await api.abrirMiProyecto(response.id);
      guardarSesionProyecto({
        token: openResponse.token,
        proyectoId: openResponse.id,
        nombre: openResponse.nombre
      });

      navigate(`/proyecto/${openResponse.id}`);
    } catch (error) {
      setMensaje(error.message);
    }
  }

  async function handleAbrir(id) {
    setMensaje("");

    try {
      const response = await api.abrirMiProyecto(id);

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

  function cerrarSesion() {
    limpiarSesionProyecto();
    limpiarSesionUsuario();
    navigate("/");
  }

  return (
    <div>
      <h1>Mis proyectos</h1>

      <button onClick={() => navigate("/cargar-proyecto")}>
        Cargar proyecto ajeno
      </button>

      <button onClick={cerrarSesion} style={{ marginLeft: "0.5rem" }}>
        Cerrar sesión
      </button>

      <form onSubmit={handleCrear} style={{ marginTop: "1rem" }}>
        <h2>Crear proyecto</h2>

        <input
          type="text"
          placeholder="Nombre del proyecto"
          value={nombre}
          onChange={(e) => setNombre(e.target.value)}
          required
        />

        <input
          type="password"
          placeholder="Contraseña del proyecto"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          style={{ marginLeft: "0.5rem" }}
        />

        <button type="submit" style={{ marginLeft: "0.5rem" }}>
          Crear
        </button>
      </form>

      {mensaje && <p>{mensaje}</p>}

      <ul>
        {proyectos.map((p) => (
          <li key={p.id}>
            {p.nombre}
            <button onClick={() => handleAbrir(p.id)} style={{ marginLeft: "0.5rem" }}>
              Abrir
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}
