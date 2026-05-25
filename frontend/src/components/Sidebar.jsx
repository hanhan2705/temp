import { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { getStoredUser, getUserDisplay } from '../utils/user';
import { getRequests } from '../api';
import { PERMISSIONS } from '../utils/permissions';

export default function Sidebar() {
  const location = useLocation();
  const navigate = useNavigate();
  const user = getStoredUser();
  const display = getUserDisplay(user);
  const role = display.role;
  const [pendingCount, setPendingCount] = useState(0);
  const [collapsed, setCollapsed] = useState(false);

  useEffect(() => {
    if (PERMISSIONS.approveRequests(role)) {
      getRequests('', 'PENDING')
        .then((res) => setPendingCount(res.data.data?.length || 0))
        .catch(() => setPendingCount(0));
    }
  }, [location.pathname, role]);

  const getActive = (p) => (location.pathname === p ? 'nav-item active' : 'nav-item');

  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
  };

  const isEmployee = role === 'EMPLOYEE';

  return (
    <aside className={`sidebar ${collapsed ? 'collapsed' : ''}`}>
      {/* <div className="sidebar-toggle" onClick={() => setCollapsed(!collapsed)}>
        <i className="ti ti-menu-2" />
      </div> */}
      <div className="sidebar-logo">
        <div className="logo-icon" onClick={() => setCollapsed(!collapsed)}><i className="ti ti-device-laptop" style={{ fontSize: '18px', color: '#fff' }} /></div>
        <div>
          {!collapsed && (
            <div>
              <div className="logo-text">AssetFlow</div>
              <div className="logo-sub">v1.0.0</div>
            </div>
          )}
        </div>
      </div>
      <div className="sidebar-role">
        <div className="role-user">
          {collapsed
            ? display.name.split(' ').pop()
            : display.name}
        </div>
        <div className="role-badge">{display.role}</div>
      </div>
      <nav className="sidebar-nav">
        <div className="nav-section">Tổng quan</div>
        <Link to="/" className={getActive('/')}>
          <i className="ti ti-layout-dashboard" /> 
          {!collapsed && <span>Dashboard</span>}
        </Link>
        <div className="nav-section">Quản lý</div>
        {(PERMISSIONS.viewAllDevices(role) || PERMISSIONS.viewMyDevicesOnly(role)) && (
          <Link to="/assets" className={getActive('/assets')}>
            <i className="ti ti-device-laptop" /> 
            {!collapsed && <span>{isEmployee ? 'Thiết bị của tôi' : 'Thiết bị'}</span>}
            
          </Link>
        )}
        <Link to="/requests" className={getActive('/requests')}>
          <i className="ti ti-file-text" /> 
          {!collapsed && <span>Yêu cầu</span>}
          {pendingCount > 0 && PERMISSIONS.approveRequests(role) && (
            <span className="nav-badge">{pendingCount}</span>
          )}
        </Link>
        {PERMISSIONS.viewAllocationHistory(role) && (
          <Link to="/allocation" className={getActive('/allocation')}>
            <i className="ti ti-arrow-right-circle" /> 
            {!collapsed && <span>Cấp phát</span>}
          </Link>
        )}
        {PERMISSIONS.viewDepreciation(role) && (
          <Link to="/depreciation" className={getActive('/depreciation')}>
            <i className="ti ti-trending-down" /> 
            {!collapsed && <span>Khấu hao</span>}
          </Link>
        )}
        <div className="nav-section">Hệ thống</div>
        {(PERMISSIONS.viewAllocationHistory(role) || role === 'HR') && (
          <Link to="/history" className={getActive('/history')}>
            <i className="ti ti-history" /> 
            {!collapsed && <span>Lịch sử</span>}
          </Link>
        )}
        {PERMISSIONS.viewUsers(role) && (
          <Link to="/users" className={getActive('/users')}>
            <i className="ti ti-users-group" /> 
            {!collapsed && <span>Tài khoản</span>}
          </Link>
        )}
      </nav>
      <div className="sidebar-footer">
        <button type="button" className="logout-btn" onClick={handleLogout}>
          <i className="ti ti-logout" style={{ fontSize: '16px' }} />  
          {!collapsed && <span>Đăng xuất</span>}
        </button>
      </div>
    </aside>
  );
}
