import { useEffect, useState } from 'react';
import { getDashboardSummary } from '../api';
import { useNavigate } from 'react-router-dom';
import { getStoredUser } from '../utils/user';

function EmployeeDashboard({ data }) {
  const navigate = useNavigate();

  const formatNotif = (n) => {
    const type = n.requestType === 'ALLOCATION' ? 'Cấp phát' : 'Thu hồi';
    const status =
      n.status === 'PENDING'
        ? 'đang chờ IT duyệt'
        : n.status === 'APPROVED'
          ? 'đã được duyệt'
          : 'bị từ chối';
    return `Yêu cầu ${type} (YC-${String(n.id).padStart(4, '0')}) ${status}`;
  };

  const notifColor = (status) => {
    if (status === 'PENDING') return 'var(--warning)';
    if (status === 'APPROVED') return 'var(--success)';
    return 'var(--danger)';
  };

  return (
    <div className="page active emp-dashboard">
      <p className="emp-dashboard-intro">Tổng quan nhanh tài sản và yêu cầu của bạn</p>

      <div className="emp-stats-row">
        <div className="emp-stat">
          <span className="emp-stat-value" style={{ color: 'var(--info)' }}>{data.myDevices}</span>
          <span className="emp-stat-label">Thiết bị đang được cấp</span>
        </div>
        <div className="emp-stat">
          <span className="emp-stat-value" style={{ color: 'var(--warning)' }}>{data.myPendingRequests}</span>
          <span className="emp-stat-label">Chờ duyệt</span>
        </div>
        <div className="emp-stat">
          <span className="emp-stat-value" style={{ color: 'var(--success)' }}>{data.myCompletedRequests}</span>
          <span className="emp-stat-label">Đã hoàn thành</span>
        </div>
        <div className="emp-stat">
          <span className="emp-stat-value" style={{ color: 'var(--danger)' }}>{data.myRejectedRequests}</span>
          <span className="emp-stat-label">Bị từ chối</span>
        </div>
      </div>

      <div className="emp-quick-links">
        <button type="button" className="emp-quick-card" onClick={() => navigate('/assets')}>
          <i className="ti ti-device-laptop" />
          <span className="emp-quick-title">Thiết bị của tôi</span>
          <span className="emp-quick-sub">{data.myDevices} thiết bị đang sử dụng</span>
        </button>
        <button type="button" className="emp-quick-card" onClick={() => navigate('/requests')}>
          <i className="ti ti-file-text" />
          <span className="emp-quick-title">Yêu cầu của tôi</span>
          <span className="emp-quick-sub">
            {data.myPendingRequests > 0 ? `${data.myPendingRequests} đang chờ duyệt` : 'Gửi hoặc theo dõi yêu cầu'}
          </span>
        </button>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="card-title">Thông báo gần đây</div>
          <button type="button" className="btn btn-secondary btn-sm" onClick={() => navigate('/requests')}>
            Xem yêu cầu
          </button>
        </div>
        <div className="card-body" style={{ padding: '0 20px' }}>
          {(data.recentNotifications || []).length === 0 ? (
            <div className="empty-state" style={{ padding: '28px 0' }}>
              <div className="empty-text">Chưa có thông báo nào</div>
            </div>
          ) : (
            (data.recentNotifications || []).map((n) => (
              <div key={n.id} className="activity-item">
                <div className="activity-dot" style={{ background: notifColor(n.status) }} />
                <div className="activity-text">{formatNotif(n)}</div>
                <div className="activity-time">
                  {n.date ? new Date(n.date).toLocaleDateString('vi-VN') : '—'}
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
}

export default function DashBoard() {
  const [data, setData] = useState(null);
  const navigate = useNavigate();
  const role = getStoredUser()?.role;
  const isEmployee = role === 'EMPLOYEE';

  useEffect(() => {
    getDashboardSummary().then((res) => setData(res.data)).catch(console.error);
  }, []);

  if (!data) {
    return <div style={{ padding: '24px', color: 'var(--text3)' }}>Đang tải...</div>;
  }

  if (isEmployee || data.scope === 'PERSONAL') {
    return <EmployeeDashboard data={data} />;
  }

  const pendingCount = data.pendingRequests ?? 0;

  return (
    <div className="page active" id="page-dashboard">
      <div className="stats-grid">
        <div className="stat-card blue">
          <div className="stat-icon blue"><i className="ti ti-device-laptop" /></div>
          <div className="stat-label">Tổng thiết bị</div>
          <div className="stat-value" style={{ color: 'var(--info)' }}>{data.totalDevices}</div>
          <div className="stat-sub">Trong hệ thống</div>
        </div>
        <div className="stat-card green">
          <div className="stat-icon green"><i className="ti ti-circle-check" /></div>
          <div className="stat-label">Đang cấp phát</div>
          <div className="stat-value" style={{ color: 'var(--success)' }}>{data.assignedDevices}</div>
          <div className="stat-sub">
            {data.totalDevices ? Math.round((data.assignedDevices / data.totalDevices) * 100) : 0}% tổng thiết bị
          </div>
        </div>
        <div className="stat-card amber">
          <div className="stat-icon amber"><i className="ti ti-package" /></div>
          <div className="stat-label">Có sẵn trong kho</div>
          <div className="stat-value" style={{ color: 'var(--warning)' }}>{data.availableDevices}</div>
          <div className="stat-sub">Sẵn sàng cấp phát</div>
        </div>
        <div className="stat-card red">
          <div className="stat-icon red"><i className="ti ti-alert-triangle" /></div>
          <div className="stat-label">Yêu cầu chờ duyệt</div>
          <div className="stat-value" style={{ color: 'var(--danger)' }}>{pendingCount}</div>
          <div className="stat-sub">Cần IT xử lý</div>
        </div>
      </div>

      <div className="grid-3">
        <div className="card">
          <div className="card-header">
            <div className="card-title">Yêu cầu gần đây</div>
            <button type="button" className="btn btn-secondary btn-sm" onClick={() => navigate('/requests')}>
              Xem tất cả
            </button>
          </div>
          <div className="card-body" style={{ padding: 0 }}>
            <table>
              <thead>
                <tr>
                  <th>Nhân viên</th>
                  <th>Loại</th>
                  <th>Trạng thái</th>
                  <th>Ngày gửi</th>
                </tr>
              </thead>
              <tbody>
                {(data.recentRequests || []).map((req) => (
                  <tr key={req.id}>
                    <td><div style={{ fontWeight: 500 }}>{req.employeeName}</div></td>
                    <td>
                      {req.requestType === 'ALLOCATION' ? (
                        <span className="badge badge-info">Cấp phát</span>
                      ) : (
                        <span className="badge badge-danger">Thu hồi</span>
                      )}
                    </td>
                    <td>
                      <span
                        className={`badge ${
                          req.status === 'PENDING' ? 'badge-warning' : req.status === 'APPROVED' ? 'badge-success' : 'badge-danger'
                        }`}
                      >
                        {req.status === 'PENDING' ? 'Chờ duyệt' : req.status === 'APPROVED' ? 'Đã duyệt' : 'Từ chối'}
                      </span>
                    </td>
                    <td>{req.date ? new Date(req.date).toLocaleDateString('vi-VN') : '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
        <div className="card">
          <div className="card-header"><div className="card-title">Phân bổ thiết bị</div></div>
          <div className="card-body">
            <div style={{ marginBottom: '16px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '6px' }}>
                <span style={{ fontSize: '13px' }}>Đang cấp phát</span>
                <span className="mono" style={{ fontSize: '13px' }}>
                  {data.assignedDevices} / {data.totalDevices}
                </span>
              </div>
              <div className="progress">
                <div
                  className="progress-bar"
                  style={{ width: `${(data.assignedDevices / (data.totalDevices || 1)) * 100}%`, background: 'var(--info)' }}
                />
              </div>
            </div>
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '6px' }}>
                <span style={{ fontSize: '13px' }}>Có sẵn</span>
                <span className="mono" style={{ fontSize: '13px' }}>
                  {data.availableDevices} / {data.totalDevices}
                </span>
              </div>
              <div className="progress">
                <div
                  className="progress-bar"
                  style={{ width: `${(data.availableDevices / (data.totalDevices || 1)) * 100}%`, background: 'var(--success)' }}
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
