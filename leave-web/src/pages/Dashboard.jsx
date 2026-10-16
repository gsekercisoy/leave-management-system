import { useEffect, useState } from 'react';
import api from '../api.js';

export default function Dashboard() {
  const [leaves, setLeaves] = useState([]);
  const [balance, setBalance] = useState(null);
  const [form, setForm] = useState({ startDate: '', endDate: '', reason: '' });
  const [error, setError] = useState('');

  async function load() {
    const [l, b] = await Promise.all([api.get('/leaves'), api.get('/balances/me')]);
    setLeaves(l.data);
    setBalance(b.data);
  }

  useEffect(() => {
    load().catch(() => setError('Failed to load data.'));
  }, []);

  async function submit(e) {
    e.preventDefault();
    setError('');
    try {
      await api.post('/leaves', form);
      setForm({ startDate: '', endDate: '', reason: '' });
      await load();
    } catch (err) {
      setError(err.response?.data || 'Failed to create request.');
    }
  }

  return (
    <div style={{ maxWidth: 800, margin: '24px auto', padding: '0 16px' }}>
      <h2>My Leave</h2>
      {balance && (
        <p>
          Balance {balance.year}: {balance.totalDays - balance.usedDays} / {balance.totalDays} days left
        </p>
      )}
      <form onSubmit={submit} style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        <input type="date" value={form.startDate}
          onChange={(e) => setForm({ ...form, startDate: e.target.value })} required />
        <input type="date" value={form.endDate}
          onChange={(e) => setForm({ ...form, endDate: e.target.value })} required />
        <input placeholder="Reason" value={form.reason}
          onChange={(e) => setForm({ ...form, reason: e.target.value })} required />
        <button type="submit">Request</button>
      </form>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <table border="1" cellPadding="6" style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr><th>Start</th><th>End</th><th>Reason</th><th>Status</th></tr>
        </thead>
        <tbody>
          {leaves.map((l) => (
            <tr key={l.id}>
              <td>{l.startDate?.slice(0, 10)}</td>
              <td>{l.endDate?.slice(0, 10)}</td>
              <td>{l.reason}</td>
              <td>{l.status}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
