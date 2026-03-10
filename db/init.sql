-- =========================
-- usuario
-- =========================
CREATE TABLE IF NOT EXISTS usuario (
  id BIGSERIAL PRIMARY KEY,
  nombre_usuario TEXT NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- =========================
-- proyecto
-- =========================
CREATE TABLE IF NOT EXISTS proyecto (
  id BIGSERIAL PRIMARY KEY,
  usuario_id BIGINT NOT NULL REFERENCES usuario(id) ON DELETE CASCADE,
  nombre TEXT NOT NULL,
  password_hash TEXT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE (usuario_id, nombre)
);

-- =========================
-- busqueda
-- =========================
CREATE TABLE IF NOT EXISTS busqueda (
  id BIGSERIAL PRIMARY KEY,
  proyecto_id BIGINT NOT NULL REFERENCES proyecto(id) ON DELETE CASCADE,
  orden INT NOT NULL,
  ris_texto_original TEXT NOT NULL,
  marcador INT NOT NULL DEFAULT 0,
  UNIQUE (proyecto_id, orden)
);

-- =========================
-- criterio
-- =========================
CREATE TABLE IF NOT EXISTS criterio (
  id BIGSERIAL PRIMARY KEY,
  busqueda_id BIGINT NOT NULL REFERENCES busqueda(id) ON DELETE CASCADE,
  tipo INT NOT NULL CHECK (tipo IN (1,2)),
  texto TEXT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_criterio_busqueda_tipo
  ON criterio (busqueda_id, tipo);

-- =========================
-- paper
-- =========================
CREATE TABLE IF NOT EXISTS paper (
  id BIGSERIAL PRIMARY KEY,
  busqueda_id BIGINT NOT NULL REFERENCES busqueda(id) ON DELETE CASCADE,
  url TEXT NULL,
  doi TEXT NULL,
  titulos TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
  autores TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
  keywords TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
  resumen TEXT NULL,
  anio_publicacion INT NULL,
  ris_bloque TEXT NOT NULL,
  orden_original INT NOT NULL,
  UNIQUE (busqueda_id, orden_original)
);

CREATE INDEX IF NOT EXISTS idx_paper_busqueda_url
  ON paper (busqueda_id, url);

-- =========================
-- decision
-- =========================
CREATE TABLE IF NOT EXISTS decision (
  id BIGSERIAL PRIMARY KEY,
  busqueda_id BIGINT NOT NULL REFERENCES busqueda(id) ON DELETE CASCADE,
  paper_id BIGINT NOT NULL REFERENCES paper(id) ON DELETE CASCADE,
  tipo INT NOT NULL CHECK (tipo IN (1,2)),
  nota TEXT NULL,
  UNIQUE (busqueda_id, paper_id)
);

-- =========================
-- decision_criterio
-- =========================
CREATE TABLE IF NOT EXISTS decision_criterio (
  decision_id BIGINT NOT NULL REFERENCES decision(id) ON DELETE CASCADE,
  criterio_id BIGINT NOT NULL REFERENCES criterio(id) ON DELETE RESTRICT,
  PRIMARY KEY (decision_id, criterio_id)
);

CREATE INDEX IF NOT EXISTS idx_decision_criterio_criterio
  ON decision_criterio (criterio_id);