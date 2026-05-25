import { useEffect, useState } from 'react';
import { getActivityHistory } from '../api';
import { getApiError } from '../utils/apiError';
import { useToast } from './Toast';

export default function History() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const { showToast } = useToast();

  useEffect(() => {
    getActivityHistory()
      .then((res) => setRows(res.data.data || []))
      .catch((err) => showToast(getApiError(err, 'Không tải được lịch sử'), 'error'))
      .finally(() => setLoading(false));
  }, [showToast]);

  if (loading) {
    return <div style={{ padding: '24px', color: 'var(--text3)' }}>Đang tải...</div>;
  }

  return (
    <div className="page active" id="page-history">
      <div className="card">
        <div className="card-header">
          <div className="card-title">Toàn bộ lịch sử hoạt động</div>
        </div>
        <div className="card-body" style={{ padding: 0 }}>
          <table>
            <thead>
              <tr>
                <th>Thời gian</th>
                <th>Loại</th>
                <th>Nhân viên</th>
                <th>Thiết bị</th>
                <th>Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={`${r.type}-${r.time}-${i}`}>
                  <td className="mono">{new Date(r.time).toLocaleString('vi-VN')}</td>
                  <td>
                    <span className={`badge ${r.type === 'ALLOCATION' ? 'badge-info' : 'badge-danger'}`}>
                      {r.type === 'ALLOCATION' ? 'Cấp phát' : 'Thu hồi'}
                    </span>
                  </td>
                  <td>{r.employeeName}</td>
                  <td>{r.deviceName} ({r.deviceCode})</td>
                  <td><span className="badge badge-success">Hoàn tất</span></td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr>
                  <td colSpan={5} style={{ textAlign: 'center', color: 'var(--text3)' }}>
                    Chưa có hoạt động nào — duyệt cấp phát/thu hồi để ghi lịch sử
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
