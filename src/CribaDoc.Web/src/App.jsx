import { BrowserRouter, Routes, Route } from "react-router-dom";
import InicioPage from "./pages/InicioPage";
import DashboardPage from "./pages/DashboardPage";
import CargarProyectoPage from "./pages/CargarProyectoPage";
import ProyectoPage from "./pages/ProyectoPage";
import ConfigurarBusquedaPage from "./pages/ConfigurarBusquedaPage";
import CribadoPage from "./pages/CribadoPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<InicioPage />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/cargar-proyecto" element={<CargarProyectoPage />} />
        <Route path="/proyecto/:id" element={<ProyectoPage />} />
        <Route path="/busqueda/:id/configurar" element={<ConfigurarBusquedaPage />} />
        <Route path="/busqueda/:id" element={<CribadoPage />} />
      </Routes>
    </BrowserRouter>
  );
}