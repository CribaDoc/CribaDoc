# CribaDoc

CribaDoc es una aplicación para el cribado sistemático de artículos científicos a partir de archivos **RIS**.

La aplicación se ejecuta con **Docker Desktop**, por lo que no hace falta instalar la base de datos, la API ni la web por separado.

---

## Requisito previo

Necesario:

- **Docker Desktop** instalado
- **Docker Desktop** abierto

---

## Instalación y arranque

1. Abre **Docker Desktop**
2. Abre una terminal en la carpeta del repositorio
3. Ejecuta:

```bash
docker compose up -d --build
```

Este comando:

- construye la aplicación
- arranca la base de datos
- arranca la API
- arranca la web

## Acceso a la aplicación

Una vez arrancado todo, abre en el navegador:

```text
http://localhost:5173
```

## Parar y reanudar la ejecución

### Parar la aplicación sin borrar datos

```bash
docker compose stop
```

### Volver a arrancarla después

```bash
docker compose start
```

## Borrar datos y empezar de cero

Si quieres borrar completamente los datos guardados, ejecuta:

```bash
docker compose down -v
```

**Importante:**

- con este comando se pierden los datos guardados.

## Resumen rápido

### Instalar / arrancar

```bash
docker compose up -d --build
```

### Parar

```bash
docker compose stop
```

### Reanudar

```bash
docker compose start
```

### Borrar todos los datos

```bash
docker compose down -v
```

## Nota

Si usas `docker compose down -v`, la base de datos se reinicia y se pierde toda la información almacenada.
