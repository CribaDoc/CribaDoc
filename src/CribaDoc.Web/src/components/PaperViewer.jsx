export default function PaperViewer({ paper }) {
  if (!paper) {
    return <p>No hay paper actual cargado.</p>;
  }

  return (
    <div>
      <h2>{paper.titulo || "Sin título"}</h2>

      {paper.autores?.length > 0 && (
        <>
          <h3>Autores</h3>
          <p>{paper.autores.join(", ")}</p>
        </>
      )}

      {paper.keywords?.length > 0 && (
        <>
          <h3>Keywords</h3>
          <p>{paper.keywords.join(", ")}</p>
        </>
      )}

      {paper.anioPublicacion && (
        <>
          <h3>Año</h3>
          <p>{paper.anioPublicacion}</p>
        </>
      )}

      {paper.doi && (
        <>
          <h3>DOI</h3>
          <p>{paper.doi}</p>
        </>
      )}

      {paper.url && (
        <>
          <h3>URL</h3>
          <p>
            <a href={paper.url} target="_blank" rel="noreferrer">
              {paper.url}
            </a>
          </p>
        </>
      )}

      {paper.resumen && (
        <>
          <h3>Resumen</h3>
          <p>{paper.resumen}</p>
        </>
      )}
    </div>
  );
}