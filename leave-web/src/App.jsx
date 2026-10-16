import { BrowserRouter, Routes, Route, Link, Navigate, useNavigate } from 'react-router-dom';
import Login from './pages/Login.jsx';
import Register from './pages/Register.jsx';
import Dashboard from './pages/Dashboard.jsx';
import Approvals from './pages/Approvals.jsx';
import { getUser, logout } from './api.js';

function PrivateRoute({ children }) {
  return localStorage.getItem('token') ? children : <Navigate to="/login" replace />;
}

function ManagerRoute({ children }) {
  const user = getUser();
  if (!localStorage.getItem('token')) return <Navigate to="/login" replace />;
  if (user?.role !== 'Manager' && user?.role !== 'Admin')
    return <Navigate to="/" replace />;
  return children;
}

function Nav() {
  const user = getUser();
  const navigate = useNavigate();
  if (!localStorage.getItem('token')) return null;
  return (
    <nav style={{ display: 'flex', gap: 12, padding: 12, borderBottom: '1px solid #ddd' }}>
      <Link to="/">Dashboard</Link>
      {(user?.role === 'Manager' || user?.role === 'Admin') && (
        <Link to="/approvals">Approvals</Link>
      )}
      <span style={{ marginLeft: 'auto' }}>
        {user?.username} ({user?.role}){' '}
        <button
          onClick={() => {
            logout();
            navigate('/login');
          }}
        >
          Logout
        </button>
      </span>
    </nav>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <Nav />
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/" element={<PrivateRoute><Dashboard /></PrivateRoute>} />
        <Route path="/approvals" element={<ManagerRoute><Approvals /></ManagerRoute>} />
      </Routes>
    </BrowserRouter>
  );
}
