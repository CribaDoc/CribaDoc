import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import { guardarSesionUsuario } from "../session";

export default function InicioPage() {
  const navigate = useNavigate();

  const [modo, setModo] = useState("login");
  const [nombreUsuario, setNombreUsuario] = useState("");
  const [password, setPassword] = useState("");
  const [mensaje, setMensaje] = useState("");

  async function handleSubmit(e) {
    e.preventDefault();
    setMensaje("");

    try {
      const response =
        modo === "login"
          ? await api.login({ nombreUsuario, password })
          : await api.register({ nombreUsuario, password });

      guardarSesionUsuario({
        token: response.token,
        userId: response.id,
        nombreUsuario: response.nombreUsuario
      });

      navigate("/dashboard");
    } catch (error) {
      setMensaje(error.message);
    }
  }

  return (
    <div>
      <h1>CribaDoc</h1>

      <div>
        <button type="button" onClick={() => setModo("login")}>
          Iniciar sesión
        </button>
        <button
          type="button"
          onClick={() => setModo("register")}
          style={{ marginLeft: "0.5rem" }}
        >
          Registrarse
        </button>
      </div>

      <form onSubmit={handleSubmit} style={{ marginTop: "1rem" }}>
        <input
          type="text"
          placeholder="Usuario"
          value={nombreUsuario}
          onChange={(e) => setNombreUsuario(e.target.value)}
          required
        />

        <input
          type="password"
          placeholder="Contraseña"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          style={{ marginLeft: "0.5rem" }}
        />

        <button type="submit" style={{ marginLeft: "0.5rem" }}>
          {modo === "login" ? "Entrar" : "Crear usuario"}
        </button>
      </form>

      {mensaje && <p>{mensaje}</p>}
    </div>
  );
}