export default function StatsBar({ stats }) {
  if (!stats) {
    return null;
  }

  return (
    <div style={{ marginBottom: "1rem" }}>
      <p>Total: {stats.total}</p>
      <p>Marcador: {stats.marcador}</p>
      <p>Result: {stats.result}</p>
      <p>Deleted: {stats.deleted}</p>
      <p>Restantes: {stats.remaining}</p>
    </div>
  );
}