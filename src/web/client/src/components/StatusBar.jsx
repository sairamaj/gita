export default function StatusBar({ status, extra }) {
  return (
    <div className="status-bar">
      <strong>Status:</strong> {status || "Idle"}
      {extra ? <span className="status-extra">• {extra}</span> : null}
    </div>
  );
}
