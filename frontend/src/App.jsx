import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import DashBoard from './pages/DashBoard';
import Assets from './pages/Assets';
import Requests from './pages/Requests';
import AllocationHistory from './pages/AllocationHistory';
import Login from './pages/Login';
import Sidebar from './components/Sidebar';
import Depreciation from './pages/Depreciation';
import History from './pages/History';
import Users from './pages/Users';
import Topbar from './components/Topbar';
import { ToastProvider } from './components/Toast';
import { canAccessRoute } from './utils/permissions';
import './App.css';

function PrivateRoute({ children, path }) {
  const token = localStorage.getItem('accessToken');
  if (!token) return <Navigate to="/login" />;
  if (path && !canAccessRoute(path)) return <Navigate to="/" replace />;
  return children;
}

function AppLayout({ children }) {
  return (
    <div className="app-shell">
      <Sidebar />
      <div className="app-main">
        <Topbar />
        <div className="content">{children}</div>
      </div>
    </div>
  );
}

function App() {
  return (
    <ToastProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/" element={<PrivateRoute path="/"><AppLayout><DashBoard /></AppLayout></PrivateRoute>} />
          <Route path="/assets" element={<PrivateRoute path="/assets"><AppLayout><Assets /></AppLayout></PrivateRoute>} />
          <Route path="/requests" element={<PrivateRoute path="/requests"><AppLayout><Requests /></AppLayout></PrivateRoute>} />
          <Route path="/allocation" element={<PrivateRoute path="/allocation"><AppLayout><AllocationHistory /></AppLayout></PrivateRoute>} />
          <Route path="/depreciation" element={<PrivateRoute path="/depreciation"><AppLayout><Depreciation /></AppLayout></PrivateRoute>} />
          <Route path="/history" element={<PrivateRoute path="/history"><AppLayout><History /></AppLayout></PrivateRoute>} />
          <Route path="/users" element={<PrivateRoute path="/users"><AppLayout><Users /></AppLayout></PrivateRoute>} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </ToastProvider>
  );
}

export default App;
