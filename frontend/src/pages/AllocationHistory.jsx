import { useEffect, useState } from 'react';
import { getAllocationHistory, getRecoveryHistory } from '../api';
import { getApiError } from '../utils/apiError';
import { useToast } from '../components/Toast';

export default function AllocationHistory() {
  const [allocations, setAllocations] = useState([]);
  const [recoveries, setRecoveries] = useState([]);
  const [loading, setLoading] = useState(true);
  const { showToast } = useToast();

  useEffect(() => {
    Promise.all([getAllocationHistory(), getRecoveryHistory()])
      .then(([aRes, rRes]) => {
        setAllocations(aRes.data.data || []);
        setRecoveries(rRes.data.data || []);
      })
      .catch((err) => showToast(getApiError(err, 'Không tải được lịch sử'), 'error'))
      .finally(() => setLoading(false));
  }, [showToast]);

  if (loading) {
    return <div style={{ padding: '24px', color: 'var(--text3)' }}>Đang tải...</div>;
  }

  return (
    <div className="page active" id="page-allocation">
      <div className="card" style={{ marginBottom: '20px' }}>
        <div className="card-header">
          <div className="card-title">Lịch sử cấp phát</div>
        </div>
        <div className="card-body" style={{ padding: 0 }}>
          <table>
            <thead>
              <tr>
                <th>Mã</th>
                <th>Thiết bị</th>
                <th>Nhân viên</th>
                <th>Ngày cấp</th>
                <th>Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              {allocations.map((row) => (
                <tr key={row.id}>
                  <td className="mono">{row.code}</td>
                  <td>{row.deviceName} ({row.deviceCode})</td>
                  <td>{row.employeeName}</td>
                  <td>{new Date(row.date).toLocaleDateString('vi-VN')}</td>
                  <td><span className="badge badge-success">{row.status === 'COMPLETED' ? 'Hoàn tất' : row.status}</span></td>
                </tr>
              ))}
              {allocations.length === 0 && (
                <tr><td colSpan={5} style={{ textAlign: 'center', color: 'var(--text3)' }}>Chưa có lịch sử cấp phát</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card">
        <div className="card-header"><div className="card-title">Lịch sử thu hồi</div></div>
        <div className="card-body" style={{ padding: 0 }}>
          <table>
            <thead>
              <tr>
                <th>Mã</th>
                <th>Thiết bị</th>
                <th>Nhân viên</th>
                <th>Ngày thu hồi</th>
                <th>Tình trạng TB</th>
              </tr>
            </thead>
            <tbody>
              {recoveries.map((row) => (
                <tr key={row.id}>
                  <td className="mono">{row.code}</td>
                  <td>{row.deviceName} ({row.deviceCode})</td>
                  <td>{row.employeeName}</td>
                  <td>{new Date(row.date).toLocaleDateString('vi-VN')}</td>
                  <td>
                    <span className={`badge ${row.condition === 'GOOD' || row.condition === 'OK' ? 'badge-success' : 'badge-warning'}`}>
                      {row.condition}
                    </span>
                  </td>
                </tr>
              ))}
              {recoveries.length === 0 && (
                <tr><td colSpan={5} style={{ textAlign: 'center', color: 'var(--text3)' }}>Chưa có lịch sử thu hồi</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
