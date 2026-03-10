import { obtenerProjectToken, obtenerUserToken } from "../session";

const API_URL = "http://localhost:8080";

async function request(path, options = {}) {
  const response = await fetch(`${API_URL}${path}`, options);
  const contentType = response.headers.get("content-type") || "";

  if (!response.ok) {
    let errorMessage = `Error HTTP ${response.status}`;

    if (contentType.includes("application/json")) {
      const data = await response.json();
      errorMessage = data.message || data.title || JSON.stringify(data);
    } else {
      const text = await response.text();
      if (text) errorMessage = text;
    }

    throw new Error(errorMessage);
  }

  if (response.status === 204) return null;
  if (contentType.includes("application/json")) return response.json();
  return response.text();
}

async function requestUser(path, options = {}) {
  const token = obtenerUserToken();

  return request(path, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      ...(options.headers || {})
    }
  });
}

async function requestProject(path, options = {}) {
  const token = obtenerProjectToken();

  return request(path, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      ...(options.headers || {})
    }
  });
}

async function downloadProjectFile(path, fallbackFilename) {
  const token = obtenerProjectToken();

  const response = await fetch(`${API_URL}${path}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Error HTTP ${response.status}`);
  }

  const blob = await response.blob();
  const disposition = response.headers.get("content-disposition") || "";
  const match = disposition.match(/filename="?([^"]+)"?/i);
  const filename = match?.[1] || fallbackFilename;

  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  window.URL.revokeObjectURL(url);
}

export const api = {
  register(data) {
    return request("/auth/register", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data)
    });
  },

  login(data) {
    return request("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data)
    });
  },

  listarMisProyectos() {
    return requestUser("/me/projects", { method: "GET" });
  },

  crearMiProyecto(data) {
    return requestUser("/me/projects", {
      method: "POST",
      body: JSON.stringify(data)
    });
  },

  abrirMiProyecto(proyectoId) {
    return requestUser(`/me/projects/${proyectoId}/open`, {
      method: "POST"
    });
  },

  cargarProyecto(data) {
    return requestUser("/projects/load", {
      method: "POST",
      body: JSON.stringify(data)
    });
  },

  obtenerResumenProyecto(proyectoId) {
    return requestProject(`/projects/${proyectoId}/summary`, { method: "GET" });
  },

  importarBusqueda(proyectoId, risText) {
    return requestProject(`/projects/${proyectoId}/busquedas`, {
      method: "POST",
      body: JSON.stringify(risText)
    });
  },

  obtenerCriterios(busquedaId) {
    return requestProject(`/busquedas/${busquedaId}/criterios`, { method: "GET" });
  },

  crearCriterio(busquedaId, data) {
    return requestProject(`/busquedas/${busquedaId}/criterios`, {
      method: "POST",
      body: JSON.stringify(data)
    });
  },

  obtenerPaperActual(busquedaId) {
    return requestProject(`/busquedas/${busquedaId}/current`, { method: "GET" });
  },

  obtenerEstadisticasBusqueda(busquedaId) {
    return requestProject(`/busquedas/${busquedaId}/stats`, { method: "GET" });
  },

  crearDecision(busquedaId, data) {
    return requestProject(`/busquedas/${busquedaId}/decision`, {
      method: "POST",
      body: JSON.stringify({
        tipo: data.tipo,
        nota: data.nota,
        criteriosAplicados: data.criterioIds
      })
    });
  },

  descargarRisGlobal(proyectoId) {
    return downloadProjectFile(
      `/projects/${proyectoId}/export/result.ris`,
      `proyecto-${proyectoId}-result.ris`
    );
  },

  descargarExcelProyecto(proyectoId) {
    return downloadProjectFile(
      `/projects/${proyectoId}/export/executive.xlsx`,
      `proyecto-${proyectoId}-executive.xlsx`
    );
  },

  descargarRisBusqueda(busquedaId) {
    return downloadProjectFile(
      `/busquedas/${busquedaId}/export/result.ris`,
      `busqueda-${busquedaId}-result.ris`
    );
  }
};