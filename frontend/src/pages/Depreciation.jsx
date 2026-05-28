import { useEffect, useState, useCallback } from 'react';
import { getDepreciations, getDevices, setupDepreciation } from '../api';
import { useToast } from '../components/Toast';
import { getApiError } from '../utils/apiError';

export default function Depreciation() {
  const [data, setData] = useState([]);
  const [devices, setDevices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState({ deviceId: '', method: 'STRAIGHT_LINE', usefulLifeMonths: 36, initialValue: '', salvageValue: 0, period: 'MONTH' });
  const { showToast } = useToast();

  const fetchData = useCallback(() => {
    Promise.all([getDepreciations(), getDevices()])
      .then(([depRes, devRes]) => {
        setData(depRes.data.data || []);
        setDevices(devRes.data.data || []);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleSave = async () => {
    if (!form.deviceId || !form.initialValue) {
      showToast('Vui lòng điền đầy đủ thông tin', 'warning');
      return;
    }
    const dev = withoutDep.find((d) => d.id === Number(form.deviceId));
    if (!dev || !dev.purchaseDate || dev.originalCost === null || dev.originalCost === undefined) {
      showToast('Thiết bị thiếu thông tin ngày mua hoặc nguyên giá để tính khấu hao!', 'error');
      return;
    }
    try {
      setLoading(true);
      await setupDepreciation({
        deviceId: Number(form.deviceId),
        method: form.method,
        initialValue: Number(form.initialValue),
        usefulLifeMonths: Number(form.usefulLifeMonths) || 36,
        salvageValue: Number(form.salvageValue) || 0,
        period: form.period || 'MONTH',
      });
      setShowModal(false);
      showToast('Thiết lập khấu hao thành công!', 'success');
      fetchData();
    } catch (err) {
      showToast(getApiError(err, 'Thiết lập khấu hao thất bại'), 'error');
      setLoading(false);
    }
  };

  const withoutDep = devices.filter((d) => !data.some((dep) => dep.deviceId === d.id));

  return (
    <div className="page active" id="page-depreciation">
      <div className="action-bar">
        <div style={{ fontSize: '13px', color: 'var(--text3)' }}>
          Quản lý khấu hao tài sản theo phương pháp đường thẳng hoặc số dư giảm dần
        </div>
        <button type="button" className="btn btn-primary" onClick={() => setShowModal(true)}>
          <i className="ti ti-plus" /> Thiết lập khấu hao
        </button>
      </div>

      <div className="grid-2" style={{ marginBottom: '20px' }}>
        <div className="card">
          <div className="card-header"><div className="card-title">Tổng quan khấu hao</div></div>
          <div className="card-body">
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
              <div style={{ background: 'var(--bg3)', borderRadius: '8px', padding: '14px', textAlign: 'center' }}>
                <div style={{ fontSize: '22px', fontWeight: 700, color: 'var(--info)', fontFamily: "'DM Mono', monospace" }}>
                  {data.length}
                </div>
                <div style={{ fontSize: '12px', color: 'var(--text3)', marginTop: '4px' }}>Đang khấu hao</div>
              </div>
              <div style={{ background: 'var(--bg3)', borderRadius: '8px', padding: '14px', textAlign: 'center' }}>
                <div style={{ fontSize: '22px', fontWeight: 700, color: 'var(--success)', fontFamily: "'DM Mono', monospace" }}>
                  {withoutDep.length}
                </div>
                <div style={{ fontSize: '12px', color: 'var(--text3)', marginTop: '4px' }}>Chưa thiết lập</div>
              </div>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="card-header"><div className="card-title">Tổng giá trị tài sản</div></div>
          <div className="card-body">
            <div style={{ marginBottom: '14px' }}>
              <div style={{ fontSize: '12px', color: 'var(--text3)', marginBottom: '4px' }}>Tổng nguyên giá (đã KH)</div>
              <div style={{ fontSize: '22px', fontWeight: 700, fontFamily: "'DM Mono', monospace" }}>
                {data
                  .reduce((s, d) => s + (d.device?.originalCost || 0), 0)
                  .toLocaleString('vi-VN')}
                đ
              </div>
            </div>
            <div>
              <div style={{ fontSize: '12px', color: 'var(--text3)', marginBottom: '4px' }}>Tổng giá trị còn lại</div>
              <div style={{ fontSize: '22px', fontWeight: 700, fontFamily: "'DM Mono', monospace", color: 'var(--success)' }}>
                {data.reduce((s, d) => s + (d.currentValue || 0), 0).toLocaleString('vi-VN')}đ
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-header"><div className="card-title">Chi tiết khấu hao từng thiết bị</div></div>
        <div className="card-body" style={{ padding: '16px' }}>
          {loading ? (
            <div style={{ color: 'var(--text3)' }}>Đang tải...</div>
          ) : (
            data.map((d) => {
              const original = d.device?.originalCost || 1;
              const pct = Math.min(100, Math.max(0, ((original - (d.currentValue || 0)) / original) * 100));
              return (
                <div className="dep-card" key={d.id}>
                  <div className="dep-header">
                    <div>
                      <div className="dep-name">{d.device?.name || 'Thiết bị'}</div>
                      <div className="dep-code">
                        TB-{String(d.deviceId).padStart(3, '0')}
                      </div>
                    </div>
                    <span className="badge badge-info">
                      {d.method === 'STRAIGHT_LINE' ? 'Đường thẳng' : d.method}
                    </span>
                  </div>
                  <div className="dep-values">
                    <div className="dep-val-item">
                      Nguyên giá
                      <span>{original.toLocaleString('vi-VN')}đ</span>
                    </div>
                    <div className="dep-val-item">
                      Còn lại
                      <span style={{ color: 'var(--success)' }}>{(d.currentValue || 0).toLocaleString('vi-VN')}đ</span>
                    </div>
                    <div className="dep-val-item">
                      Phương pháp
                      <span>{d.method === 'STRAIGHT_LINE' ? 'Đường thẳng' : d.method}</span>
                    </div>
                  </div>
                  <div className="progress" style={{ height: '6px' }}>
                    <div className="progress-bar" style={{ width: `${pct}%`, background: 'var(--warning)' }} />
                  </div>
                  <div style={{ fontSize: '11.5px', color: 'var(--text3)', marginTop: '5px' }}>
                    Đã khấu hao {Math.round(pct)}%
                  </div>
                </div>
              );
            })
          )}
          {!loading && data.length === 0 && (
            <div className="empty-state">
              <div className="empty-icon"><i className="ti ti-chart-line" /></div>
              <div className="empty-text">Chưa có dữ liệu khấu hao</div>
            </div>
          )}
        </div>
      </div>

      {showModal && (
        <div className="modal-overlay open" onClick={(e) => e.target === e.currentTarget && setShowModal(false)}>
          <div className="modal">
            <div className="modal-header">
              <div className="modal-title">Thiết lập khấu hao</div>
              <div className="modal-close" onClick={() => setShowModal(false)}><i className="ti ti-x" /></div>
            </div>
            <div className="modal-body">
              <div className="form-group">
                <div className="form-label">Chọn thiết bị *</div>
                <select
                  className="form-select"
                  value={form.deviceId}
                  onChange={(e) => {
                    const dev = withoutDep.find((d) => d.id === Number(e.target.value));
                    setForm({
                      ...form,
                      deviceId: e.target.value,
                      initialValue: dev?.originalCost?.toString() || '',
                    });
                  }}
                >
                  <option value="">-- Chọn thiết bị --</option>
                  {withoutDep.map((d) => (
                    <option key={d.id} value={d.id}>
                      TB-{String(d.id).padStart(3, '0')} — {d.name} ({d.originalCost?.toLocaleString('vi-VN')}đ)
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <div className="form-label">Phương pháp khấu hao *</div>
                <select
                  className="form-select"
                  value={form.method}
                  onChange={(e) => setForm({ ...form, method: e.target.value })}
                >
                  <option value="STRAIGHT_LINE">Đường thẳng (Straight-Line)</option>
                  <option value="DECLINING">Số dư giảm dần (Declining Balance)</option>
                </select>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <div className="form-label">Thời gian sử dụng (tháng)</div>
                  <input
                    className="form-input"
                    type="number"
                    value={form.usefulLifeMonths}
                    onChange={(e) => setForm({ ...form, usefulLifeMonths: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <div className="form-label">Giá trị hiện tại (VNĐ) *</div>
                  <input
                    className="form-input"
                    type="number"
                    value={form.initialValue}
                    onChange={(e) => setForm({ ...form, initialValue: e.target.value })}
                  />
                </div>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <div className="form-label">Kỳ tính khấu hao *</div>
                  <select
                    className="form-select"
                    value={form.period}
                    onChange={(e) => setForm({ ...form, period: e.target.value })}
                  >
                    <option value="MONTH">Hàng tháng (Monthly)</option>
                    <option value="QUARTER">Hàng quý (Quarterly)</option>
                    <option value="YEAR">Hàng năm (Yearly)</option>
                  </select>
                </div>
                <div className="form-group">
                  <div className="form-label">Giá trị thu hồi ước tính</div>
                  <input
                    className="form-input"
                    type="number"
                    value={form.salvageValue}
                    onChange={(e) => setForm({ ...form, salvageValue: e.target.value })}
                  />
                </div>
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>Hủy</button>
              <button type="button" className="btn btn-primary" onClick={handleSave}>
                <i className="ti ti-device-floppy" /> Lưu cài đặt
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
