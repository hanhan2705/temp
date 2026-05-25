import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { getStoredUser, getUserDisplay } from '../utils/user';

const pageTitles = {
  '/': 'Dashboard',
  '/assets': 'Quản lý Thiết bị',
  '/requests': 'Quản lý Yêu cầu',
  '/allocation': 'Cấp phát & Thu hồi',
  '/depreciation': 'Quản lý Khấu hao',
  '/history': 'Lịch sử Hoạt động',
  '/users': 'Quản lý Tài khoản',
};

export default function Topbar() {
  const location = useLocation();
  const navigate = useNavigate();
  const user = getStoredUser();
  const display = getUserDisplay(user);
  const [searchVal, setSearchVal] = useState('');

  let title = pageTitles[location.pathname] || 'Dashboard';
  if (location.pathname === '/assets' && user?.role === 'EMPLOYEE') {
    title = 'Thiết bị của tôi';
  }
  if (location.pathname === '/' && user?.role === 'EMPLOYEE') {
    title = 'Tổng quan cá nhân';
  }

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      const q = searchVal.trim();
      if (q) {
        if (location.pathname === '/users') {
          navigate(`/users?q=${encodeURIComponent(q)}`);
        } else {
          navigate(`/assets?q=${encodeURIComponent(q)}`);
        }
      }
    }
  };

  return (
    <header className="topbar">
      <div className="topbar-title">{title}</div>
      <div className="topbar-search">
        <i className="ti ti-search" style={{ fontSize: '15px', color: 'var(--text3)' }} />
        <input
          type="text"
          placeholder="Tìm kiếm thiết bị, nhân viên..."
          value={searchVal}
          onChange={(e) => setSearchVal(e.target.value)}
          onKeyDown={handleKeyDown}
        />
      </div>
      <div className="topbar-actions">
        <div className="icon-btn" title="Thông báo">
          <i className="ti ti-bell" />
          <div className="notif-dot" />
        </div>
        <div className="avatar" title={display.email} style={{ background: display.accent }}>
          {display.initials}
        </div>
      </div>
    </header>
  );
}
