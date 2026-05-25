import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../api';
import { useToast } from './Toast';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [showPassword, setShowPassword] = useState(false);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const res = await login({ username: email, password });
      localStorage.setItem('accessToken', res.data.accessToken);
      localStorage.setItem('user', JSON.stringify(res.data.user));
      showToast(`Đăng nhập thành công! Xin chào, ${res.data.user.fullName}`, 'success');
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.error?.message || 'Đăng nhập thất bại');
    }
  };

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-logo">
          <div className="logo-icon"><i className="ti ti-device-laptop" style={{ fontSize: '20px', color: '#fff' }} /></div>
          <div>
            <div className="logo-text">AssetFlow</div>
            <div className="logo-sub">IT ASSET MANAGEMENT</div>
          </div>
        </div>
        <div className="login-title">Đăng nhập</div>
        
        <form onSubmit={handleLogin} autoComplete='off'>
          
          <div className="form-group">
            <div className="form-label">Email</div>
            <input
              className="form-input"
              type="email"
              autoComplete='off'
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
            />
          </div>
          <div className="form-group">
            <div className="form-label">Mật khẩu</div>
            <input
              className="form-input"
              type={showPassword ? "text" : "password"}
              autoComplete='off'
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              required
            />
            <button
              type="button"
              className="password-toggle"
              onClick={() => setShowPassword(!showPassword)}
            >
              {showPassword ? "Ẩn" : "Hiện"}
            </button>
          </div>
          <button type="submit" className="login-btn">Đăng nhập vào hệ thống →</button>
        </form>
        <div style={{ marginTop: '16px', fontSize: '12px', color: 'var(--text3)', textAlign: 'center' }}>
          Demo: admin@ / hr@ / mai@company.com — mật khẩu 12345678
        </div>
        {error && (
          <div style={{ color: 'var(--danger)', marginTop: '16px', fontSize: '13px', textAlign: 'center' }}>{error}</div>
        )}
      </div>
    </div>
  );
}
