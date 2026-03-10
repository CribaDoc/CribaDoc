export default function BusquedaTable({
  busquedas,
  onEntrar,
  onExportarRis
}) {
  if (!busquedas?.length) {
    return <p>No hay búsquedas todavía.</p>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Orden</th>
          <th>Total</th>
          <th>Marcador</th>
          <th>Result</th>
          <th>Deleted</th>
          <th>Acciones</th>
        </tr>
      </thead>
      <tbody>
        {busquedas.map((b) => (
          <tr key={b.busquedaId}>
            <td>{b.orden}</td>
            <td>{b.total}</td>
            <td>{b.marcador}</td>
            <td>{b.result}</td>
            <td>{b.deleted}</td>
            <td>
              <button onClick={() => onEntrar(b.busquedaId)}>Entrar</button>

              <button
                onClick={() => onExportarRis(b.busquedaId)}
                style={{ marginLeft: "0.5rem" }}
              >
                Exportar RIS
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}