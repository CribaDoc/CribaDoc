export default function ListaCriterios({ criterios }) {
  const inclusion = criterios?.inclusion ?? [];
  const exclusion = criterios?.exclusion ?? [];

  return (
    <div>
      <h2>Criterios</h2>

      <h3>Inclusión</h3>
      {!inclusion.length ? (
        <p>No hay criterios de inclusión.</p>
      ) : (
        <ul>
          {inclusion.map((c) => (
            <li key={c.id}>{c.texto}</li>
          ))}
        </ul>
      )}

      <h3>Exclusión</h3>
      {!exclusion.length ? (
        <p>No hay criterios de exclusión.</p>
      ) : (
        <ul>
          {exclusion.map((c) => (
            <li key={c.id}>{c.texto}</li>
          ))}
        </ul>
      )}
    </div>
  );
}