export function guardarSesionUsuario({ token, userId, nombreUsuario }) {
  localStorage.setItem("userToken", token);
  localStorage.setItem("userId", String(userId));
  localStorage.setItem("nombreUsuario", nombreUsuario ?? "");
}

export function guardarSesionProyecto({ token, proyectoId, nombre }) {
  localStorage.setItem("projectToken", token);
  localStorage.setItem("token", token); // compatibilidad con código antiguo
  localStorage.setItem("proyectoId", String(proyectoId));
  localStorage.setItem("proyectoNombre", nombre ?? "");
}

// Compatibilidad con el front viejo
export function guardarSesion({ token, proyectoId, nombre }) {
  guardarSesionProyecto({ token, proyectoId, nombre });
}

export function obtenerUserToken() {
  return localStorage.getItem("userToken");
}

export function obtenerProjectToken() {
  return localStorage.getItem("projectToken") || localStorage.getItem("token");
}

// Compatibilidad con el front viejo
export function obtenerToken() {
  return obtenerProjectToken();
}

export function obtenerProyectoId() {
  return localStorage.getItem("proyectoId");
}

export function obtenerProyectoNombre() {
  return localStorage.getItem("proyectoNombre");
}

export function limpiarSesionUsuario() {
  localStorage.removeItem("userToken");
  localStorage.removeItem("userId");
  localStorage.removeItem("nombreUsuario");
}

export function limpiarSesionProyecto() {
  localStorage.removeItem("projectToken");
  localStorage.removeItem("token");
  localStorage.removeItem("proyectoId");
  localStorage.removeItem("proyectoNombre");
}

// Compatibilidad con el front viejo
export function limpiarSesion() {
  limpiarSesionProyecto();
}