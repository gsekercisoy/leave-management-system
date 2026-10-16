import { useEffect, useState } from 'react';
import api from '../api.js';

export default function Approvals() {
  const [leaves, setLeaves] = useState([]);
  const [error, setError] = useState('');

  async function load() {
    const { data } = await api.get('/leaves/all');
    setLeaves(data);
  }

  useEffect(() => {
    load().catch(() => setError('Failed to load requests.'));
  }, []);

  async function decide(id, action) {
    setError('');
    try {
      await api.post(`/leaves/${id}/${action}`);
      await load();
    } catch (err) {
      setError(err.response?.data || `Failed to ${action}.`);
    }
  }

  return (
    <div style={{ maxWidth: 900, margin: '24px auto', padding: '0 16px' }}>
      <h2>Approvals</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <table border="1" cellPadding="6" style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr><th>ID</th><th>User</th><th>Start</th><th>End</th><th>Reason</th><th>Status</th><th>Actions</th></tr>
        </thead>
        <tbody>
          {leaves.map((l) => (
            <tr key={l.id}>
              <td>{l.id}</td>
              <td>{l.userId}</td>
              <td>{l.startDate?.slice(0, 10)}</td>
              <td>{l.endDate?.slice(0, 10)}</td>
              <td>{l.reason}</td>
              <td>{l.status}</td>
              <td>
                {l.status === 'Pending' && (
                  <>
                    <button onClick={() => decide(l.id, 'approve')}>Approve</button>{' '}
                    <button onClick={() => decide(l.id, 'reject')}>Reject</button>
                  </>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
